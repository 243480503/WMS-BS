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
	/// 过账记录表
	/// </summary>
    public class T_TransRecordEntity : IEntity<T_TransRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
		/// <summary>
		/// 过账记录ID
		/// </summary>
		public string F_Id { get; set; }
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
		/// 单据编码
		/// </summary>
		public string OrderCode { get; set; }
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
		/// 最大过账次数
		/// </summary>
		public int? MaxTransCount { get; set; }
		/// <summary>
		/// 失败次数
		/// </summary>
		public int? ErrCount { get; set; }
		/// <summary>
		/// 最后过账时间
		/// </summary>
		public DateTime? LastTime { get; set; }
		/// <summary>
		/// 是否忽略
		/// </summary>
		public string IsIgnore { get; set; }
		/// <summary>
		/// 状态
		/// OK  成功
		/// Err 失败
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// 消息内容
		/// </summary>
		public string Info { get; set; }
		/// <summary>
		/// 发送报文
		/// </summary>
		public string SendText { get; set; }
		/// <summary>
		/// 接收报文
		/// </summary>
		public string GetText { get; set; }
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
