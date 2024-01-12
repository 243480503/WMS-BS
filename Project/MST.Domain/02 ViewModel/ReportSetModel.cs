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
    public class ReportSetModel
	{
        /// <summary>
        /// 主键ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 数据源ID
        /// </summary>
        public string DataSourceID { get; set; }
        /// <summary>
        /// 报表名称
        /// </summary>
        public string ReportName { get; set; }
        /// <summary>
        /// 报表编码
        /// </summary>
        public string ReportCode { get; set; }
        /// <summary>
        /// 报表类型
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// 报表配置
        /// </summary>
        public string ReportSetConfig { get; set; }
        /// <summary>
        /// 报表设置实体
        /// </summary>
        public ReportSetConfigModel ReportSetConfigModel { get; set; }
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
