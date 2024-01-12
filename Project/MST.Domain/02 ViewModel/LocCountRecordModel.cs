using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
	public class LocCountRecordModel
	{
		/// <summary>
		/// 盘点记录ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 盘点单ID
		/// </summary>
		public string LocCountID { get; set; }
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
		/// 区域类型
		/// </summary>
		public string AreaType { get; set; }
		/// <summary>
		/// 货位ID
		/// </summary>
		public string LocationID { get; set; }
		/// <summary>
		/// 货位编码
		/// </summary>
		public string LocationCode { get; set; }
		/// <summary>
		/// 容器ID
		/// </summary>
		public string ContainerID { get; set; }
		/// <summary>
		/// 容器大类
		/// </summary>
		public string ContainerKind { get; set; }
		public string ContainerKindName { get; set; }
		/// <summary>
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }
		/// <summary>
		/// 盘点实际箱码
		/// </summary>
		public string FactBarCode { get; set; }
		/// <summary>
		/// 储位状态
		/// </summary>
		public string LocState { get; set; }
		public string LocStateName { get; set; }
		public string State { get; set; }
		public string StateName { get; set; }

		public string Remark { get; set; }
		/// <summary>
		/// 禁用状态
		/// </summary>
		public string ForbiddenState { get; set; }
		public string ForbiddenStateName { get; set; }
		/// <summary>
		/// 盘点状态 New 新建，Counting 盘点中，Overing 结束中，Over 结束
		/// </summary>
		public string CountState { get; set; }
		public string CountStateName { get; set; }
		/// <summary>
		/// 盘点结果		
		/// Inner_SameBoxCode 箱码一致
		/// Inner_DiffBoxCode 箱码不一致
		/// Inner_MoreBoxCode 多余箱码
		/// Inner_Empty 正常空货位
		/// Inner_NotFindBoxCode 未找到箱码
		/// 
		/// Outer_Normal 正常
		/// Outer_MoreQty 多余数量
		/// Outer_LessQty 少数量 （未盘点到的一律是少数量，需删除库存）
		/// Outer_MoreItemBarcode 多标签（需新增库存）
		/// </summary>
		public string CountResult { get; set; }
		public string CountResultName { get; set; }
		/// <summary>
		/// 生成方式
		/// Auto 自动
		/// MAN 手动
		/// </summary>
		public string GenType { get; set; }
		public string GenTypeName { get; set; }
		/// <summary>
		/// 是否已处理异常
		/// </summary>
		public string IsConfirm { get; set; }
		/// <summary>
		/// 来源单号
		/// </summary>
		public string RefOrderCode { get; set; }
		/// <summary>
		/// 异常处理站台
		/// </summary>
		public string StationID { get; set; }
	    /// <summary>
		/// 本单据所有待处理异常总数
		/// </summary>
		public int MustTimes { get; set; }	
		/// <summary>
		/// 本单据待处理异常数
		/// </summary>
		public int NoCountTimes { get; set; }
		/// <summary>
		/// 异常处理进度
		/// </summary>
		public string ReadyConfirmAndOrderNeed { get; set; }
		/// <summary>
		/// 异常提示信息
		/// </summary>
		public string ErrorMsg { get; set; }
		public string ErrSolution { get; set; }
		/// <summary>
		/// 单据状态
		/// </summary>
		public string OrderState { get; set; }
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
