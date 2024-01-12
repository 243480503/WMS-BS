/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.IRepository.SystemSecurity;
using MST.Repository.SystemSecurity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MST.Application.SystemSecurity
{
    public class DbBackupApp
    {
        private IDbBackupRepository service = new DbBackupRepository();

        public List<DbBackupEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<DbBackupEntity>();
            var queryParam = queryJson.ToJObject();
            if (!queryParam["condition"].IsEmpty() && !queryParam["keyword"].IsEmpty())
            {
                string condition = queryParam["condition"].ToString();
                string keyword = queryParam["keyword"].ToString();
                switch (condition)
                {
                    case "DbName":
                        expression = expression.And(t => t.F_DbName.Contains(keyword));
                        break;
                    case "FileName":
                        expression = expression.And(t => t.F_FileName.Contains(keyword));
                        break;
                }
            }
            return service.FindList(expression, pagination).OrderByDescending(o => o.F_BackupTime).ToList();
        }

        public string GetDBName()
        {
            return service.GetDBName();
        }

        public DbBackupEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.DeleteForm(keyValue);
        }
        public void SubmitForm(DbBackupEntity dbBackupEntity)
        {
            dbBackupEntity.F_Id = Common.GuId();
            dbBackupEntity.F_EnabledMark = true;
            dbBackupEntity.F_BackupTime = DateTime.Now;
            dbBackupEntity.F_CreatorTime = DateTime.Now;
            service.ExecuteDbBackup(dbBackupEntity, 60*5);
        }
    }
}
