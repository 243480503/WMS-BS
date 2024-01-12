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
    public class TaskHisController : ControllerBase
    {
        private T_TaskHisApp taskHisApp = new T_TaskHisApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            List<T_TaskHisEntity> data = taskHisApp.GetList(pagination, keyword);
            IList<ItemsDetailEntity> enumStatelist = itemsDetailApp.FindEnum<T_TaskHisEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumInOutTypelist = itemsDetailApp.FindEnum<T_TaskHisEntity>(o => o.TaskInOutType).ToList();
            IList<ItemsDetailEntity> enumOrderTypelist = itemsDetailApp.FindEnum<T_TaskHisEntity>(o => o.OrderType).ToList();
            IList<ItemsDetailEntity> enumTaskTypelist = itemsDetailApp.FindEnum<T_TaskHisEntity>(o => o.TaskType).ToList();
            
            IList<TaskHisModel> taskModelList = new List<TaskHisModel>();
            foreach (T_TaskHisEntity task in data)
            {
                TaskHisModel model = task.ToObject<TaskHisModel>();
                model.TaskInOutTypeName = enumInOutTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskInOutType).F_ItemName;
                model.StateName = enumStatelist.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.TaskTypeName = enumTaskTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskType).F_ItemName;
                model.OrderTypeName = enumOrderTypelist.FirstOrDefault(o => o.F_ItemCode == model.OrderType).F_ItemName;
                taskModelList.Add(model);
            }

            //taskModelList = taskModelList.OrderByDescending(o => o.OverTime).ToList();

            var resultList = new
            {
                rows = taskModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = taskHisApp.GetForm(keyValue);
            return Content(data.ToJson());
        }
    }
}
