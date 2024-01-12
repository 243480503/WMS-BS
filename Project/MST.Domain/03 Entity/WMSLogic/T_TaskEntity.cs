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
    /// 任务表
    /// </summary>
    public class T_TaskEntity : IEntity<T_TaskEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
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
        /// 出入库类型
        /// InType  入库类型
        /// OutType  出库类型
        /// </summary>
        public string TaskInOutType { get; set; }
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
        /// 目标地址ID,T_Station的ID
        /// </summary>
        public string TagLocationID { get; set; }
        /// <summary>
        /// 目标地址编码(站台的TagAddress地址，或货位地址编码)
        /// </summary>
        public string TagLocationCode { get; set; }
        /// <summary>
        /// WCS申请入库时的站台，或出库时需求去的站台 (即站台ID，WCS入库、出库、回库 时使用）
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
        /// 当前任务所在的设备ID（3D前端用到）
        /// </summary>
        public string CurEquID3D { get; set; }
        /// <summary>
        /// 波次明细ID
        /// </summary>
        public string WaveDetailID { get; set; }
        /// <summary>
        /// 单据项次
        /// </summary>
        public int? SEQ { get; set; }

        /// <summary>
		/// 指定设备执行该任务
		/// </summary>
		public string PointExecRobotCode { get; set; }

        /// <summary>
        /// 任务优先级（1到127，越大优先级越高）
        /// 10  最低(非回库产生的入库)
        /// 20  较低(所有出库)
        /// 30  较高(盘点、质检、移库)
        /// 40  最高(回库产生的入库)
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
        /// 是否WCS任务
        /// </summary>
        public string IsWcsTask { get; set; }
        /// <summary>
        /// 执行设备ID
        /// </summary>
		public string ExecEquID { get; set; }
        /// <summary>
        /// 是否可执行
        /// </summary>
		public string IsCanExec { get; set; }
        /// <summary>
        /// 对接时间
        /// </summary>
		public DateTime? SendWCSTime { get; set; }
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
        /// OffRack 下架单
        /// </summary>
		public string OrderType { get; set; }
        /// <summary>
        /// 是否RF手动分配货位(料箱、料架在RF组盘时点击入库按钮确定，纸箱无法指定，除非使用RF扫码入库)
        /// </summary>
        public string IsHandPointLoc { get; set; }
        /// <summary>
        /// 单据ID
        /// </summary>
        public string OrderID { get; set; }
        /// <summary>
        /// 单据明细ID
        /// </summary>
		public string OrderDetailID { get; set; }
        /// <summary>
        /// 单据编码
        /// </summary>
		public string OrderCode { get; set; }
        /// <summary>
        /// AGV所需映射的目标货位编码
        /// </summary>
        public string TagWCSLocCode { get; set; }

        /// <summary>
        /// AGV所需映射的起始货位编码
        /// </summary>
        public string SrcWCSLocCode { get; set; }
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
