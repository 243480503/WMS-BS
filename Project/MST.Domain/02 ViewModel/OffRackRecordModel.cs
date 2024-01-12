using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class OffRackRecordModel
    {
		/// <summary>
		/// 下架记录ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 下架单ID
		/// </summary>
		public string OffRackID { get; set; }
		/// <summary>
		/// 下架单据
		/// </summary>
		public string OffRackCode { get; set; }
		/// <summary>
		/// 来源单号
		/// </summary>
		public string RefOrderCode { get; set; }
		/// <summary>
		/// 下架单明细ID
		/// </summary>
		public string OffRackDetailID { get; set; }
		/// <summary>
		/// 库存明细ID
		/// </summary>
		public string ContainerDetailID { get; set; }
		/// 起始地址
		/// </summary>
		public string SrcLocationID { get; set; }
		public string SrcLocationCode { get; set; }
		/// <summary>
		/// 目标地址
		/// </summary>
		public string TagLocationID { get; set; }
		public string TagLocationCode { get; set; }
		public string TagLocationName { get; set; }
		/// <summary>
		/// 下架货位ID
		/// </summary>
		public string LocationID { get; set; }
		/// <summary>
		/// 下架货位编码
		/// </summary>
		public string LocationCode { get; set; }
		/// <summary>
		/// 下架到达站台
		/// </summary>
		public string StationID { get; set; }
		/// <summary>
		/// 下架到达站台编码
		/// </summary>
		public string StationCode { get; set; }
		/// <summary>
		/// 下架到达站台名称
		/// </summary>
		public string StationName { get; set; }
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
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }
		/// <summary>
		/// 标签条码
		/// </summary>
		public string ItemBarCode { get; set; }
		/// <summary>
		/// 物料ID
		/// </summary>
		public string ItemID { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string ItemCode { get; set; }
		/// <summary>
		/// 物料名称
		/// </summary>
		public string ItemName { get; set; }
		/// <summary>
		/// 物料单位
		/// </summary>
		public string ItemUnitText { get; set; }
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
		/// 批次
		/// </summary>
		public string Lot { get; set; }
		/// <summary>
		/// 原数量
		/// </summary>
		public decimal? Qty { get; set; }
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
		public int ValidityDayNum { get; set; }
		/// <summary>
		/// 容器大类
		/// </summary>
		public string ContainerKind { get; set; }
		public string ContainerKindName { get; set; }
		/// <summary>
		/// 是否贴标
		/// </summary>
		public string IsItemMark { get; set; }
		/// <summary>
		/// 下架单状态	New 新建,	OffRacking 正在下架,	Over 结束
		/// </summary>
		public string State { get; set; }
		public string StateName { get; set; }
		/// <summary>
		/// 是否已到达站台位
		/// </summary>
		public string IsArrive { get; set; }
		/// <summary>
		/// 是否需要回库
		/// </summary>
		public string IsNeedBackWare { get; set; }
		/// <summary>
		/// 回库是否已扫码
		/// </summary>
		public string IsScanBack { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Remark { get; set; }
		/// <summary>
		/// 单据需求数量
		/// </summary>
		public decimal? OrderQty { get; set; }
		/// <summary>
		/// 当前/总
		/// </summary>
		public string ReadyQutAndOrderNeed { get; set; }
		/// <summary>
		/// 最大次数
		/// </summary>
		public int MustTimes { get; set; }
		/// <summary>
		/// 剩余操作次数
		/// </summary>
		public int NoPickTimes { get; set; }
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
