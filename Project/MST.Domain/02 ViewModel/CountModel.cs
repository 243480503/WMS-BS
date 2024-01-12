/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 盘点表
    /// </summary>
    public class CountModel
    {
        public string F_Id { get; set; }    /// 盘点单ID
		public string CountCode { get; set; }   /// 盘点单编码
		public string AreaType { get; set; }
        public string AreaTypeName { get; set; }    /// 区域类型
                                                    /// Cube 立体库
                                                    /// Flat 平库
        public string ERPHouseCode { get; set; }
        public string ERPHouseName { get; set; }    /// ERP仓库
		public string CountMode { get; set; }
        public string CountModeName { get; set; }   /// 盘点模式 
                                                    /// GoodsToPeople 货到人
                                                    /// PeopleToGoods 人到货
        public string CountMethod { get; set; }
        public string CountMethodName { get; set; } /// 盘点方法
                                                    /// ByItem 指定物料
                                                    /// ByLocation 指定货位
        public string StationID { get; set; }   /// 盘点站台ID
        public string StationName { get; set; }
        public string IsOpen { get; set; }  /// 是否明盘 false 盲盘, true 明盘
		public string State { get; set; }
        public string StateName { get; set; }   /// 盘点单状态
                                                /// New 新建
                                                /// Counting 盘点中
                                                /// Over 结束
        public string AuditState { get; set; }
        public string AuditStateName { get; set; }  /// 盘点单审核状态
                                                    /// WaitAudit 待审核
                                                    /// Auditing 审核中
                                                    /// Pass 通过
                                                    /// UnPass 不通过
        public string AuditResult { get; set; }
        public string AuditResultName { get; set; } /// 确认审核状态
                                                   /// Applied  已确认
                                                   /// WaitApply 未确认
        public string RefOrderCode { get; set; }    /// 来源单号
		public string GenType { get; set; } /// 生成方式
                                            /// ERP 接口
                                            /// MAN 手动
        public string GenTypeName { get; set; }
		public string Remark { get; set; }  /// 备注
        public bool? F_DeleteMark { get; set; } /// 是否删除
        public DateTime? F_CreatorTime { get; set; }    /// 创建时间
        public string F_CreatorUserId { get; set; }    /// 创建人ID
        public string CreatorUserName { get; set; }    /// 创建人名称
        public string F_DeleteUserId { get; set; }    /// 删除操作人
        public DateTime? F_DeleteTime { get; set; }    /// 删除操作时间
        public string DeleteUserName { get; set; }    /// 删除操作人名称
        public DateTime? F_LastModifyTime { get; set; }    /// 修改时间
        public string F_LastModifyUserId { get; set; }    /// 修改人ID
        public string ModifyUserName { get; set; }    /// 修改人名称
    }
}
