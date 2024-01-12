/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
	public class TaskHisModel
	{

		/// <summary>
		/// 任务ID
		/// </summary>
		public string F_Id { get; set; }

		/// <summary>
		/// 任务编码
		/// </summary>
		public string TaskNo { get; set; }

		/// <summary>
		/// InType  入库类型,   OutType  出库类型
		/// </summary>
		public string TaskInOutType { get; set; }

		/// <summary>
		/// 出入库类型
		/// </summary>
		public string TaskInOutTypeName { get; set; }

		/// <summary>
		/// TaskType_CheckPickOut 质检取样出库,   TaskType_CheckPickIn 质检取样入库,   TaskType_CheckReturnOut 质检还样出库,   TaskType_CheckReturnIn 质检还样入库,   TaskType_CountOut 盘点出库,   TaskType_CountIn  盘点入库,   TaskType_OrderIn 库存入库,   TaskType_OutType 领料出库,   TaskType_OutTypeReturn 余料回库(针对领料出库),   TaskType_BackType 退料出库,   TaskType_BackTypeReturn 退料回库(针对领料出库),   TaskType_DamageType 报损出库,   TaskType_DamageTypeReturn 报损回库
		/// </summary>
		public string TaskType { get; set; }

		/// <summary>
		/// 任务类型
		/// </summary>
		public string TaskTypeName { get; set; }

		/// <summary>
		/// 容器ID
		/// </summary>
		public string ContainerID { get; set; }

		/// <summary>
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }

		/// <summary>
		/// 起始地址ID
		/// </summary>
		public string SrcLocationID { get; set; }

		/// <summary>
		/// 起始地址编码
		/// </summary>
		public string SrcLocationCode { get; set; }

		/// <summary>
		/// 目标地址ID
		/// </summary>
		public string TagLocaitonID { get; set; }

		/// <summary>
		/// 是否手动指定货位
		/// </summary>
		public string IsHandPointLoc { get; set; }

		/// <summary>
		/// 目标地址编码
		/// </summary>
		public string TagLocationCode { get; set; }

		/// <summary>
		/// 1  最低(移库任务),   2  较低(盘点),   3  较高(质检),   4  最高(库存入库、库存出库)
		/// </summary>
		public int? Level { get; set; }

		/// <summary>
		/// 是否WCS任务
		/// </summary>
		public string IsWcsTask { get; set; }

		/// <summary>
		/// 执行设备ID
		/// </summary>
		public string ExecEquID { get; set; }

		/// <summary>
		/// New 新建(创建的时候，且发送给WCS之后),   Execing  执行中（WCS返回状态并修改）,   HungUp 挂起,   Over 完成（WCS返回完状态）
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// 任务状态
		/// </summary>
		public string StateName { get; set; }

		/// <summary>
		/// 是否可执行
		/// </summary>
		public string IsCanExec { get; set; }

		/// <summary>
		/// 对接时间
		/// </summary>
		public DateTime? SendWCSTime { get; set; }

		/// <summary>
		/// PurchaseIn  采购入库单,   Check          质检单,   Count           盘点单,   OutType     领料出库单,   BackType  退料出库单,   DamageType 报损出库单
		/// </summary>
		public string OrderType { get; set; }

		/// <summary>
		/// 指定设备执行该任务
		/// </summary>
		public string PointExecRobotCode { get; set; }

		/// <summary>
		/// 项次
		/// </summary>
		public int? SEQ { get; set; }

		/// <summary>
		/// 单据类型
		/// </summary>
		public string OrderTypeName { get; set; }

		/// <summary>
		/// 单据ID
		/// </summary>
		public string OrderID { get; set; }

		/// <summary>
		/// 单据明细ID
		/// </summary>
		public string OrderDetailID { get; set; }

		/// <summary>
		/// AGV所需映射的目标货位编码
		/// </summary>
		public string TagWCSLocCode { get; set; }

		/// <summary>
		/// AGV所需映射的起始货位编码
		/// </summary>
		public string SrcWCSLocCode { get; set; }

		/// <summary>
		/// 单据编码
		/// </summary>
		public string OrderCode { get; set; }

		/// <summary>
		/// 完成时间
		/// </summary>
		public DateTime? OverTime { get; set; }

		/// <summary>
		/// 发送到WCS的报文(WCSApplayResult对象)
		/// </summary>
		public string SendMsg { get; set; }

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
