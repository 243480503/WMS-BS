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
    /// 出库单据报表
    /// </summary>
    public class OutBoundOrderReportModel
    {
        /// <summary>
        /// 联合主键(T_OutBound.F_Id + T_OutBoundDetail.F_Id)
        /// </summary>
        public string F_Id { get; set; }
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

        public string Factory { get; set; }
        /// <summary>
        /// 应出
        /// </summary>
        public decimal? Qty { get; set; }
        public string ERPHouseCode { get; set; }
        public string SupplierName { get; set; }
        /// <summary>
        /// 锁定数量
        /// </summary>
        public decimal? WaveQty { get; set; }
        /// <summary>
        /// 已出库
        /// </summary>
        public decimal? OverQty { get; set; }
    }
}
