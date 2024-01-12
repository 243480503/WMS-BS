/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Data
{
    /// <summary>
    /// 仓储接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public interface IRepositoryBase<TEntity> where TEntity : class, new()
    {
        int Insert(TEntity entity);
        int Insert(List<TEntity> entitys);
        int Update(TEntity entity);
        int Delete(TEntity entity);
        int Delete(Expression<Func<TEntity, bool>> predicate);
        TEntity FindEntity(object keyValue);
        TEntity FindEntity(Expression<Func<TEntity, bool>> predicate);

        TEntity FindEntityAsNoTracking(Expression<Func<TEntity, bool>> predicate);

        IQueryable<TEntity> IQueryable();
        IQueryable<TEntity> IQueryable(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> IQueryableAsNoTracking(Expression<Func<TEntity, bool>> predicate);
        List<TEntity> FindList(string strSql);
        List<TEntity> FindList(string strSql, Pagination pagination);
        List<TEntity> FindList(string strSql, DbParameter[] dbParameter);
        List<TEntity> FindList(Pagination pagination);
        List<TEntity> FindList(Expression<Func<TEntity, bool>> predicate, Pagination pagination);

    }
}
