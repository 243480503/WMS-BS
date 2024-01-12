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
  /// 物料种类表
  /// </summary>
  public class T_ItemKindEntity : IEntity<T_ItemKindEntity>, ICreationAudited, IModificationAudited, IDeleteAudited , IWMSEntity
  {
		/// <summary>
		/// 物料种类ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
        /// 物料种类编码
        /// </summary>
		public string KindCode { get; set; }
		/// <summary>
        /// 物料种类名称
        /// </summary>
		public string KindName { get; set; }
		/// <summary>
        /// ParentID = 0 表示顶级
        /// </summary>
		public string ParentID { get; set; }
		/// <summary>
		/// 是否基础数据
		/// </summary>
		public string IsBase { get; set; }
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
