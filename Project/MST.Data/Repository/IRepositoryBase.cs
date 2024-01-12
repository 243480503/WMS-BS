/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Data
{
    public interface IRepositoryBase : IDisposable
    {
        IRepositoryBase BeginTrans();
        int Commit();

        int CommitWithOutRollBack();
        void RollBack();
        int SaveChanges();
        int Insert<TEntity>(TEntity entity) where TEntity : class;

        int Insert<TEntity>(TEntity entity, OperatorModel user) where TEntity : class;
        int Insert<TEntity>(List<TEntity> entitys) where TEntity : class;
        int Update<TEntity>(TEntity entity) where TEntity : class;
        int Update<TEntity>(TEntity entity, OperatorModel user) where TEntity : class;
        int Update<TEntity>(List<TEntity> entitys) where TEntity : class;
        int Delete<TEntity>(TEntity entity) where TEntity : class;

        int Delete<TEntity>(List<TEntity> entity) where TEntity : class;
        int Delete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        TEntity FindEntity<TEntity>(object keyValue) where TEntity : class;
        TEntity FindEntity<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        TEntity FindEntityAsNoTracking<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        IQueryable<TEntity> IQueryable<TEntity>() where TEntity : class;
        IQueryable<TEntity> IQueryable<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        List<TEntity> FindList<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        List<TEntity> FindListAsNoTracking<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;

        List<TEntity> FindList<TEntity>(string strSql) where TEntity : class;
        List<TEntity> FindList<TEntity>(string strSql, DbParameter[] dbParameter) where TEntity : class;
        List<TEntity> FindList<TEntity>(Pagination pagination) where TEntity : class, new();
        List<TEntity> FindList<TEntity>(Expression<Func<TEntity, bool>> predicate, Pagination pagination) where TEntity : class, new();

        bool BulkInsert<TEntity>(IList<TEntity> list) where TEntity : class;

        bool BulkDelete<TEntity>(IList<TEntity> list) where TEntity : class;

        bool BulkUpdate<TEntity>(IList<TEntity> list) where TEntity : class;
        bool BulkSaveChanges();

        DataTable GetDataTable(string sql, params SqlParameter[] parameters);
    }
}
