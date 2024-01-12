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
    /// 区域表
    /// </summary>
    public class T_StationEntity : IEntity<T_StationEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 站台ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 站台编码
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 站台名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 当前单据ID
        /// </summary>
        public string CurOrderID { get; set; }  
        /// <summary>
        /// 站台单据明细的ID（出库站台的情况下，哪怕是同一个容器，该值也会经常变动，出库逻辑请勿用该值做判断）
        /// </summary>
        public string CurOrderDetailID { get; set; }
        /// <summary>
        /// 波次ID
        /// </summary>
        public string WaveID { get; set; }
        /// <summary>
        /// 当前单据类型
        /// PurchaseIn  采购入库单
        /// WaitCheckType 待检入库单
        /// BackSample 质检还样单
        /// GetSample 质检取样单
        /// Count	盘点单
        /// GetItemOut 领料出库单
        /// WarehouseBackOut 仓退出库单
        /// VerBackOut 验退出库单
        /// EmptyIn 空料箱入库
        /// EmptyOut 空料箱出库
        /// OffRack 下架单
        /// </summary>
        public string OrderType { get; set; }
        /// <summary>
        /// 当前容器条码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 用途 多选，逗号分割
        /// CheckPickOut  质检取样出库
        /// CheckPickIn 质检取样入库
        /// CheckReturnOut 质检还样出库
        /// CheckReturnIn 质检还样入库
        /// CountOut	盘点出库
        /// CountIn 盘点入库
        /// PurchaseIn 采购入库
        /// WaitCheck 待检入库
        /// GetItemOut 领料出库
        /// GetItemBack 领料回库
        /// WarehouseBackOut 仓退出库
        /// VerBackOut 验退出库
        /// EmptyIn 空料箱入库
        /// EmptyOut 空料箱出库
        /// OffRackOut 下架出库
        /// </summary>
        public string UseCode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 到达地址
        /// </summary>
        public string TagAddress { get; set; }
        /// <summary>
        /// 离开地址
        /// </summary>
        public string LeaveAddress { get; set; }
        /// <summary>
        /// 是否故障
        /// </summary>
        public string IsErr { get; set; }
        /// <summary>
        /// 故障消息
        /// </summary>
        public string ErrMsg { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public string IsEnable { get; set; }
        /// <summary>
        /// 当前模式。Normal正常模式，Empty空箱模式
        /// </summary>
        public string CurModel { get; set; }
        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
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
