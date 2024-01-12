/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.ReportModel
{
    /// <summary>
    /// 入库单据报表
    /// </summary>
    public class InBoundOrderReportModel
    {
        /// <summary>
        /// 来源单据
        /// </summary>
        public string RefOrderCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Lot { get; set; }
        /// <summary>
        /// 生成日期
        /// </summary>
        public DateTime? ProductDate { get; set; }
        /// <summary>
        /// 应收
        /// </summary>
        public decimal? Qty { get; set; }
        public string ERPWarehouseCode { get; set; }
        public string CreateCompany { get; set; }
        public string SupplierUserName { get; set; }
        /// <summary>
        /// 已收
        /// </summary>
        public decimal? ReceivedQty { get; set; }
        /// <summary>
        /// 已入库
        /// </summary>
        public decimal? OverQty { get; set; }
    }
}
