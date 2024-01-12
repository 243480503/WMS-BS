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
    public class T_CountResultApp
    {
        private IT_CountResultRepository service = new T_CountResultRepository();
        public IQueryable<T_CountResultEntity> FindList(Expression<Func<T_CountResultEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public T_CountResultEntity FindEntity(Expression<Func<T_CountResultEntity, bool>> predicate)
        {
            T_CountResultEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_CountResultEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_CountResultEntity> GetList(string countDetailID = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_CountResultEntity>();
            if (!string.IsNullOrEmpty(countDetailID))
            {
                expression = expression.And(t => t.CountDetailID == countDetailID);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.ItemCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public List<T_CountResultEntity> GetList(Pagination pagination, string countDetailID = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_CountResultEntity>();
            if (!string.IsNullOrEmpty(countDetailID))
            {
                expression = expression.And(t => t.CountDetailID == countDetailID);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.ItemCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }
        public void Delete(Expression<Func<T_CountResultEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_CountResultEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_CountResultEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.RefOrderCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public T_CountResultEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_CountResultEntity itemsEntity, string keyValue)
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
        public void Insert(T_CountResultEntity entity)
        {
            service.Insert(entity);
        }
    }
}
