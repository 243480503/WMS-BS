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
    public class T_ReportSetApp
    {
        private IT_ReportSetRepository service = new T_ReportSetRepository();
        public IQueryable<T_ReportSetEntity> FindList(Expression<Func<T_ReportSetEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ReportSetEntity FindEntity(Expression<Func<T_ReportSetEntity, bool>> predicate)
        {
            T_ReportSetEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_ReportSetEntity> FindListAsNoTracking(Expression<Func<T_ReportSetEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_ReportSetEntity FindEntityAsNoTracking(Expression<Func<T_ReportSetEntity, bool>> predicate)
        {
            T_ReportSetEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_ReportSetEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_ReportSetEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_ReportSetEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.ReportName.ToString().Contains(keyword));
            }
            return service.FindList(expression, pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }

        public void Delete(Expression<Func<T_ReportSetEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public T_ReportSetEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_ReportSetEntity itemsEntity, string keyValue)
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

        public void Insert(T_ReportSetEntity entity)
        {
            service.Insert(entity);
        }
    }
}
