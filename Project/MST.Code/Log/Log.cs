/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using log4net;
using System;

namespace MST.Code
{
    /// <summary>
    /// Log4日志写入
    /// </summary>
    public class Log
    {
        private ILog logger;
        public Log(ILog log)
        {
            this.logger = log;
        }
        public void Debug(LogObj logObj)
        {
            string log = logObj.ToJson();
            this.logger.Debug(log);
        }
        /// <summary>
        /// 文本写入
        /// </summary>
        /// <param name="logObj"></param>
        public void Error(LogObj logObj)
        {
            string log = logObj.ToJson();
            this.logger.Error(log);
        }

        /// <summary>
        /// 文本日志
        /// </summary>
        /// <param name="logObj"></param>
        public void Info(LogObj logObj)
        {
            string log = logObj.ToJson();
            this.logger.Info(log);
        }
        public void Warn(LogObj logObj)
        {
            string log = logObj.ToJson();
            this.logger.Warn(log);
        }
    }

    /// <summary>
    /// Log4日志对象
    /// </summary>
    public class LogObj
    {
        public string Path { get; set; }
        public object Parms { get; set; }
        public object Message { get; set; }

        public object ReturnData { get; set; }

        public DateTime? CurTime { get; set; }
    }
}
