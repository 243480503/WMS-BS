/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class ReceiveRecordModel
    {
        public string F_Id { get; set; }    /// 收货记录ID
        public string InBoundID { get; set; }   /// 入库单ID
        public string InBoundDetailID { get; set; } /// 入库单明细ID
        public int? SEQ { get; set; }   /// 入库明细项次
        public string InBoundCode { get; set; } /// 入库单明细ID
        public string RefOrderCode { get; set; }
        public decimal? OrderQty { get; set; }  /// 单据需要入库的数量
        public string CurQtyAndNeedQty { get; set; }    /// 已收/未收
        public string ReceiveStaionID { get; set; } /// 收货站台ID  物料默认的入库站台（设备ID）
        public string ContainerType { get; set; }
        public string ContainerTypeName { get; set; }   /// 容器类型
        public string ContainerKind { get; set; }   /// 容器大类
        public string CheckState { get; set; }
        public decimal? UnitQty { get; set; }   /// 单位数量
        public string ItemUnitText { get; set; }    /// 物料单位
        public string CheckStateName { get; set; }  /// 质检状态
                                                    /// Qua 合格
                                                    /// UnQua 不合格
                                                    /// UnNeed 免检
                                                    /// WaitCheck 待检
        public string AreaID { get; set; }  /// 存放区域
        public string Factory { get; set; } /// 生产厂家
        public int? ValidityDayNum { get; set; }    /// 有效期天数
        public DateTime? OverdueDate { get; set; }  /// 失效日期
        public string ERPWarehouseCode { get; set; }    /// ERP仓库编码
        public string BarCode { get; set; } /// 容器条码
        public string ItemBarCode { get; set; } /// 若是纸箱，此处填写纸箱容器条码
        public string ItemID { get; set; }  /// 物料ID
        public string IsItemMark { get; set; }  /// 是否贴标
        public string ItemCode { get; set; }    /// 物料编码
        public string ItemName { get; set; }    /// 物料名称
        public decimal? Qty { get; set; }   /// 物料已收数量
        public DateTime? ProductDate { get; set; }  /// 生产日期
        public string Lot { get; set; } /// 批号
        public string Spec { get; set; }    /// 规格
        public string SupplierUserID { get; set; }  /// 供应商ID
        public string SupplierUserName { get; set; }  /// 供应商ID
        public string DoneUserID { get; set; }   /// 收货人ID
        public string DoneUserName { get; set; }    /// 收货人姓名
        public string LocationID { get; set; }  /// 货位ID
        public string LocationCode { get; set; }     /// 货位编码
        public string State { get; set; }
        public string StateName { get; set; }   /// 收货状态
                                                /// NewGroup  新组货(指扫码入料箱的初始状态)
                                                /// LockOver 已封箱(指料箱已经不再继续放料)
                                                /// PutawayOver 已上架(指已将库存放到货架)
        public string TransState { get; set; }
        public string TransStateName { get; set; }  /// 过账状态 
                                                    /// UnNeedTrans 无需过账
                                                    /// WaittingTrans 待过账
                                                    /// OverTrans 已过账
                                                    /// FailTrans 过账失败
        public string FailDesc { get; set; }    /// 过账失败原因
        public bool? F_DeleteMark { get; set; }  /// 是否删除
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

