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
	/// 离线下架表
	/// </summary>
	public class T_OffLineDownEntity : IEntity<T_OffLineDownEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
	{
		/// <summary>
		/// 离线下架ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 离线数据ID
		/// </summary>
		public string RFID { get; set; }
		/// <summary>
		/// 分组编码
		/// </summary>
		public string GroupNum { get; set; }
		/// <summary>
		/// ERP单据编码
		/// </summary>
		public string ERPRefCode { get; set; }
		/// <summary>
		/// 生成方式
		/// </summary>
		public string GenType { get; set; }
		/// <summary>
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }
		/// <summary>
		/// 标签条码
		/// </summary>
		public string ItemBarCode { get; set; }
		/// <summary>
		/// 过账任务ID
		/// </summary>
		public string TransTaskID { get; set; }
		/// <summary>
		/// 是否可用(true可用, false不可用，忽略)
		/// </summary>
		public string IsEnable { get; set; }
		/// <summary>
		/// 出库数量
		/// </summary>
		public decimal? Qty { get; set; }

		/// <summary>
		/// 处理状态
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// 是否已生成过账任务
		/// </summary>
		public string IsTransTask { get; set; }

		/// <summary>
		/// 物料编码(来源于库存)
		/// </summary>
		public string ConDetailItemCode { get; set; }

		/// <summary>
		/// 物料名称
		/// </summary>
		public string ConDetailItemName { get; set; }

		/// <summary>
		/// 容器大类(来源于库存)
		/// </summary>
		public string ConDetailContainerKind { get; set; }

		/// <summary>
		/// 容器种类(来源于库存)
		/// </summary>
		public string ConDetailContainerType { get; set; }

		/// <summary>
		/// 货位状态(来源于拣选前的货位)
		/// </summary>
		public string ConDetailLocationState { get; set; }

		/// <summary>
		/// 货位编码(来源于库存)
		/// </summary>
		public string ConDetailLocationCode { get; set; }

		/// <summary>
		/// 批次(来源于库存)
		/// </summary>
		public string ConDetailLot { get; set; }

		/// <summary>
		/// 库存编码(来源于库存)
		/// </summary>
		public string ConDetailSupplierCode { get; set; }

		/// <summary>
		/// 供应商名称(来源于库存)
		/// </summary>
		public string ConDetailSupplierName { get; set; }

		/// <summary>
		/// 库存ID(来源于库存)
		/// </summary>
		public string ConDetailContainerDetailID { get; set; }

		/// <summary>
		/// 处理前的库存(来源于库存)
		/// </summary>
		public decimal? ConDetailQty { get; set; }

		/// <summary>
		/// 区域编码(来源于库存)
		/// </summary>
		public string ConDetailAreaCode { get; set; }

		/// <summary>
		/// 区域名称(来源于库存)
		/// </summary>
		public string ConDetailAreaName { get; set; }

		/// <summary>
		/// 库存状态(来源于库存)
		/// </summary>

		public string ConDetailState { get; set; }

		/// <summary>
		/// 库存切片时间
		/// </summary>
		public DateTime? ConDetailPhoTime { get; set; }

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
