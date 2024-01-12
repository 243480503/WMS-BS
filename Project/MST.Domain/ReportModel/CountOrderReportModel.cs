/*******************************************************************************
 * Copyright ? 2021 迈思特版权所有
 * Author: MST_WMS
 * Description: WMS平台
 * Website：www.maisite.com
*********************************************************************************/
using System;

namespace MST.Domain.ReportModel
{
    /// <summary>
    /// 出库单据报表
    /// </summary>
    public class CountOrderReportModel
    {
        /// <summary>
        /// 联合主键(T_Count.F_Id + T_CountDetail.F_Id)
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
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }

        public string Factory { get; set; }
        /// <summary>
        /// 应盘
        /// </summary>
        public decimal? Qty { get; set; }
        public string ERPHouseCode { get; set; }
        public string SupplierName { get; set; }
        /// <summary>
        /// 已盘
        /// </summary>
        public decimal? CountQty { get; set; }
    }
}
