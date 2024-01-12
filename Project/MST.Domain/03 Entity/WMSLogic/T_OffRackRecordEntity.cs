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
	/// 下架记录表
	/// </summary>
    public class T_OffRackRecordEntity : IEntity<T_OffRackRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
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
		/// 下架单明细ID
		/// </summary>
		public string OffRackDetailID { get; set; }
		/// <summary>
		/// 库存明细ID
		/// </summary>
		public string ContainerDetailID { get; set; }
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
		/// 目标地址ID
		/// </summary>
		public string TagLocationID { get; set; }
		/// <summary>
		/// 容器ID
		/// </summary>
		public string ContainerID { get; set; }
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
		/// 项次
		/// </summary>
		public string Lot { get; set; }
		/// <summary>
		/// 原数量
		/// </summary>
		public decimal? Qty { get; set; }
		/// <summary>
		/// 是否贴标
		/// </summary>
		public string IsItemMark { get; set; }
		/// <summary>
		/// 下架单状态	New 新建,	OffRacking 正在下架,	Over 结束
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// 出库任务编码
		/// </summary>
		public string TaskNo { get; set; }
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
