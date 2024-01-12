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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.SystemManage
{
    public class RoleApp
    {
        /// <summary>
        /// 最后一次权限修改时间，用于实时刷新权限
        /// </summary>
        public static DateTime LastModifyRoleAuthorize = DateTime.MinValue;

        private IRoleRepository service = new RoleRepository();
        private ModuleApp moduleApp = new ModuleApp();
        private ModuleButtonApp moduleButtonApp = new ModuleButtonApp();

        public RoleEntity FindEntity(Expression<Func<RoleEntity, bool>> predicate)
        {
            RoleEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<RoleEntity> FindList(Expression<Func<RoleEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public List<RoleEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<RoleEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_FullName.Contains(keyword) || t.F_EnCode.Contains(keyword));
            }
            expression = expression.And(t => t.F_Category == 1);
            return service.IQueryable(expression).OrderBy(t => t.F_SortCode).ToList();
        }

        public List<RoleEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<RoleEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_FullName.Contains(keyword) || t.F_EnCode.Contains(keyword));
            }
            expression = expression.And(t => t.F_Category == 1);
            return service.FindList(expression, pagination).OrderBy(o => o.F_SortCode).ToList();
        }
        public RoleEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.DeleteForm(keyValue);
        }
        public void SubmitForm(RoleEntity roleEntity, string[] permissionIds, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                roleEntity.F_Id = keyValue;
            }
            else
            {
                roleEntity.F_Id = Common.GuId();
            }
            var moduledata = moduleApp.GetList();
            var buttondata = moduleButtonApp.GetList();
            List<RoleAuthorizeEntity> roleAuthorizeEntitys = new List<RoleAuthorizeEntity>();
            foreach (var itemId in permissionIds)
            {
                RoleAuthorizeEntity roleAuthorizeEntity = new RoleAuthorizeEntity();
                roleAuthorizeEntity.F_Id = Common.GuId();
                roleAuthorizeEntity.F_ObjectType = 1;
                roleAuthorizeEntity.F_ObjectId = roleEntity.F_Id;
                roleAuthorizeEntity.F_ItemId = itemId;
                if (moduledata.Find(t => t.F_Id == itemId) != null)
                {
                    roleAuthorizeEntity.F_ItemType = 1;
                }
                if (buttondata.Find(t => t.F_Id == itemId) != null)
                {
                    roleAuthorizeEntity.F_ItemType = 2;
                }
                roleAuthorizeEntitys.Add(roleAuthorizeEntity);
            }
            service.SubmitForm(roleEntity, roleAuthorizeEntitys, keyValue);
        }
    }
}
