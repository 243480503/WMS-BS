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
    public class T_ContainerTypeApp
    {
        private IT_ContainerTypeRepository service = new T_ContainerTypeRepository();
        public IQueryable<T_ContainerTypeEntity> FindList(Expression<Func<T_ContainerTypeEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ContainerTypeEntity FindEntity(Expression<Func<T_ContainerTypeEntity, bool>> predicate)
        {
            T_ContainerTypeEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_ContainerTypeEntity> FindListAsNoTracking(Expression<Func<T_ContainerTypeEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_ContainerTypeEntity FindEntityAsNoTracking(Expression<Func<T_ContainerTypeEntity, bool>> predicate)
        {
            T_ContainerTypeEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_ContainerTypeEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_ContainerTypeEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public List<T_ContainerTypeEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_ContainerTypeEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.ContainerTypeCode.Contains(keyword) || t.ContainerTypeName.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public T_ContainerTypeEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_ContainerTypeEntity itemsEntity, string keyValue)
        {
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

        public void Insert(T_ContainerTypeEntity entity)
        {
            service.Insert(entity);
        }
    }
}
