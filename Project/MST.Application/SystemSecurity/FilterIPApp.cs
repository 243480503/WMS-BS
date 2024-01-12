/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.IRepository.SystemSecurity;
using MST.Repository.SystemSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.SystemSecurity
{
    public class FilterIPApp
    {
        private IFilterIPRepository service = new FilterIPRepository();

        public List<FilterIPEntity> GetList(Pagination pagination, string keyword)
        {
            var expression = ExtLinq.True<FilterIPEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_StartIP.Contains(keyword));
            }
            return service.FindList(expression, pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }

        public IQueryable<FilterIPEntity> FindList(Expression<Func<FilterIPEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public FilterIPEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(FilterIPEntity filterIPEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(filterIPEntity.F_Description)) filterIPEntity.F_Description = filterIPEntity.F_Description.Replace("\n", " ");
            if (!string.IsNullOrEmpty(keyValue))
            {
                filterIPEntity.Modify(keyValue);
                service.Update(filterIPEntity);
            }
            else
            {
                filterIPEntity.Create();
                service.Insert(filterIPEntity);
            }
        }
    }
}
