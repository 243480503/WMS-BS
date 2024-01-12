/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Application.WebMsg
{

    /// <summary>
    /// 主页消息类型
    /// </summary>
    public enum MsgType
    {
        /// <summary>
        /// 主页未读消息
        /// </summary>
        [Description("主页未读消息")]
        NoReadNum,
        /// <summary>
        /// 测试
        /// </summary>
        [Description("测试")]
        Test,
        /// <summary>
        /// 禁止登录
        /// </summary>
        [Description("禁止登录")]
        EnableLogin
    }

}
