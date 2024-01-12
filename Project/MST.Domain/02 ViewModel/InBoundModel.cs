/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 创建方式:
    ///1、人工界面创建(优先实现)
    ///2、接口创建(其次实现)，接口报文分为主表和从表两部分，主表数据插入采购订单表，从表数据插入采购订单明细表（报文待讨论）
    ///3、Excel导入(最后实现)，表格内容为一张单表，经过分组查询后，可拆分为主表数据和从表数据（导入模板待讨论）
    /// </summary>
    public class InBoundModel
    {
        public string F_Id { get; set; }    /// 入库单ID
        public string InBoundCode { get; set; } /// 采购入库单编码
        public string InBoundType { get; set; }
        public string InBoundTypeName { get; set; } /// 单据类型
        public string SupplierUserID { get; set; }  /// 供应商ID
        public string SupplierUserName { get; set; }    /// 供应商名称
        public string SupplierUserCode { get; set; }    /// 供应商编码
        public string StationID { get; set; }   /// 站台ID
        public string State { get; set; }
        public string StateName { get; set; }   /// New  新建, Receiveing 收货中,  Over  结束
        public string TransState { get; set; }
        /// <summary>
        /// 来源单号
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// ERP来源单号对应ERP的入库单号，用于ERP指定入库单号进行出库(没有则与来源单号一致)
        /// </summary>
        public string ERPInDocCode { get; set; }
        public string GenType { get; set; }
        public string GenTypeName { get; set; } /// ERP 接口,   MAN 手动
        public string Remark { get; set; }  /// 备注

        public IList<InBoundDetailModel> InBoundDetailList { get; set; }
        public bool? F_DeleteMark { get; set; } /// 是否删除
        public DateTime? F_CreatorTime { get; set; }    /// 创建时间
        public string F_CreatorUserId { get; set; } /// 创建人ID
        public string CreatorUserName { get; set; } /// 创建人名称
        public string F_DeleteUserId { get; set; }  /// 删除操作人
        public DateTime? F_DeleteTime { get; set; } /// 删除操作时间
        public string DeleteUserName { get; set; }  /// 删除操作人名称
        public DateTime? F_LastModifyTime { get; set; } /// 修改时间
        public string F_LastModifyUserId { get; set; }  /// 修改人ID
        public string ModifyUserName { get; set; }   /// 修改人名称
    }
}
