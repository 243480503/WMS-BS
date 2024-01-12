/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 库存物料统计
    /// </summary>
    public class V_ConDetailGroupByItemReportModel
	{
        /// <summary>
        /// 联合主键
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemID { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string ItemUnitText { get; set; }
        /// <summary>
        /// 失效日期
        /// </summary>
        public string OverdueDate { get; set; }
        /// <summary>
        /// 统计数量
        /// </summary>
        public decimal? SumQty { get; set; }

        /// <summary>
        /// 质检状态(源自库存表)
        /// </summary>
        public string CheckState { get; set; }

        /// <summary>
        /// 质检状态名称(源自库存表)
        /// </summary>
        public string CheckStateName { get; set; }

        /// <summary>
        /// 库存记录条数，当前可代表容器数量
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// 容器大类
        /// </summary>
        public string ContainerKind { get; set; }
        /// <summary>
        /// 容器大类名称
        /// </summary>
        public string ContainerKindName { get; set; }
    }
}
