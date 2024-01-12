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
    /// 盘点审核结果表
    /// </summary>
    public class T_CountResultEntity : IEntity<T_CountResultEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 盘点审核结果ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 来源单编码
        /// </summary>
        public string RefOrderCode { get; set; }
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
        /// 盘点审核结果
        /// </summary>
        public string CountResult { get; set; }
        /// <summary>
        /// ERP仓编码
        /// </summary>
        public string ERPWarehouseCode { get; set; }
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
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime? AccessTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 是否已应用该结果
        /// </summary>
        public string IsUsed { get; set; }
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
