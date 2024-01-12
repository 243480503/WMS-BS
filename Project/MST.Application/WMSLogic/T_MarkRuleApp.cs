/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_MarkRuleApp
    {
        private IT_MarkRuleRepository service = new T_MarkRuleRepository();
        public IQueryable<T_MarkRuleEntity> FindList(Expression<Func<T_MarkRuleEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_MarkRuleEntity FindEntity(Expression<Func<T_MarkRuleEntity, bool>> predicate)
        {
            T_MarkRuleEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_MarkRuleEntity> GetList()
        {
            return service.IQueryable().ToList();
        }



        public T_MarkRuleEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }

        public void Delete(Expression<Func<T_MarkRuleEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public IQueryable<T_MarkRuleEntity> Tab(Expression<Func<T_MarkRuleEntity, bool>> predicate)
        {
            var expression = ExtLinq.True<T_MarkRuleEntity>();
            expression.And(predicate);
            return service.IQueryable(expression);
        }

        public IQueryable<T_MarkRuleEntity> Tab()
        {
            var expression = ExtLinq.True<T_MarkRuleEntity>();
            return service.IQueryable(expression);
        }

        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_MarkRuleEntity itemsEntity, string keyValue)
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

        public void Insert(T_MarkRuleEntity entity)
        {
            service.Insert(entity);
        }

        public void Update(T_MarkRuleEntity entity)
        {
            service.Update(entity);
        }
    }
}
