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
	/// 下架单表
	/// </summary>
    public class T_OffRackEntity : IEntity<T_OffRackEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
		/// <summary>
		/// 下架单ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 下架单编码
		/// </summary>
		public string OffRackCode { get; set; }

		/// <summary>
		/// 区域ID
		/// </summary>
		public string AreaID { get; set; }
		/// <summary>
		/// 区域类型
		/// </summary>
		public string AreaType { get; set; }
		/// <summary>
		/// 下架单状态。New 新建，OffRacking 正在下架,Over 结束
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// 指定方法。ByItem 指定物料，ByLocation 指定货位
		/// </summary>
		public string OffRackMethod { get; set; }
		/// <summary>
		/// 来源单号
		/// </summary>
		public string RefOrderCode { get; set; }
		/// <summary>
		/// 生成方式。ERP 接口，MAN 手动
		/// </summary>
		public string GenType { get; set; }
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
