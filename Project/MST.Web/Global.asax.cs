using MST.Application.APIPost;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Domain.ViewModel;
using System;
using System.Web.Mvc;
using System.Web.Routing;
using MST.Web.Areas.PC_BaseDataManage.Controllers;
using MST.Application;
using System.IO;

namespace MST.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {

        /// <summary>
        /// 启动应用程序
        /// </summary>
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            TimerTaskController.Timer_Init();//子任务初始化

            LogObj logObj = new LogObj();
            logObj.Path = "Application_Start";
            logObj.Message = "应用程序启动初始化";
            LogFactory.GetLogger().Info(logObj);
        }

        /// <summary>
        /// 因程序存在定时器，需防止IIS超时被回收（调试期间不能覆盖IIS）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Application_End(object sender, EventArgs e)
        {
            LogObj logWakeUp = new LogObj();
            logWakeUp.Path = "Application_End";
            try
            {
                //是否自动释放应用程序池
                string IsFreeWebPool = System.Configuration.ConfigurationManager.AppSettings["IsFreeWebPool"].ToString();
                if (IsFreeWebPool == "false") //不允许自动释放，则重新激活应用程序池
                {
                    LogObj logObj = new LogObj();
                    logObj.Path = "Application_End";
                    logObj.Message = "应用程序池即将被回收";
                    LogFactory.GetLogger().Info(logObj);



                    string WebHost = System.Configuration.ConfigurationManager.AppSettings["WebHost"].ToString();
                    string WebPort = System.Configuration.ConfigurationManager.AppSettings["WebPort"].ToString();
                    System.Net.WebClient wc = new System.Net.WebClient();
                    System.IO.Stream stream = wc.OpenRead(WebHost + ":" + WebPort);
                    System.IO.StreamReader reader = new StreamReader(stream);
                    string html = reader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(html))
                    {
                        logWakeUp.Message = "唤醒程序成功";
                        LogFactory.GetLogger().Info(logWakeUp);
                    }
                    reader.Close();
                    reader.Dispose();
                    stream.Close();
                    stream.Dispose();
                    wc.Dispose();
                }
                else
                {
                    logWakeUp.Message = "程序已释放";
                    LogFactory.GetLogger().Info(logWakeUp);
                }
            }
            catch (Exception ex)
            {
                logWakeUp.Message = "唤醒异常:" + ex.Message;
                LogFactory.GetLogger().Info(logWakeUp);
            }
        }
    }
}