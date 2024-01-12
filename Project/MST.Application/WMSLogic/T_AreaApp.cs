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
    public class T_AreaApp
    {
        private IT_AreaRepository service = new T_AreaRepository();
        public IQueryable<T_AreaEntity> FindList(Expression<Func<T_AreaEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_AreaEntity FindEntity(Expression<Func<T_AreaEntity, bool>> predicate)
        {
            T_AreaEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_AreaEntity> FindListAsNoTracking(Expression<Func<T_AreaEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_AreaEntity FindEntityAsNoTracking(Expression<Func<T_AreaEntity, bool>> predicate)
        {
            T_AreaEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_AreaEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_AreaEntity> GetList(Pagination pagination)
        {
            return service.FindList(pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }

        public void Delete(Expression<Func<T_AreaEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_AreaEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_AreaEntity itemsEntity, string keyValue)
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

        public void Insert(T_AreaEntity entity)
        {
            service.Insert(entity);
        }
    }
}
