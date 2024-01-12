/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.WMSLogic;
using System;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 创建方式:
    ///1、人工界面创建(优先实现)
    ///2、接口创建(其次实现)，接口报文分为主表和从表两部分，主表数据插入采购订单表，从表数据插入采购订单明细表（报文待讨论）
    ///3、Excel导入(最后实现)，表格内容为一张单表，经过分组查询后，可拆分为主表数据和从表数据（导入模板待讨论）
    /// </summary>
    public class InBoundDetailModel
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
        /// 质检状态 ： Qua 合格，UnQua 不合格，UnNeed 免检，WaitCheck 待检
        /// </summary>
        public string CheckState { get; set; }
        public string CheckStateName { get; set; }

        /// <summary>
        /// 对应的库存质检状态
        /// </summary>
        public string ConDetailCheckState { get; set; }

        /// <summary>
        /// 对应的库存质检状态名称
        /// </summary>
        public string ConDetailCheckStateName { get; set; }

        public string TransState { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemID { get; set; }

        /// <summary>
        /// 单位数量
        /// </summary>
        public decimal? UnitQty { get; set; }

        /// <summary>
        /// 物料单位
        /// </summary>
        public string ItemUnitText { get; set; }

        /// <summary>
        /// 有效期天数
        /// </summary>
        public int? ValidityDayNum { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime? OverdueDate { get; set; }

        /// <summary>
        /// 已入库数量
        /// </summary>
        public decimal? OverInQty { get; set; }

        /// <summary>
        /// 存放区域ID
        /// </summary>
        public string StoredAreaID { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }

        /// <summary>
        /// 执行方式（手动、设备）
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// 是否散箱模式
        /// </summary>
        public string IsSplitModel { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        public string StationID { get; set; }

        /// <summary>
        /// 是否贴标
        /// </summary>
        public string IsItemMark { get; set; }

        /// <summary>
        /// 入库站台编码
        /// </summary>
        public string InPostionCode { get; set; }

        /// <summary>
        /// 入库站台名称
        /// </summary>
        public string InPostionName { get; set; }

        /// <summary>
        /// 物料容器类型大类
        /// </summary>
        public string ContainerKind { get; set; }

        /// <summary>
        /// 物料容器类型大类名称
        /// </summary>
        public string ContainerKindName { get; set; }

        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }

        /// <summary>
        /// ERP仓库
        /// </summary>
        public string ERPWarehouseCode { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ProductDate { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public decimal? Qty { get; set; }

        /// <summary>
        /// 存储区域ID
        /// </summary>
        public string StoreAreaID { get; set; }

        /// <summary>
        /// 当前已收数量
        /// </summary>
        public decimal? CurQty { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
        /// 强制批号控制
        /// </summary>
        public string IsMustLot { get; set; }   

        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// 是否收货数量必须一致
        /// </summary>
        public string IsMustQtySame { get; set; }

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
