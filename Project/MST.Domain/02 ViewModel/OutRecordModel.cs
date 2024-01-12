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
	public class OutRecordModel
	{
		/// <summary>
		/// 出库记录ID
		/// </summary>
		public string F_Id { get; set; }

		/// <summary>
		/// 出库单明细ID
		/// </summary>
		public string OutBoundDetailID { get; set; }

		public string OutBoundID { get; set; }
		public string OutBoundType { get; set; }
		public string OutBoundTypeName { get; set; }
        /// <summary>
        /// ERP仓库编码
        /// </summary>
        public string ERPHouseCode { get; set; }
        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskID { get; set; }

		/// <summary>
		/// 起始地址ID
		/// </summary>
		public string SrcLocationID { get; set; }

		/// <summary>
		/// 起始地址编码
		/// </summary>
		public string SrcLocationCode { get; set; }

		/// <summary>
		/// 生产厂家
		/// </summary>
		public string CreateCompany { get; set; }

		/// <summary>
		/// 生产日期
		/// </summary>
		public DateTime? ProductDate { get; set; }

		public DateTime? OverdueDate { get; set; }

		/// <summary>
		/// 有效期天数
		/// </summary>
		public int? ValidityDayNum { get; set; }

		public string Factory { get; set; }

		/// <summary>
		/// 出库单编码
		/// </summary>
		public string RefOrderCode { get; set; }

		/// <summary>
		/// 单据所需数量
		/// </summary>
		public decimal? OrderQty { get; set; }

		/// <summary>
		/// 已出/总数 
		/// </summary>
		public string ReadyQutAndOrderNeed { get; set; }

		/// <summary>
		/// 目标地址编码
		/// </summary>
		public string TagLocationCode { get; set; }

		/// <summary>
		/// 目标地址ID
		/// </summary>
		public string TagLocaitonID { get; set; }

		/// <summary>
		/// 目标地址名称(站台名称)
		/// </summary>
		public string StationName { get; set; }

		/// <summary>
		/// 目标地址ID(站台ID)
		/// </summary>
		public string StationID { get; set; }

		/// <summary>
		/// 目标区域ID
		/// </summary>
		public string TagAreaID { get; set; }

		/// <summary>
		/// 任务编码
		/// </summary>
		public string TaskNo { get; set; }

		/// <summary>
		/// 容器明细ID
		/// </summary>
		public string ContainerDetailID { get; set; }

		/// <summary>
		/// 容器ID
		/// </summary>
		public string ContainerID { get; set; }

		/// <summary>
		/// 容器大类
		/// </summary>
		public string ContainerKind { get; set; }

		/// <summary>
		/// 容器大类名称
		/// </summary>
		public string ContainerKindName { get; set; }

		/// <summary>
		/// 容器类型
		/// </summary>
		public string ContainerType { get; set; }

		/// <summary>
		/// 容器类型名称
		/// </summary>
		public string ContainerTypeName { get; set; }

		/// <summary>
		/// 货位编码
		/// </summary>
		public string LocationCode { get; set; }

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
		/// 物料贴标条码
		/// </summary>
		public string ItemBarCode { get; set; }

		/// <summary>
		/// 波次ID
		/// </summary>
		public string WaveID { get; set; }

		/// <summary>
		/// 波次编码
		/// </summary>
		public string WaveCode { get; set; }

		/// <summary>
		/// 波次明细ID
		/// </summary>
		public string WaveDetailID { get; set; }

		/// <summary>
		/// 是否贴标
		/// </summary>
		public string IsItemMark { get; set; }


		/// <summary>
		/// 可用数量（初始值）
		/// </summary>
		public decimal? CanUseQty { get; set; }

		/// <summary>
		/// 容器当前物料总数量（初始值）
		/// </summary>
		public decimal? OldQty { get; set; }

		/// <summary>
		/// 应拣数量(或当前操作数量)
		/// </summary>
		public decimal? NeedQty { get; set; }

		/// <summary>
		/// 已拣数量
		/// </summary>
		public decimal? PickedQty { get; set; }

		/// <summary>
		/// 总应拣次数
		/// </summary>
		public decimal? MustTimes { get; set; }

		/// <summary>
		/// 未拣次数
		/// </summary>
		public decimal? NoPickTimes { get; set; }

		/// <summary>
		/// 容器当前物料剩余数量（拣选完成后剩余实物数量）
		/// </summary>
		public decimal? AfterQty { get; set; }

		/// <summary>
		/// 容器条码
		/// </summary>
		public string BarCode { get; set; }

		/// <summary>
		/// 物料单位
		/// </summary>
		public string ItemUnitText { get; set; }

		/// <summary>
		/// 批号
		/// </summary>
		public string Lot { get; set; }
		/// <summary>
		/// 规格
		/// </summary>
		public string Spec { get; set; }
		/// <summary>
		/// 单价
		/// </summary>
		public decimal? Price { get; set; }

		/// <summary>
		/// 供应商ID
		/// </summary>
		public string SupplierID { get; set; }

		/// <summary>
		/// 供应商编码
		/// </summary>
		public string SupplierCode { get; set; }

		/// <summary>
		/// 供应商
		/// </summary>
		public string SupplierName { get; set; }

		/// <summary>
		/// 收货记录ID
		/// </summary>
		public string ReceiveRecordID { get; set; }

		/// <summary>
		/// 是否特殊物料
		/// </summary>
		public string IsSpecial { get; set; }

		/// <summary>
		/// WaitPick 待拣选,   OverPick 已拣选
		/// </summary>
		public string State { get; set; }

		public string StateName { get; set; }

		/// <summary>
		/// 出库单编码
		/// </summary>
		public string OrderCode { get; set; }

		/// <summary>
		/// 拣选时间
		/// </summary>
		public DateTime? PickDate { get; set; }
		public string PickUserName { get; set; }

        /// <summary>
        /// 是否自动指定
        /// </summary>
        public string IsAuto { get; set; }
		/// <summary>
		/// 过账状态 
		/// UnNeedTrans 无需过账
		/// WaittingTrans 待过账
		/// OverTrans 已过账
		/// FailTrans 过账失败
		/// </summary>
		public string TransState { get; set; }
		public string TransStateName { get; set; }

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

