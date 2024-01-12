/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.ViewModel
{
    public class TransRecordModel
    {
		public string F_Id { get; set; }    /// 过账记录ID
		public string OrderType { get; set; }
		public string OrderTypeName { get; set; }    /// 单据类型
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

		public string OrderCode { get; set; }    /// 单据编码
		public int? MaxTransCount { get; set; }    /// 最大过账次数
		public int? ErrCount { get; set; }    /// 失败次数
		public DateTime? LastTime { get; set; }    /// 最后过账时间
		public string IsIgnore { get; set; }    /// 是否忽略
		public string OrderID { get; set; }
		public string OrderDetailID { get; set; }
		public int? SEQ { get; set; }
		public string State { get; set; }
		public string StateName { get; set; }    /// 状态
												 /// OK  成功
												 /// Err 失败
		public string Info { get; set; }    /// 消息内容
		public string SendText { get; set; }    /// 发送报文
		public string GetText { get; set; }    /// 接收报文
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
