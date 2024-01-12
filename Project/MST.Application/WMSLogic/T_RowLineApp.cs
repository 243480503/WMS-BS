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
    /// <summary>
    /// 巷道对应的行
    /// </summary>
    public class T_RowLineApp
    {
        private IT_RowLineRepository service = new T_RowLineRepository();
        public IQueryable<T_RowLineEntity> FindList(Expression<Func<T_RowLineEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_RowLineEntity FindEntity(Expression<Func<T_RowLineEntity, bool>> predicate)
        {
            T_RowLineEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_RowLineEntity> FindListAsNoTracking(Expression<Func<T_RowLineEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_RowLineEntity FindEntityAsNoTracking(Expression<Func<T_RowLineEntity, bool>> predicate)
        {
            T_RowLineEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_RowLineEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_RowLineEntity> GetList(Pagination pagination,string wayID,string keyword = "")
        {
            var expression = ExtLinq.True<T_RowLineEntity>();

            if (!string.IsNullOrEmpty(wayID))
            {
                expression = expression.And(o=>o.DevRowID == wayID);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.Line.ToString()==keyword);
            }
            return service.FindList(expression, pagination).ToList();
        }

        public void Delete(Expression<Func<T_RowLineEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }


        public T_RowLineEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_RowLineEntity itemsEntity, string keyValue)
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

        public void Insert(T_RowLineEntity entity)
        {
            service.Insert(entity);
        }
    }
}
