/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using System;

namespace MST.Domain.ViewModel
{
    public class LocationInfoModel
    {
		public string LocationInfoID { get { return $"{F_Id}-{ItemID}"; } }
		public string F_Id { get; set; }    /// 货位ID
		public string AreaID { get; set; }  /// 区域ID
		public string AreaCode { get; set; }    /// 区域编码
		public string AreaName { get; set; }    /// 区域名称
		public string AreaType { get; set; }    /// Cube 立体库,		Flat 平库
		public string AreaTypeName { get; set; }
		public string LocationCode { get; set; }    /// 货位编码
		public string LocationType { get; set; }    /// Flat 平库,Cube 立库
		public string LocationTypeName { get; set; }    /// 货位类型名称 Flat 平库,Cube 立库
		public string State { get; set; }   /// In 待入库,   Out 待出库,   Empty 空,   Stored 已存储
		public string StateName { get; set; }
		public string ForbiddenState { get; set; }  /// OnlyIn 可入不可出,   OnlyOut 可出不可入,   Lock  锁定 ,   Normal 正常
		public string ForbiddenStateName { get; set; }

		public string ItemID { get; set; }  /// 物料ID
		public string ContainerID { get; set; } /// 容器ID
		public string ContainerType { get; set; }       /// 容器类型	Box      纸箱,   Plastic  料箱
		public string ContainerTypeName { get; set; }
		public string ContainerKind { get; set; }
		public string ContainerKindName { get; set; }
		public string KindCode { get; set; }    /// 物料种类编码
		public string KindName { get; set; }    /// 物料种类名称
		public string ItemName { get; set; }    /// 物料名称
		public string ItemCode { get; set; }    /// 物料编码
		public string ItemBarCode { get; set; } /// 贴标条码
		public string BarCode { get; set; }
		public decimal? Qty { get; set; }   /// 不包含  待出库数量 和 取样数量
		public decimal? OutQty { get; set; }    /// 待出库数量
		public decimal? CheckQty { get; set; }  /// 取样数量
		public string ItemUnitText { get; set; }    /// 物料单位
		public string CheckState { get; set; }
		public string CheckDetailID { get; set; }
		public string CheckID { get; set; }
		public string ItemState { get; set; }   /// Normal  正常,   Freeze  冻结 ，仅表示手动操作造成的冻结 。 (质检造成的冻结请用IsCheckFreeze，盘点造成的冻结请用IsCountFreeze)
		public string ItemStateName { get; set; }
		public string IsCheckFreeze { get; set; }
		public string IsCountFreeze { get; set; }
		public string Lot { get; set; } /// 批号
		public string ERPWarehouseCode { get; set; }    /// ERP仓库编码
		public DateTime? ProductDate { get; set; }  /// 生产日期
		public DateTime? OverdueDate { get; set; }  /// 过期时间
		public string SupplierID { get; set; }  /// 供应商ID
		public string SupplierCode { get; set; }    /// 供应商编码
		public string SupplierName { get; set; }    /// 供应商
		public string ReceiveRecordID { get; set; } /// 收货记录ID
		public string IsSpecial { get; set; }   /// 是否特殊物料

												/// <summary>
												/// 是否基础数据
												/// </summary>
		public string IsBase { get; set; }
	}
}
