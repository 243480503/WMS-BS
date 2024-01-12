/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.SystemManage;
using MST.Domain.IRepository.SystemManage;
using MST.Repository.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.SystemManage
{
    public class OrganizeApp
    {
        private IOrganizeRepository service = new OrganizeRepository();
        public OrganizeEntity FindEntity(Expression<Func<OrganizeEntity, bool>> predicate)
        {
            OrganizeEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public IQueryable<OrganizeEntity> FindList(Expression<Func<OrganizeEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public List<OrganizeEntity> GetList()
        {
            return service.IQueryable().OrderBy(t => t.F_CreatorTime).ToList();
        }
        public OrganizeEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            if (service.IQueryable().Count(t => t.F_ParentId.Equals(keyValue)) > 0)
            {
                throw new Exception("删除失败！操作的对象包含了下级数据。");
            }
            else
            {
                service.Delete(t => t.F_Id == keyValue);
            }
        }
        public void SubmitForm(OrganizeEntity organizeEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                organizeEntity.Modify(keyValue);
                service.Update(organizeEntity);
            }
            else
            {
                organizeEntity.Create();
                service.Insert(organizeEntity);
            }
        }
    }
}
