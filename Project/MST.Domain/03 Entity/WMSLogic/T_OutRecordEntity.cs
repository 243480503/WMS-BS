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
    /// 出库记录表
    /// </summary>
    public class T_OutRecordEntity : IEntity<T_OutRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 出库记录ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 出库单明细ID
        /// </summary>
        public string OutBoundDetailID { get; set; }
        /// <summary>
        /// 出库单ID
        /// </summary>
        public string OutBoundID { get; set; }
        /// <summary>
        /// 出库单类型
        /// </summary>
        public string OutBoundType { get; set; }
        /// <summary>
        /// ERP仓库编码
        /// </summary>
        public string ERPHouseCode { get; set; }
        /// <summary>
        /// 项次
        /// </summary>
        public int? SEQ { get; set; }
        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskID { get; set; }
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
        /// 过期日期
        /// </summary>
        public DateTime? OverdueDate { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }
        /// <summary>
        /// 有效期天数
        /// </summary>
        public int? ValidityDayNum { get; set; }
        /// <summary>
        /// 厂家
        /// </summary>
        public string Factory { get; set; }
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
        /// 容器编码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 容器类型
        /// </summary>
        public string ContainerType { get; set; }
        /// <summary>
        /// 容器类型名称
        /// </summary>
        public string ContainerTypeName { get; set; }
        /// <summary>
        /// 容器大类
        /// </summary>
        public string ContainerKind { get; set; }
        /// <summary>
        /// 过账状态
        /// </summary>
        public string TransState { get; set; }
        /// <summary>
        /// 是否贴标
        /// </summary>
        public string IsItemMark { get; set; }
        /// <summary>
        /// 容器大类名称
        /// </summary>
        public string ContainerKindName { get; set; }
        /// <summary>
        /// 起始地址ID
        /// </summary>
        public string SrcLocationID { get; set; }
        /// <summary>
        /// 起始地址编码
        /// </summary>
        public string SrcLocationCode { get; set; }
        /// <summary>
        /// 目标区域ID
        /// </summary>
        public string TagAreaID { get; set; }
        /// <summary>
        /// 目标地址ID，Station的ID
        /// </summary>
        public string TagLocationID { get; set; }
        /// <summary>
        /// 目标地址的Address（不是站台编码）
        /// </summary>
        public string TagLocationCode { get; set; }
        /// <summary>
        /// 目标地址名称(站台名称)
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 容器当前物料总数量
        /// </summary>
        public decimal? OldQty { get; set; }
        /// <summary>
        /// 应拣数量
        /// </summary>
        public decimal? NeedQty { get; set; }
        /// <summary>
        /// 已拣数量
        /// </summary>
        public decimal? PickedQty { get; set; }
        /// <summary>
        /// 容器当前物料剩余数量
        /// </summary>
        public decimal? AfterQty { get; set; }
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
        /// <summary>
        /// 出库单编码
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 拣选时间
        /// </summary>
        public DateTime? PickDate { get; set; }
        /// <summary>
        /// 拣选人ID
        /// </summary>
        public string PickUserID { get; set; }
        /// <summary>
        /// 拣选人名称
        /// </summary>
        public string PickUserName { get; set; }
        /// <summary>
        /// 是否通过波次指定出库物料
        /// </summary>
        public string IsAuto { get; set; }
        /// <summary>
        /// 是否已到达站台位
        /// </summary>
        public string IsArrive { get; set; }
        /// <summary>
        /// 是否需要回库
        /// </summary>
        public string IsNeedBackWare { get; set; }
        /// <summary>
        /// 回库是否已扫码
        /// </summary>
        public string IsScanBack { get; set; }
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
