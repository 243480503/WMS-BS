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
  /// 标签记录表
  /// </summary>
  public class T_MarkRecordEntity : IEntity<T_MarkRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited , IWMSEntity
  {
		/// <summary>
		/// 标签记录ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
        /// 打印规则ID
        /// </summary>
		public string MarkRuleID { get; set; }
		/// <summary>
        /// 标签编码(纸箱代表容器编码，料箱或料架代表子编码)
        /// </summary>
		public string BarCode { get; set; }
		/// <summary>
        /// 供应商编码
        /// </summary>
		public string SupplierCode { get; set; }
		/// <summary>
        /// 供应商名称
        /// </summary>
		public string SupplierName { get; set; }
		/// <summary>
		/// 物料ID
		/// </summary>
		public string ItemID { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string ItemCode { get; set; }
		/// <summary>
		/// 物料名称
		/// </summary>
		public string ItemName { get; set; }
		/// <summary>
		/// 是否手动打印（仅纸箱散件入库时为true，料箱和料架不论是否打印子标签均默认为false）
		/// </summary>
		public string IsHandPrint { get; set; }
		/// <summary>
		/// 是否使用
		/// </summary>
		public string IsUsed { get; set; }
		/// <summary>
        /// 物料数量
        /// </summary>
		public decimal? Qty { get; set; }
		/// <summary>
        /// 批号
        /// </summary>
		public string Lot { get; set; }
		/// <summary>
        /// 补打张数(不算初次打印)
        /// </summary>
		public int? RepairPicNum { get; set; }
		/// <summary>
        /// 打印张数(算上补打次数)
        /// </summary>
		public int? PicNum { get; set; }
		/// <summary>
        /// 失败原因
        /// </summary>
		public string FailMsg { get; set; }
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
