/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System.Web.Mvc;

namespace MST.Web.Areas.ReportManage.Controllers
{
    public class EchartsController : Controller
    {
        //
        // GET: /ReportManage/Echarts/

        public ActionResult LibraryOperation()
        {
            return View();
        }

        public ActionResult JobTrend()
        {
            return View();
        }

    }
}
