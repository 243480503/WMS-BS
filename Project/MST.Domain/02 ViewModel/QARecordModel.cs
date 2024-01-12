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
	public class QARecordModel
	{
		/// <summary>
		/// 质检记录ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 质检单ID
		/// </summary>
		public string QAID { get; set; }
		/// <summary>
		/// 来源单号
		/// </summary>
		public string RefOrderCode { get; set; }
		/// <summary>
		/// 容器类型
		/// </summary>
		public string ContainerType { get; set; }
		public string ContainerTypeName { get; set; }
		/// <summary>
		/// 厂商
		/// </summary>
		public string Factory { get; set; }
		/// <summary>
		/// 生产日期
		/// </summary>
		public DateTime? ProductDate { get; set; }
		/// <summary>
		/// 过期日期
		/// </summary>
		public DateTime? OverdueDate { get; set; }
		/// <summary>
		/// 有效期天数
		/// </summary>
		public int ValidityDayNum { get; set; }
		/// <summary>
		/// 容器大类
		/// </summary>
		public string ContainerKind { get; set; }
		public string ContainerKindName { get; set; }
		/// <summary>
		/// 质检明细ID
		/// </summary>
		public string QADetailID { get; set; }
		/// <summary>
		/// 库存ID
		/// </summary>
		public string ContainerDetailID { get; set; }
		/// <summary>
		/// 是否贴标
		/// </summary>
		public string IsItemMark { get; set; }
		/// <summary>
		/// 起始地址
		/// </summary>
		public string SrcLocationID { get; set; }
		public string SrcLocationCode { get; set; }
		/// <summary>
		/// 目标地址
		/// </summary>
		public string TagLocationID { get; set; }
		public string TagLocationCode { get; set; }
		public string TagLocationName { get; set; }
		/// <summary>
		/// 质检单据类型（BackSample质检还样，GetSample质检取样）
		/// </summary>
		public string QAOrderType { get; set; }
		/// <summary>
		/// 标签条码
		/// </summary>
		public string ItemBarCode { get; set; }
		/// <summary>
		/// 容器ID
		/// </summary>
		public string ContainerID { get; set; }
		/// <summary>
		/// 过账状态
		/// </summary>
		public string TransState { get; set; }
		/// <summary>
		/// 库存原数量（取样前库存数量）
		/// </summary>
		public decimal? OldQty { get; set; }
		/// <summary>
		/// 波次单
		/// </summary>
		public string WaveID { get; set; }
		public string WaveCode { get; set; }
		public string WaveDetailID { get; set; }
		/// <summary>
		/// 取样数量
		/// </summary>
		public decimal? PickedQty { get; set; }
		/// <summary>
		/// 取样后库存数量
		/// </summary>
		public decimal? AfterQty { get; set; }
		/// <summary>
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }
		/// <summary>
		/// 物料
		/// </summary>
		public string ItemID { get; set; }
		public string ItemName { get; set; }
		public string ItemCode { get; set; }
		public string ItemUnitText { get; set; }
		/// <summary>
		/// 取样总数
		/// </summary>
		public decimal? SampleSumNum { get; set; }
		/// <summary>
		/// 应取样标签个数
		/// </summary>
		public decimal? SampleSumCnt { get; set; }
		/// <summary>
		/// 已取样标签个数
		/// </summary>
		public decimal? ChooseSumCnt { get; set; }
		/// <summary>
		/// 供应商
		/// </summary>
		public string SupplierUserID { get; set; }
		public string SupplierUserName { get; set; }
		/// <summary>
		/// ERP仓
		/// </summary>
		public string ERPHouseCode { get; set; }

		public string CreateCompany { get; set; }
		/// <summary>
		/// 单据需求数量
		/// </summary>
		public decimal? OrderQty { get; set; }

		public string ReadyQutAndOrderNeed { get; set; }

		public int MustTimes { get; set; }
		/// <summary>
		/// 剩余操作次数
		/// </summary>
		public int NoPickTimes { get; set; }
		/// <summary>
		/// 质检站台
		/// </summary>
		public string StationID { get; set; }
		/// <summary>
		/// 批号
		/// </summary>
		public string Lot { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 应拣数量(或当前取样数量)
        /// </summary>
        public decimal? Qty { get; set; }
		/// <summary>
		/// 已还数量
		/// </summary>
		public decimal? ReturnQty { get; set; }
		/// <summary>
		/// 是否需还样
		/// </summary>
		public string IsNeedBack { get; set; }
		/// <summary>
		/// 是否还样完毕
		/// </summary>
		public string IsReturnOver { get; set; }
		/// <summary>
		/// 状态： New 新建，Picking 取样中，Picked 已取样，Returning 还样中，Over 结束
		/// </summary>
		public string State { get; set; }
		public string StateName { get; set; }
		/// <summary>
		/// 是否自动指定
		/// </summary>
		public string IsAuto { get; set; }
		/// <summary>
		/// 是否外观质检
		/// </summary>
		public string IsAppearQA { get; set; }
		public string IsAppearQAStr { get; set; }
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

