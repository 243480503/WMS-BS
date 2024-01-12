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
    public class T_LocCountRecordApp
    {
        private IT_LocCountRecordRepository service = new T_LocCountRecordRepository();
        public IQueryable<T_LocCountRecordEntity> FindList(Expression<Func<T_LocCountRecordEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_LocCountRecordEntity FindEntity(Expression<Func<T_LocCountRecordEntity, bool>> predicate)
        {
            T_LocCountRecordEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public void Delete(Expression<Func<T_LocCountRecordEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_LocCountRecordEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_LocCountRecordEntity> GetList(Pagination pagination, string locCountID, string keyword = "")
        {
            var expression = ExtLinq.True<T_LocCountRecordEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.LocCountID == locCountID && (t.LocationCode.Contains(keyword) || t.BarCode.Contains(keyword)));
            }
            else expression = expression.And(t => t.LocCountID == locCountID);
            return service.FindList(expression, pagination).ToList();
        }
        public T_LocCountRecordEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_LocCountRecordEntity locCountRecordEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                locCountRecordEntity.Modify(keyValue);
                service.Update(locCountRecordEntity);
            }
            else
            {
                locCountRecordEntity.Create();
                service.Insert(locCountRecordEntity);
            }
        }
        public void Insert(T_LocCountRecordEntity entity)
        {
            service.Insert(entity);
        }
    }
}
