/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.WMSLogic;
using MST.Code;
using System.Web.Mvc;

namespace MST.Web
{
    [HandlerLogin]
    public abstract class ControllerBase : Controller
    {
        public Log FileLog
        {
            get { return LogFactory.GetLogger(this.GetType().ToString()); }
        }

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult Form()
        {
            return View();
        }
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult Details()
        {
            return View();
        }

        /// <summary>
        /// 前端js获取新单据编码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAuthorize(false)]
        public ActionResult GenNum(string ruleCode)
        {
            var code = new { Code = T_CodeGenApp.GenNum(ruleCode) };
            return Content(code.ToJson());
        }

        /// <summary>
        /// 前端js获取是否系统用户，便于隐藏基础数据复选框
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAuthorize(false)]
        public ActionResult IsSysUser()
        {
            var isSys = new { isSys = "false" };
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            if(user!=null && user.IsSystem)
            {
                isSys = new { isSys = "true" };
            }
            return Content(isSys.ToJson());
        }

        protected virtual ActionResult Warning(string message)
        {
            return Content(new AjaxResult { state = ResultType.warning.ToString(), message = message }.ToJson());
        }

        protected virtual ActionResult Info(string message)
        {
            return Content(new AjaxResult { state = ResultType.info.ToString(), message = message }.ToJson());
        }

        protected virtual ActionResult Success(string message)
        {
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = message }.ToJson());
        }
        protected virtual ActionResult Success(string message, object data)
        {
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = message, data = data }.ToJson());
        }

        /// <summary>
        /// 返回异常结果
        /// </summary>
        /// <param name="message">用户弹窗消息</param>
        /// <param name="exceptionMsg">浏览器请求响应返回消息,可为空</param>
        /// <returns></returns>
        protected virtual ActionResult Error(string message, string exceptionMsg)
        {
            return Content(new AjaxResult { state = ResultType.error.ToString(), message = message, data = exceptionMsg }.ToJson());
        }
    }
}
