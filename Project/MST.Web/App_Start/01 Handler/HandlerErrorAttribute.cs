/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using System.Web.Mvc;

namespace MST.Web
{
    public class HandlerErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
            context.ExceptionHandled = true;
            context.HttpContext.Response.StatusCode = 200;
            context.Result = new ContentResult { Content = new AjaxResult { state = ResultType.error.ToString(), message = context.Exception.Message }.ToJson() };
        }

        private void WriteLog(ExceptionContext context)
        {
            if (context == null)
                return;
            var log = LogFactory.GetLogger(context.Controller.ToString());

            LogObj logObj = new LogObj();
            logObj.Path = "MST.Web.HandlerErrorAttribute.WriteLog";
            logObj.Parms = new { context = context };
            logObj.Message = context.Exception;

            log.Error(logObj);
        }
    }
}