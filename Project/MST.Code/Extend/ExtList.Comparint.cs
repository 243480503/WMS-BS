/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System.Collections.Generic;
using System.Linq;

namespace MST.Code
{
    public class ExtList<T> : IEqualityComparer<T> where T : class, new()
    {
         private string[] comparintFiledName = { };

        public ExtList() { }
        public ExtList(params string[] comparintFiledName)
        {
            this.comparintFiledName = comparintFiledName;
        }
        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            if (x == null && y == null)
            {
                return false;
            }
            if (comparintFiledName.Length == 0)
            {
                return x.Equals(y);
            }
            bool result = true;
            var typeX = x.GetType();//获取类型
            var typeY = y.GetType();
            foreach (var filedName in comparintFiledName)
            {
                var xPropertyInfo = (from p in typeX.GetProperties() where p.Name.Equals(filedName) select p).FirstOrDefault();
                var yPropertyInfo = (from p in typeY.GetProperties() where p.Name.Equals(filedName) select p).FirstOrDefault();

                result = result
                    && xPropertyInfo != null && yPropertyInfo != null
                    && xPropertyInfo.GetValue(x, null).ToString().Equals(yPropertyInfo.GetValue(y, null));
            }
            return result;
        }
        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}
