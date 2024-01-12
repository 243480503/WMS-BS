/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

namespace MST.Code
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// 每页行数
        /// </summary>
        public int rows { get; set; }

        private int _page;
        /// <summary>
        /// 当前页
        /// </summary>
        public int page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
            }
        }
        /// <summary>
        /// 排序列
        /// </summary>
        public string sidx { get; set; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public string sord { get; set; }

        private int _records;
        /// <summary>
        /// 总记录数
        /// </summary>
        public int records
        {
            get
            {

                return _records;
            }
            set
            {
                _records = value;

                if (_page > this.total)
                {
                    _page = this.total;
                }
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        public int total
        {
            get
            {
                if (records > 0)
                {

                    return records % this.rows == 0 ? records / this.rows : records / this.rows + 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
