/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.SystemManage;
using MST.Code;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MST.Web
{
    public class HandlerAuthorizeAttribute : ActionFilterAttribute
    {
        public bool Ignore { get; set; }
        public HandlerAuthorizeAttribute(bool ignore = true)
        {
            Ignore = ignore;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (OperatorProvider.Provider.GetCurrent() == null)
            {
                return;
            }

            OperatorProvider.Provider.RefrshCookieExpTime(OperatorProvider.Provider.GetCurrent());

            if (OperatorProvider.Provider.GetCurrent().IsSystem)
            {
                return;
            }
            if (Ignore == false)
            {
                return;
            }
            if (!this.ActionAuthorize(filterContext))
            {
                StringBuilder sbScript = new StringBuilder();
                sbScript.Append("<script type='text/javascript'>alert('很抱歉！您的权限不足，访问被拒绝！');</script>");
                filterContext.Result = new ContentResult() { Content = sbScript.ToString() };
                return;
            }
        }
        private bool ActionAuthorize(ActionExecutingContext filterContext)
        {
            var operatorProvider = OperatorProvider.Provider.GetCurrent();
            var roleId = operatorProvider.RoleId;
            var moduleId = WebHelper.GetCookie("MST_currentmoduleid");
            var action = HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"].ToString();
            bool isSuccess = new RoleAuthorizeApp().ActionValidate(roleId, moduleId, action, RoleApp.LastModifyRoleAuthorize);
            return isSuccess;
        }
    }
}