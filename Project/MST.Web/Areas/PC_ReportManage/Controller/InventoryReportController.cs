/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using MST.Application;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Code.Excel;
using MST.Code.Extend;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_ReportManage.Controllers
{
    public class InventoryReportController : ControllerBase
    {
        private V_ConDetailGroupByItemReportApp inventoryRepApp = new V_ConDetailGroupByItemReportApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_ERPWarehouseApp erpWarehouseApp = new T_ERPWarehouseApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_ItemKindApp itemKindApp = new T_ItemKindApp();
        private T_InBoundApp inBoundApp = new T_InBoundApp();
        private T_LocationApp locationApp = new T_LocationApp();

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult DetailsList()
        {
            return View();
        }

        /// <summary>
        /// 获取按物料库存统计的数据列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="groupByType"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string groupByType, string keyword)
        {
            List<V_ConDetailGroupByItemReportEntity> data = inventoryRepApp.GetList(pagination, keyword);
            List<V_ConDetailGroupByItemReportModel> inventoryModelList = new List<V_ConDetailGroupByItemReportModel>();
            IList<ItemsDetailEntity> dicCheckList = new ItemsDetailApp().FindEnum<T_ContainerDetailEntity>(o => o.CheckState);
            IList<ItemsDetailEntity> dicKindList = new ItemsDetailApp().FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind);
            foreach (var item in data)
            {
                V_ConDetailGroupByItemReportModel model = item.ToObject<V_ConDetailGroupByItemReportModel>();
                model.CheckStateName = dicCheckList.FirstOrDefault(o => o.F_ItemCode == model.CheckState).F_ItemName;
                model.ContainerKindName = dicKindList.FirstOrDefault(o => o.F_ItemCode == model.ContainerKind).F_ItemName;
                inventoryModelList.Add(model);
            }

            var resultList = new
            {
                rows = inventoryModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDetailsGridJson(Pagination pagination, string keyValue, string keyword)
        {
            T_ContainerDetailApp inventoryApp = new T_ContainerDetailApp();
            T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
            V_ConDetailGroupByItemReportEntity repEntity = inventoryRepApp.FindEntity(o => o.F_Id == keyValue);
            List<T_ContainerDetailEntity> data = inventoryApp.GetReportList(pagination, repEntity.ItemID, repEntity.Lot, keyword);
            IList<ItemsDetailEntity> enumCheckStateList = itemsDetailApp.FindEnum<T_ContainerDetailEntity>(o => o.CheckState).ToList();
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_ContainerDetailEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumLocationStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.State).ToList();
            List<ContainerDetailModel> inventoryModelList = new List<ContainerDetailModel>();
            foreach (var item in data)
            {
                ContainerDetailModel model = item.ToObject<ContainerDetailModel>();
                model.CheckStateName = enumCheckStateList.FirstOrDefault(o => o.F_ItemCode == model.CheckState).F_ItemName;
                model.ContainerTypeName = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType).ContainerTypeName;
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                T_LocationEntity loc = locationApp.FindEntity(o => o.F_Id == item.LocationID);
                model.LocationState = loc.State;
                model.LocationStateName = enumLocationStateList.FirstOrDefault(o => o.F_ItemCode == loc.State).F_ItemName;
                T_ERPWarehouseEntity t_ERPWarehouseEntity = erpWarehouseApp.FindEntity(o => o.ERPHouseCode == item.ERPWarehouseCode);
                if (t_ERPWarehouseEntity != null) model.ERPWarehouseName = t_ERPWarehouseEntity.ERPHouseName;

                T_InBoundEntity inBoundEntity = inBoundApp.FindEntity(o => o.F_Id == model.InBoundID);
                if (inBoundEntity != null) model.RefInBoundCode = inBoundEntity.RefOrderCode;
                else model.RefInBoundCode = "";

                inventoryModelList.Add(model);
            }

            var resultList = new
            {
                rows = inventoryModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }


        #region Excel文档下载
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult InventoryDownFile()
        {
            List<V_ConDetailGroupByItemReportEntity> data = inventoryRepApp.GetList();
            List<V_ConDetailGroupByItemReportModel> inventoryModelList = new List<V_ConDetailGroupByItemReportModel>();
            IList<ItemsDetailEntity> dicCheckList = new ItemsDetailApp().FindEnum<T_ContainerDetailEntity>(o => o.CheckState);
            IList<ItemsDetailEntity> dicKindList = new ItemsDetailApp().FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind);
            foreach (var item in data)
            {
                V_ConDetailGroupByItemReportModel model = item.ToObject<V_ConDetailGroupByItemReportModel>();
                model.CheckStateName = dicCheckList.FirstOrDefault(o => o.F_ItemCode == model.CheckState).F_ItemName;
                model.ContainerKindName = dicKindList.FirstOrDefault(o => o.F_ItemCode == model.ContainerKind).F_ItemName;
                inventoryModelList.Add(model);
            }

            NPOIExcel Excel = new NPOIExcel();
            var excelTemp = inventoryModelList.Select(o => new
            {
                物料编码 = o.ItemCode,
                物料名称 = o.ItemName,
                生产厂家 = o.Factory,
                规格 = o.Spec,
                批号 = o.Lot,
                库存数量 = (int)o.SumQty,
                计量单位 = o.ItemUnitText,
                失效日期 = o.OverdueDate,
                质检状态 = o.CheckStateName,
                容器大类 = o.ContainerKindName,
                容器数量 = o.Count,
            }).ToList();
            DataTable table = excelTemp.ToDataTable();

            string dir = "/Resource/ExcelFilesDown/";
            string target = Server.MapPath(dir);
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }
            string filename = "库存统计_" + DateTime.Now.ToString("yyy-MM-dd") + ".xlsx";  /// Guid.NewGuid().ToString().ToUpper()
            string pathPhy = target + filename;
            if (FileHelper.IsExistFile(pathPhy))
            {
                System.IO.File.Delete(pathPhy);
            }
            Excel.ToExcel(table, "库存统计", "库存统计", pathPhy);

            var resultList = new
            {
                Path = dir + filename
            };
            return Content(resultList.ToJson());
        }
        #endregion
    }
}
