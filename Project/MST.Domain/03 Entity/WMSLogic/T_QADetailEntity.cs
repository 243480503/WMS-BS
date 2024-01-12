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
	/// 质检单明细表
	/// </summary>
	public class T_QADetailEntity : IEntity<T_QADetailEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
	{
		/// <summary>
		/// 质检单明细ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 项次
		/// </summary>
		public int? SEQ { get; set; }
		/// <summary>
		/// 质检单ID
		/// </summary>
		public string QAID { get; set; }
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
        /// 供应商ID
        /// </summary>
        public string SupplierUserID { get; set; }
		/// <summary>
		/// 供应商编码
		/// </summary>
		public string SupplierCode { get; set; }
		/// <summary>
		/// 供应商名称
		/// </summary>
		public string SupplierUserName { get; set; }
		/// <summary>
		/// 质检结果状态
		/// New 新建
		/// ERP 接口录入
		/// MAN 人工录入
		/// </summary>
		public string ResultState { get; set; }

		/// <summary>
		/// 执行方式（手动、设备）
		/// </summary>
		public string ActionType { get; set; }

		/// <summary>
		/// 质检站台ID
		/// </summary>
		public string StationID { get; set; }
		/// <summary>
		/// 批号
		/// </summary>
		public string Lot { get; set; }
        public string Spec { get; set; }
        public string ItemUnitText { get; set; }
        public DateTime? OverdueDate { get; set; }
        /// <summary>
        /// 质检结果
        /// Qua 合格
        /// UnQua 不合格
        /// UnNeed 免检
        /// WaitCheck	待检
        /// </summary>
        public string QAResult { get; set; }
		/// <summary>
		/// 是否破坏性质检
		/// </summary>
		public string IsBroken { get; set; }
		/// <summary>
		/// 是否外观质检
		/// </summary>
		public string IsAppearQA { get; set; }
		/// <summary>
		/// 波次数量
		/// </summary>
		public decimal? WaveQty { get; set; }
		/// <summary>
		/// 已出数量
		/// </summary>
		public decimal? OutQty { get; set; }
		/// <summary>
		/// 取样总数
		/// </summary>
		public decimal? SampleSumNum { get; set; }
		/// <summary>
		/// 取样标签个数
		/// </summary>
		public decimal? SampleSumCnt { get; set; }
		/// <summary>
		/// 结果推送时间
		/// </summary>
		public DateTime? ResultSendTime { get; set; }
		/// <summary>
		/// 取样方式
		/// Hand	手动
		/// Auto	自动
		/// </summary>
		public string SampleType { get; set; }
		/// <summary>
		/// 明细状态
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
