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
    public class EBoardModel
    {
        public string Timetamp { get; set; }
        public DateTime? DateTime { get; set; }
        public string MsgID { get; set; }
        public string Func { get; set; }
        public object Param { get; set; }
    }
}
