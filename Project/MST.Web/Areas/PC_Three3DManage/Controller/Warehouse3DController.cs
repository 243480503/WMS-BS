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
/**********精确到主容器的波次运算***************/
namespace MST.Web.Areas.PC_Three3DManage.Controllers
{
    [HandlerLogin(false)]
    public class Warehouse3DController : ControllerBase
    {
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_LocationApp locationApp = new T_LocationApp();


        #region 获取货位列表  
        [HttpGet]
        public ActionResult GetDBData()
        {
            using (IRepositoryBase db = new RepositoryBase().BeginTrans())
            {
                IList<T_TaskEntity> TaskList = new T_TaskApp().FindList(o => true).ToList();

                var InEquList = db.IQueryable<T_EquEntity>(o => o.EquType == "MecLine" && o.GroupCode == "StationIn_Normal").GroupJoin(db.IQueryable<T_TaskEntity>(o => true), equ => equ.F_Id, task => task.CurEquID3D,
                    (equ, task) => new { equ = equ, task = task }).SelectMany(temp => temp.task.DefaultIfEmpty(), (temp, grp)
                             => new
                             {
                                 EquID = temp.equ.F_Id,
                                 EquCode = temp.equ.EquCode,
                                 WCSCode = temp.equ.WCSCode,
                                 WCSData = temp.equ.WCSData,
                                 EquName = temp.equ.EquName,
                                 EquSort = temp.equ.Sort,

                                 TaskID = grp==null?null:grp.F_Id,
                                 TaskNo = grp == null ? null : grp.TaskNo,
                                 OrderCode = grp == null ? null : grp.OrderCode,
                                 TaskType = grp == null ? null : grp.TaskType,
                                 BarCode = grp == null ? null : grp.BarCode,
                                 ContainerType = grp == null ? null : grp.ContainerType,
                                 OrderID = grp == null ? null : grp.OrderID,
                                 OrderType = grp == null ? null : grp.OrderType,
                                 ContainerID = grp == null ? null : grp.ContainerID
                             })
                    .GroupJoin(db.IQueryable<T_ContainerEntity>(o => true), equ => equ.ContainerID, con => con.F_Id, (equ, con) => new { equ = equ, con = con }).SelectMany(temp => temp.con.DefaultIfEmpty(), (temp, grp)
                                         => new
                                         {
                                             EquID = temp.equ.EquID,
                                             EquCode = temp.equ.EquCode,
                                             WCSCode = temp.equ.WCSCode,
                                             WCSData = temp.equ.WCSData,
                                             EquName = temp.equ.EquName,
                                             EquSort = temp.equ.EquSort,
                                             TaskID = temp.equ.TaskID,
                                             TaskNo = temp.equ.TaskNo,
                                             OrderCode = temp.equ.OrderCode,
                                             TaskType = temp.equ.TaskType,
                                             BarCode = temp.equ.BarCode,
                                             ContainerType = temp.equ.ContainerType,
                                             OrderType = temp.equ.OrderType,
                                             ContainerID = temp.equ.ContainerID,
                                             OrderID = temp.equ.OrderID,
                                             ContainerKindType = grp == null ? null : grp.ContainerKind
                                         })
                    .GroupJoin(db.IQueryable<T_ReceiveRecordEntity>(o => true), equ => new { BarCode = equ.BarCode, OrderID = equ.OrderID }, rec => new { BarCode = rec.BarCode, OrderID = rec.InBoundID }, (equ, rec) => new { equ = equ, rec = rec }).SelectMany(temp => temp.rec.DefaultIfEmpty(), (temp, grp)
                                        => new
                                        {
                                            EquID = temp.equ.EquID,
                                            EquCode = temp.equ.EquCode,
                                            WCSCode = temp.equ.WCSCode,
                                            WCSData = temp.equ.WCSData,
                                            EquName = temp.equ.EquName,
                                            EquSort = temp.equ.EquSort,
                                            TaskID = temp.equ.TaskID,
                                            TaskNo = temp.equ.TaskNo,
                                            OrderCode = temp.equ.OrderCode,
                                            TaskType = temp.equ.TaskType,
                                            BarCode = temp.equ.BarCode,
                                            ContainerType = temp.equ.ContainerType,
                                            OrderType = temp.equ.OrderType,
                                            ContainerID = temp.equ.ContainerID,
                                            OrderID = temp.equ.OrderID,
                                            ContainerKindType = temp.equ.ContainerKindType,
                                            Qty = grp == null ? null : grp.Qty,
                                            ItemID = grp == null ? null : grp.ItemID,
                                            Lot = grp == null ? null : grp.Lot
                                        })
                    .GroupJoin(db.IQueryable<T_ItemEntity>(o => true), equ => equ.ItemID, item => item.F_Id, (equ, item) => new { equ = equ, item = item }).SelectMany(temp => temp.item.DefaultIfEmpty(), (temp, grp)
                                         => new
                                         {
                                             EquID = temp.equ.EquID,
                                             EquCode = temp.equ.EquCode,
                                             WCSCode = temp.equ.WCSCode,
                                             WCSData = temp.equ.WCSData,
                                             EquName = temp.equ.EquName,
                                             EquSort = temp.equ.EquSort,
                                             TaskID = temp.equ.TaskID,
                                             TaskNo = temp.equ.TaskNo,
                                             OrderCode = temp.equ.OrderCode,
                                             TaskType = temp.equ.TaskType,
                                             BarCode = temp.equ.BarCode,
                                             ContainerType = temp.equ.ContainerType,
                                             OrderType = temp.equ.OrderType,
                                             ContainerID = temp.equ.ContainerID,
                                             OrderID = temp.equ.OrderID,
                                             ContainerKindType = temp.equ.ContainerKindType,
                                             Qty = temp.equ.Qty,
                                             ItemID = temp.equ.ItemID,
                                             Lot = temp.equ.Lot,

                                             ItemName = grp == null ? null : grp.ItemName,
                                             ItemCode = grp == null ? null : grp.ItemCode
                                         }).GroupBy(o => new
                                         {
                                             EquID = o.EquID,
                                             EquCode = o.EquCode,
                                             WCSCode = o.WCSCode,
                                             WCSData = o.WCSData,
                                             EquName = o.EquName,
                                             EquSort = o.EquSort
                                         }).Select(o => new
                                         {
                                             EquID = o.Key.EquID,
                                             EquCode = o.Key.EquCode,
                                             WCSCode = o.Key.WCSCode,
                                             WCSData = o.Key.WCSData,
                                             EquName = o.Key.EquName,
                                             EquSort = o.Key.EquSort,

                                             IsHasTask = o.Count(t=>t.TaskID!=null) > 0,
                                             Task = o.Where(t=>t.TaskID!=null).GroupBy(m => new { 
                                                 m.TaskID, 
                                                 m.TaskNo, 
                                                 m.OrderCode, 
                                                 m.TaskType, 
                                                 m.BarCode, 
                                                 m.ContainerType, 
                                                 m.OrderType, 
                                                 m.ContainerID, 
                                                 m.OrderID, 
                                                 m.ContainerKindType 
                                             }).Select(k => new
                                             {
                                                 TaskID=   k.Key.TaskID,
                                                 TaskNo = k.Key.TaskNo,
                                                 OrderCode= k.Key.OrderCode,
                                                 TaskType = k.Key.TaskType,
                                                 BarCode = k.Key.BarCode,
                                                 ContainerType= k.Key.ContainerType,
                                                 OrderType= k.Key.OrderType,
                                                 ContainerID = k.Key.ContainerID,
                                                 OrderID= k.Key.OrderID,
                                                 ContainerKindType= k.Key.ContainerKindType,
                                                 IsHasRec=k.Count(c=>c.ItemID!=null)>0,
                                                 RecList = k.Where(c=>c.ItemID!=null).ToList()
                                             }).ToList()
                                         }).OrderBy(o=>o.EquSort).ToList();

                var OutEquList = db.IQueryable<T_EquEntity>(o => o.EquType == "MecLine" && o.GroupCode == "StationOut_Normal").GroupJoin(db.IQueryable<T_TaskEntity>(o => true), equ => equ.F_Id, task => task.CurEquID3D,
                    (equ, task) => new { equ = equ, task = task }).SelectMany(temp => temp.task.DefaultIfEmpty(), (temp, grp)
                             => new
                             {
                                 EquID = temp.equ.F_Id,
                                 EquCode = temp.equ.EquCode,
                                 WCSCode = temp.equ.WCSCode,
                                 WCSData = temp.equ.WCSData,
                                 EquName = temp.equ.EquName,
                                 EquSort = temp.equ.Sort,

                                 TaskID = grp == null ? null : grp.F_Id,
                                 TaskNo = grp == null ? null : grp.TaskNo,
                                 OrderCode = grp == null ? null : grp.OrderCode,
                                 TaskType = grp == null ? null : grp.TaskType,
                                 BarCode = grp == null ? null : grp.BarCode,
                                 ContainerType = grp == null ? null : grp.ContainerType,
                                 OrderID = grp == null ? null : grp.OrderID,
                                 OrderType = grp == null ? null : grp.OrderType,
                                 ContainerID = grp == null ? null : grp.ContainerID
                             })
                    .GroupJoin(db.IQueryable<T_ContainerEntity>(o => true), equ => equ.ContainerID, con => con.F_Id, (equ, con) => new { equ = equ, con = con }).SelectMany(temp => temp.con.DefaultIfEmpty(), (temp, grp)
                                         => new
                                         {
                                             EquID = temp.equ.EquID,
                                             EquCode = temp.equ.EquCode,
                                             WCSCode = temp.equ.WCSCode,
                                             WCSData = temp.equ.WCSData,
                                             EquName = temp.equ.EquName,
                                             EquSort = temp.equ.EquSort,
                                             TaskID = temp.equ.TaskID,
                                             TaskNo = temp.equ.TaskNo,
                                             OrderCode = temp.equ.OrderCode,
                                             TaskType = temp.equ.TaskType,
                                             BarCode = temp.equ.BarCode,
                                             ContainerType = temp.equ.ContainerType,
                                             OrderType = temp.equ.OrderType,
                                             ContainerID = temp.equ.ContainerID,
                                             OrderID = temp.equ.OrderID,
                                             ContainerKindType = grp == null ? null : grp.ContainerKind
                                         })
                    .GroupJoin(db.IQueryable<T_OutRecordEntity>(o => true), equ => new { BarCode = equ.BarCode, OrderID = equ.OrderID }, rec => new { BarCode = rec.BarCode, OrderID = rec.OutBoundID }, (equ, rec) => new { equ = equ, rec = rec }).SelectMany(temp => temp.rec.DefaultIfEmpty(), (temp, grp)
                                        => new
                                        {
                                            EquID = temp.equ.EquID,
                                            EquCode = temp.equ.EquCode,
                                            WCSCode = temp.equ.WCSCode,
                                            WCSData = temp.equ.WCSData,
                                            EquName = temp.equ.EquName,
                                            EquSort = temp.equ.EquSort,
                                            TaskID = temp.equ.TaskID,
                                            TaskNo = temp.equ.TaskNo,
                                            OrderCode = temp.equ.OrderCode,
                                            TaskType = temp.equ.TaskType,
                                            BarCode = temp.equ.BarCode,
                                            ContainerType = temp.equ.ContainerType,
                                            OrderType = temp.equ.OrderType,
                                            ContainerID = temp.equ.ContainerID,
                                            OrderID = temp.equ.OrderID,
                                            ContainerKindType = temp.equ.ContainerKindType,
                                            Qty = grp == null ? null : grp.NeedQty,
                                            ItemID = grp == null ? null : grp.ItemID,
                                            Lot = grp == null ? null : grp.Lot
                                        })
                    .GroupJoin(db.IQueryable<T_ItemEntity>(o => true), equ => equ.ItemID, item => item.F_Id, (equ, item) => new { equ = equ, item = item }).SelectMany(temp => temp.item.DefaultIfEmpty(), (temp, grp)
                                         => new
                                         {
                                             EquID = temp.equ.EquID,
                                             EquCode = temp.equ.EquCode,
                                             WCSCode = temp.equ.WCSCode,
                                             WCSData = temp.equ.WCSData,
                                             EquName = temp.equ.EquName,
                                             EquSort = temp.equ.EquSort,
                                             TaskID = temp.equ.TaskID,
                                             TaskNo = temp.equ.TaskNo,
                                             OrderCode = temp.equ.OrderCode,
                                             TaskType = temp.equ.TaskType,
                                             BarCode = temp.equ.BarCode,
                                             ContainerType = temp.equ.ContainerType,
                                             OrderType = temp.equ.OrderType,
                                             ContainerID = temp.equ.ContainerID,
                                             OrderID = temp.equ.OrderID,
                                             ContainerKindType = temp.equ.ContainerKindType,
                                             Qty = temp.equ.Qty,
                                             ItemID = temp.equ.ItemID,
                                             Lot = temp.equ.Lot,

                                             ItemName = grp == null ? null : grp.ItemName,
                                             ItemCode = grp == null ? null : grp.ItemCode
                                         }).GroupBy(o => new
                                         {
                                             EquID = o.EquID,
                                             EquCode = o.EquCode,
                                             WCSCode = o.WCSCode,
                                             WCSData = o.WCSData,
                                             EquName = o.EquName,
                                             EquSort = o.EquSort
                                         }).Select(o => new
                                         {
                                             EquID = o.Key.EquID,
                                             EquCode = o.Key.EquCode,
                                             WCSCode = o.Key.WCSCode,
                                             WCSData = o.Key.WCSData,
                                             EquName = o.Key.EquName,
                                             EquSort = o.Key.EquSort,

                                             IsHasTask = o.Count(t => t.TaskID != null) > 0,
                                             Task = o.Where(t => t.TaskID != null).GroupBy(m => new {
                                                 m.TaskID,
                                                 m.TaskNo,
                                                 m.OrderCode,
                                                 m.TaskType,
                                                 m.BarCode,
                                                 m.ContainerType,
                                                 m.OrderType,
                                                 m.ContainerID,
                                                 m.OrderID,
                                                 m.ContainerKindType
                                             }).Select(k => new
                                             {
                                                 TaskID = k.Key.TaskID,
                                                 TaskNo = k.Key.TaskNo,
                                                 OrderCode = k.Key.OrderCode,
                                                 TaskType = k.Key.TaskType,
                                                 BarCode = k.Key.BarCode,
                                                 ContainerType = k.Key.ContainerType,
                                                 OrderType = k.Key.OrderType,
                                                 ContainerID = k.Key.ContainerID,
                                                 OrderID = k.Key.OrderID,
                                                 ContainerKindType = k.Key.ContainerKindType,
                                                 IsHasRec = k.Count(c => c.ItemID != null) > 0,
                                                 RecList = k.Where(c => c.ItemID != null).ToList()
                                             }).ToList()
                                         }).OrderBy(o => o.EquSort).ToList();


                var data = db.IQueryable<T_LocationEntity>(o => o.LocationType == "Cube").GroupJoin(db.IQueryable<T_ContainerEntity>(o => true), loc => loc.F_Id, con => con.LocationID,
                    (loc, con) => new { loc = loc, con = con }).SelectMany(temp => temp.con.DefaultIfEmpty(), (temp, grp)
                     => new
                     {
                         AreaCode = temp.loc.AreaCode,
                         AreaName = temp.loc.AreaName,
                         AreaType = temp.loc.AreaType,
                         LocationID = temp.loc.F_Id,
                         LocationType = temp.loc.LocationType,
                         LocationCode = temp.loc.LocationCode,
                         LocationState = temp.loc.State,
                         Line = temp.loc.Line,
                         Layer = temp.loc.Layer,
                         ColNum = temp.loc.ColNum,
                         Deep = temp.loc.Deep,
                         ForbiddenState = temp.loc.ForbiddenState,

                         BarCode = grp == null ? null : grp.BarCode,
                         ContainerID = grp == null ? null : grp.F_Id,
                         ContainerKind = grp == null ? null : grp.ContainerKind
                     })

                    .GroupJoin(db.IQueryable<T_ContainerDetailEntity>(o => true), loc => loc.ContainerID, det => det.ContainerID,
                    (loc, det) => new { loc = loc, det = det }).SelectMany(temp => temp.det.DefaultIfEmpty(), (temp, grp)
                              => new
                              {
                                  AreaCode = temp.loc.AreaCode,
                                  AreaName = temp.loc.AreaName,
                                  AreaType = temp.loc.AreaType,
                                  LocationID = temp.loc.LocationID,
                                  LocationType = temp.loc.LocationType,
                                  LocationCode = temp.loc.LocationCode,
                                  ContainerID = temp.loc.ContainerID,
                                  Line = temp.loc.Line,
                                  Layer = temp.loc.Layer,
                                  ColNum = temp.loc.ColNum,
                                  Deep = temp.loc.Deep,
                                  LocationState = temp.loc.LocationState,
                                  ForbiddenState = temp.loc.ForbiddenState,

                                  ItemCode = grp == null ? null : grp.ItemCode,
                                  Qty = grp == null ? null : grp.Qty,
                                  BarCode = grp == null ? null : grp.BarCode,
                                  ContainerDetailID = grp == null ? null : grp.F_Id,
                                  ContainerKind = grp == null ? null : grp.ContainerKind,
                                  ContainerState = grp == null ? null : grp.State,
                                  ItemID = grp == null ? null : grp.ItemID,
                                  ItemName = grp == null ? null : grp.ItemName,
                                  Lot = grp == null ? null : grp.Lot
                              })

                    .GroupBy(o => new
                    {
                        o.AreaCode,
                        o.AreaName,
                        o.AreaType,
                        o.LocationID,
                        o.LocationCode,
                        o.LocationType,
                        o.ContainerKind,
                        o.ContainerID,
                        o.LocationState,
                        o.BarCode,
                        o.Line,
                        o.Layer,
                        o.ColNum,
                        o.Deep,
                        o.ForbiddenState
                    })
                    .Select(m => new
                    {
                        m.Key.AreaCode,
                        m.Key.AreaName,
                        m.Key.AreaType,
                        m.Key.LocationID,
                        m.Key.LocationCode,
                        m.Key.LocationType,
                        m.Key.Line,
                        m.Key.Layer,
                        m.Key.ColNum,
                        m.Key.Deep,
                        m.Key.ContainerKind,
                        m.Key.ContainerID,
                        m.Key.BarCode,
                        m.Key.LocationState,
                        m.Key.ForbiddenState,
                        IsHasContainer = m.Where(k => k.ContainerDetailID != null).ToList().Count > 0,
                        ContainerList = m.Where(k => k.ContainerDetailID != null).ToList()
                    }).OrderBy(o=>o.AreaCode).ThenBy(o => o.Line).ThenBy(o => o.ColNum).ThenBy(o => o.Layer).ToList();
                var postData = new { TaskList = TaskList, InEquList = InEquList, OutEquList = OutEquList, LocationList = data };
                return Content(postData.ToJson());
            }
        }
        #endregion
    }
}
