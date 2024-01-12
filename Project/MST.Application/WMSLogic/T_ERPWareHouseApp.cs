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
    public class T_ERPWarehouseApp
    {
        private IT_ERPWarehouseRepository service = new T_ERPWarehouseRepository();
        public IQueryable<T_ERPWarehouseEntity> FindList(Expression<Func<T_ERPWarehouseEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ERPWarehouseEntity FindEntity(Expression<Func<T_ERPWarehouseEntity, bool>> predicate)
        {
            T_ERPWarehouseEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_ERPWarehouseEntity> FindListAsNoTracking(Expression<Func<T_ERPWarehouseEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_ERPWarehouseEntity FindEntityAsNoTracking(Expression<Func<T_ERPWarehouseEntity, bool>> predicate)
        {
            T_ERPWarehouseEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_ERPWarehouseEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_ERPWarehouseEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.ERPHouseCode.Contains(keyword) || t.ERPHouseName.Contains(keyword));
            }
            return service.FindList(expression, pagination).OrderBy(o => o.ERPHouseCode).ToList();
        }

        public void Delete(Expression<Func<T_ERPWarehouseEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_ERPWarehouseEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_ERPWarehouseEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);

        }
        public void DeleteForm(string keyValue)

        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_ERPWarehouseEntity itemsEntity, string keyValue)

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

        public void Insert(T_ERPWarehouseEntity entity)
        {
            service.Insert(entity);
        }
    }
}
