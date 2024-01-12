/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.Entity.WMSLogic
{
    /// <summary>
    /// 质检记录表
    /// </summary>
    public class T_QARecordEntity : IEntity<T_QARecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 质检记录ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 质检单ID
        /// </summary>
        public string QAID { get; set; }
        /// <summary>
        /// 项次
        /// </summary>
        public int? SEQ { get; set; }
        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskID { get; set; }
        /// <summary>
        /// 任务编码
        /// </summary>
        public string TaskNo { get; set; }
        /// <summary>
        /// 容器类型
        /// </summary>
        public string ContainerType { get; set; }
        public string ContainerTypeName { get; set; }
        /// <summary>
        /// 容器大类
        /// </summary>
        public string ContainerKind { get; set; }
        public string ContainerKindName { get; set; }
        /// <summary>
        /// 起始地址ID
        /// </summary>
        public string SrcLocationID { get; set; }
        /// <summary>
        /// 起始地址编码
        /// </summary>
        public string SrcLocationCode { get; set; }
        /// <summary>
        /// 是否物料贴标
        /// </summary>
        public string IsItemMark { get; set; }
        /// <summary>
        /// 厂商
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }
        /// <summary>
        /// 过期日期
        /// </summary>
        public DateTime? OverdueDate { get; set; }
        /// <summary>
        /// 有效期天数
        /// </summary>
        public int? ValidityDayNum { get; set; }
        /// <summary>
        /// T_Station的ID
        /// </summary>
        public string TagLocationID { get; set; }
        /// <summary>
        /// 目标站台编码
        /// </summary>
        public string TagLocationCode { get; set; }
        public string TagLocationName { get; set; }
        /// <summary>
        /// 质检单据类型（BackSample质检还样，GetSample质检取样）
        /// </summary>
        public string QAOrderType { get; set; }
        /// <summary>
        /// 拣选人ID
        /// </summary>
        public string PickUserID { get; set; }
        /// <summary>
        /// 标签条码
        /// </summary>
        public string ItemBarCode { get; set; }
        /// <summary>
        /// 原数量
        /// </summary>
        public decimal? OldQty { get; set; }
        /// <summary>
        /// 波次单ID
        /// </summary>
        public string WaveID { get; set; }
        /// <summary>
        /// 波次单编码
        /// </summary>
        public string WaveCode { get; set; }
        /// <summary>
        /// 波次明细
        /// </summary>
        public string WaveDetailID { get; set; }
        /// <summary>
        /// 已取样数量
        /// </summary>
        public decimal? PickedQty { get; set; }
        /// <summary>
        /// 操作后数量
        /// </summary>
        public decimal? AfterQty { get; set; }
        /// <summary>
        /// 物料单位
        /// </summary>
        public string ItemUnitText { get; set; }
        /// <summary>
        /// 质检单明细ID
        /// </summary>
        public string QADetailID { get; set; }
        /// <summary>
        /// 库存明细ID
        /// </summary>
        public string ContainerDetailID { get; set; }
        /// <summary>
        /// 容器ID
        /// </summary>
        public string ContainerID { get; set; }
        /// <summary>
        /// 过账状态
		/// New		新建
		/// UnNeedTrans	免过账
		/// WaittingTrans	待过账
		/// Traning	过账中
		/// OverTrans	已过账
		/// FailTrans	过账失败
        /// </summary>
        public string TransState { get; set; }
        /// <summary>
        /// 容器条码
        /// </summary>
        public string BarCode { get; set; }
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
        /// 供应商ID
        /// </summary>
        public string SupplierUserID { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierUserCode { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierUserName { get; set; }
        /// <summary>
        /// 收货记录ID
        /// </summary>
        public string ReceiveRecordID { get; set; }
        /// <summary>
        /// ERP仓库编码
        /// </summary>
        public string ERPHouseCode { get; set; }
        /// <summary>
        /// 是否关键物料
        /// </summary>
        public string IsSpecial { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 应取样数量
        /// </summary>
        public decimal? Qty { get; set; }
        /// <summary>
        /// 已还数量
        /// </summary>
        public decimal? ReturnQty { get; set; }
        /// <summary>
        /// 是否需还样
        /// </summary>
        public string IsNeedBack { get; set; }
        /// <summary>
        /// 是否还样完毕(RF提交还样时为true)
        /// </summary>
        public string IsReturnOver { get; set; }
        /// <summary>
        /// 盘点状态
		/// New			新建
		/// WavedPart	部分波次
		/// Waved		波次完成
		/// Outing		出库中
		/// Picking		取样中
		/// Picked		已取样
		///	WaitReturn	待还样
		///	Returning	还样中
		/// WaitResult	待录入结果
		/// Over		结束
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 质检单编码
        /// </summary>
        public string QACode { get; set; }
        /// <summary>
        /// 取样人
        /// </summary>
        public string PickUserName { get; set; }
        /// <summary>
        /// 取样时间
        /// </summary>
        public DateTime? PickDate { get; set; }
        /// <summary>
        /// 是否通过波次指定
        /// </summary>
        public string IsAuto { get; set; }
        /// <summary>
        /// 是否外观质检
        /// </summary>
        public string IsAppearQA { get; set; }
        /// <summary>
        /// 是否已到达站台位（取样时）
        /// </summary>
        public string IsArrive_Get { get; set; }
        /// <summary>
        /// 容器（包含空容器）是否需要回库（取样时）
        /// </summary>
        public string IsNeedBackWare_Get { get; set; }
        /// <summary>
        /// 取样回库是否已扫码（取样时）
        /// </summary>
        public string IsScanBack_Get { get; set; }
        /// <summary>
        /// 是否已到达站台位（还样时）
        /// </summary>
        public string IsArrive_Back { get; set; }
        /// <summary>
        /// 是否需要回库（还样时）
        /// </summary>
        public string IsNeedBackWare_Back { get; set; }
        /// <summary>
        /// 还样回库是否流道已申请扫码（还样时）
        /// </summary>
        public string IsScanBack_Back { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool? F_DeleteMark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? F_CreatorTime { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public string F_CreatorUserId { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreatorUserName { get; set; }
        /// <summary>
        /// 删除操作人
        /// </summary>
        public string F_DeleteUserId { get; set; }
        /// <summary>
        /// 删除操作时间
        /// </summary>
        public DateTime? F_DeleteTime { get; set; }
        /// <summary>
        /// 删除操作人名称
        /// </summary>
        public string DeleteUserName { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? F_LastModifyTime { get; set; }
        /// <summary>
        /// 修改人ID
        /// </summary>
        public string F_LastModifyUserId { get; set; }
        /// <summary>
        /// 修改人名称
        /// </summary>
        public string ModifyUserName { get; set; }
    }
}
