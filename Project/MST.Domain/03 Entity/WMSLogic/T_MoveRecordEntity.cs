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
    /// 移库记录表
    /// </summary>
    public class T_MoveRecordEntity : IEntity<T_MoveRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 移库记录ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 移库单ID
        /// </summary>
        public string MoveID { get; set; }
        /// <summary>
        /// 容器ID
        /// </summary>
        public string ContainerID { get; set; }
        /// <summary>
        /// 容器条码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// New 新建,   Outing 出库中,   Over 结束
        /// </summary>
        public string SrcLocationID { get; set; }
        /// <summary>
        /// ERP 接口,   MAN 手动
        /// </summary>
        public string SrcLocationCode { get; set; }
        /// <summary>
        /// 目标货位ID
        /// </summary>
        public string TagLocationID { get; set; }
        /// <summary>
        /// 目标货位编码
        /// </summary>
        public string TagLocationCode { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemID { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 移库创建方式（手动指定目标货位PointTag,手动指定源货位PointSource,自动生成Auto）
        /// </summary>
        public string GenType { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        public string Spec { get; set; }
        public string ItemUnitText { get; set; }
        public DateTime? OverdueDate { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireDate { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string SupplierID { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 入库单ID
        /// </summary>
        public string InBoundID { get; set; }
        /// <summary>
        /// 入库单明细ID
        /// </summary>
        public string InBoundDetailID { get; set; }
        /// <summary>
        /// 收货记录ID
        /// </summary>
        public string ReceiveRecordID { get; set; }
        /// <summary>
        /// 移库数量
        /// </summary>
        public decimal? Qty { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 容器大类
        /// </summary>
        public string ContainerKind { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? OverTime { get; set; }

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
