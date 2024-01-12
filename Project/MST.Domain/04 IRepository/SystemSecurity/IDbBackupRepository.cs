/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Data;
using MST.Domain.Entity.SystemSecurity;

namespace MST.Domain.IRepository.SystemSecurity
{
    public interface IDbBackupRepository : IRepositoryBase<DbBackupEntity>
    {
        void DeleteForm(string keyValue);

        /// <summary>
        /// 数据库备份
        /// </summary>
        /// <param name="dbBackupEntity"></param>
        /// <param name="timeOut">单位秒</param>
        void ExecuteDbBackup(DbBackupEntity dbBackupEntity,int timeOut);

        string GetDBName();
    }
}
