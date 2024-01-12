/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Data;
using MST.Data.Extensions;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.IRepository.SystemSecurity;
using MST.Repository.SystemSecurity;

namespace MST.Repository.SystemSecurity
{
    public class DbBackupRepository : RepositoryBase<DbBackupEntity>, IDbBackupRepository
    {
        public void DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                var dbBackupEntity = db.FindEntity<DbBackupEntity>(keyValue);
                if (dbBackupEntity != null)
                {
                    FileHelper.DeleteFile(dbBackupEntity.F_FilePath);
                }
                db.Delete<DbBackupEntity>(dbBackupEntity);
                db.Commit();
            }
        }

        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <returns></returns>
        public string GetDBName()
        {
            return dbcontext.Database.Connection.Database;
        }

        public void ExecuteDbBackup(DbBackupEntity dbBackupEntity, int timeOut)
        {
            DbHelper.ExecuteSqlCommand(string.Format("backup database {0} to disk ='{1}'", dbBackupEntity.F_DbName, dbBackupEntity.F_FilePath), timeOut);
            dbBackupEntity.F_FileSize = FileHelper.ToFileSize(FileHelper.GetFileSize(dbBackupEntity.F_FilePath));
            dbBackupEntity.F_FilePath = "/Resource/DbBackup/" + dbBackupEntity.F_FileName;
            this.Insert(dbBackupEntity);
        }
    }
}
