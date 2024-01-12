/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.ViewModel
{
	/// <summary>
	/// 下架单表
	/// </summary>
    public class OffRackModel
    {
		public string F_Id { get; set; }    /// 下架单ID
		public string OffRackCode { get; set; }   /// 下架单编码

												  /// <summary>
												  /// 区域ID
												  /// </summary>
		public string AreaID { get; set; }
		public string AreaType { get; set; }
		public string AreaTypeName { get; set; }    /// 区域类型
													/// Cube 立体库
													/// Flat 平库
		public string State { get; set; }
		public string StateName { get; set; }   /// 下架单状态
												/// New 新建
												/// OffRacking 正在下架
												/// Over 结束
		public string OffRackMethod { get; set; }
		public string OffRackMethodName { get; set; } /// 指定方法
													  /// ByItem 指定物料
													  /// ByLocation 指定货位
		public string RefOrderCode { get; set; }    /// 来源单号
		public string GenType { get; set; }
		public string GenTypeName { get; set; } /// 生成方式
												/// ERP 接口
												/// MAN 手动
		public string Remark { get; set; }  /// 备注
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
