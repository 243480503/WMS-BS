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
    /// 盘点记录表
    /// </summary>
    public class T_CountRecordEntity : IEntity<T_CountRecordEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 盘点记录ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 项次
        /// </summary>
		public int? SEQ { get; set; }
        /// <summary>
        /// 盘点单ID
        /// </summary>
		public string CountID { get; set; }
        /// <summary>
        /// 盘点单明细ID
        /// </summary>
		public string CountDetailID { get; set; }
        /// <summary>
        /// 容器ID
        /// </summary>
		public string ContainerID { get; set; }
        /// <summary>
        /// 库存明细ID
        /// </summary>
		public string ContainerDetailID { get; set; }
        /// <summary>
        /// 盘点站台ID
        /// </summary>
		public string StationID { get; set; }
        /// <summary>
        /// 容器条码
        /// </summary>
		public string BarCode { get; set; }
        /// <summary>
        /// 贴标条码
        /// </summary>
		public string ItemBarCode { get; set; }
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
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 过账状态    UnNeedTrans 免过账,    WaittingTrans 待过帐,  OverTrans 已过帐,  FailTrans 过账失败
        /// </summary>
        public string TransState { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string SupplierUserID { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
		public string SupplierUserName { get; set; }
        /// <summary>
        /// 货位ID
        /// </summary>
		public string LocationID { get; set; }
        /// <summary>
        /// 货位编码
        /// </summary>
		public string LocationCode { get; set; }
        /// <summary>
        /// 目标地址，T_Station的ID
        /// </summary>
        public string TagLocationID { get; set; }
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
        /// 失效日期
        /// </summary>
        public DateTime? OverdueDate {  get; set; }
        /// <summary>
        /// 是否物料贴标
        /// </summary>
        public string IsItemMark { get; set; }

        /// <summary>
        /// ERP仓库编码
        /// </summary>
        public string ERPCode { get; set; }
        /// <summary>
        /// 盘点数量
        /// </summary>
        public decimal? CountQty { get; set; }
        /// <summary>
        /// 原数量
        /// </summary>
		public decimal? Qty { get; set; }
        /// <summary>
        /// 是否出库盘点
        /// </summary>
		public string IsOutCount { get; set; }
        /// <summary>
        /// 盘点实际箱码
        /// </summary>
		public string FactBarCode { get; set; }
        /// <summary>
        /// 盘点结果
        /// Inner_SameBoxCode 箱码一致
        /// Inner_DiffBoxCode 箱码不一致
        /// Inner_MoreBoxCode 多余箱码
        /// Inner_Empty 正常空货位
        /// Inner_NotFindBoxCode 未找到箱码
        /// 
        /// Outer_Normal 正常
        /// Outer_MoreQty 多余数量
        /// Outer_LessQty 少数量 （未盘点到的一律是少数量，需删除库存）
        /// Outer_MoreItemBarcode 多标签（需新增库存）
        /// </summary>
        public string CountResult { get; set; }
        /// <summary>
        /// 盘点状态
        /// New 新建
        /// Counting 盘点中
        /// NoNeed 免盘
        /// Over 结束
        /// </summary>
        public string CountState { get; set; }
        /// <summary>
        /// 审核状态
        /// WaitAudit 待审核
        /// Auditing 审核中
        /// Pass  通过
        /// UnPass 不通过
        /// </summary>
        public string AuditState { get; set; }
        /// <summary>
        /// 生成方式    Auto 自动,    MAN 手动
        /// </summary>
        public string GenType { get; set; }
        /// <summary>
        /// 是否虚拟物料条码
        /// </summary>
        public string IsVirItemBarCode { get; set; }
        /// <summary>
        /// 是否是新增记录（新增库存时1）
        /// </summary>
		public string IsAdd { get; set; }
        /// <summary>
        /// 是否已更新库存
        /// </summary>
        public string IsUpdate { get; set; }
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
