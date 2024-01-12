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
    public class T_MoveRecordApp
    {
        private IT_MoveRecordRepository service = new T_MoveRecordRepository();
        public IQueryable<T_MoveRecordEntity> FindList(Expression<Func<T_MoveRecordEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_MoveRecordEntity FindEntity(Expression<Func<T_MoveRecordEntity, bool>> predicate)
        {
            T_MoveRecordEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public void Delete(Expression<Func<T_MoveRecordEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_MoveRecordEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_MoveRecordEntity> GetList(Pagination pagination, string moveID, string keyword = "")
        {
            var expression = ExtLinq.True<T_MoveRecordEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.MoveID == moveID && (t.ItemName.Contains(keyword) || t.ItemCode.Contains(keyword) || t.Lot.Contains(keyword)));
            }
            else expression = expression.And(t => t.MoveID == moveID);
            return service.FindList(expression, pagination).ToList();
        }
        public T_MoveRecordEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_MoveRecordEntity moveRecordEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                moveRecordEntity.Modify(keyValue);
                service.Update(moveRecordEntity);
            }
            else
            {
                moveRecordEntity.Create();
                service.Insert(moveRecordEntity);
            }
        }
        public void Insert(T_MoveRecordEntity entity)
        {
            service.Insert(entity);
        }
    }
}
