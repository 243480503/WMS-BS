/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using System;

namespace MST.Domain.Entity.WMSLogic
{
    public class V_InBoundOrderReportEntity : IEntity<V_InBoundOrderReportEntity>
	{
        /// <summary>
        /// 联合主键(T_InBound.F_Id + T_InBoundDetail.F_Id)
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 来源单据
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
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
        /// 应收数量
        /// </summary>
        public decimal? Qty { get; set; }
        /// <summary>
        /// ERP仓库
        /// </summary>
        public string ERPHouseName { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string SupplierUserName { get; set; }
        /// <summary>
        /// 已收数量
        /// </summary>
        public decimal? ReceivedQty { get; set; }
        /// <summary>
        /// 已入库数量
        /// </summary>
        public decimal? OverQty { get; set; }
        /// <summary>
        /// 入库日期
        /// </summary>
        public string F_CreatorTime { get; set; }
    }
}
