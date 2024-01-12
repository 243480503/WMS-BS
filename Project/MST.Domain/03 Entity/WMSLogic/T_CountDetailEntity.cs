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
  /// 盘点单明细表
  /// </summary>
  public class T_CountDetailEntity : IEntity<T_CountDetailEntity>, ICreationAudited, IModificationAudited, IDeleteAudited , IWMSEntity
  {
		/// <summary>
		/// 盘点单明细ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 项次
		/// </summary>
		public int? SEQ { get; set; }
		/// <summary>
		/// 盘点单ID
		/// </summary>
		public string CountID { get; set; }
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
		/// 供应商名称
		/// </summary>
		public string SupplierUserName { get; set; }
		/// <summary>
		/// 盘点站台ID
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
        /// ERP账目数量
        /// </summary>
        public decimal? ERPQty { get; set; }
		/// <summary>
		/// 原数量
		/// </summary>
		public decimal? Qty { get; set; }
		/// <summary>
		/// 盘点数量
		/// </summary>
		public decimal? CountQty { get; set; }
		/// <summary>
		/// 盘点明细状态		New  新建,	Counting 在库盘点中,	Outing出库盘点中,	Over  结束
		/// </summary>
		public string CountState { get; set; }
		/// <summary>
		/// 盘点结果		Pass 通过,	UnPass 不通过
		/// </summary>
		public string CountResult { get; set; }
		/// <summary>
		/// 盘点单审核状态	WaitAudit 待审核,	Auditing 审核中,		Pass 通过,	UnPass 不通过
		/// </summary>
		public string AuditState { get; set; }
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
