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
    public class V_QAOrderReportApp
    {
        private IV_QAOrderReportRepository service = new V_QAOrderReportRepository();
        public IQueryable<V_QAOrderReportEntity> FindList(Expression<Func<V_QAOrderReportEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public V_QAOrderReportEntity FindEntity(Expression<Func<V_QAOrderReportEntity, bool>> predicate)
        {
            V_QAOrderReportEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<V_QAOrderReportEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<V_QAOrderReportEntity> FindList(string strSql, Pagination pagination)
        {
            return service.FindList(strSql, pagination).ToList();
        }

        public List<V_QAOrderReportEntity> FindList(Pagination pagination,string keyword)
        {
            var expression = ExtLinq.True<V_QAOrderReportEntity>();
            if(!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(o => o.RefOrderCode.Contains(keyword) || o.ItemCode.Contains(keyword) || o.ItemName.Contains(keyword) || o.Lot.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }
    }
}



