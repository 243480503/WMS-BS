/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.WMSLogic;
using System;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 出入库流水表
    /// </summary>
    public class InOutDetailModel
    {
        /// <summary>
        /// 库存流水ID
        /// </summary>
		public string F_Id { get; set; }

        /// <summary>
        /// 库存明细ID
        /// </summary>
        public string ContainerDetailID { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemID { get; set; }

        /// <summary>
        /// 容器ID
        /// </summary>
        public string ContainerID { get; set; }

        /// <summary>
        /// Box      纸箱,   Plastic  料箱
        /// </summary>
        public string ContainerType { get; set; }
        public string ContainerTypeName { get; set; }
        public string ContainerKind { get; set; }
        public string ContainerKindName { get; set; }
        /// <summary>
        /// 货位ID
        /// </summary>
        public string LocationID { get; set; }

        /// <summary>
        /// 入库单编码
        /// </summary>
        public string InBoundCode { get; set; }

        /// <summary>
        /// 入库单来源编码
        /// </summary>
        public string InBoundRefCode { get; set; }

        /// <summary>
		/// ERP来源单号对应ERP的入库单号，用于ERP指定入库单号进行出库(没有则与来源单号一致)
		/// </summary>
		public string ERPInDocCode { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }

        public int? InBoundSEQ { get; set; }

        /// <summary>
        /// 容器条码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 货位编码
        /// </summary>
        public string LocationNo { get; set; }

        /// <summary>
        /// 区域ID
        /// </summary>
        public string AreaID { get; set; }

        /// <summary>
        /// 区域编码
        /// </summary>
        public string AreaCode { get; set; }

        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 物料种类编码
        /// </summary>
        public string KindCode { get; set; }

        /// <summary>
        /// 物料种类名称
        /// </summary>
        public string KindName { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 贴标条码
        /// </summary>
        public string ItemBarCode { get; set; }

        /// <summary>
        /// 不包含  待出库数量 和 取样数量
        /// </summary>
        public decimal? Qty { get; set; }

        /// <summary>
        /// 待出库数量
        /// </summary>
        public decimal? OutQty { get; set; }

        /// <summary>
        /// 取样数量
        /// </summary>
        public decimal? CheckQty { get; set; }

        /// <summary>
        /// 物料单位
        /// </summary>
        public string ItemUnitText { get; set; }

        /// <summary>
        /// Qua 合格,   UnQua 不合格,   UnNeed  免检,   WaitCheck 待检
        /// </summary>
        public string CheckState { get; set; }

        /// <summary>
        /// 质检单明细ID
        /// </summary>
        public string CheckDetailID { get; set; }

        /// <summary>
        /// 质检单ID
        /// </summary>
        public string CheckID { get; set; }

        /// <summary>
        /// Normal  正常,   Freeze  冻结 ，仅表示手动操作造成的冻结 。 (质检造成的冻结请用IsCheckFreeze，盘点造成的冻结请用IsCountFreeze)
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// 是否质检冻结
        /// </summary>
        public string IsCheckFreeze { get; set; }

        /// <summary>
        /// 是否盘点冻结
        /// </summary>
        public string IsCountFreeze { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? OverdueDate { get; set; }

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
        /// OutType 出库,   InType   入库
        /// </summary>
        public string InOutType { get; set; }
        public string InOutTypeName { get; set; }
        public string OrderType { get; set; }
        public string OrderTypeName { get; set; }   /// 单据类型
                                                    /// PurchaseIn  采购入库单
                                                    /// WaitCheckType 待检入库单
                                                    /// BackSample 质检还样单
                                                    /// GetSample 质检取样单
                                                    /// Count	盘点单
                                                    /// GetItemOut 领料出库单
                                                    /// WarehouseBackOut 仓退出库单
                                                    /// VerBackOut 验退出库单
                                                    /// EmptyIn 空料箱入库
                                                    /// EmptyOut 空料箱出库
                                                    /// <summary>
                                                    /// 变动数量（出库为负）
                                                    /// </summary>
        public decimal? ChangeQty { get; set; }

        /// <summary>
        /// 变动后数量
        /// </summary>
        public decimal? AfterChangeQty { get; set; }

        /// <summary>
        /// 任务编码
        /// </summary>
        public string TaskNo { get; set; }

        /// <summary>
        /// 单据ID
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// 单据明细ID
        /// </summary>
        public string OrderDetailID { get; set; }
        /// <summary>
        /// 引起变动的单据Code
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 引起变动的来源单据Code
        /// </summary>
        public string OrderRefCode { get; set; }

        /// <summary>
        /// 引起变动的单据明细项次
        /// </summary>
        public int? OrderSEQ { get; set; }

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
