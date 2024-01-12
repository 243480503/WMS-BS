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
    public class T_InBoundDetailApp
    {
        private IT_InBoundDetailRepository service = new T_InBoundDetailRepository();

        public IQueryable<T_InBoundDetailEntity> FindList(Expression<Func<T_InBoundDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_InBoundDetailEntity FindEntity(Expression<Func<T_InBoundDetailEntity, bool>> predicate)
        {
             T_InBoundDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }


        public List<T_InBoundDetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_InBoundDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }

        public void Delete(Expression<Func<T_InBoundDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public List<T_InBoundDetailEntity> GetList(Pagination pagination, string InBoundID, string keyword)
        {
            var expression = ExtLinq.True<T_InBoundDetailEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.InBoundID == InBoundID && (t.ItemName.Contains(keyword) ||t.ItemCode.Contains(keyword) ));
            }
            else expression = expression.And(t => t.InBoundID == InBoundID);
            return service.FindList(expression, pagination).ToList();
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_InBoundDetailEntity itemsEntity, string keyValue)
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

        public void Update(T_InBoundDetailEntity entity)
        {
            service.Update(entity);
        }

        public void Insert(T_InBoundDetailEntity entity)
        {
            service.Insert(entity);
        }
    }
}
