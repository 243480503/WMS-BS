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
    /// 货位状态变更记录表
    /// </summary>
    public class T_LocationStateDetailEntity : IEntity<T_LocationStateDetailEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 货位状态变更记录ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 区域编码
        /// </summary>
        public string AreaCode { get; set; }
        /// <summary>
        /// 货位ID
        /// </summary>
        public string LocationID { get; set; }
        /// <summary>
        /// 货位编码
        /// </summary>
        public string LocationCode { get; set; }
        /// <summary>
        /// 变更前状态
        /// </summary>
        public string PreState { get; set; }
        /// <summary>
        /// 变更后状态
        /// </summary>
        public string PostState { get; set; }
        /// <summary>
        /// OutType 出库,   InType   入库
        /// </summary>
        public string InOutType { get; set; }
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
		/// 单据ID
		/// </summary>
		public string OrderID { get; set; }
        /// <summary>
        /// 引起变动的单据Code
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 单据明细ID
        /// </summary>
        public string OrderDetailID { get; set; }
		/// <summary>
		/// 引起变动的单据来源单号
		/// </summary>
		public string OrderRefCode { get; set; }
		/// <summary>
		/// 引起变动的单据明细项次
		/// </summary>
		public int? OrderSEQ { get; set; }
        /// <summary>
        /// 任务编码
        /// </summary>
        public string TaskNo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
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
