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
	/// 库存快照明细表
	/// </summary>
	public class T_PhoDetailEntity : IEntity<T_PhoDetailEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
	{
		/// <summary>
		/// 库存快照明细ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 库存快照ID
		/// </summary>
		public string PhoID { get; set; }
		/// <summary>
		/// 库存快照明细ID
		/// </summary>
		public string F_Id_Detail { get; set; }
		/// <summary>
		/// 物料ID
		/// </summary>
		public string ItemID { get; set; }
		/// <summary>
		/// 容器ID
		/// </summary>
		public string ContainerID { get; set; }
		/// <summary>
		/// 容器类型
		/// </summary>
		public string ContainerType { get; set; }
		/// <summary>
		/// 容器大类
		/// </summary>
		public string ContainerKind { get; set; }
		/// <summary>
		/// 货位ID
		/// </summary>
		public string LocationID { get; set; }
		/// <summary>
		/// 货位编码
		/// </summary>
		public string LocationNo { get; set; }
		/// <summary>
		/// 区域ID
		/// </summary>
		public string AreaID { get; set; }
		/// <summary>
		/// 区域编码
		/// </summary>
		public string AreaCode { get; set; }
		/// <summary>
		/// 区域名称
		/// </summary>
		public string AreaName { get; set; }
		/// <summary>
		/// 物料种类编码
		/// </summary>
		public string KindCode { get; set; }
		/// <summary>
		/// 物料种类名称
		/// </summary>
		public string KindName { get; set; }
		/// <summary>
		/// 物料名称
		/// </summary>
		public string ItemName { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string ItemCode { get; set; }
		/// <summary>
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }
		/// <summary>
		/// 贴标条码
		/// </summary>
		public string ItemBarCode { get; set; }

		/// <summary>
		/// 当前数量
		/// </summary>
		public decimal? Qty { get; set; }
		/// <summary>
		/// 待出库数量
		/// </summary>
		public decimal? OutQty { get; set; }
		/// <summary>
		/// 取样数量
		/// </summary>
		public decimal? CheckQty { get; set; }
		/// <summary>
		/// 物料单位
		/// </summary>
		public string ItemUnitText { get; set; }
		/// <summary>
		/// 质检状态
		/// </summary>
		public string CheckState { get; set; }
		/// <summary>
		/// 质检单明细ID
		/// </summary>
		public string CheckDetailID { get; set; }
		/// <summary>
		/// 质检单ID
		/// </summary>
		public string CheckID { get; set; }
		/// <summary>
		/// 有效期天数
		/// </summary>
		public int? ValidityDayNum { get; set; }
		/// <summary>
		/// 生产厂家
		/// </summary>
		public string Factory { get; set; }
		/// <summary>
		/// 是否贴物料标签
		/// </summary>
		public string IsItemMark { get; set; }
		/// <summary>
		/// 库存状态
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// 是否质检冻结
		/// </summary>
		public string IsCheckFreeze { get; set; }
		/// <summary>
		/// 是否盘点冻结
		/// </summary>
		public string IsCountFreeze { get; set; }
		/// <summary>
		/// 批号
		/// </summary>
		public string Lot { get; set; }
		/// <summary>
		/// 规格
		/// </summary>
		public string Spec { get; set; }
		/// <summary>
		/// ERP仓库编码
		/// </summary>
		public string ERPWarehouseCode { get; set; }
		/// <summary>
		/// 生产日期
		/// </summary>
		public DateTime? ProductDate { get; set; }
		/// <summary>
		/// 过期时间
		/// </summary>
		public DateTime? OverdueDate { get; set; }
		/// <summary>
		/// 供应商ID
		/// </summary>
		public string SupplierID { get; set; }
		/// <summary>
		/// 供应商编码
		/// </summary>
		public string SupplierCode { get; set; }
		/// <summary>
		/// 供应商名称
		/// </summary>
		public string SupplierName { get; set; }
		/// <summary>
		/// 收货记录ID
		/// </summary>
		public string ReceiveRecordID { get; set; }
		/// <summary>
		/// 是否特殊物料
		/// </summary>
		public string IsSpecial { get; set; }
		/// <summary>
		/// 是否虚拟条码
		/// </summary>
		public string IsVirItemBarCode { get; set; }
		/// <summary>
		/// 入库单ID
		/// </summary>
		public string InBoundID { get; set; }
		/// <summary>
		/// 入库来源单号
		/// </summary>
		public string RefInBoundCode { get; set; }
		/// <summary>
		/// 入库明细ID
		/// </summary>
		public string InBoundDetailID { get; set; }
		/// <summary>
		/// 入库项次
		/// </summary>
		public int? SEQ { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Remark { get; set; }
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
