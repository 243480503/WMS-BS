/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using System;
using System.Text;
using System.Web.Mvc;

namespace MST.Web
{
    public class HandlerLoginAttribute : AuthorizeAttribute
    {
        public bool Ignore = true;
        public HandlerLoginAttribute(bool ignore = true)
        {
            Ignore = ignore;
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (Ignore == false)
            {
                return;
            }
            if (OperatorProvider.Provider.GetCurrent() == null)
            {
                WebHelper.WriteCookie("MST_login_error", "overdue");
                if(filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                {
                    string msg = ((object)"ajax_overdue").ToJson();
                    filterContext.HttpContext.Response.Write(msg);
                    filterContext.HttpContext.Response.ContentType = "text/html";
                    filterContext.HttpContext.Response.End();
                    filterContext.Result = new ContentResult();
                }
                else
                {
                    filterContext.HttpContext.Response.Write("<script>top.location.href = '/Login/Index';</script>");
                    filterContext.HttpContext.Response.ContentType = "text/html";
                    filterContext.HttpContext.Response.End();
                    filterContext.Result = new ContentResult();
                }
                return;
            }
            else
            {
                OperatorProvider.Provider.RefrshCookieExpTime(OperatorProvider.Provider.GetCurrent());
            }
        }
    }
}