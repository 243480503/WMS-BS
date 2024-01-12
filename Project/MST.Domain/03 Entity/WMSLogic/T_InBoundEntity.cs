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
    /// 产生方式：
    ///1、自动创建（收货完成后，通过收货记录产生送料单表，并通过物料数量是否匹配，产生采购入库单）
    ///2、接口产生
    /// </summary>
    public class T_InBoundEntity : IEntity<T_InBoundEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 入库单ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 采购入库单编码
        /// </summary>
        public string InBoundCode { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string InBoundType { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string SupplierUserID { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierUserName { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierUserCode { get; set; }
        /// <summary>
        /// 站台ID
        /// </summary>
        public string StationID { get; set; }
        /// <summary>
        /// 入库单状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 过账状态
        /// </summary>
        public string TransState { get; set; }
        /// <summary>
        /// 来源单号
        /// </summary>
        public string RefOrderCode { get; set; }

        /// <summary>
        /// ERP来源单号对应ERP的入库单号，用于ERP指定入库单号进行出库(没有则与来源单号一致)
        /// </summary>
        public string ERPInDocCode { get; set; }
        /// <summary>
        /// ERP 接口,   MAN 手动
        /// </summary>
        public string GenType { get; set; }
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
