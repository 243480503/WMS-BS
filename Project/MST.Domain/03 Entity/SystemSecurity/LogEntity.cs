/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.Entity.SystemSecurity
{
    public class LogEntity : IEntity<LogEntity>, ICreationAudited
    {
        /// <summary>
        /// 日志主键
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? F_Date { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string F_Account { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string F_NickName { get; set; }
        /// <summary>
        /// 日志类型
        /// </summary>
        public string F_Type { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string F_IPAddress { get; set; }
        /// <summary>
        /// IP所在地址
        /// </summary>
        public string F_IPAddressName { get; set; }
        /// <summary>
        /// 系统模块ID
        /// </summary>
        public string F_ModuleId { get; set; }
        /// <summary>
        /// 系统模块
        /// </summary>
        public string F_ModuleName { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public bool? F_Result { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string F_Description { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? F_CreatorTime { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        public string F_CreatorUserId { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string F_Path { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string F_Param { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string F_Msg { get; set; }
    }
}
