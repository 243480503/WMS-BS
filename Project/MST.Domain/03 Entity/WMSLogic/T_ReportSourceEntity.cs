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
    /// 报表数据源表
    /// </summary>
    public class T_ReportSourceEntity : IEntity<T_ReportSourceEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 数据源名称
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// 数据源编码
        /// </summary>
        public string SourceCode { get; set; }
        /// <summary>
        /// 是否数据库
        /// </summary>
        public string IsDB { get; set; }
        /// <summary>
        /// 是否网络数据(即是否包含IP与用户名密码)
        /// </summary>
        public string IsTelnet { get; set; }
        /// <summary>
        /// SqlServer,Orcale,MySql。(IsDB为true有效)
        /// </summary>
        public string DetailType { get; set; }

        /// <summary>
        /// 接口返回的格式：Json,xml。(IsDB为false有效)
        /// </summary>
        public string ReqTextType { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// 查询类型:View,Table,Sql
        /// </summary>
        public string SelectType { get; set; }
        public string IP { get; set; }

        public string Uid { get; set; }

        public string Pwd { get; set; }

        public string URL { get; set; }
        public string Sql { get; set; }

        public string TableCode { get; set; }
        public string ViewCode { get; set; }

        /// <summary>
        /// 列配置
        /// </summary>
        public string ColumnJson { get; set; }
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
