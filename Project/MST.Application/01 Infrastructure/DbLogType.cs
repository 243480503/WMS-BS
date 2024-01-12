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

namespace MST.Application
{
    public enum DbLogType
    {
        [Description("其他")]
        Other = 0,
        [Description("登录")]
        Login = 1,
        [Description("退出")]
        Exit = 2,
        [Description("访问")]
        Visit = 3,
        [Description("新增")]
        Create = 4,
        [Description("删除")]
        Delete = 5,
        [Description("修改")]
        Update = 6,
        [Description("提交")]
        Submit = 7,
        [Description("异常")]
        Exception = 8,
    }
}
