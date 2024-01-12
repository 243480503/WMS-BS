/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class PrintItemBarCodeModel
	{
		public string InBoundDetailID { get; set; }

		/// <summary>
		/// 物料编码
		/// </summary>
		public string ItemCode { get; set; }

		/// <summary>
		/// 物料名称
		/// </summary>
		public string ItemName { get; set; }

		/// <summary>
		/// 生产厂家
		/// </summary>
		public string CreateCompany { get; set; }

		/// <summary>
		/// 批号
		/// </summary>
		public string Lot { get; set; }

		/// <summary>
		/// 生产日期
		/// </summary>
		public DateTime? ProductDate { get; set; }

		/// <summary>
		/// 失效日期
		/// </summary>
		public DateTime? OverdueDate { get; set; }

		/// <summary>
		/// 单位
		/// </summary>
		public string ItemUnitText { get; set; }

		/// <summary>
		/// 单位数量
		/// </summary>
		public decimal? UnitQty { get; set; }

		/// <summary>
		///规格
		/// </summary>
		public string Spec { get; set; }

		/// <summary>
		/// 入库总数
		/// </summary>
		public decimal? Qty { get; set; }

		/// <summary>
		/// 起始编码
		/// </summary>
		public string BeginNum { get; set; }

		/// <summary>
		/// 打印张数
		/// </summary>
		public int? PrintNum { get; set; }

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
