/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using System;

namespace MST.Domain.Entity.WMSLogic
{
    public class V_LocationInfoEntity : IEntity<V_LocationInfoEntity>
	{
		/// <summary>
		/// 联合主键(T_Location.F_Id + T_ContainerDetail.F_Id)
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 货位ID
		/// </summary>
		public string LocationID { get; set; }
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
		/// 区域类型。Cube 立体库,		Flat 平库
		/// </summary>
		public string AreaType { get; set; }
		public string AreaTypeName { get; set; }
		/// <summary>
		/// 货位编码
		/// </summary>
		public string LocationCode { get; set; }
		/// <summary>
		/// 货位类型	Flat 平库,Cube 立库
		/// </summary>
		public string LocationType { get; set; }
		public string LocationTypeName { get; set; }
		/// <summary>
		/// 货位状态。In 待入库,   Out 待出库,   Empty 空,   Stored 已存储
		/// </summary>
		public string State { get; set; }
		public string StateName { get; set; }
		/// <summary>
		/// 货位锁定状态。OnlyIn 可入不可出,   OnlyOut 可出不可入,   Lock  锁定 ,   Normal 正常
		/// </summary>
		public string ForbiddenState { get; set; }
		public string ForbiddenStateName { get; set; }
		/// <summary>
		/// 库存ID
		/// </summary>
		public string DetailID { get; set; }
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
		public string ContainerTypeName { get; set; }
		/// <summary>
		/// 容器大类	Box	纸箱,   Plastic  料箱
		/// </summary>
		public string ContainerKind { get; set; }
		public string ContainerKindName { get; set; }
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
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 贴标条码
        /// </summary>
        public string ItemBarCode { get; set; }
		/// <summary>
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }
		/// <summary>
		/// 不包含  待出库数量 和 取样数量
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
		public string CheckStateName { get; set; }
		/// <summary>
		/// 质检明细ID
		/// </summary>
		public string CheckDetailID { get; set; }
		/// <summary>
		/// 质检单ID
		/// </summary>
		public string CheckID { get; set; }
		/// <summary>
		/// 物料状态。Normal  正常,   Freeze  冻结 ，仅表示手动操作造成的冻结 。 (质检造成的冻结请用IsCheckFreeze，盘点造成的冻结请用IsCountFreeze)
		/// </summary>
		public string ItemState { get; set; }
		public string ItemStateName { get; set; }
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
		public string ERPWarehouseName { get; set; }
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
		/// 供应商
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
	}
}
