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
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_OutBoundDetailApp
    {
        private IT_OutBoundDetailRepository service = new T_OutBoundDetailRepository();
        public IQueryable<T_OutBoundDetailEntity> FindList(Expression<Func<T_OutBoundDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_OutBoundDetailEntity FindEntity(Expression<Func<T_OutBoundDetailEntity, bool>> predicate)
        {
            T_OutBoundDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_OutBoundDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_OutBoundDetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_OutBoundDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public List<T_OutBoundDetailEntity> GetList(Pagination pagination, string outBoundID, string keyword = "")
        {
            var expression = ExtLinq.True<T_OutBoundDetailEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.OutBoundID == outBoundID && (t.ItemName.Contains(keyword) ||t.ItemCode.Contains(keyword)));
            }
            else expression = expression.And(t => t.OutBoundID == outBoundID);
            return service.FindList(expression, pagination).ToList();
        }
        public void SubmitForm(T_OutBoundDetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_OutBoundDetailEntity entity)
        {
            service.Insert(entity);
        }
    }
}
