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
	/// 定时任务表
	/// </summary>
	public class T_TimerEntity : IEntity<T_TimerEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
	{
		/// <summary>
		/// 定时任务ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 任务名称
		/// </summary>
		public string TimerName { get; set; }
		/// <summary>
		/// 描述
		/// </summary>
		public string Remark { get; set; }
		/// <summary>
		/// 是否更改（已更改，定时器需要重新开始）
		/// </summary>
		public string IsChange { get; set; }
		/// <summary>
		/// 开始时间
		/// </summary>
		public DateTime? BeginTime { get; set; }
		/// <summary>
		/// 结束时间
		/// </summary>
		public DateTime? EndTime { get; set; }
		/// <summary>
		/// 已执行次数
		/// </summary>
		public int? OverCount { get; set; }
		/// <summary>
		/// 最大执行次数
		/// </summary>
		public int? MaxCount { get; set; }
		/// <summary>
		/// 频率(秒)
		/// </summary>
		public int? Rate { get; set; }
		/// <summary>
		/// 是否基础数据
		/// </summary>
		public string IsBase { get; set; }
		/// <summary>
		/// 任务器编码
		/// </summary>
		public string TimerCode { get; set; }
		/// <summary>
		/// 是否启用
		/// </summary>
		public string IsEnable { get; set; }
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
