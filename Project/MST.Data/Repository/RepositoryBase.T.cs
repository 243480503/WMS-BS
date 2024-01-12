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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MST.Data
{
    /// <summary>
    /// 仓储实现
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class,new()
    {
        public MSTDbContext dbcontext = new MSTDbContext();
        public int Insert(TEntity entity)
        {
            dbcontext.Entry<TEntity>(entity).State = EntityState.Added;
            return dbcontext.SaveChanges();
        }

        public int Insert(List<TEntity> entitys)
        {
            foreach (var entity in entitys)
            {
                dbcontext.Entry<TEntity>(entity).State = EntityState.Added;
            }
            return dbcontext.SaveChanges();
        }
        public int Update(TEntity entity)
        {
            dbcontext.Set<TEntity>().Attach(entity);
            PropertyInfo[] props = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    if (prop.GetValue(entity, null).ToString() == "&nbsp;")
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = null;
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
            }
            return dbcontext.SaveChanges();
        }

        public int Delete(TEntity entity)
        {
            dbcontext.Set<TEntity>().Attach(entity);
            dbcontext.Entry<TEntity>(entity).State = EntityState.Deleted;
            return dbcontext.SaveChanges();
        }
        public int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var entitys = dbcontext.Set<TEntity>().Where(predicate).ToList();
            entitys.ForEach(m => dbcontext.Entry<TEntity>(m).State = EntityState.Deleted);
            return dbcontext.SaveChanges();
        }
        public TEntity FindEntity(object keyValue)
        {
            return dbcontext.Set<TEntity>().Find(keyValue);
        }
        public TEntity FindEntity(Expression<Func<TEntity, bool>> predicate)
        {
            return dbcontext.Set<TEntity>().FirstOrDefault(predicate);
        }

        public TEntity FindEntityAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        {
            return dbcontext.Set<TEntity>().AsNoTracking().FirstOrDefault(predicate);
        }



        public IQueryable<TEntity> IQueryable()
        {
            return dbcontext.Set<TEntity>();
        }
        public IQueryable<TEntity> IQueryable(Expression<Func<TEntity, bool>> predicate)
        {
            return dbcontext.Set<TEntity>().Where(predicate);
        }

        public IQueryable<TEntity> IQueryableAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        {
            return dbcontext.Set<TEntity>().AsNoTracking().Where(predicate);
        }

        public List<TEntity> FindList(string strSql)
        {
            return dbcontext.Database.SqlQuery<TEntity>(strSql).ToList<TEntity>();
        }
        public List<TEntity> FindList(string strSql, DbParameter[] dbParameter)
        {
            return dbcontext.Database.SqlQuery<TEntity>(strSql, dbParameter).ToList<TEntity>();
        }

        public List<TEntity> FindList(string strSql, Pagination pagination)
        {
            bool isAsc = pagination.sord.ToLower() == "asc" ? true : false;
            string[] _order = pagination.sidx.Split(',');
            MethodCallExpression resultExp = null;
            var tempData = dbcontext.Database.SqlQuery<TEntity>(strSql).AsQueryable();

            //使用SQLServer2012及以上版本的OFFSET-FETCH 语法，需要主键做排序
            IList<string> orderWithID = _order.ToList();
            orderWithID.Add("F_Id asc");
            _order = orderWithID.ToArray();

            foreach (string item in _order)
            {
                string _orderPart = item;
                _orderPart = Regex.Replace(_orderPart, @"\s+", " ");
                string[] _orderArray = _orderPart.Split(' ');
                string _orderField = _orderArray[0];
                bool sort = isAsc;
                if (_orderArray.Length == 2)
                {
                    isAsc = _orderArray[1].ToUpper() == "ASC" ? true : false;
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

        public List<TEntity> FindList(Pagination pagination)
        {
            bool isAsc = pagination.sord.ToLower() == "asc" ? true : false;
            string[] _order = pagination.sidx.Split(',');
            MethodCallExpression resultExp = null;
            var tempData = dbcontext.Set<TEntity>().AsQueryable();
            if (_order.Count() > 0)
            {
                //使用SQLServer2012及以上版本的OFFSET-FETCH 语法，需要主键做排序
                IList<string> orderWithID = _order.ToList();
                orderWithID.Add("F_Id asc");
                _order = orderWithID.ToArray();

                foreach (string item in _order)
                {
                    string _orderPart = item;
                    _orderPart = Regex.Replace(_orderPart, @"\s+", " ");
                    string[] _orderArray = _orderPart.Split(' ');
                    string _orderField = _orderArray[0];
                    bool sort = isAsc;
                    if (_orderArray.Length == 2)
                    {
                        isAsc = _orderArray[1].ToUpper() == "ASC" ? true : false;
                    }
                    var parameter = Expression.Parameter(typeof(TEntity), "t");
                    var property = typeof(TEntity).GetProperty(_orderField);
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    if (resultExp == null)
                    {
                        resultExp = Expression.Call(typeof(Queryable), isAsc ? "OrderBy" : "OrderByDescending", new Type[] { typeof(TEntity), property.PropertyType }, tempData.Expression, Expression.Quote(orderByExp));
                        tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
                    }
                    else
                    {
                        resultExp = Expression.Call(typeof(Queryable), isAsc ? "ThenBy" : "ThenByDescending", new Type[] { typeof(TEntity), property.PropertyType }, tempData.Expression, Expression.Quote(orderByExp));
                        tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
                    }
                }
            }
            else
            {
                tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
            }
            pagination.records = tempData.Count();
            tempData = tempData.Skip<TEntity>(pagination.rows * ((pagination.page < 1 ? 1 : pagination.page) - 1)).Take<TEntity>(pagination.rows).AsQueryable();
            return tempData.ToList();
        }
        public List<TEntity> FindList(Expression<Func<TEntity, bool>> predicate, Pagination pagination)
        {
            bool isAsc = pagination.sord.ToLower() == "asc" ? true : false;
            string[] _order = pagination.sidx.Split(',');
            MethodCallExpression resultExp = null;
            var tempData = dbcontext.Set<TEntity>().Where(predicate);
            if (_order.Count() > 0)
            {
                //使用SQLServer2012及以上版本的OFFSET-FETCH 语法，需要主键做排序
                IList<string> orderWithID = _order.ToList();
                orderWithID.Add("F_Id asc");
                _order = orderWithID.ToArray();

                foreach (string item in _order)
                {
                    string _orderPart = item;
                    _orderPart = Regex.Replace(_orderPart, @"\s+", " ");
                    string[] _orderArray = _orderPart.Split(' ');
                    string _orderField = _orderArray[0];
                    bool sort = isAsc;
                    if (_orderArray.Length == 2)
                    {
                        isAsc = _orderArray[1].ToUpper() == "ASC" ? true : false;
                    }
                    var parameter = Expression.Parameter(typeof(TEntity), "t");
                    var property = typeof(TEntity).GetProperty(_orderField);
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    if (resultExp == null)
                    {
                        resultExp = Expression.Call(typeof(Queryable), isAsc ? "OrderBy" : "OrderByDescending", new Type[] { typeof(TEntity), property.PropertyType }, tempData.Expression, Expression.Quote(orderByExp));
                        tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
                    }
                    else
                    {
                        resultExp = Expression.Call(typeof(Queryable), isAsc ? "ThenBy" : "ThenByDescending", new Type[] { typeof(TEntity), property.PropertyType }, tempData.Expression, Expression.Quote(orderByExp));
                        tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
                    }
                }
            }
            else
            {
                tempData = tempData.Provider.CreateQuery<TEntity>(resultExp);
            }
            pagination.records = tempData.Count();
            tempData = tempData.Skip<TEntity>(pagination.rows * ((pagination.page < 1 ? 1 : pagination.page) - 1)).Take<TEntity>(pagination.rows).AsQueryable();
            return tempData.ToList();
        }
    }
}
