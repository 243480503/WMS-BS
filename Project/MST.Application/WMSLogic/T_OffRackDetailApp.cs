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
    public class T_OffRackDetailApp
    {
        private IT_OffRackDetailRepository service = new T_OffRackDetailRepository();
        public IQueryable<T_OffRackDetailEntity> FindList(Expression<Func<T_OffRackDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public T_OffRackDetailEntity FindEntity(Expression<Func<T_OffRackDetailEntity, bool>> predicate)
        {
            T_OffRackDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_OffRackDetailEntity> GetList()
        {
            return service.IQueryable().OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public void Delete(Expression<Func<T_OffRackDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_OffRackDetailEntity> GetList(Pagination pagination, string OffRackID, string keyword)
        {
            var expression = ExtLinq.True<T_OffRackDetailEntity>();
            if (!string.IsNullOrEmpty(keyword)) expression = expression.And(t => t.OffRackID == OffRackID && t.LocationCode.Contains(keyword));
            else expression = expression.And(t => t.OffRackID == OffRackID);
            return service.FindList(expression, pagination).ToList();
        }

        public T_OffRackDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_OffRackDetailEntity itemsEntity, string keyValue)
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
        public void Insert(T_OffRackDetailEntity entity)
        {
            service.Insert(entity);
        }
    }
}
