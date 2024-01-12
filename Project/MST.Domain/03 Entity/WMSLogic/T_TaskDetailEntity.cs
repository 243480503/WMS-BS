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
	/// 任务明细表(容器编码、波次、单据明细 确定一条数据，一个目标地址 确定一条主任务)
	/// </summary>
	public class T_TaskDetailEntity : IEntity<T_TaskDetailEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
	{
		/// <summary>
		/// 任务明细ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 容器ID
		/// </summary>
		public string ContainerID { get; set; }
		/// <summary>
		/// 容器编码
		/// </summary>
		public string BarCode { get; set; }
		/// <summary>
		/// 容器类型
		/// </summary>
		public string ContainerType { get; set; }
		/// <summary>
		/// 起始地址ID
		/// </summary>
		public string SrcLocationID { get; set; }
		/// <summary>
		/// 波次编码
		/// </summary>
		public string WaveCode { get; set; }
		/// <summary>
		/// 波次ID
		/// </summary>
		public string WaveID { get; set; }
		/// <summary>
		/// 波次明细ID
		/// </summary>
		public string WaveDetailID { get; set; }
		/// <summary>
		/// 单据类型
		/// PurchaseIn  采购入库单
		/// WaitCheckType 待检入库单
		/// BackSample 质检还样单
		/// GetSample 质检取样单
		/// Count	盘点单
		/// GetItemOut 领料出库单
		/// WarehouseBackOut 仓退出库单
		/// VerBackOut 验退出库单
		/// EmptyIn 空料箱入库
		/// EmptyOut 空料箱出库
		/// </summary>
		public string OrderType { get; set; }
		/// <summary>
		/// 起始地址编码
		/// </summary>
		public string SrcLocationCode { get; set; }
		/// <summary>
		/// 目标地址ID
		/// </summary>
		public string TagLocationID { get; set; }
		/// <summary>
		/// 目标地址编码
		/// </summary>
		public string TagLocationCode { get; set; }
		/// <summary>
		/// 单据ID
		/// </summary>
		public string OrderID { get; set; }
		/// <summary>
		/// 单据明细ID
		/// </summary>
		public string OrderDetailID { get; set; }
		/// <summary>
		/// 项次
		/// </summary>
		public int? SEQ { get; set; }
		/// <summary>
		/// 库存明细ID
		/// </summary>
		public string ContainerDetailID { get; set; }
		/// <summary>
		/// 贴标条码
		/// </summary>
		public string ItemBarCode { get; set; }
		/// <summary>
		/// 单据编码
		/// </summary>
		public string OrderCode { get; set; }
		/// <summary>
		/// 是否当前任务
		/// </summary>
		public string IsCurTask { get; set; }
		/// <summary>
		/// 当前任务ID
		/// </summary>
		public string TaskID { get; set; }
		/// <summary>
		/// 当前任务编码
		/// </summary>
		public string TaskNo { get; set; }
		/// <summary>
		/// 完成时间
		/// </summary>
		public DateTime? OverTime { get; set; }
		/// <summary>
		/// 是否已完成
		/// </summary>
		public string IsOver { get; set; }
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