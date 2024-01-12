/*******************************************************************************
 * Copyright ? 2021 迈思特版权所有
 * Author: MST
 * Description: WMS平台
 * Website：www.maisite.com
*********************************************************************************/

using MST.Application.WMSLogic;
using MST.Code;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_InventoryManage.Controllers
{
    public class LocationMoveController : ControllerBase
    {
        private V_LocationInfoApp locationInfoApp = new V_LocationInfoApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            string strSql = "SELECT * FROM V_LocationInfo WHERE (1=1)";
            string strTemp;
            List<V_LocationInfoEntity> data = new List<V_LocationInfoEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                strTemp = $"{strSql} AND ItemCode LIKE '%{keyword}%'";
                data = locationInfoApp.FindList(strTemp, pagination);

                if (data.Count == 0)
                {
                    strTemp = $"{strSql} AND LocationCode LIKE '%{keyword}%'";
                    data = locationInfoApp.FindList(strTemp, pagination);
                }
            }
            else data = locationInfoApp.FindList(strSql, pagination);

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