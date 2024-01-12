using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
	/// <summary>
	/// 消息列表模型
	/// </summary>
    public class SendMsgModel
    {
		/// <summary>
		/// 消息ID
		/// </summary>
		public string F_Id { get; set; }

		/// <summary>
		/// 消息内容
		/// </summary>
		public string Msg { get; set; }

		/// <summary>
		/// 消息类型
		/// </summary>
		public string MsgType { get; set; }

		/// <summary>
		/// 消息类型名称
		/// </summary>
		public string MsgTypeName { get; set; }

		/// <summary>
		/// 标识消息的唯一标识（需配合消息类型）
		/// </summary>
		public string KeyCode { get; set; }

		/// <summary>
		/// 推送时间
		/// </summary>
		public DateTime? SendTime { get; set; }

		/// <summary>
		/// 是否站内已读
		/// </summary>
		public string IsReadOver { get; set; }

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
