/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.ViewModel;
using System;

namespace MST.Domain.Entity.WMSLogic
{
    /// <summary>
    /// 物料表
    /// </summary>
    public class T_ItemEntity : IEntity<T_ItemEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 物料ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 物料种类ID
        /// </summary>
        public string ItemKindID { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 物料单位
        /// </summary>
        public string ItemUnitText { get; set; }

        
        /// <summary>
        /// 物料种类编码
        /// </summary>
        public string KindCode { get; set; }
        /// <summary>
        /// 物料种类名称
        /// </summary>
        public string KindName { get; set; }    
        /// <summary>
        /// 预警天数
        /// </summary>
        public int? ValidityWarning { get; set; }

        /// <summary>
        /// 自定义入库规则(不填则按默认入库规则)
        /// </summary>
        public string CustomizeInRule { get; set; }

        /// <summary>
        /// 自定义入库规则优先级(同容器多规则优先级，值越大优先级越大)
        /// </summary>
        public int? CustomizeInRulePriority { get; set; }
        /// <summary>
        /// 预警数量
        /// </summary>
        public decimal? WarningQty { get; set; }
        /// <summary>
        /// 最大库存
        /// </summary>
        public decimal? MaxQty { get; set; }
        /// <summary>
        /// 保留库存（非紧急出库不可使用）
        /// </summary>
        public decimal? MinQty { get; set; }
        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// ERP仓位编码
        /// </summary>
        public string ERPWarehouseCode { get; set; }

        /// <summary>
        /// 是否防潮
        /// </summary>
        public string IsDampproof { get; set; }
        /// <summary>
        /// 是否贴标(纸箱不可对单独物料贴标，只有料箱和料架可以对单独物料贴标)
        /// </summary>
        public string IsItemMark { get; set; }
        /// <summary>
        /// 强制批号控制
        /// </summary>
        public string IsMustLot { get; set; }
        /// <summary>
        /// 是否允许混批
        /// </summary>
        public string IsMixLot { get; set; }
        /// <summary>
        /// 是否允许混料
        /// </summary>
        public string IsMixItem { get; set; }
        /// <summary>
        /// 是否允许合格不合格混放
        /// </summary>
        public string IsMixQA { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 入库容器类型
        /// </summary>
        public string ContainerType { get; set; }
        /// <summary>
        /// 有效期时长（年、月、天），0为永不过期
        /// </summary>
        public int? ValidityDayNum { get; set; }

        /// <summary>
        /// 过期时间单位(年、月、日)
        /// </summary>
        public string ValidityUnitType { get; set; }
        /// <summary>
        /// 单位数量(包装数量)
        /// </summary>
        public decimal? UnitQty { get; set; }
        /// <summary>
        /// 来料抽检百分比
        /// </summary>
        public decimal? CheckPerc { get; set; }
        /// <summary>
        /// 每箱抽检百分比
        /// </summary>
        public decimal? CheckBoxPerc { get; set; }
        /// <summary>
        /// 是否需质检(ERP传单时，已有质检结果，但在自建单时需用到此字段)
        /// </summary>
        public string IsNeedCheck { get; set; }
        /// <summary>
        /// 码垛类型
        /// </summary>
        public string StackType { get; set; }
        /// <summary>
        /// 是否特殊物料
        /// </summary>
        public string IsSpecial { get; set; }
        /// <summary>
        /// 是否破坏性质检
        /// </summary>
        public string IsBroken { get; set; }

        /// <summary>
        /// 是否靠窗
        /// </summary>
        public string IsPriority { get; set; }
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
