/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.WMSLogic;
using MST.Code;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_ReportManage.Controllers
{
    public class QAOrderReportController : ControllerBase
    {
        private V_QAOrderReportApp V_qABoundOrderReport = new V_QAOrderReportApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination,string keyword)
        {
            var data = V_qABoundOrderReport.FindList(pagination, keyword);
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
