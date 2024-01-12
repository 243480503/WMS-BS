/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace MST.Code
{
    public class CustTimer:Timer
    {
        /// <summary>
        /// 定时器名称
        /// </summary>
        public string TimerName { get; set; }

        /// <summary>
        /// 定时器编码
        /// </summary>
        public string TimerCode { get; set; }

        /// <summary>
        /// 该计时器已执行的次数(挂载的事件)
        /// </summary>
        public int OverCount { get; set; }

        /// <summary>
        /// 允许执行的最大次数(挂载的事件)
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// 是否已挂载事件
        /// </summary>
        public bool IsEventInit { get; set; }

        /// <summary>
        /// 线程锁
        /// </summary>
        public object LockObj { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 定时器是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 定时器运行次数(指触发次数，非挂载的事件，挂载事件可能因条件不满足而未执行)
        /// </summary>
        public int TickNum { get; set; }

        /// <summary>
        /// 距离下次执行挂载事件的时间(秒)
        /// </summary>
        public int NextBeginTime { get; set; }

        /// <summary>
        /// 定时器已运行时间(秒)
        /// </summary>
        public int CurRunTimer { get; set; }
    }
}
