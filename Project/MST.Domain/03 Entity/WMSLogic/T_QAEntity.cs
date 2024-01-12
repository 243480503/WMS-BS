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
  /// 质检单表
  /// </summary>
  public class T_QAEntity : IEntity<T_QAEntity>, ICreationAudited, IModificationAudited, IDeleteAudited , IWMSEntity
  {
		/// <summary>
		/// 质检单ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 质检单编码
		/// </summary>
		public string QACode { get; set; }
		/// <summary>
		/// 质检单类型
		/// GetSample	抽检单
		/// BackSample	还样单
		/// </summary>
		public string QAOrderType { get; set; }
		/// <summary>
		/// 单据状态
		/// New			新建
		/// WavedPart	部分波次
		/// Waved		波次完成
		/// Outing		出库中
		/// Picking		取样中
		/// Picked		已取样
		///	WaitReturn	待还样
		///	Returning	还样中
		/// WaitResult	待录入结果
		/// Over		结束
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// 站台ID
		/// </summary>
		public string StationID { get; set; }
		/// <summary>
		/// 过账状态
		/// New		新建
		/// UnNeedTrans	免过账
		/// WaittingTrans	待过账
		/// Traning	过账中
		/// OverTrans	已过账
		/// FailTrans	过账失败
		/// </summary>
		public string TransState { get; set; }
		/// <summary>
		/// 来源单号(取样单为ERP来源单号，还样单为取样单的WMS单据编码)
		/// </summary>
		public string RefOrderCode { get; set; }

		/// <summary>
		/// 对应的来源入库单
		/// </summary>
		public string RefInBoundCode { get; set; }
		/// <summary>
		/// 生成方式
		/// ERP  接口
		/// MAN 手动
		/// Excel 表格导入
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
