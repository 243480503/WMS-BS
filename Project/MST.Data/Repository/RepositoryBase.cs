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
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MST.Data
{
    /// <summary>
    /// 仓储实现
    /// </summary>
    public class RepositoryBase : IRepositoryBase, IDisposable
    {
        private MSTDbContext dbcontext = new MSTDbContext();
        private DbTransaction dbTransaction { get; set; }
        //public IRepositoryBase BeginTrans()
        //{
        //    DbConnection dbConnection = ((IObjectContextAdapter)dbcontext).ObjectContext.Connection;
        //    if (dbConnection.State == ConnectionState.Closed)
        //    {
        //        dbConnection.Open();
        //    }
        //    dbTransaction = dbConnection.BeginTransaction();
        //    return this;
        //}

        /// <summary>
        /// 开启事务，带超时时间(秒)
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public IRepositoryBase BeginTrans(int timeOut)
        {
            IObjectContextAdapter adpter = ((IObjectContextAdapter)dbcontext);
            adpter.ObjectContext.CommandTimeout = timeOut;
            DbConnection dbConnection = adpter.ObjectContext.Connection;
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            dbTransaction = dbConnection.BeginTransaction();
            return this;
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IRepositoryBase BeginTrans()
        {
            int timeOut = Convert.ToInt32(Configs.GetValue("CommandTimeout"));
            IObjectContextAdapter adpter = ((IObjectContextAdapter)dbcontext);
            adpter.ObjectContext.CommandTimeout = timeOut;
            DbConnection dbConnection = adpter.ObjectContext.Connection;
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            dbTransaction = dbConnection.BeginTransaction();
            return this;
        }

        public int Commit()
        {
            try
            {
                var returnValue = dbcontext.SaveChanges();
                if (dbTransaction != null)
                {
                    dbTransaction.Commit();
                }
                return returnValue;
            }
            catch (Exception)
            {
                if (dbTransaction != null)
                {
                    this.dbTransaction.Rollback();
                }
                throw;
            }
            finally
            {
                this.Dispose();
            }
        }

        public int CommitWithOutRollBack()
        {
            try
            {
                var returnValue = dbcontext.SaveChanges();
                if (dbTransaction != null)
                {
                    dbTransaction.Commit();
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int SaveChanges()
        {
            try
            {
                int result = dbcontext.SaveChanges();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RollBack()
        {
            if (dbTransaction != null)
            {
                this.dbTransaction.Rollback();
            }
            this.Dispose();
        }

        public void Dispose()
        {
            if (dbTransaction != null)
            {
                this.dbTransaction.Dispose();
            }
            this.dbcontext.Dispose();
        }
        public int Insert<TEntity>(TEntity entity) where TEntity : class
        {
            dbcontext.Entry<TEntity>(entity).State = EntityState.Added;
            PropertyInfo[] props = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name == "CreatorUserName")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserName;
                }
                if (prop.Name == "F_CreatorUserId")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserId;
                }
                if (prop.Name == "F_CreatorTime")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = DateTime.Now;
                }
            }
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int Insert<TEntity>(TEntity entity, OperatorModel user) where TEntity : class
        {
            dbcontext.Entry<TEntity>(entity).State = EntityState.Added;
            PropertyInfo[] props = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name == "CreatorUserName")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = user.UserName;
                }
                if (prop.Name == "F_CreatorUserId")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = user.UserId;
                }
                if (prop.Name == "F_CreatorTime")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = DateTime.Now;
                }
            }
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int Insert<TEntity>(List<TEntity> entitys) where TEntity : class
        {
            foreach (var entity in entitys)
            {
                dbcontext.Entry<TEntity>(entity).State = EntityState.Added;
            }
            return dbTransaction == null ? this.Commit() : 0;
        }
        public bool BulkInsert<TEntity>(IList<TEntity> list) where TEntity : class
        {
            dbcontext.BulkInsert(list);
            return true;
        }

        public bool BulkDelete<TEntity>(IList<TEntity> list) where TEntity : class
        {
            dbcontext.BulkDelete(list);
            return true;
        }

        public bool BulkUpdate<TEntity>(IList<TEntity> list) where TEntity : class
        {
            dbcontext.BulkUpdate(list);
            return true;
        }

        public bool BulkSaveChanges()
        {
            dbcontext.BulkSaveChanges();
            return true;
        }

        public int Update<TEntity>(List<TEntity> entitys) where TEntity : class
        {
            foreach (var entity in entitys)
            {
                dbcontext.Set<TEntity>().Attach(entity);
                PropertyInfo[] props = entity.GetType().GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    if (prop.GetValue(entity, null) != null)
                    {
                        if (prop.GetValue(entity, null).ToString() == "&nbsp;")
                        {
                            dbcontext.Entry(entity).Property(prop.Name).CurrentValue = null;
                        }
                        dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                    }

                    if (prop.Name == "ModifyUserName")
                    {
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserName;
                        dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                    }
                    if (prop.Name == "F_LastModifyUserId")
                    {
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserId;
                        dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                    }
                    if (prop.Name == "F_LastModifyTime")
                    {
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = DateTime.Now;
                        dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                    }
                }
            }
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int Update<TEntity>(TEntity entity) where TEntity : class
        {
            dbcontext.Set<TEntity>().Attach(entity);
            PropertyInfo[] props = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    if (prop.GetValue(entity, null).ToString() == "&nbsp;")
                    {
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = null;
                    }
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "ModifyUserName")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserName;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "F_LastModifyUserId")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserId;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "F_LastModifyTime")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = DateTime.Now;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
            }
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int UpdateDate<TEntity>(TEntity entity) where TEntity : class
        {
            dbcontext.Set<TEntity>().Attach(entity);
            PropertyInfo[] props = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    if (prop.GetValue(entity, null).ToString() == "&nbsp;")
                    {
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = null;
                    }
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "ProductDate" && prop.GetValue(entity, null) == null)
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = null;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "ModifyUserName")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserName;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "F_LastModifyUserId")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = OperatorProvider.Provider.GetCurrent().UserId;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "F_LastModifyTime")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = DateTime.Now;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
            }
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int Update<TEntity>(TEntity entity, OperatorModel user) where TEntity : class
        {
            dbcontext.Set<TEntity>().Attach(entity);
            PropertyInfo[] props = entity.GetType().GetProperties();
            if (user == null)
            {
                user = OperatorProvider.Provider.GetCurrent();
            }
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    if (prop.GetValue(entity, null).ToString() == "&nbsp;")
                    {
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = null;
                    }
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }

                if (prop.Name == "ModifyUserName")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = user.UserName;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "F_LastModifyUserId")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = user.UserId;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
                if (prop.Name == "F_LastModifyTime")
                {
                    dbcontext.Entry(entity).Property(prop.Name).CurrentValue = DateTime.Now;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
            }
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int Delete<TEntity>(TEntity entity) where TEntity : class
        {
            dbcontext.Set<TEntity>().Attach(entity);
            dbcontext.Entry<TEntity>(entity).State = EntityState.Deleted;
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int Delete<TEntity>(List<TEntity> entitys) where TEntity : class
        {
            foreach (TEntity entity in entitys)
            {
                dbcontext.Set<TEntity>().Attach(entity);
                dbcontext.Entry<TEntity>(entity).State = EntityState.Deleted;
            }
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var entitys = dbcontext.Set<TEntity>().Where(predicate).ToList();
            entitys.ForEach(m => dbcontext.Entry<TEntity>(m).State = EntityState.Deleted);
            return dbTransaction == null ? this.Commit() : 0;
        }

        public int DeleteBulk<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var entitys = dbcontext.Set<TEntity>().Where(predicate).ToList();
            entitys.ForEach(m => dbcontext.Entry<TEntity>(m).State = EntityState.Deleted);
            return dbTransaction == null ? this.Commit() : 0;
        }

        public TEntity FindEntity<TEntity>(object keyValue) where TEntity : class
        {
            return dbcontext.Set<TEntity>().Find(keyValue);
        }
        public TEntity FindEntity<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return dbcontext.Set<TEntity>().FirstOrDefault(predicate);
        }

        public TEntity FindEntityAsNoTracking<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return dbcontext.Set<TEntity>().AsNoTracking<TEntity>().FirstOrDefault(predicate);
        }
        public IQueryable<TEntity> IQueryable<TEntity>() where TEntity : class
        {
            return dbcontext.Set<TEntity>();
        }
        public IQueryable<TEntity> IQueryable<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return dbcontext.Set<TEntity>().Where(predicate);
        }

        public List<TEntity> FindList<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return dbcontext.Set<TEntity>().Where(predicate).ToList();
        }

        public List<TEntity> FindListAsNoTracking<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return dbcontext.Set<TEntity>().AsNoTracking<TEntity>().Where(predicate).ToList();
        }

        public List<TEntity> FindList<TEntity>(string strSql) where TEntity : class
        {
            return dbcontext.Database.SqlQuery<TEntity>(strSql).ToList<TEntity>();
        }

        public DataTable GetDataTable(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, dbcontext.Database.Connection.ConnectionString))
                {
                    DataTable dt = new DataTable();
                    if (parameters != null)
                    {
                        adapter.SelectCommand.Parameters.AddRange(parameters);
                    }
                    adapter.Fill(dt);
                    this.Dispose();
                    return dt;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<TEntity> FindList<TEntity>(string strSql, DbParameter[] dbParameter) where TEntity : class
        {
            return dbcontext.Database.SqlQuery<TEntity>(strSql, dbParameter).ToList<TEntity>();
        }
        public List<TEntity> FindList<TEntity>(Pagination pagination) where TEntity : class, new()
        {
            bool isAsc = pagination.sord.ToLower() == "asc" ? true : false;
            string[] _order = pagination.sidx.Split(',');
            MethodCallExpression resultExp = null;
            var tempData = dbcontext.Set<TEntity>().AsQueryable();
            foreach (string item in _order)
            {
                string _orderPart = item;
                _orderPart = Regex.Replace(_orderPart, @"\s+", " ");
                string[] _orderArry = _orderPart.Split(' ');
                string _orderField = _orderArry[0];
                bool sort = isAsc;
                if (_orderArry.Length == 2)
                {
                    isAsc = _orderArry[1].ToUpper() == "ASC" ? true : false;
                }
                var parameter = Expression.Parameter(typeof(TEntity), "t");
                var property = typeof(TEntity).GetProperty(_orderField);
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                resultExp = Expression.Call(typeof(Queryable), isAsc ? "OrderBy" : "OrderByDescending", new Type[] { typeof(TEntity), property.PropertyType }, tempData.Expression, Expression.Quote(orderByExp));
            }
            tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
            pagination.records = tempData.Count();
            tempData = tempData.Skip<TEntity>(pagination.rows * ((pagination.page < 1 ? 1 : pagination.page) - 1)).Take<TEntity>(pagination.rows).AsQueryable();
            return tempData.ToList();
        }
        public List<TEntity> FindList<TEntity>(Expression<Func<TEntity, bool>> predicate, Pagination pagination) where TEntity : class, new()
        {
            bool isAsc = pagination.sord.ToLower() == "asc" ? true : false;
            string[] _order = pagination.sidx.Split(',');
            MethodCallExpression resultExp = null;
            var tempData = dbcontext.Set<TEntity>().Where(predicate);
            foreach (string item in _order)
            {
                string _orderPart = item;
                _orderPart = Regex.Replace(_orderPart, @"\s+", " ");
                string[] _orderArry = _orderPart.Split(' ');
                string _orderField = _orderArry[0];
                bool sort = isAsc;
                if (_orderArry.Length == 2)
                {
                    isAsc = _orderArry[1].ToUpper() == "ASC" ? true : false;
                }
                var parameter = Expression.Parameter(typeof(TEntity), "t");
                var property = typeof(TEntity).GetProperty(_orderField);
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                resultExp = Expression.Call(typeof(Queryable), isAsc ? "OrderBy" : "OrderByDescending", new Type[] { typeof(TEntity), property.PropertyType }, tempData.Expression, Expression.Quote(orderByExp));
            }
            tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
            pagination.records = tempData.Count();
            tempData = tempData.Skip<TEntity>(pagination.rows * ((pagination.page < 1 ? 1 : pagination.page) - 1)).Take<TEntity>(pagination.rows).AsQueryable();
            return tempData.ToList();
        }
    }
}
