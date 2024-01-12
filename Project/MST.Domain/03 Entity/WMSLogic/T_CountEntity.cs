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
  /// 盘点单表
  /// </summary>
  public class T_CountEntity : IEntity<T_CountEntity>, ICreationAudited, IModificationAudited, IDeleteAudited , IWMSEntity
  {
		/// <summary>
		/// 盘点单ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 盘点单编码
		/// </summary>
		public string CountCode { get; set; }
		/// <summary>
		/// 区域类型		Cube 立体库,	Flat 平库
		/// </summary>
		public string AreaType { get; set; }
		/// <summary>
		/// ERP仓库
		/// </summary>
		public string ERPHouseCode { get; set; }
		/// <summary>
		/// 盘点模式		GoodsToPeople 货到人,	PeopleToGoods 人到货
		/// </summary>
		public string CountMode { get; set; }
		/// <summary>
		/// 盘点方法		ByItem 指定物料,		ByLocation 指定货位
		/// </summary>
		public string CountMethod { get; set; }
		/// <summary>
		/// 盘点站台ID
		/// </summary>
		public string StationID { get; set; }
		/// <summary>
		/// 是否明盘
		/// </summary>
		public string IsOpen { get; set; }
		/// <summary>
		/// 盘点单状态		New 新建,	Counting 盘点中,		Over 结束
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// 盘点单审核状态	WaitAudit 待审核,	Auditing 审核中,		Pass 通过,	UnPass 不通过
		/// </summary>
		public string AuditState { get; set; }
		/// <summary>
		/// 确认审核状态		Applied  已确认,	WaitApply 未确认
		/// </summary>
		public string AuditResult { get; set; }
		/// <summary>
		/// 来源单号
		/// </summary>
		public string RefOrderCode { get; set; }
		/// <summary>
		/// 生成方式		ERP 接口,	MAN 手动
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
