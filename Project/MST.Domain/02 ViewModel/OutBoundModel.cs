/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;

namespace MST.Domain.ViewModel
{
  /// <summary>
  /// 出库单
  /// </summary>
  public class OutBoundModel
  {
 
      /// <summary>
        /// 出库单ID
        /// </summary>
		public string F_Id { get; set; }
		
		/// <summary>
        /// 出库单编码
        /// </summary>
		public string OutBoundCode { get; set; }

		/// <summary>
		/// 来源单号
		/// </summary>
		public string RefOrderCode { get; set; }

		/// <summary>
		/// New 新建,   Outing 出库中,   Over 结束
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// 状态名称
		/// </summary>
		public string StateName { get; set; }

		/// <summary>
		/// 是否紧急出库
		/// </summary>
		public string IsUrgent { get; set; }

		/// <summary>
		/// 站台ID
		/// </summary>
		public string StationID { get; set; }

		/// <summary>
		/// ERP 接口,   MAN 手动
		/// </summary>
		public string GenType { get; set; }

		/// <summary>
		/// 指定出库单要出的入库单ID
		/// </summary>
		public string PointInBoundID { get; set; }

		public string PointInBoundCode { get; set; }

		/// <summary>
		/// 质检单ID
		/// </summary>
		public string QAID { get; set; }
		/// <summary>
		/// 质检单编码
		/// </summary>
		public string QACode { get; set; }


		/// <summary>
		/// 生成方式名称
		/// </summary>
		public string GenTypeName { get; set; }

		/// <summary>
		/// GetItemOut 领料出库,WarehouseBackOut 仓退出库,VerBackOut 验退出库
		/// </summary>
		public string OutBoundType { get; set; }

		/// <summary>
		/// 出库单类型名称
		/// </summary>
		public string OutBoundTypeName { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string Remark { get; set; }

		/// <summary>
		/// 接收部门
		/// </summary>
		public string ReceiveDepartmentId { get; set; }
		public string ReceiveDepartment { get; set; }
		/// <summary>
		/// 接收人ID
		/// </summary>
		public string ReceiveUserId { get; set; }
		/// <summary>
		/// 接收人名称
		/// </summary>
		public string ReceiveUserName { get; set; }

		public IList<OutBoundDetailModel> OutBoundDetailList { get; set; }

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
