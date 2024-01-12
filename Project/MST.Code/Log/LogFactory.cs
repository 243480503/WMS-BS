/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using log4net;
using System;
using System.IO;
using System.Web;

namespace MST.Code
{
    /// <summary>
    /// 文本日志
    /// </summary>
    public class LogFactory
    {
        static LogFactory()
        {
            var rootPath= AppDomain.CurrentDomain.BaseDirectory;

            FileInfo configFile = new FileInfo(rootPath+"\\Configs\\log4net.config");
            log4net.Config.XmlConfigurator.Configure(configFile);
        }
        public static Log GetLogger(Type type)
        {
            return new Log(LogManager.GetLogger(type));
        }
        public static Log GetLogger(string str)
        {
            return new Log(LogManager.GetLogger(str));
        }
        /// <summary>
        /// 创建文本日志对象
        /// </summary>
        /// <returns></returns>
        public static Log GetLogger()
        {
            return new Log(LogManager.GetLogger(""));
        }
    }
}
