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
    public class T_ReportSourceApp
    {
        private IT_ReportSourceRepository service = new T_ReportSourceRepository();
        public IQueryable<T_ReportSourceEntity> FindList(Expression<Func<T_ReportSourceEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ReportSourceEntity FindEntity(Expression<Func<T_ReportSourceEntity, bool>> predicate)
        {
            T_ReportSourceEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_ReportSourceEntity> FindListAsNoTracking(Expression<Func<T_ReportSourceEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_ReportSourceEntity FindEntityAsNoTracking(Expression<Func<T_ReportSourceEntity, bool>> predicate)
        {
            T_ReportSourceEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_ReportSourceEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_ReportSourceEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_ReportSourceEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.SourceName.Contains(keyword));
            }
            return service.FindList(expression, pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }

        public void Delete(Expression<Func<T_ReportSourceEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_ReportSourceEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }

        public void Update(T_ReportSourceEntity entity)
        {
            service.Update(entity);
        }
        public void SubmitForm(T_ReportSourceEntity itemsEntity, string keyValue)
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

        public void Insert(T_ReportSourceEntity entity)
        {
            service.Insert(entity);
        }
    }
}
