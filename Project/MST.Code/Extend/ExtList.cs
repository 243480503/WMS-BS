/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MST.Code.Extend
{
    public static class ExtList
    {
        /// <summary>
        /// 获取表里某页的数据
        /// </summary>
        /// <param name="data">表数据</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="allPage">返回总页数</param>
        /// <returns>返回当页表数据</returns>
        public static IList<T> GetPage<T>(this IList<T> data, int pageIndex, int pageSize, out int allPage)
        {
            allPage = 1;
            return null;
        }

        /// <summary>
        /// 针对聚合后的List做分页与排序，该List只能是全部数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="pagination"></param>
        /// <param name="filter">可为空</param>
        /// <returns></returns>
        public static IList<T> GetPage<T>(this IList<T> data, Pagination pagination, Func<T, bool> filter)
        {
            IEnumerable<T> temp = null;
            if (filter == null)
            {
                temp = data.Where(o => true);
            }
            else
            {
                temp = data.Where(filter);
            }
            bool isAsc = pagination.sord.ToLower() == "asc" ? true : false;
            string[] _order = pagination.sidx.Split(',');
            if (_order.Count() > 0)
            {
                IOrderedEnumerable<T> orderEnumerable = temp.OrderBy(o => 0);
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

                    ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
                    var property = Expression.Property(parameter, _orderField);
                    var lambdaExp = Expression.Lambda<Func<T, object>>(Expression.Convert(property, typeof(object)), parameter);
                    var lambda = lambdaExp.Compile();
                    orderEnumerable = isAsc ? orderEnumerable.ThenBy(lambda) : orderEnumerable.ThenByDescending(lambda);
                }
                data = orderEnumerable.ToList();
            }
            else
            {
                data = temp.ToList();
            }

            pagination.records = data.Count();

            data = data.Skip<T>(pagination.rows * ((pagination.page < 1 ? 1 : pagination.page) - 1)).Take<T>(pagination.rows).ToList();

            return data;
        }

        /// <summary>
        /// IList转成List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> IListToList<T>(IList list)
        {
            T[] array = new T[list.Count];
            list.CopyTo(array, 0);
            return new List<T>(array);
        }



        /// <summary>
        /// List转DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {

            DataTable dtReturn = new DataTable();
            PropertyInfo[] oProps = null;
            foreach (T rec in list)
            {
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        // 当字段类型是Nullable<>时
                        Type colType = pi.PropertyType; if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }
                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }
                DataRow dr = dtReturn.NewRow(); foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue(rec, null);
                }
                dtReturn.Rows.Add(dr);
            }
            return (dtReturn);
        }
    }
}
