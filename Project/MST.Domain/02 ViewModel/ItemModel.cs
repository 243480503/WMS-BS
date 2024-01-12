/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class ItemModel
    {
        public string F_Id { get; set; }    /// 物料ID
        public string ItemKindID { get; set; }  /// 物料种类ID
        public string ItemCode { get; set; }    /// 物料编码
        public string ItemName { get; set; }    /// 物料名称
        public string ItemUnitText { get; set; }    /// 物料单位
        public string KindCode { get; set; }    /// 物料种类编码
        public string KindName { get; set; }    /// 物料种类名称
        public decimal? WarningQty { get; set; }    /// 预警数量
        public decimal? MaxQty { get; set; } /// 最大库存
        public decimal? MinQty { get; set; }    /// 保留库存（非紧急出库不可使用）
        public string IsBase { get; set; }  /// 是否基础数据
                                            /// 规格
        public string Spec { get; set; }

        /// <summary>
        /// 入库容器类型
        /// </summary>
        public string ContainerType { get; set; }
        public string ContainerTypeName { get; set; }
        public string ContainerKind { get; set; }
        public string ContainerKindName { get; set; }

        /// <summary>
        /// 是否防潮
        /// </summary>
        public string IsDampproof { get; set; }


        /// <summary>
        /// 是否贴标
        /// </summary>
        public string IsItemMark { get; set; }
        public int? ValidityWarning { get; set; }
        public string ERPWarehouseCode { get; set; }    /// ERP仓位编码
        public string ERPWarehouseName { get; set; }

        /// <summary>
        /// 自定义入库规则(不填则按默认入库规则)
        /// </summary>
        public string CustomizeInRule { get; set; }

        /// <summary>
        /// 自定义入库规则优先级(同容器多规则优先级，值越大优先级越大)
        /// </summary>
        public int? CustomizeInRulePriority { get; set; }
        public string IsMustLot { get; set; }   /// 强制批号控制
        public string IsMixLot { get; set; }    /// 是否允许混批
        public string IsMixItem { get; set; }    /// 是否允许混料
        public string IsMixQA { get; set; } /// 是否允许合格不合格混放
        public IList<T_ItemInStationEntity> InStationList { get; set; } /// 入库站台
        public T_ItemInStationEntity InStation { get; set; }    /// 默认入库站台
        public IList<T_ItemAreaEntity> StoredAreaList { get; set; } /// 存放区域
        public T_ItemAreaEntity StoredArea { get; set; }    /// 默认存放区
        public string Factory { get; set; } /// 生产厂家
        public int? ValidityDayNum { get; set; }    /// 有效期时长(年、月、日)

        /// <summary>
        /// 有效期时间单位(Year、Month、Day)
        /// </summary>
        public string ValidityUnitType { get; set; }

        /// <summary>
        /// 有效期时间单位(年、月、日)
        /// </summary>
        public string ValidityUnitTypeName { get; set; }
        public decimal? UnitQty { get; set; }   /// 单位数量(包装数量)
        public decimal? CheckPerc { get; set; } /// 来料抽检百分比
        public decimal? CheckBoxPerc { get; set; }  /// 每箱抽检百分比
        public string IsNeedCheck { get; set; } /// 是否需要质检
        public string StackType { get; set; }   /// 码垛类型
        public string IsSpecial { get; set; }   /// 是否特殊物料
        public string IsBroken { get; set; }    /// 是否破坏性质检

        /// <summary>
        /// 是否优先(因区域中的货位，还存在以物料区分的优先货位)
        /// </summary>
        public string IsPriority { get; set; }
        public bool? F_DeleteMark { get; set; } /// 是否删除
        public DateTime? F_CreatorTime { get; set; }    /// 创建时间
        public string F_CreatorUserId { get; set; } /// 创建人ID
        public string CreatorUserName { get; set; } /// 创建人名称
        public string F_DeleteUserId { get; set; }  /// 删除操作人
        public DateTime? F_DeleteTime { get; set; } /// 删除操作时间
        public string DeleteUserName { get; set; }  /// 删除操作人名称
        public DateTime? F_LastModifyTime { get; set; } /// 修改时间
        public string F_LastModifyUserId { get; set; }  /// 修改人ID
        public string ModifyUserName { get; set; }  /// 修改人名称
    }
}
