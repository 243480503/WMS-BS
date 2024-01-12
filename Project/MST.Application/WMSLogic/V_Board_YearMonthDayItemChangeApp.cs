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
    public class V_Board_YearMonthDayItemChangeApp
    {
        private IV_Board_YearMonthDayItemChangeRepository service = new V_Board_YearMonthDayItemChangeRepository();
        public IQueryable<V_Board_YearMonthDayItemChangeEntity> FindList(Expression<Func<V_Board_YearMonthDayItemChangeEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public V_Board_YearMonthDayItemChangeEntity FindEntity(Expression<Func<V_Board_YearMonthDayItemChangeEntity, bool>> predicate)
        {
            V_Board_YearMonthDayItemChangeEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<V_Board_YearMonthDayItemChangeEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<V_Board_YearMonthDayItemChangeEntity> FindList(string strSql, Pagination pagination)
        {
            return service.FindList(strSql, pagination).ToList();
        }        
    }
}



