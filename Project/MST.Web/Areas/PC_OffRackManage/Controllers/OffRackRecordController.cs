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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_OffRackManage.Controllers
{
    public class OffRackRecordController : Controller
    {
        private T_OffRackRecordApp offRackRecordApp = new T_OffRackRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();

        [HttpGet]
        [HandlerAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string offRackDetailID, string keyword)
        {
            var data = offRackRecordApp.GetList(pagination, offRackDetailID, keyword);
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_OffRackRecordEntity>(o => o.State).ToList();

            IList<OffRackRecordModel> offRackRecordModelList = new List<OffRackRecordModel>();
            foreach (T_OffRackRecordEntity entity in data)
            {
                OffRackRecordModel model = entity.ToObject<OffRackRecordModel>();
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == entity.State).F_ItemName;
                offRackRecordModelList.Add(model);
            }

            var resultList = new
            {
                rows = offRackRecordModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }
    }
}
