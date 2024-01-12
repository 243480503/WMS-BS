/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using System;

namespace MST.Domain.Entity.WMSLogic
{
    public class V_Board_YearMonthDayItemChangeEntity : IEntity<V_Board_YearMonthDayItemChangeEntity>
    {
        /// <summary>
        /// 联合主键
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 库存快照单时间
        /// </summary>
        public DateTime? PhoTime { get; set; }
        /// <summary>
        /// 库存快照编码
        /// </summary>
        public string PhoCode { get; set; }
        /// <summary>
        /// 库存快照ID
        /// </summary>
        public string PhoID { get; set; }
        /// <summary>
        /// 年
        /// </summary>
        public int? YearNum { get; set; }
        /// <summary>
        /// 月
        /// </summary>
        public int? MonthNum { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>

        public string ItemName { get; set; }
        /// <summary>
        /// 统计数量
        /// </summary>
        public decimal? SumQty { get; set; }
    }
}
