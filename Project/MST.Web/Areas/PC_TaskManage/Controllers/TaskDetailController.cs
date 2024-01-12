/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using MST.Application.SystemManage;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_TaskManage.Controllers
{
    public class TaskDetailController : Controller
    {
        private T_TaskDetailApp taskDetailApp = new T_TaskDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();

        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string taskID, string keyword)
        {
            var data = taskDetailApp.GetList(pagination, taskID, keyword);

            var resultList = new
            {
                rows = data,
                total = pagination.total,
                page = pagination.page,

                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
    }
}
