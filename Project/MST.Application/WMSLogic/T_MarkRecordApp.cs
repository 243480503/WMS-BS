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
    public class T_MarkRecordApp
    {
        private IT_MarkRecordRepository service = new T_MarkRecordRepository();
        public IQueryable<T_MarkRecordEntity> FindList(Expression<Func<T_MarkRecordEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_MarkRecordEntity FindEntity(Expression<Func<T_MarkRecordEntity, bool>> predicate)
        {
            T_MarkRecordEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_MarkRecordEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_MarkRecordEntity> GetList(Pagination pagination, string ruleID, string keyword = "")
        {
            var expression = ExtLinq.True<T_MarkRecordEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.MarkRuleID == ruleID && (t.BarCode.Contains(keyword) ||t.ItemCode.Contains(keyword)) );
            }
            else expression = expression.And(t => t.MarkRuleID == ruleID);
            return service.FindList(expression, pagination).ToList();
        }
        public void Delete(Expression<Func<T_MarkRecordEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_MarkRecordEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_MarkRecordEntity itemsEntity, string keyValue)
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

        public void Insert(T_MarkRecordEntity entity)
        {
            service.Insert(entity);
        }
        public void Update(T_MarkRecordEntity entity)
        {
            service.Update(entity);
        }
    }
}
