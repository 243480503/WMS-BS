/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using System;

namespace MST.Domain.Entity.WMSLogic
{
    public class V_QAOrderReportEntity : IEntity<V_QAOrderReportEntity>
	{
        /// <summary>
        /// 联合主键(T_QA.F_Id + T_QADetail.F_Id)
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
        /// 是否破坏性质检
        /// </summary>
        public string IsBroken { get; set; }
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
        /// 质检日期
        /// </summary>
        public string PickDate { get; set; }
        /// <summary>
        /// 质检结果
        /// </summary>
        public string QAResultName { get; set; }
        /// <summary>
        /// 厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 是否外观质检
        /// </summary>
        public string IsAppearQA { get; set; }
        /// <summary>
        /// 应质检数量
        /// </summary>
        public decimal? Qty { get; set; }
        /// <summary>
        /// ERP仓库编码
        /// </summary>
        public string ERPHouseName { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// 已取样
        /// </summary>
        public decimal? PickedQty { get; set; }
        /// <summary>
        /// 已还样
        /// </summary>
        public decimal? ReturnQty { get; set; }
        /// <summary>
        /// 是否需要回库
        /// </summary>
        public string IsNeedBack { get; set; }
    }
}
