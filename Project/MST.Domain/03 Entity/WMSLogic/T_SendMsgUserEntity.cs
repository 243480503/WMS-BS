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
	/// 消息接收人
	/// </summary>
	public class T_SendMsgUserEntity : IEntity<T_SendMsgUserEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
	{
		/// <summary>
		/// 消息接收人ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 定时器ID
		/// </summary>
		public string TimerID { get; set; }
		/// <summary>
		/// 消息类型
		/// </summary>
		public string MsgType { get; set; }
		/// <summary>
		/// 个人UserID,   或,   角色ID
		/// </summary>
		public string ReceiveID { get; set; }
		/// <summary>
		/// 是否邮件推送
		/// </summary>
		public string IsSendEmail { get; set; }
		/// <summary>
		/// 是否短信推送
		/// </summary>
		public string IsSendPho { get; set; }
		/// <summary>
		/// 是否微信推送
		/// </summary>
		public string IsSendWeChat { get; set; }
		/// <summary>
		/// 是否站内推送
		/// </summary>
		public string IsSendInner { get; set; }
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
