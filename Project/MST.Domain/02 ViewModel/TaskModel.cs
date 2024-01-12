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
	public class TaskModel
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
		/// 出入库类型（InType  入库类型、OutType  出库类型）
		/// </summary>
		public string TaskInOutType { get; set; }
		/// <summary>
		/// 出入库类型名称
		/// </summary>
		public string TaskInOutTypeName { get; set; }

		/// <summary>
		/// 任务类型
		/// TaskType_CheckPickOut	质检取样出库
		/// TaskType_CheckPickIn	质检取样入库
		/// TaskType_CheckReturnOut 质检还样出库
		/// TaskType_CheckReturnIn	质检还样入库
		/// TaskType_CountOut		盘点出库
		/// TaskType_CountIn		盘点入库
		/// TaskType_CountOut		在库盘点（AGV盘）
		/// TaskType_PurchaseIn		采购入库
		/// TaskType_WaitCheck		待检入库
		/// TaskType_GetItemOut		领料出库
		/// TaskType_GetItemBack	领料回库
		/// TaskType_WarehouseBackOut 仓退出库
		/// TaskType_VerBackOut		验退出库
		/// TaskType_EmptyIn		空托盘入库
		/// TaskType_EmptyOut		空托盘出库
		/// TaskType_MoveType		移库类型
		/// TaskType_CarryType		搬运类型
		/// TaskType_OffRackType    下架出库
		/// </summary>
		public string TaskType { get; set; }

		/// <summary>
		/// 任务类型名称
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
		/// 容器类型
		/// </summary>
		public string ContainerType { get; set; }

		/// <summary>
		/// 起始地址ID
		/// </summary>
		public string SrcLocationID { get; set; }

		/// <summary>
		/// 起始地址编码(站台地址，或货位地址编码)
		/// </summary>
		public string SrcLocationCode { get; set; }

		/// <summary>
		/// 目标地址区域ID（入库货位分配使用）
		/// </summary>
		public string TagAreaID { get; set; }

		/// <summary>
		/// 目标地址ID
		/// </summary>
		public string TagLocationID { get; set; }

		/// <summary>
		/// 目标地址编码(站台地址，或货位地址编码)
		/// </summary>
		public string TagLocationCode { get; set; }

		/// <summary>
		/// 目标地址编码名称(站台名称)
		/// </summary>

		public string TagLocationCodeName { get; set; }

		/// <summary>
		/// 是否手动分配货位
		/// </summary>

		public string IsHandPointLoc { get; set; }

		/// <summary>
		/// 申请入库时，物料的默认入库站台(即设备ID，手动模拟WCS入库时使用）
		/// </summary>

		public string ApplyStationID { get; set; }

		/// <summary>
		/// 波次ID
		/// </summary>
		public string WaveID { get; set; }

		/// <summary>
		/// 波次编码
		/// </summary>
		public string WaveCode { get; set; }

		/// <summary>
		/// 波次明细ID
		/// </summary>
		public string WaveDetailID { get; set; }   

		/// <summary>
		/// 指定设备执行该任务
		/// </summary>
		public string PointExecRobotCode { get; set; }

		/// <summary>
		/// 单据项次
		/// </summary>
		public int? SEQ { get; set; }

		/// <summary>
		/// 任务优先级（1-255），越大优先级越高
		/// 1  最低(库存入库、移库任务)
		/// 2  较低(盘点)
		/// 3  较高(质检)
		/// 4  最高(库存出库)
		/// </summary>
		public int? Level { get; set; }

		/// <summary>
		/// 任务状态
		/// New 新建(创建的时候，且发送给WCS之后)
		/// Execing  执行中（WCS返回状态并修改）
		/// HungUp 挂起
		/// Over 完成（WCS返回完状态）
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// 任务状态名称
		/// </summary>
		public string StateName { get; set; }  
												
		/// <summary>
		/// 是否由WCS执行
		/// </summary>
		public string IsWcsTask { get; set; }

		/// <summary>
		/// 实际执行设备ID
		/// </summary>
		public string ExecEquID { get; set; }

		/// <summary>
		/// 是否可执行
		/// </summary>
		public string IsCanExec { get; set; }

		/// <summary>
		/// 对接时间(发送给WCS的时间)
		/// </summary>
		public DateTime? SendWCSTime { get; set; }

		/// <summary>
		/// 单据类型
		/// PurchaseIn  采购入库单
		/// Check	质检单
		/// Count	盘点单
		/// OutType	领料出库单
		/// BackType  退料出库单
		/// DamageType 报损出库单
		/// OffRack 下架单
		/// </summary>
		public string OrderType { get; set; }

		/// <summary>
		/// 单据类型名称
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
		/// 发送到WCS的报文(WCSApplayResult对象)
		/// </summary>
		public string SendMsg { get; set; }

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
