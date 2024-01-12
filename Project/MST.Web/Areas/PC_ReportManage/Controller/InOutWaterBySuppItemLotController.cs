/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.SystemManage;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Code.Excel;
using MST.Code.Extend;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_ReportManage.Controllers
{
    public class InOutWaterBySuppItemLotController : ControllerBase
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
        public ActionResult GetGridJson(Pagination pagination, DateTime? begin, DateTime? end, string lot, string supplierName, string itemName, string factoryName, string inOutType)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                Expression<Func<T_InOutDetailEntity, bool>> query = ExtLinq.True<T_InOutDetailEntity>();
                query = query.And(o => o.IsBroken != "false"); /// 排除非破坏性质检（取样还样）
                if (!string.IsNullOrEmpty(lot))
                {
                    query = query.And(o => o.Lot.Contains(lot));
                }
                if (!string.IsNullOrEmpty(supplierName))
                {
                    query = query.And(o => o.SupplierName.Contains(supplierName));
                }
                if (!string.IsNullOrEmpty(factoryName))
                {
                    query = query.And(o => o.Factory.Contains(factoryName));
                }
                if (!string.IsNullOrEmpty(itemName))
                {
                    query = query.And(o => o.ItemName.Contains(itemName));
                }
                if (begin != null)
                {
                    query = query.And(o => o.F_CreatorTime >= begin);
                }
                if (end != null)
                {
                    DateTime endTemp = end.Value.AddDays(1);
                    query = query.And(o => o.F_CreatorTime < endTemp);
                }

                if (inOutType == "OutType")
                {
                    query = query.And(o => o.InOutType == "OutType");
                }
                else
                {
                    query = query.And(o => o.InOutType == "InType");
                }


                var data = db.FindList<T_InOutDetailEntity>(query).GroupBy(o =>
                     new
                     {
                         ItemCode = o.ItemCode,
                         ItemName = o.ItemName,
                         Spec = o.Spec,
                         KindCode = o.KindCode,
                         KindName = o.KindName,
                         ItemUnitText = o.ItemUnitText,
                         Factory = o.Factory,
                         SupplierName = o.SupplierName,
                         Lot = o.Lot,
                         OverdueDate = o.OverdueDate
                     }).Select(o => new
                     {
                         F_Id = o.Key.SupplierName + o.Key.ItemCode + o.Key.Lot+o.Key.OverdueDate,
                         ItemCode = o.Key.ItemCode,
                         ItemName = o.Key.ItemName,
                         Spec = o.Key.Spec,
                         KindCode = o.Key.KindCode,
                         KindName = o.Key.KindName,
                         ItemUnitText = o.Key.ItemUnitText,
                         Factory = o.Key.Factory,
                         SupplierName = o.Key.SupplierName,
                         Lot = o.Key.Lot,
                         OverdueDate = o.Key.OverdueDate,
                         SumQty = o.Sum(k => k.ChangeQty)
                     }).ToList();

                data = data.GetPage(pagination, null).ToList();

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


        #region Excel文档下载
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult DownFile(DateTime? begin, DateTime? end, string lot, string supplierName, string itemName, string factoryName, string inOutType)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                Expression<Func<T_InOutDetailEntity, bool>> query = ExtLinq.True<T_InOutDetailEntity>();
                query = query.And(o => o.IsBroken != "false"); /// 排除非破坏性质检（取样还样）
                if (!string.IsNullOrEmpty(lot))
                {
                    query = query.And(o => o.Lot.Contains(lot));
                }
                if (!string.IsNullOrEmpty(supplierName))
                {
                    query = query.And(o => o.SupplierName.Contains(supplierName));
                }
                if (!string.IsNullOrEmpty(factoryName))
                {
                    query = query.And(o => o.Factory.Contains(factoryName));
                }
                if (!string.IsNullOrEmpty(itemName))
                {
                    query = query.And(o => o.ItemName.Contains(itemName));
                }
                if (begin != null)
                {
                    query = query.And(o => o.F_CreatorTime >= begin);
                }
                if (end != null)
                {
                    DateTime endTemp = end.Value.AddDays(1);
                    query = query.And(o => o.F_CreatorTime < endTemp);
                }

                if (inOutType == "OutType")
                {
                    query = query.And(o => o.InOutType == "OutType");
                }
                else
                {
                    query = query.And(o => o.InOutType == "InType");
                }


                var data = db.FindList<T_InOutDetailEntity>(query).GroupBy(o =>
                     new
                     {
                         ItemCode = o.ItemCode,
                         ItemName = o.ItemName,
                         Spec = o.Spec,
                         KindCode = o.KindCode,
                         KindName = o.KindName,
                         ItemUnitText = o.ItemUnitText,
                         Factory = o.Factory,
                         SupplierName = o.SupplierName,
                         Lot = o.Lot,
                         OverdueDate = o.OverdueDate
                     }).Select(o => new
                     {
                         F_Id = o.Key.ItemCode + o.Key.Lot,
                         ItemCode = o.Key.ItemCode,
                         ItemName = o.Key.ItemName,
                         Spec = o.Key.Spec,
                         KindCode = o.Key.KindCode,
                         KindName = o.Key.KindName,
                         ItemUnitText = o.Key.ItemUnitText,
                         Factory = o.Key.Factory,
                         SupplierName = o.Key.SupplierName,
                         Lot = o.Key.Lot,
                         OverdueDate = o.Key.OverdueDate,
                         SumQty = o.Sum(k => k.ChangeQty)
                     }).ToList();
                NPOIExcel Excel = new NPOIExcel();
                var excelTemp = data.Select(o => new
                {
                    物料编码 = o.ItemCode,
                    物料名称 = o.ItemName,
                    批号 = o.Lot,
                    物料类型 = o.KindName,
                    生产厂家 = o.Factory,
                    供应商 = o.SupplierName,
                    规格 = o.Spec,
                    计量单位 = o.ItemUnitText,
                    失效日期 = o.OverdueDate?.ToDateString(),
                    总数量 = (int)(o.SumQty.Value)
                }).ToList();
                DataTable table = excelTemp.ToDataTable();

                string dir = "/Resource/ExcelFilesDown/";
                string target = Server.MapPath(dir);
                if (!Directory.Exists(target))
                {
                    Directory.CreateDirectory(target);
                }

                string title = "出入库按供应商物料批次统计_" + (inOutType == "OutType" ? "(出库)" : "(入库)");

                string filename = title + DateTime.Now.ToString("yyy-MM-dd") + ".xlsx";  /// Guid.NewGuid().ToString().ToUpper()
                string pathPhy = target + filename;
                if (FileHelper.IsExistFile(pathPhy))
                {
                    System.IO.File.Delete(pathPhy);
                }
                Excel.ToExcel(table, title, title, pathPhy);

                var resultList = new
                {
                    Path = dir + filename
                };
                return Content(resultList.ToJson());
            }
        }
        #endregion
    }
}
