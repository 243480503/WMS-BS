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
    public class AuthorizeActionModel
    {
        public string F_Id { set; get; }
        public string F_UrlAddress { set; get; }

        public string Name { get; set; }

        public DateTime LastGetTime { get; set; }
    }
}
