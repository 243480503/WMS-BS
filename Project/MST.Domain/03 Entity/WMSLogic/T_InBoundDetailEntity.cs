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
    /// 入库单明细
    /// </summary>
    public class T_InBoundDetailEntity : IEntity<T_InBoundDetailEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 入库单明细ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 入库单ID
        /// </summary>
        public string InBoundID { get; set; }
        /// <summary>
        /// 采购单项次
        /// </summary>
        public int? SEQ { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemID { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 过账状态
        /// </summary>
        public string TransState { get; set; }
        /// <summary>
        /// 有效期天数(为0表示永不过期)
        /// </summary>
        public int? ValidityDayNum { get; set; }
        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime? OverdueDate { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string ItemUnitText { get; set; }
        /// <summary>
        /// 已入库数量
        /// </summary>
        public decimal? OverInQty { get; set; }
        /// <summary>
        /// 质检状态 ： Qua 合格，UnQua 不合格，UnNeed 免检，WaitCheck 待检
        /// </summary>
        public string CheckState { get; set; }
        /// <summary>
        /// ERP仓库编码
        /// </summary>
        public string ERPWarehouseCode { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }
        /// <summary>
        /// 是否散箱模式
        /// </summary>
        public string IsSplitModel { get; set; }
        /// <summary>
        /// 入库站台
        /// </summary>
        public string StationID { get; set; }
        /// <summary>
        /// 存储区域ID
        /// </summary>
        public string StoreAreaID { get; set; }
        /// <summary>
        /// 总数量
        /// </summary>
        public decimal? Qty { get; set; }
        /// <summary>
        /// 当前已收数量
        /// </summary>
        public decimal? CurQty { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// 是否收货数量必须一致
        /// </summary>
        public string IsMustQtySame { get; set; }

        /// <summary>
        /// 执行方式（手动、设备）
        /// </summary>
        public string ActionType { get; set; }
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
