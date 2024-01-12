/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_SupplierApp
    {
        private IT_SupplierRepository service = new T_SupplierRepository();
        public IQueryable<T_SupplierEntity> FindList(Expression<Func<T_SupplierEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_SupplierEntity FindEntity(Expression<Func<T_SupplierEntity, bool>> predicate)
        {
            T_SupplierEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_SupplierEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_SupplierEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_SupplierEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_SupplierEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_DeleteMark == false && (t.SupplierName.Contains(keyword) ||t.SupplierCode.Contains(keyword)) );
            }
            else expression = expression.And(t => t.F_DeleteMark == false);
            return service.FindList(expression, pagination).ToList();
        }
        public T_SupplierEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_SupplierEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(itemsEntity.Remark)) itemsEntity.Remark = itemsEntity.Remark.Replace("\n", " ");
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.Create();
                service.Insert(itemsEntity);
            }
        }

        public void Update(T_SupplierEntity itemsEntity)
        {
            service.Update(itemsEntity);
        }

        public void Insert(T_SupplierEntity entity)
        {
            service.Insert(entity);
        }
    }
}
