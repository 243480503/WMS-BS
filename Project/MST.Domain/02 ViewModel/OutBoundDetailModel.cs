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
	/// 出库单明细
	/// </summary>
	public class OutBoundDetailModel
	{

		/// <summary>
		/// 出库单明细ID
		/// </summary>
		public string F_Id { get; set; }

		/// <summary>
		/// 出库单ID
		/// </summary>
		public string OutBoundID { get; set; }

		/// <summary>
		/// 出库单编码
		/// </summary>
		public string OutBoundCode { get; set; }

		/// <summary>
		/// 单据类型
		/// </summary>
		public string OutBoundType { get; set; }
		/// <summary>
		/// 项次
		/// </summary>
		public int? SEQ { get; set; }

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
		/// 是否紧急出库
		/// </summary>
		public string IsUrgent { get; set; }
		/// <summary>
		/// 指定出库单要出的入库单ID
		/// </summary>
		public string PointInBoundID { get; set; }

		/// <summary>
		/// 批号
		/// </summary>
		public string Lot { get; set; }
		/// <summary>
		/// 规格
		/// </summary>
		public string Spec {  get; set; }
		/// <summary>
		/// 计量单位
		/// </summary>
		public string ItemUnitText { get; set; }
		/// <summary>
		/// 失效日期
		/// </summary>
		public DateTime? OverdueDate { get; set; }

        /// <summary>
        /// 需求数量
        /// </summary>
        public decimal? Qty { get; set; }

		/// <summary>
		/// 单位数量
		/// </summary>
		public decimal? UnitQty { get; set; }

		/// <summary>
		/// 出库站台(单据不提供则从物料明细中获取)
		/// </summary>
		public string StationID { get; set; }

		/// <summary>
		/// 出库站台名称
		/// </summary>
		public string StationName { get; set; }

		/// <summary>
		/// 执行方式（手动、设备）
		/// </summary>
		public string ActionType { get; set; }

		/// <summary>
		/// 库存总量
		/// </summary>
		public decimal? AllQty { get; set; }

		/// <summary>
		/// 可用数量 
		/// </summary>
		public decimal? CanUseQty { get; set; }

		/// <summary>
		/// 出库站台编码
		/// </summary>
		public string StationCode { get; set; }

		/// <summary>
		/// 目标地址
		/// </summary>
		public string TagAddressCode { get; set; }
		/// <summary>
		/// 容器种类
		/// </summary>
		public string ContainerKind { get; set; }
		/// <summary>
		/// 容器种类
		/// </summary>
		public string ContainerKindName { get; set; }

		/// <summary>
		/// 已出数量
		/// </summary>
		public decimal? OutQty { get; set; }

		/// <summary>
		/// 波次数量
		/// </summary>
		public decimal? WaveQty { get; set; }

		/// <summary>
		/// 按对应入库单出库（入库单的来源单号）
		/// </summary>
		public string SourceInOrderCode { get; set; }


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
		/// 出库状态
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// 出库状态名称
		/// </summary>
		public string StateName { get; set; }

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
