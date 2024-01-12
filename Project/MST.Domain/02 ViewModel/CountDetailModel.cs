using System;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 盘点明细表
    /// </summary>
    public class CountDetailModel
    {
        public string F_Id { get; set; }    /// 盘点单明细ID
		public int? SEQ { get; set; }	/// 项次
		public string CountID { get; set; } /// 盘点单ID
		public string ItemID { get; set; }  /// 物料ID
		public string ItemName { get; set; }    /// 物料名称
		public string ItemCode { get; set; }    /// 物料编码
        public string Factory { get; set; } /// 生产厂家
        public string SupplierUserID { get; set; }  /// 供应商ID
		public string SupplierUserName { get; set; }    /// 供应商名称
		public string StationID { get; set; }	/// 盘点站台ID
        public string StationCode { get; set; }
        public string StationName { get; set; }
        /// <summary>
        /// 物料容器类型大类
        /// </summary>
        public string ContainerKind { get; set; }

        /// <summary>
        /// 物料容器类型大类名称
        /// </summary>
        public string ContainerKindName { get; set; }
        /// <summary>
        /// 是否贴标
        /// </summary>
        public string IsItemMark { get; set; }
        /// <summary>
		/// ERP账目数量
		/// </summary>
		public decimal? ERPQty { get; set; }
        public string Lot { get; set; } /// 批号
        public string Spec {  get; set; }   /// 规格
        public string ItemUnitText {  get; set; }   /// 计量单位
        public DateTime? OverdueDate { get; set; }   /// 失效日期
        public decimal? Qty { get; set; }   /// 原数量
        public decimal? CountQty { get; set; }  /// 盘点数量
		public string CountState { get; set; }
        public string StateName { get; set; }  /// 盘点明细状态 
                                                    /// New  新建
                                                    /// Counting 盘点中
                                                    /// Over  结束
        public string AuditState { get; set; }
        public string AuditStateName { get; set; }  /// 盘点单审核状态
                                                    /// WaitAudit 待审核
                                                    /// Auditing 审核中
                                                    /// Pass  通过
                                                    /// UnPass 不通过
        public string CountResult { get; set; }
        public string CountResultName { get; set; }  /// 明细盘点结果
                                                     /// Pass 通过
                                                     /// UnPass 不通过

        public bool? F_DeleteMark { get; set; } /// 是否删除
        public DateTime? F_CreatorTime { get; set; }    /// 创建时间
        public string F_CreatorUserId { get; set; }    /// 创建人ID
        public string CreatorUserName { get; set; }    /// 创建人名称
        public string F_DeleteUserId { get; set; }    /// 删除操作人
        public DateTime? F_DeleteTime { get; set; }    /// 删除操作时间
        public string DeleteUserName { get; set; }    /// 删除操作人名称
        public DateTime? F_LastModifyTime { get; set; }    /// 修改时间
        public string F_LastModifyUserId { get; set; }    /// 修改人ID
        public string ModifyUserName { get; set; }    /// 修改人名称

    }
}
