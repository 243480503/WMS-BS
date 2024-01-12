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
    public class QADetailModel
    {
		/// <summary>
		/// 质检单明细ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 项次
		/// </summary>
		public int? SEQ { get; set; }
		/// <summary>
		/// 质检单ID
		/// </summary>
		public string QAID { get; set; }
		/// <summary>
		/// 物料ID
		/// </summary>
		public string ItemID { get; set; }
		/// <summary>
		/// 物料名称
		/// </summary>
		public string ItemName { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string ItemCode { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string SupplierID { get; set; }
		public string SupplierUserID { get; set; }
		/// <summary>
		/// 供应商编码
		/// </summary>
		public string SupplierCode { get; set; }
		/// <summary>
		/// 供应商名称
		/// </summary>
		public string SupplierUserName { get; set; }
		public string SupplierName { get; set; }
		/// <summary>
		/// 质检结果状态(New 新建,ERP 接口录入,MAN 人工录入)
		/// </summary>
		public string ResultState { get; set; }
        public string ResultStateName { get; set; }
		/// <summary>
		/// 质检站台ID
		/// </summary>
		public string StationID { get; set; }
		/// <summary>
		/// 批号
		/// </summary>
		public string Lot { get; set; }
        public string Spec { get; set; }
        public string ItemUnitText { get; set; }
        public DateTime? OverdueDate { get; set; }
        /// <summary>
        /// 质检结果
        /// Qua 合格,   UnQua 不合格
        /// </summary>
        public string QAResult { get; set; }
		/// <summary>
		/// 是否破坏性质检
		/// </summary>
		public string IsBroken { get; set; }
		/// <summary>
		/// 是否外观质检
		/// </summary>
		public string IsAppearQA { get; set; }
		public string IsAppearQAStr { get; set; }
		/// <summary>
		/// 波次数量
		/// </summary>
		public decimal? WaveQty { get; set; }
		/// <summary>
		/// 已出数量
		/// </summary>
		public decimal? OutQty { get; set; }
		/// <summary>
		/// 取样总数
		/// </summary>
		public decimal? SampleSumNum { get; set; }
		/// <summary>
		/// 取样标签个数
		/// </summary>
		public decimal? SampleSumCnt { get; set; }
		/// <summary>
		/// 结果推送时间
		/// </summary>
		public DateTime? ResultSendTime { get; set; }

		/// <summary>
		/// 执行方式（手动、设备）
		/// </summary>
		public string ActionType { get; set; }
		/// <summary>
		/// 取样方式
		/// </summary>
		public string SampleType { get; set; }
		/// <summary>
		/// 单据状态
		/// New 新建;	Picking 取样中;	 Picked 已取样;	Returning 还样中;	Over 结束	
		/// </summary>
		public string State { get; set; }
		public string StateName { get; set; }
		public bool? F_DeleteMark { get; set; } /// 是否删除
		public DateTime? F_CreatorTime { get; set; }    /// 创建时间
		public string F_CreatorUserId { get; set; }/// 创建人ID
		public string CreatorUserName { get; set; } /// 创建人名称
		public string F_DeleteUserId { get; set; }  /// 删除操作人
		public DateTime? F_DeleteTime { get; set; } /// 删除操作时间
		public string DeleteUserName { get; set; }  /// 删除操作人名称
		public DateTime? F_LastModifyTime { get; set; } /// 修改时间
		public string F_LastModifyUserId { get; set; }  /// 修改人ID
		public string ModifyUserName { get; set; }  /// 修改人名称
													/// <summary>
													/// 来源入库单编码
													/// </summary>
		public string RefInBoundCode { get; set; }
		public string RefInBoundID { get; set; }


		public string IsCanSetResult { get; set; } /// 是否可以录入结果
        public string TaskID { get; set; }
        public string TaskNo { get; set; }
        public string ContainerType { get; set; }
        public string ContainerTypeName { get; set; }
        public string ContainerKind { get; set; }
        public string ContainerKindName { get; set; }
        public string SrcLocationID { get; set; }
        public string SrcLocationCode { get; set; }
        public string TagLocationID { get; set; }
        public string TagLocationCode { get; set; }
        public string TagLocationName { get; set; }
        public decimal? OldQty { get; set; }
        public decimal? PickedQty { get; set; }
        public decimal? QtySum { get; set; }    /// 库存总数量(前端展示且移动到左侧时需要)
        public string TagAddressCode { get; set; }
        public string QAResultName { get; set; }
        public string QACode { get; set; }
        public string StationName { get; set; }
        public string StationCode { get; set; }
	}
}
