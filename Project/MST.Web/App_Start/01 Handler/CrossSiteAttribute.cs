/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

public class CrossSiteAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
        //允许哪些url可以跨域请求到本域
        //actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        //actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        ////允许的请求方法，一般是GET,POST,PUT,DELETE,OPTIONS
        //actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,OPTIONS,DELETE");
        //actionExecutedContext.Response.Headers.Add("Access-Control-Expose-Headers", "*");
        ////允许哪些请求头可以跨域
        //actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Headers", "Origin,X-Requested-With,Content-Type,Accept,Authorization,X-CSRF-TOKEN");
        //base.OnActionExecuted(actionExecutedContext);
    }


}