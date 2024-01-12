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
    /// 货位盘点单
    /// </summary>
    public class T_LocCountEntity : IEntity<T_LocCountEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 货位盘点单ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 货位盘点编码
        /// </summary>
        public string LocCountCode { get; set; }
        /// <summary>
        /// 来源单号
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// New 新建,   Counting 盘点中,   Over 结束
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// ERP 接口,   MAN 手动
        /// </summary>
        public string GenType { get; set; }
        /// <summary>
        /// 区域ID
        /// </summary>
        public string AreaID { get; set; }
        /// <summary>
        /// 区域编码
        /// </summary>
        public string AreaCode { get; set; }

        /// <summary>
        /// 指定执行小车的编码（为空则指任何小车均可执行）
        /// </summary>
        public string PointRobotCode { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 任务单元数
        /// </summary>
        public int? TaskCellNum { get; set; }
        /// <summary>
        /// 是否区域全盘
        /// </summary>
        public string IsAllCount { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 执行人
        /// </summary>
        public string DoneUserID { get; set; }
        /// <summary>
        /// 执行人名称
        /// </summary>
        public string DoneUserName { get; set; }
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
