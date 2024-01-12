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
    /// 收货记录表
    /// </summary>
    public class T_ReceiveRecordEntity : IEntity<T_ReceiveRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 收货记录ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 入库单ID
        /// </summary>
        public string InBoundID { get; set; }
        /// <summary>
        /// 入库单明细ID
        /// </summary>
        public string InBoundDetailID { get; set; }

        /// <summary>
        /// 入库明细项次
        /// </summary>
        public int? SEQ { get; set; }
        /// <summary>
        /// 收货站台ID
        /// </summary>
        public string ReceiveStaionID { get; set; }
        /// <summary>
        /// 容器类型
        /// </summary>
        public string ContainerType { get; set; }
        /// <summary>
        /// 容器大类
        /// </summary>
        public string ContainerKind { get; set; }
        /// <summary>
        /// 质检状态。Qua 合格，UnQua 不合格，UnNeed 免检，WaitCheck 待检
        /// </summary>
        public string CheckState { get; set; }
        /// <summary>
        /// 存放区域
        /// </summary>
        public string AreaID { get; set; }
        /// <summary>
        /// ERP仓库编码
        /// </summary>
        public string ERPWarehouseCode { get; set; }
        /// <summary>
        /// 容器条码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 标签条码。若是纸箱，此处填写纸箱容器条码
        /// </summary>
        public string ItemBarCode { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemID { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 是否贴标
        /// </summary>
        public string IsItemMark { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 有效期天数
        /// </summary>
        public int? ValidityDayNum { get; set; }
        /// <summary>
        /// 是否已扫码
        /// </summary>
        public string IsScanOver { get; set; }
        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime? OverdueDate { get; set; }
        /// <summary>
        /// 物料数量
        /// </summary>
        public decimal? Qty { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string ItemUnitText { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string SupplierUserID { get; set; }
        /// <summary>
        /// 收货人ID
        /// </summary>
        public string DoneUserID { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string DoneUserName { get; set; }
        /// <summary>
        /// 货位ID
        /// </summary>
        public string LocationID { get; set; }
        /// <summary>
        /// 货位编码
        /// </summary>
        public string LocationCode { get; set; }
        /// <summary>
        /// 收货状态
        /// NewGroup  新组货(指扫码入料箱的初始状态)
        /// LockOver 已封箱(指料箱已经不再继续放料)
        /// PutawayOver 已上架(指已将库存放到货架)
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 过账状态。UnNeedTrans 无需过账，WaittingTrans 待过账，OverTrans 已过账,FailTrans 过账失败
        /// </summary>
        public string TransState { get; set; }
        /// <summary>
        /// 过账失败原因
        /// </summary>
        public string FailDesc { get; set; }
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
