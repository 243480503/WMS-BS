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
    public class T_CountDetailApp
    {
        private IT_CountDetailRepository service = new T_CountDetailRepository();
        public IQueryable<T_CountDetailEntity> FindList(Expression<Func<T_CountDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_CountDetailEntity FindEntity(Expression<Func<T_CountDetailEntity, bool>> predicate)
        {
            T_CountDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_CountDetailEntity> GetList()
        {
            return service.IQueryable().OrderByDescending(t => t.F_CreatorTime).ToList();
        }

        public void Delete(Expression<Func<T_CountDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_CountDetailEntity> GetList(string CountID, string keyword)
        {
            var expression = ExtLinq.True<T_CountDetailEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.CountID == CountID && (t.ItemName.Contains(keyword) || t.ItemCode.Contains(keyword)));
            }
            else expression = expression.And(t => t.CountID == CountID);
            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public List<T_CountDetailEntity> GetList(Pagination pagination, string CountID, string keyword)
        {
            var expression = ExtLinq.True<T_CountDetailEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.CountID == CountID && (t.ItemName.Contains(keyword) ||t.ItemCode.Contains(keyword)));
            }
            else expression = expression.And(t => t.CountID == CountID);
            return service.FindList(expression, pagination).ToList();
        }

        public T_CountDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_CountDetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_CountDetailEntity entity)
        {
            service.Insert(entity);
        }
    }
}
