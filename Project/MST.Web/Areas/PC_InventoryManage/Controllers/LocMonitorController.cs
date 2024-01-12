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
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_InventoryManage.Controllers
{
    public class LocMonitorController : ControllerBase
    {
        private T_AreaApp areaApp = new T_AreaApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();


        /// <summary>
        /// 侧视图获取仓库数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SelectLocationInfo_Side(string itemName, string itemCode, string lot, string areaID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == areaID);
                    int? maxMapLayer = db.FindList<T_LocationEntity>(o => o.AreaID == area.F_Id).Max(o => o.MapLayer);
                    int? maxMapColumn = db.FindList<T_LocationEntity>(o => o.AreaID == area.F_Id).Max(o => o.MapColNum);
                    int? maxMapLine = db.FindList<T_LocationEntity>(o => o.AreaID == area.F_Id).Max(o => o.MapLine);

                    IList<LocationModel> locationModels = new List<LocationModel>();
                    IQueryable<T_LocationEntity> queryLoc = db.IQueryable<T_LocationEntity>(o => o.AreaCode == area.AreaCode);
                    IQueryable<T_ContainerEntity> queryCon = db.IQueryable<T_ContainerEntity>(o => o.F_DeleteMark == false);
                    IQueryable<T_ContainerDetailEntity> queryContainer = db.IQueryable<T_ContainerDetailEntity>(o => true);


                    //有查询条件
                    bool hasWhere = (!string.IsNullOrEmpty(itemName)) || (!string.IsNullOrEmpty(itemCode)) || (!string.IsNullOrEmpty(lot));
                    if (hasWhere)
                    {
                        queryLoc = queryLoc.Where(o => o.State == "Stored" || o.State == "Out" || o.State == "In");

                        if (!string.IsNullOrEmpty(itemName))
                        {
                            queryContainer = queryContainer.Where(o => o.ItemName == itemName);
                        }

                        if (!string.IsNullOrEmpty(itemCode))
                        {
                            queryContainer = queryContainer.Where(o => o.ItemCode == itemCode);
                        }

                        if (!string.IsNullOrEmpty(lot))
                        {
                            queryContainer = queryContainer.Where(o => o.Lot == lot);
                        }
                    }

                    locationModels = queryLoc.GroupJoin(queryCon, m => m.F_Id, n => n.LocationID, (m, n) => new { m = m, n = n }).SelectMany(o => o.n.DefaultIfEmpty(), (j, k) => new LocationModel
                    {
                        LocationCode = j.m.LocationCode,
                        LocationType = j.m.LocationType,
                        MapLayer = j.m.MapLayer,
                        MapColNum = j.m.MapColNum,
                        MapLine = j.m.MapLine,
                        Layer = j.m.Layer,
                        ColNum = j.m.ColNum,
                        Line = j.m.Line,
                        State = j.m.State,
                        ForbiddenState = j.m.ForbiddenState,
                        AreaCode = j.m.AreaCode,
                        AreaID = j.m.AreaID,
                        AreaName = j.m.AreaName,
                        containerModel = (k == null ? null : new ContainerModel
                        {
                            F_Id = k.F_Id,
                            ContainerKind = k.ContainerKind,
                            BarCode = k.BarCode,
                            containerDetailModel = queryContainer.Where(a => a.ContainerID == k.F_Id).Select(h => new ContainerDetailModel
                            {
                                Qty = h.Qty,
                                ItemBarCode = h.ItemBarCode,
                                ItemCode = h.ItemCode,
                                ItemName = h.ItemName,
                                Lot = h.Lot,
                                OverdueDate = h.OverdueDate,
                                ProductDate = h.ProductDate,
                                SupplierCode = h.SupplierCode,
                                SupplierID = h.SupplierID,
                                SupplierName = h.SupplierName,
                                OutQty = h.OutQty,
                                IsCheckFreeze = h.IsCheckFreeze,
                                IsCountFreeze = h.IsCountFreeze,
                                State = h.State,
                                CheckState = h.CheckState
                            }).ToList()
                        })
                    }).ToList();

                    if (hasWhere)
                    {
                        locationModels = locationModels.Where(o => o.containerModel != null && o.containerModel.containerDetailModel != null && o.containerModel.containerDetailModel.Count > 0).ToList();
                    }

                    var groupLine = locationModels.GroupBy(o => new { MapLine = o.MapLine }).Select(o => new { MapLine = o.Key.MapLine, List = o.ToList() }).OrderBy(o => o.MapLine).ToList();

                    var data = new
                    {
                        LocationStored = groupLine.Select(o => new
                        {
                            MapLine = o.MapLine,
                            Loc = o.List.Where(m => (m.State == "Stored")).Select(m => new string[] {
                            m.MapColNum.Value.ToString(),
                            m.MapLayer.Value.ToString(),
                            m.Line.Value.ToString(),
                            m.ColNum.Value.ToString(),
                            m.Layer.Value.ToString(),
                            m.LocationCode,
                            m.containerModel!=null? m.containerModel.BarCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? (m.containerModel.containerDetailModel.FirstOrDefault().Lot??""):"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? (m.containerModel.containerDetailModel.Sum(k=>k.Qty).ToString()):"",
                            "LocationStored"
                        }).ToArray()
                        }).ToList(),

                        LocationEmpty = hasWhere ? null : groupLine.Select(o => new
                        {
                            MapLine = o.MapLine,
                            Loc = o.List.Where(m => m.State == "Empty").Select(m => new string[] {
                            m.MapColNum.Value.ToString(),
                            m.MapLayer.Value.ToString(),
                            m.Line.Value.ToString(),
                            m.ColNum.Value.ToString(),
                            m.Layer.Value.ToString(),
                            m.LocationCode,
                            m.containerModel!=null? m.containerModel.BarCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? (m.containerModel.containerDetailModel.FirstOrDefault().Lot??""):"",
                            "",//Qty为空
                            "LocationEmpty"
                        }).ToArray()
                        }).ToList(),

                        LocationIn = groupLine.Select(o => new
                        {
                            MapLine = o.MapLine,
                            Loc = o.List.Where(m => (m.State == "In")).Select(m => new string[] {
                            m.MapColNum.Value.ToString(),
                            m.MapLayer.Value.ToString(),
                            m.Line.Value.ToString(),
                            m.ColNum.Value.ToString(),
                            m.Layer.Value.ToString(),
                            m.LocationCode,
                            m.containerModel!=null? m.containerModel.BarCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ?(m.containerModel.containerDetailModel.FirstOrDefault().Lot??""):"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? (m.containerModel.containerDetailModel.Sum(k=>k.Qty).ToString()):"",
                            "LocationIn"
                        }).ToArray()
                        }).ToList(),

                        LocationOut = groupLine.Select(o => new
                        {
                            MapLine = o.MapLine,
                            Loc = o.List.Where(m => (m.State == "Out")).Select(m => new string[] {
                            m.MapColNum.Value.ToString(),
                            m.MapLayer.Value.ToString(),
                            m.Line.Value.ToString(),
                            m.ColNum.Value.ToString(),
                            m.Layer.Value.ToString(),
                            m.LocationCode,
                            m.containerModel!=null? m.containerModel.BarCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName:"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? (m.containerModel.containerDetailModel.FirstOrDefault().Lot??""):"",
                            (m.containerModel!=null&& m.containerModel.containerDetailModel!=null && m.containerModel.containerDetailModel.Count>0) ? (m.containerModel.containerDetailModel.Sum(k=>k.Qty).ToString()):"",
                            "LocationOut"
                        }).ToArray()
                        }).ToList(),

                        MapLine = maxMapLine,
                        MapColumn = maxMapColumn,
                        MapLayer = maxMapLayer
                    };

                    return Success("获取成功", data);
                }
                catch (Exception ex)
                {
                    return Error("查询出错", ex.Message);
                }
            }

        }


        /// <summary>
        /// 俯视图获取行仓库数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SelectLocationInfo_Down(string itemName, string itemCode, string lot, string areaID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {


                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == areaID);
                    int? maxMapLayer = db.FindList<T_LocationEntity>(o => o.AreaID == area.F_Id).Max(o => o.MapLayer);
                    int? maxMapColumn = db.FindList<T_LocationEntity>(o => o.AreaID == area.F_Id).Max(o => o.MapColNum);
                    int? maxMapLine = db.FindList<T_LocationEntity>(o => o.AreaID == area.F_Id).Max(o => o.MapLine);

                    List<T_EquEntity> equList = db.FindList<T_EquEntity>(o => o.EquType == "Hallway");
                    if (equList != null && equList.Count > 0)
                    {
                        maxMapLayer = equList.Max(o => o.MapLayer) > maxMapLayer ? equList.Max(o => o.MapLayer) : maxMapLayer;
                        maxMapColumn = equList.Max(o => o.MapColNum) > maxMapColumn ? equList.Max(o => o.MapColNum) : maxMapColumn;
                        maxMapLine = equList.Max(o => o.MapLine) > maxMapLine ? equList.Max(o => o.MapLine) : maxMapLine;
                    }

                    IList<LocationModel> locationModels = new List<LocationModel>();
                    IQueryable<T_LocationEntity> queryLoc = db.IQueryable<T_LocationEntity>(o => o.AreaCode == area.AreaCode);
                    IQueryable<T_ContainerEntity> queryCon = db.IQueryable<T_ContainerEntity>(o => o.F_DeleteMark == false);
                    IQueryable<T_ContainerDetailEntity> queryContainer = db.IQueryable<T_ContainerDetailEntity>(o => true);


                    //有查询条件
                    bool hasWhere = (!string.IsNullOrEmpty(itemName)) || (!string.IsNullOrEmpty(itemCode)) || (!string.IsNullOrEmpty(lot));
                    if (hasWhere)
                    {
                        queryLoc = queryLoc.Where(o => o.State == "Stored" || o.State == "Out" || o.State == "In");

                        if (!string.IsNullOrEmpty(itemName))
                        {
                            queryContainer = queryContainer.Where(o => o.ItemName == itemName);
                        }

                        if (!string.IsNullOrEmpty(itemCode))
                        {
                            queryContainer = queryContainer.Where(o => o.ItemCode == itemCode);
                        }

                        if (!string.IsNullOrEmpty(lot))
                        {
                            queryContainer = queryContainer.Where(o => o.Lot == lot);
                        }
                    }

                    locationModels = queryLoc.GroupJoin(queryCon, m => m.F_Id, n => n.LocationID, (m, n) => new { m = m, n = n }).SelectMany(o => o.n.DefaultIfEmpty(), (j, k) => new LocationModel
                    {
                        LocationCode = j.m.LocationCode,
                        LocationType = j.m.LocationType,
                        MapLayer = j.m.MapLayer,
                        MapColNum = j.m.MapColNum,
                        MapLine = j.m.MapLine,
                        Layer = j.m.Layer,
                        ColNum = j.m.ColNum,
                        Line = j.m.Line,
                        State = j.m.State,
                        ForbiddenState = j.m.ForbiddenState,
                        AreaCode = j.m.AreaCode,
                        AreaID = j.m.AreaID,
                        AreaName = j.m.AreaName,
                        containerModel = (k == null ? null : new ContainerModel
                        {
                            F_Id = k.F_Id,
                            ContainerKind = k.ContainerKind,
                            BarCode = k.BarCode,
                            containerDetailModel = queryContainer.Where(a => a.ContainerID == k.F_Id).Select(h => new ContainerDetailModel
                            {
                                Qty = h.Qty,
                                ItemBarCode = h.ItemBarCode,
                                ItemCode = h.ItemCode,
                                ItemName = h.ItemName,
                                Lot = h.Lot,
                                OverdueDate = h.OverdueDate,
                                ProductDate = h.ProductDate,
                                SupplierCode = h.SupplierCode,
                                SupplierID = h.SupplierID,
                                SupplierName = h.SupplierName,
                                OutQty = h.OutQty,
                                IsCheckFreeze = h.IsCheckFreeze,
                                IsCountFreeze = h.IsCountFreeze,
                                State = h.State,
                                CheckState = h.CheckState
                            }).ToList()
                        })
                    }).ToList();

                    if (hasWhere)
                    {
                        locationModels = locationModels.Where(o => o.containerModel != null && o.containerModel.containerDetailModel != null && o.containerModel.containerDetailModel.Count > 0).ToList();
                    }

                    var groupLine = locationModels.GroupBy(o => new { MapLayer = o.MapLayer }).Select(o => new { MapLayer = o.Key.MapLayer, List = o.ToList() }).OrderBy(o => o.MapLayer).ToList();



                    var data = new
                    {
                        LocationStored = groupLine.Select(o => new
                        {
                            MapLayer = o.MapLayer,
                            Loc = o.List.Where(m => (m.State == "Stored")).Select(m => new
                            {
                                MapColNum = m.MapColNum.Value.ToString(),
                                MapLayer = m.MapLayer.Value.ToString(),
                                MapLine = m.MapLine.Value.ToString(),
                                Line = m.Line.Value.ToString(),
                                ColNum = m.ColNum.Value.ToString(),
                                Layer = m.Layer.Value.ToString(),
                                LocationCode = m.LocationCode,
                                BarCode = m.containerModel != null ? m.containerModel.BarCode : "",
                                ItemCode = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode : "",
                                ItemName = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName : "",
                                Lot = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? (m.containerModel.containerDetailModel.FirstOrDefault().Lot ?? "") : "",
                            }).ToArray()
                        }).ToList(),

                        LocationEmpty = hasWhere ? null : groupLine.Select(o => new
                        {
                            MapLayer = o.MapLayer,
                            Loc = o.List.Where(m => m.State == "Empty").Select(m => new
                            {
                                MapColNum = m.MapColNum.Value.ToString(),
                                MapLayer = m.MapLayer.Value.ToString(),
                                MapLine = m.MapLine.Value.ToString(),
                                Line = m.Line.Value.ToString(),
                                ColNum = m.ColNum.Value.ToString(),
                                Layer = m.Layer.Value.ToString(),
                                LocationCode = m.LocationCode,
                                BarCode = m.containerModel != null ? m.containerModel.BarCode : "",
                                ItemCode = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode : "",
                                ItemName = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName : "",
                                Lot = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? (m.containerModel.containerDetailModel.FirstOrDefault().Lot ?? "") : "",
                            }).ToArray()
                        }).ToList(),

                        LocationIn = groupLine.Select(o => new
                        {
                            MapLayer = o.MapLayer,
                            Loc = o.List.Where(m => (m.State == "In")).Select(m => new
                            {
                                MapColNum = m.MapColNum.Value.ToString(),
                                MapLayer = m.MapLayer.Value.ToString(),
                                MapLine = m.MapLine.Value.ToString(),
                                Line = m.Line.Value.ToString(),
                                ColNum = m.ColNum.Value.ToString(),
                                Layer = m.Layer.Value.ToString(),
                                LocationCode = m.LocationCode,
                                BarCode = m.containerModel != null ? m.containerModel.BarCode : "",
                                ItemCode = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode : "",
                                ItemName = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName : "",
                                Lot = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? (m.containerModel.containerDetailModel.FirstOrDefault().Lot ?? "") : "",
                            }).ToArray()
                        }).ToList(),

                        LocationOut = groupLine.Select(o => new
                        {
                            MapLayer = o.MapLayer,
                            Loc = o.List.Where(m => (m.State == "Out")).Select(m => new
                            {
                                MapColNum = m.MapColNum.Value.ToString(),
                                MapLayer = m.MapLayer.Value.ToString(),
                                MapLine = m.MapLine.Value.ToString(),
                                Line = m.Line.Value.ToString(),
                                ColNum = m.ColNum.Value.ToString(),
                                Layer = m.Layer.Value.ToString(),
                                LocationCode = m.LocationCode,
                                BarCode = m.containerModel != null ? m.containerModel.BarCode : "",
                                ItemCode = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemCode : "",
                                ItemName = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? m.containerModel.containerDetailModel.FirstOrDefault().ItemName : "",
                                Lot = (m.containerModel != null && m.containerModel.containerDetailModel != null && m.containerModel.containerDetailModel.Count > 0) ? (m.containerModel.containerDetailModel.FirstOrDefault().Lot ?? "") : "",
                            }).ToArray()
                        }).ToList(),

                        Hallway = equList.Select(o => new { MapColNum = o.MapColNum, MapLayer = o.MapLayer, MapLine = o.MapLine }).ToList(),

                        MapLine = maxMapLine,
                        MapColumn = maxMapColumn,
                        MapLayer = maxMapLayer
                    };

                    return Success("获取成功", data);
                }
                catch (Exception ex)
                {
                    return Error("查询出错", ex.Message);
                }
            }

        }


        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetAreaAjax()
        {
            IList<T_AreaEntity> areaList = new T_AreaApp().FindList(o => o.IsEnable == "true" && (o.AreaType == "Tunnel" || o.AreaType == "Concentrate" || o.AreaType == "AGV")).ToList();
            var data = areaList.Select(o => new { AreaID = o.F_Id, NAME = o.AreaName }).ToList();
            return Content(data.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GeItemAjax()
        {
            IList<T_ItemEntity> itemList = new T_ItemApp().FindList(o => o.F_DeleteMark == false).ToList();
            var data = itemList.Select(o => new { ITEMID = o.F_Id, NAME = o.ItemName }).ToList();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 侧视图获取行下拉框数据
        /// </summary>
        /// <param name="areaID"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetLine(string areaID)
        {
            IList<T_LocationEntity> locList = new T_LocationApp().FindList(o => o.AreaID == areaID && o.LocationType == "Cube").ToList().Select(o => new T_LocationEntity { Line = o.Line }).Distinct().OrderBy(o => o.Line).ToList();
            var data = locList.Select(o => new { LOCATIONLINE = o.Line, NAME = ("第" + o.Line + "行") }).Distinct().ToList();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 俯视图获取层下拉框数据
        /// </summary>
        /// <param name="areaID"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetLayer(string areaID)
        {
            IList<T_LocationEntity> locList = new T_LocationApp().FindList(o => o.AreaID == areaID && o.LocationType == "Cube").ToList().Select(o => new T_LocationEntity { Layer = o.Layer }).Distinct().OrderBy(o => o.Layer).ToList();
            var data = locList.Select(o => new { LOCATIONLINE = o.Layer, NAME = ("第" + o.Layer + "层") }).Distinct().ToList();
            return Content(data.ToJson());
        }
    }
}
