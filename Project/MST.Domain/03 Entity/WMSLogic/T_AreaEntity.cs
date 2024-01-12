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
	/// 区域表
	/// </summary>
	public class T_AreaEntity : IEntity<T_AreaEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
	{
		/// <summary>
		/// 区域ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 区域编码
		/// </summary>
		public string AreaCode { get; set; }
		/// <summary>
		/// 区域名称
		/// </summary>
		public string AreaName { get; set; }
		/// <summary>
		/// 货位前缀
		/// </summary>
		public string LocationPrefix { get; set; }
		/// <summary>
		/// 区域类型名称。  巷道类型 Tunnel,密集类型 Concentrate,AGV类型 AGV,平库类型 Flat
		/// </summary>
		public string AreaType { get; set; }

		/// <summary>
		/// 是否主功能区域
		/// </summary>
		public string IsMain { get; set; }

		/// <summary>
		/// 巷道倒序
		/// </summary>
		public string IsTunnelDesc { get; set; }

		/// <summary>
		/// 是否巷道均分
		/// </summary>
		public string IsEvenTunnel { get; set; }

		/// <summary>
		/// 是否ERP仓位做物理区域
		/// </summary>
		public string IsERPPy { get; set; }

		/// <summary>
		/// 是否质检状态做物理区域
		/// </summary>
		public string IsCheckPy { get; set; }

		/// <summary>
		/// 是否基础数据
		/// </summary>
		public string IsBase { get; set; }
		/// <summary>
		/// 是否虚拟区域
		/// </summary>
		public string IsAreaVir { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string Remark { get; set; }
		/// <summary>
		/// 父ID
		/// </summary>
		public string ParentID { get; set; }
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
