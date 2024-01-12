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
    /// 移库单表
    /// </summary>
    public class T_MoveEntity : IEntity<T_MoveEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 移库单ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 移库单编码
        /// </summary>
        public string MoveCode { get; set; }
        /// <summary>
        /// New 新建,   Moving 移动中,Overing 结束中,   Over 结束
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
        /// 结束一组移库后，否自动产生下一组移库任务
        /// </summary>
        public string IsAuto { get; set; }
        /// <summary>
        /// 是否逆向整理
        /// </summary>
        public string IsReverse { get; set; }
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
