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
    public class T_QADetailApp
    {
        private IT_QADetailRepository service = new T_QADetailRepository();
        public IQueryable<T_QADetailEntity> FindList(Expression<Func<T_QADetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_QADetailEntity FindEntity(Expression<Func<T_QADetailEntity, bool>> predicate)
        {
            T_QADetailEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_QADetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_QADetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_QADetailEntity> GetList(Pagination pagination, string qaID, string keyword)
        {
            var expression = ExtLinq.True<T_QADetailEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.QAID == qaID && (t.ItemName.Contains(keyword) || t.ItemCode.Contains(keyword)));
            }
            else expression = expression.And(t => t.QAID == qaID);
            return service.FindList(expression, pagination).ToList();
        }

        public T_QADetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_QADetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_QADetailEntity entity)
        {
            service.Insert(entity);
        }
        public void Update(T_QADetailEntity entity)
        {
            service.Update(entity);
        }
    }
}
