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
    public class V_OutBoundOrderReportApp
    {
        private IV_OutBoundOrderReportRepository service = new V_OutBoundOrderReportRepository();
        public IQueryable<V_OutBoundOrderReportEntity> FindList(Expression<Func<V_OutBoundOrderReportEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public V_OutBoundOrderReportEntity FindEntity(Expression<Func<V_OutBoundOrderReportEntity, bool>> predicate)
        {
            V_OutBoundOrderReportEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<V_OutBoundOrderReportEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<V_OutBoundOrderReportEntity> FindList(string strSql, Pagination pagination)
        {
            return service.FindList(strSql, pagination).ToList();
        }

        public List<V_OutBoundOrderReportEntity> FindList(Pagination pagination,string keyword)
        {
            var expression = ExtLinq.True<V_OutBoundOrderReportEntity>();
            if(!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(o => o.RefOrderCode.Contains(keyword) || o.ItemCode.Contains(keyword) || o.ItemName.Contains(keyword) || o.Lot.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }
    }
}



