/*******************************************************************************
 * Copyright ? 2021 迈思特版权所有
 * Author: MST
 * Description: WMS平台
 * Website：www.maisite.com
*********************************************************************************/
using MST.Application.WMSLogic;
using MST.Code;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_ReportEditorManage.Controllers
{
    public class ReportEditorController : ControllerBase
    {

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination,string keyword)
        {
            var data = "";
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
