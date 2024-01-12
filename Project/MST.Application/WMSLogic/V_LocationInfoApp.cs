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
    public class V_LocationInfoApp
    {
        private IV_LocationInfoRepository service = new V_LocationInfoRepository();
        public IQueryable<V_LocationInfoEntity> FindList(Expression<Func<V_LocationInfoEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public V_LocationInfoEntity FindEntity(Expression<Func<V_LocationInfoEntity, bool>> predicate)
        {
            V_LocationInfoEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<V_LocationInfoEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<V_LocationInfoEntity> FindList(string strSql, Pagination pagination)
        {
            return service.FindList(strSql, pagination).ToList();
        }

        public List<V_LocationInfoEntity> FindList(Pagination pagination, string keyword)
        {
            var expression = ExtLinq.True<V_LocationInfoEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(o => o.ItemCode.Contains(keyword) || o.LocationCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }
    }
}



