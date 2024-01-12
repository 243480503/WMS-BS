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
	/// 任务明细表(容器编码、波次、单据明细 确定一条数据，一个目标地址 确定一条主任务)
	/// </summary>
	public class TaskDetailModel
    {
		public string F_Id { get; set; }    /// 任务明细ID
		public string ContainerID { get; set; } /// 容器ID
		public string BarCode { get; set; } /// 容器编码
		public string ContainerType { get; set; }   /// 容器类型
		public string ContainerTypeName { get; set; }
		public string SrcLocationID { get; set; }   /// 起始地址ID
		public string WaveCode { get; set; }    /// 波次编码
		public string WaveID { get; set; }  /// 波次ID
		public string OrderType { get; set; }   /// 单据类型
		public string SrcLocationCode { get; set; } /// 起始地址编码
		public string TagLocationID { get; set; }   /// 目标地址ID
		public string TagLocationCode { get; set; } /// 目标地址编码
		public string OrderID { get; set; } /// 单据ID
		public string OrderDetailID { get; set; }   /// 单据明细ID
		public int? SEQ { get; set; }   /// 项次
		public string OrderCode { get; set; }   /// 单据编码
		public string IsCurTask { get; set; }   /// 是否当前任务
		public string TaskID { get; set; }  /// 当前任务ID
		public string TaskNo { get; set; }  /// 当前任务编码
		public DateTime? OverTime { get; set; } /// 完成时间
		public string IsOver { get; set; }  /// 是否已完成
		public bool? F_DeleteMark { get; set; } /// 是否删除
		public DateTime? F_CreatorTime { get; set; }    /// 创建时间
		public string F_CreatorUserId { get; set; } /// 创建人ID
		public string CreatorUserName { get; set; } /// 创建人名称
		public string F_DeleteUserId { get; set; }  /// 删除操作人
		public DateTime? F_DeleteTime { get; set; } /// 删除操作时间
		public string DeleteUserName { get; set; }  /// 删除操作人名称
		public DateTime? F_LastModifyTime { get; set; } /// 修改时间
		public string F_LastModifyUserId { get; set; }  /// 修改人ID
		public string ModifyUserName { get; set; }  /// 修改人名称
	}
}
