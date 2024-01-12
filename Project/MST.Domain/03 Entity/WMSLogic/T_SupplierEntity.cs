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
    /// 供应商表
    /// </summary>
    public class T_SupplierEntity : IEntity<T_SupplierEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 生成方式为调用 统一编码生成方法
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// New 新建,   Over 结束（可手动关闭该单据，也可在产生送料单后关闭）
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// ERP内部单据编码
        /// </summary>
        public string IsEnable { get; set; }
        /// <summary>
        /// ERP   接口,   MAN  手动,   Excel  导入
        /// </summary>
        public string SupplierAddr { get; set; }
        /// <summary>
        /// 供应商电话
        /// </summary>
        public string SupplierPho { get; set; }
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
