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
    public class ReportListController : ControllerBase
    {
        /// <summary>
        /// 列表页列表数据
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination,string keyword)
        {
            var data = new T_ReportSetApp().GetList(pagination,keyword);
            var resultList = new
            {
                rows = data,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }

        /// <summary>
        /// 编辑页获取配置数据
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = new T_ReportSetApp().FindEntity(o=>o.F_Id==keyValue);
            return Content(data.ToJson());
        }
    }
}
