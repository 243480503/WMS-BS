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
    public class T_TimerApp
    {
        private IT_TimerRepository service = new T_TimerRepository();

        public IQueryable<T_TimerEntity> FindList(Expression<Func<T_TimerEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_TimerEntity FindEntity(Expression<Func<T_TimerEntity, bool>> predicate)
        {
            T_TimerEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public IQueryable<T_TimerEntity> FindListAsNoTracking(Expression<Func<T_TimerEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_TimerEntity FindEntityAsNoTracking(Expression<Func<T_TimerEntity, bool>> predicate)
        {
            T_TimerEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }
        public List<T_TimerEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_TimerEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.TimerName.Contains(keyword) || t.TimerCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public List<T_TimerEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_TimerEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }

        public void Delete(Expression<Func<T_TimerEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_TimerEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.Create();
                itemsEntity.F_DeleteMark = false;
                service.Insert(itemsEntity);
            }
        }

        public void Insert(T_TimerEntity entity)
        {
            service.Insert(entity);
        }
    }
}
