/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace MST.Code
{
    public static class ExtTable
    {

        /// <summary>
        /// 获取表里某页的数据
        /// </summary>
        /// <param name="data">表数据</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="allPage">返回总页数</param>
        /// <returns>返回当页表数据</returns>
        public static DataTable GetPage(this DataTable data, int pageIndex, int pageSize, out int allPage)
        {
            allPage = data.Rows.Count / pageSize;
            allPage += data.Rows.Count % pageSize == 0 ? 0 : 1;
            DataTable Ntable = data.Clone();
            int startIndex = pageIndex * pageSize;
            int endIndex = startIndex + pageSize > data.Rows.Count ? data.Rows.Count : startIndex + pageSize;
            if (startIndex < endIndex)
                for (int i = startIndex; i < endIndex; i++)
                {
                    Ntable.ImportRow(data.Rows[i]);
                }
            return Ntable;
        }
        /// <summary>
        /// 根据字段过滤表的内容
        /// </summary>
        /// <param name="data">表数据</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        /// 
        public static DataTable GetDataFilter(DataTable data, string condition)
        {
            if (data != null && data.Rows.Count > 0)
            {
                if (condition.Trim() == "")
                {
                    return data;
                }
                else
                {
                    DataTable newdt = new DataTable();
                    newdt = data.Clone();
                    DataRow[] dr = data.Select(condition);
                    for (int i = 0; i < dr.Length; i++)
                    {
                        newdt.ImportRow((DataRow)dr[i]);
                    }
                    return newdt;
                }
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// DataTable转List
        /// </summary>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable dt) where T:new()
        {
            List<T> ts = new List<T>();// 定义集合
            string tempName = "";
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                PropertyInfo[] propertys = t.GetType().GetProperties();// 获得此模型的公共属性
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;
                    if (dt.Columns.Contains(tempName))
                    {
                        if (!pi.CanWrite) continue;
                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }
    }
}
