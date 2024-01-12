/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 盘点记录表
    /// </summary>
    public class CountRecordModel
    {
		public string F_Id { get; set; }    /// 盘点记录ID
		public string SEQ { get; set; } /// 项次
		public string CountID { get; set; }
		public string RefOrderCode { get; set; }
		public string CountDetailID { get; set; }   /// 盘点单明细ID
		public string ContainerID { get; set; }   /// 容器ID
		public string ContainerKind { get; set; }
		public string ContainerKindName { get; set; }
		public string ContainerDetailID { get; set; }   /// 库存明细ID
		public string StationID { get; set; }   /// 盘点站台ID
		public string BarCode { get; set; }   /// 容器条码
		public string ItemBarCode { get; set; }   /// 贴标条码
		public string ItemID { get; set; }   /// 物料ID
		public string ItemName { get; set; }   /// 物料名称
		public string ItemCode { get; set; }   /// 物料编码
		public DateTime? ProductDate { get; set; }
		public string TransState { get; set; }
		public string TransStateName { get; set; }  /// 过账状态
													/// UnNeedTrans 免过账
													/// WaittingTrans 待过帐
													/// OverTrans 已过帐
													/// FailTrans 过账失败

		public string SupplierUserID { get; set; }   /// 供应商ID
		public string SupplierUserName { get; set; }   /// 供应商名称
        public string LocationID { get; set; }   /// 货位ID
		public string LocationCode { get; set; }   /// 货位编码
		public string Lot { get; set; }   /// 批号
        public string Spec { get; set; }    /// 规格

		public string Factory { get; set; }
        public string ItemUnitText { get; set; }    /// 计量单位
        public DateTime? OverdueDate { get; set; }	/// 失效日期
        public decimal? CountQty { get; set; }   /// 盘点数量
		public decimal? Qty { get; set; }   /// 原数量
		public decimal? OrderQty { get; set; }  /// 单据数量
		public string ReadyQutAndOrderNeed { get; set; }
		public int MustTimes { get; set; }
		public int NoCountTimes { get; set; }
		public string IsOpen { get; set; }	/// 是否明盘
		public string IsOutCount { get; set; }   /// 是否出库盘点
		public string FactBarCode { get; set; } /// 盘点实际箱码
		public string CountResult { get; set; }
		public string CountResultName { get; set; } ///	盘点结果		
													/// Inner_SameBoxCode 箱码一致
													/// Inner_DiffBoxCode 箱码不一致
													/// Inner_MoreBoxCode 多余箱码
													/// Inner_Empty 正常空货位
													/// Inner_NotFindBoxCode 未找到箱码
													/// 
													/// Outer_MoreQty 多余数量
													/// Outer_LessQty 少数量
		public string State { get; set; }

		/// <summary>
		/// ERP仓库编码
		/// </summary>
		public string ERPCode { get; set; }
		public string StateName { get; set; }   /// 盘点状态    
													 /// New 新建
													 /// Counting 盘点中
													 /// NoNeed 免盘
													 /// Over 结束
		public string AuditState { get; set; }
		public string IsItemMark { get; set; }  /// 是否物料贴标
												/// true 贴标
												/// false 未贴标
		public string AuditStateName { get; set; }  /// 审核状态
													/// WaitAudit 待审核
													/// Auditing 审核中
													/// Pass  通过
													/// UnPass 不通过			
		public string GenType { get; set; }
		public string GenTypeName { get; set; } /// 生成方式
												/// Auto 自动
												/// MAN 手动
		public string IsVirItemBarCode { get; set; }	/// 是否虚拟物料条码
		public string IsAdd { get; set; }   /// 是否是新增记录（新增库存时1）
		public string IsUpdate { get; set; }  /// 是否已更新库存
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
