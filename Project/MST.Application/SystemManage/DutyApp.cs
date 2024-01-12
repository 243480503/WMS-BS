/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.IRepository.SystemManage;
using MST.Repository.SystemManage;
using System.Collections.Generic;
using System.Linq;

namespace MST.Application.SystemManage
{
    public class DutyApp
    {
        private IRoleRepository service = new RoleRepository();

        public List<RoleEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<RoleEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_FullName.Contains(keyword) || t.F_EnCode.Contains(keyword));
            }
            expression = expression.And(t => t.F_Category == 2);
            return service.IQueryable(expression).OrderBy(t => t.F_SortCode).ToList();
        }
        public RoleEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(RoleEntity roleEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(roleEntity.F_Description)) roleEntity.F_Description = roleEntity.F_Description.Replace("\n", " ");
            if (!string.IsNullOrEmpty(keyValue))
            {
                roleEntity.Modify(keyValue);
                service.Update(roleEntity);
            }
            else
            {
                roleEntity.Create();
                roleEntity.F_Category = 2;
                service.Insert(roleEntity);
            }
        }
    }
}
