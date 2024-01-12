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

namespace MST.Web.Areas.PC_ReportManage.Controllers
{
    public class InOutWaterReportController : ControllerBase
    {
        private T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination,string keyword)
        {
            var data = inOutDetailApp.FindList(pagination, keyword);
            List<ItemsDetailEntity> enumInOutTypeList = itemsDetailApp.FindEnum<T_InOutDetailEntity>(o => o.InOutType).ToList();
            List<ItemsDetailEntity> enumOrderTypeList = itemsDetailApp.FindEnum<T_InOutDetailEntity>(o => o.OrderType).ToList();
            IList<ItemsDetailEntity> enumContainerKindList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();
            IList<T_ContainerTypeEntity> containerTypeList = containerTypeApp.GetList();

            IList<InOutDetailModel> inOutDetailModelList = data.ToObject<IList<InOutDetailModel>>();
            foreach (InOutDetailModel model in inOutDetailModelList)
            {
                model.InOutTypeName = enumInOutTypeList.FirstOrDefault(o => o.F_ItemCode == model.InOutType).F_ItemName;
                model.OrderTypeName = enumOrderTypeList.FirstOrDefault(o => o.F_ItemCode == model.OrderType).F_ItemName;
                T_ContainerTypeEntity containerTypeEntity = containerTypeList.FirstOrDefault(o => o.ContainerTypeCode == model.ContainerType);
                model.ContainerKind = containerTypeEntity.ContainerKind;
                model.ContainerKindName = enumContainerKindList.FirstOrDefault(o => o.F_ItemCode == model.ContainerKind).F_ItemName;
            }

            var resultList = new
            {
                rows = inOutDetailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }
    }
}
