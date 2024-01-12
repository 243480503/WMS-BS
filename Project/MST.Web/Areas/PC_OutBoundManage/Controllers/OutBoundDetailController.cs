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
using MST.Code.Extend;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_OutBoundManage.Controllers
{
    public class OutBoundDetailController : ControllerBase
    {
        private T_OutBoundApp outBoundApp = new T_OutBoundApp();
        private T_OutBoundDetailApp OutBoundDetailApp = new T_OutBoundDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ItemApp t_ItemApp = new T_ItemApp();
        private T_StationApp t_stationApp = new T_StationApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();

        #region 获取出库单明细
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string outBoundID, string keyword)
        {
            var data = OutBoundDetailApp.GetList(pagination, outBoundID, keyword);
            IList<OutBoundDetailModel> detailModelList = data.ToObject<IList<OutBoundDetailModel>>();

            foreach (OutBoundDetailModel model in detailModelList)
            {
                model.StateName = itemsDetailApp.FindEnum<T_OutBoundDetailEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                T_StationEntity t_stationEntity = t_stationApp.FindEntity(o => o.F_Id == model.StationID);
                model.StationName = t_stationEntity.StationName;
                model.StationCode = t_stationEntity.StationCode;

                T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == model.ItemID);
                T_ContainerTypeEntity containerTypeEntity = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
                model.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == containerTypeEntity.ContainerKind).F_ItemName;
            }

            var resultList = new
            {
                rows = detailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 获取新建出库单明细弹窗右侧明细列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemSelectGridJson(Pagination pagination, string outBoundID, string keyword)
        {
            T_OutBoundEntity outbound = outBoundApp.FindEntity(o => o.F_Id == outBoundID);
            var data = OutBoundDetailApp.GetList(pagination, outBoundID, keyword);
            IList<OutBoundDetailModel> detailModelList = data.ToObject<IList<OutBoundDetailModel>>();
            foreach (OutBoundDetailModel model in detailModelList)
            {
                model.F_Id = model.ItemID + model.Lot;//对应左侧的ID
                model.StateName = itemsDetailApp.FindEnum<T_OutBoundDetailEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                T_StationEntity t_stationEntity = t_stationApp.FindEntity(o => o.F_Id == model.StationID);
                model.StationName = t_stationEntity.StationName;
                model.StationCode = t_stationEntity.StationCode;
                T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == model.ItemID);
                model.UnitQty = item.UnitQty;
                T_ContainerTypeEntity containerTypeEntity = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
                model.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == containerTypeEntity.ContainerKind).F_ItemName;

                IList<T_ContainerDetailEntity> detailList = new T_ContainerDetailApp().FindList(o => o.ItemID == model.ItemID && o.Lot == model.Lot).ToList();
                model.AllQty = detailList.Sum(o => o.Qty);
                if (outbound.OutBoundType == "VerBackOut") //验退，需增加入库单维度
                {
                    model.CanUseQty = detailList.Where(o => o.State != "Freeze"  /// 过滤系统数据 && 过滤冻结物料 
                                            && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()    /// 过滤站台物料
                                            && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false" && o.InBoundID == outbound.PointInBoundID /// 过滤质检冻结、盘点冻结物料
                                            && o.CheckState == "UnQua").Sum(o => o.Qty - o.OutQty - o.CheckQty);
                }
                else if (outbound.OutBoundType == "OtherOut")
                {
                    model.CanUseQty = detailList.Where(o => o.State != "Freeze"  /// 过滤系统数据 && 过滤冻结物料 
                                            && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()    /// 过滤站台物料
                                            && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false" /// 过滤质检冻结、盘点冻结物料
                                            && (o.CheckState == "WaitCheck" || o.CheckState == "UnNeed"
                                                  || o.CheckState == "Qua" || o.CheckState == "UnQua")).Sum(o => o.Qty - o.OutQty - o.CheckQty);
                }
                else
                {
                    model.CanUseQty = detailList.Where(o => o.State != "Freeze"  /// 过滤系统数据 && 过滤冻结物料 
                                            && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()    /// 过滤站台物料
                                            && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false" /// 过滤质检冻结、盘点冻结物料
                                            && (o.CheckState == "Qua" || o.CheckState == "UnNeed")).Sum(o => o.Qty - o.OutQty - o.CheckQty);

                }
                T_ContainerEntity container = new T_ContainerApp().FindEntity(o => o.ContainerType == item.ContainerType);
                model.ContainerKindName = new ItemsDetailApp().FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == container.ContainerKind).F_ItemName;
            }
            var resultList = new
            {
                rows = detailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(detailModelList.ToJson());
        }
        #endregion

        #region 右侧明细的来源入库单下拉数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetInOrderList(string outBoundID, string itemID, string lot)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                T_OutBoundEntity outbound = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == outBoundID);
                IList<string> RefInBoundCode = new List<string>();
                var expression = ExtLinq.True<T_ContainerDetailEntity>();
                string orderType = outbound.OutBoundType;
                switch (orderType)
                {
                    case "GetItemOut":
                        {
                            expression = expression.And(o => o.ItemID == itemID && o.Lot == lot && (o.CheckState == "Qua" || o.CheckState == "UnNeed"));
                        }
                        break;
                    case "OtherOut":
                        {
                            expression = expression.And(o => o.ItemID == itemID && o.Lot == lot);
                        }
                        break;
                    case "VerBackOut":
                        {
                            expression = expression.And(o => o.ItemID == itemID && o.Lot == lot && (o.CheckState == "UnQua" || o.CheckState == "WaitCheck"));
                        }
                        break;
                    case "WarehouseBackOut":
                        {
                            expression = expression.And(o => o.ItemID == itemID && o.Lot == lot && (o.CheckState == "Qua" || o.CheckState == "UnNeed"));
                        }
                        break;
                    default:
                        {
                            return Error("单据类型不正确", "");
                        }
                }
                RefInBoundCode = db.FindList<T_ContainerDetailEntity>(expression).Select(o => o.ERPInDocCode).Distinct().OrderBy(o => o).ToList();
                return Content(RefInBoundCode.ToJson());
            }
        }
        #endregion

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = OutBoundDetailApp.GetForm(keyValue);
            return Content(data.ToJson());
        }


        #region 获取左侧列表物料数据(领料和验退公用)
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemList(Pagination pagination, string OutBoundID, string kindID, string keyword)
        {
            T_OutBoundEntity outbound = outBoundApp.FindEntity(o => o.F_Id == OutBoundID);

            /// 过滤系统数据 
            IList<string> itemSys = new T_ItemApp().FindList(o => o.IsBase == "true").Select(o => o.F_Id).Distinct().ToArray();

            List<T_ContainerDetailEntity> data = containerDetailApp.GetInitList(pagination, kindID, keyword);

            List<LeftItem> query = new List<LeftItem>();

            if (outbound.OutBoundType == "VerBackOut") //验退，需增加入库单维度
            {
                query = data.Where(o => !itemSys.Contains(o.ItemID) && o.State != "Freeze"  /// 过滤系统数据 && 过滤冻结物料 
                                            && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()    /// 过滤站台物料
                                            && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false" && o.InBoundID == outbound.PointInBoundID /// 过滤质检冻结、盘点冻结物料
                                            && o.CheckState == "UnQua")
                                                 .GroupBy(o => new { F_Id = (o.ItemID + o.Lot), ItemID = o.ItemID, ContainerKind = o.ContainerKind, ItemCode = o.ItemCode, ItemName = o.ItemName, Lot = o.Lot, Factory = o.Factory, Spec = o.Spec, ItemUnitText = o.ItemUnitText, OverdueDate = o.OverdueDate, CheckState = o.CheckState })
                                     .Select(o => new LeftItem
                                     {
                                         F_Id = o.Key.F_Id,
                                         ItemID = o.Key.ItemID,
                                         ItemCode = o.Key.ItemCode,
                                         ItemName = o.Key.ItemName,
                                         Factory = o.Key.Factory,
                                         Lot = o.Key.Lot,
                                         Spec = o.Key.Spec,
                                         ItemUnitText = o.Key.ItemUnitText,
                                         OverdueDate = o.Key.OverdueDate,
                                         CheckState = o.Key.CheckState,
                                         CanUseQty = o.Sum(k => k.Qty - k.OutQty - k.CheckQty),
                                         ContainerKind = o.Key.ContainerKind,
                                         AllQty = data.Where(j => j.ItemID == o.Key.ItemID && j.Lot == o.Key.Lot).Sum(p => p.Qty)
                                     }).ToList();
            }
            else if (outbound.OutBoundType == "OtherOut")
            {
                query = data.Where(o => !itemSys.Contains(o.ItemID) && o.State != "Freeze"  /// 过滤系统数据 && 过滤冻结物料 
                                            && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()    /// 过滤站台物料
                                            && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false" /// 过滤质检冻结、盘点冻结物料
                                            && (o.CheckState == "WaitCheck" || o.CheckState == "UnNeed"
                                              || o.CheckState == "Qua" || o.CheckState == "UnQua")
                                            ).GroupBy(o => new { F_Id = (o.ItemID + o.Lot), ItemID = o.ItemID, ContainerKind = o.ContainerKind, ItemCode = o.ItemCode, ItemName = o.ItemName, Factory = o.Factory, Lot = o.Lot, Spec = o.Spec, ItemUnitText = o.ItemUnitText, OverdueDate = o.OverdueDate, CheckState = o.CheckState })
                                    .Select(o => new LeftItem
                                    {
                                        F_Id = o.Key.F_Id,
                                        ItemID = o.Key.ItemID,
                                        ItemCode = o.Key.ItemCode,
                                        ItemName = o.Key.ItemName,
                                        Factory = o.Key.Factory,
                                        Lot = o.Key.Lot,
                                        Spec = o.Key.Spec,
                                        ItemUnitText = o.Key.ItemUnitText,
                                        OverdueDate = o.Key.OverdueDate,
                                        CheckState = o.Key.CheckState,
                                        CanUseQty = o.Sum(k => k.Qty - k.OutQty - k.CheckQty),
                                        ContainerKind = o.Key.ContainerKind,
                                        AllQty = data.Where(j => j.ItemID == o.Key.ItemID && j.Lot == o.Key.Lot).Sum(p => p.Qty)
                                    }).ToList();
            }
            else
            {
                query = data.Where(o => !itemSys.Contains(o.ItemID) && o.State != "Freeze"  /// 过滤系统数据 && 过滤冻结物料 
                                            && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()    /// 过滤站台物料
                                            && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false" /// 过滤质检冻结、盘点冻结物料
                                            && (o.CheckState == "Qua" || o.CheckState == "UnNeed")
                                            ).GroupBy(o => new { F_Id = (o.ItemID + o.Lot), ItemID = o.ItemID, ContainerKind = o.ContainerKind, ItemCode = o.ItemCode, ItemName = o.ItemName, Factory = o.Factory, Lot = o.Lot, Spec = o.Spec, ItemUnitText = o.ItemUnitText, OverdueDate = o.OverdueDate, CheckState = o.CheckState })
                                    .Select(o => new LeftItem
                                    {
                                        F_Id = o.Key.F_Id,
                                        ItemID = o.Key.ItemID,
                                        ItemCode = o.Key.ItemCode,
                                        ItemName = o.Key.ItemName,
                                        Factory = o.Key.Factory,
                                        Lot = o.Key.Lot,
                                        Spec = o.Key.Spec,
                                        ItemUnitText = o.Key.ItemUnitText,
                                        OverdueDate = o.Key.OverdueDate,
                                        CheckState = o.Key.CheckState,
                                        CanUseQty = o.Sum(k => k.Qty - k.OutQty - k.CheckQty),
                                        ContainerKind = o.Key.ContainerKind,
                                        AllQty = data.Where(j => j.ItemID == o.Key.ItemID && j.Lot == o.Key.Lot).Sum(p => p.Qty)
                                    }).ToList();

            }

            T_ItemApp itemApp = new T_ItemApp();
            List<ItemsDetailEntity> dicList = new ItemsDetailApp().FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind);
            foreach (LeftItem cell in query)
            {
                T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == cell.ItemID);
                cell.ContainerKindName = dicList.FirstOrDefault(o => o.F_ItemCode == cell.ContainerKind).F_ItemName;
                cell.UnitQty = item.UnitQty;
                if (cell.OverdueDate != null && item.ValidityWarning != null)
                {
                    var days = ((DateTime)cell.OverdueDate).Subtract(DateTime.Now).Days;
                    if (days < 0) cell.OverdueWarning = 2;
                    else if (days <= item.ValidityWarning) cell.OverdueWarning = 1;
                }
            }

            query = query.GetPage(pagination, null).ToList();
            var resultList = new
            {
                rows = query,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
            //return Content(query.ToJson());
        }

        public class LeftItem
        {
            public string F_Id { get; set; }
            public string ItemID { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Factory { get; set; } /// 生产厂家
            public string Lot { get; set; }
            public string Spec { get; set; }
            public DateTime? OverdueDate { get; set; }
            public string ContainerKindName { get; set; } /// 容器大类名称
            public string ContainerKind { get; set; }
            public string CheckState { get; set; }
            public decimal? CanUseQty { get; set; } /// 可用库存
            public decimal? AllQty { get; set; }    /// 总库存
            public decimal? UnitQty { get; set; }   /// 单位数量
            /// <summary>
            /// 计量单位名称
            /// </summary>
            public string ItemUnitText { get; set; }
            /// <summary>
            /// 物料失效预警：0 正常 1 预警 2 失效
            /// </summary>
            public int OverdueWarning { get; set; } = 0;
        }
        #endregion

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult TransferForm()
        {
            return View();
        }

        #region 新建/修改出库明细(可能没用到)
        //[HttpPost]
        //[HandlerAjaxOnly]
        //[ValidateAntiForgeryToken]
        //public ActionResult SubmitForm(T_OutBoundDetailEntity OutBoundDetailEntity, string keyValue)
        //{
        //    LogObj logObj = new LogObj();
        //    logObj.Path = "OutBoundDetailController.SubmitForm";
        //    logObj.Parms = new { OutBoundDetailEntity = OutBoundDetailEntity, keyValue = keyValue };

        //    LogEntity logEntity = new LogEntity();
        //    logEntity.F_ModuleName = "出库明细";
        //    logEntity.F_Type = DbLogType.Submit.ToString();
        //    logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
        //    logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
        //    logEntity.F_Description = "新建/修改出库明细";
        //    logEntity.F_Path = logObj.Path;
        //    logEntity.F_Param = logObj.Parms.ToJson();

        //    try
        //    {
        //        T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == OutBoundDetailEntity.ItemID);
        //        T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
        //        string containerKind = containerType.ContainerKind;
        //        if (containerKind == "Rack")
        //        {
        //            T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
        //            OutBoundDetailEntity.StationID = t_Station.F_Id;
        //        }
        //        else if (containerKind == "Plastic")
        //        {
        //            T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
        //            OutBoundDetailEntity.StationID = t_Station.F_Id;
        //        }
        //        else if (containerKind == "Box")
        //        {
        //            T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
        //            OutBoundDetailEntity.StationID = t_Station.F_Id;
        //        }
        //        else
        //        {
        //            return Error("未知的容器类型", "");
        //        }
        //        OutBoundDetailEntity.WaveQty = 0;
        //        //OutBoundDetailEntity.ActionType = "Init";
        //        OutBoundDetailApp.SubmitForm(OutBoundDetailEntity, keyValue);

        //        logObj.Message = "操作成功";
        //        LogFactory.GetLogger().Info(logObj);

        //        logEntity.F_Result = true;
        //        new LogApp().WriteDbLog(logEntity);

        //        return Success("操作成功。");
        //    }
        //    catch (Exception ex)
        //    {
        //        logObj.Message = ex;
        //        LogFactory.GetLogger().Error(logObj);

        //        logEntity.F_Result = false;
        //        logEntity.F_Msg = ex.ToJson();
        //        new LogApp().WriteDbLog(logEntity);

        //        return Error("操作失败。", ex.ToJson());
        //    }
        //}
        #endregion

        #region PC保存右侧选中列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFormList(string OutBoundDetailEntityListStr, string OutBoundID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutBoundDetailController.SubmitFormList"; //按实际情况修改
                logObj.Parms = new { OutBoundEntity = OutBoundDetailEntityListStr, keyValue = OutBoundID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存出库单明细"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_OutBoundEntity OutBoundEntity = outBoundApp.FindEntity(o => o.F_Id == OutBoundID);
                    if (OutBoundEntity.State != "New")
                    {
                        return Error("单据不是新建状态。", "");
                    }

                    IList<T_OutBoundDetailEntity> OutBoundDetailList = OutBoundDetailEntityListStr.ToObject<IList<T_OutBoundDetailEntity>>();

                    foreach (T_OutBoundDetailEntity entity in OutBoundDetailList)
                    {
                        T_OutBoundDetailEntity outBoundDetail = OutBoundDetailApp.FindEntity(o => o.OutBoundID == entity.OutBoundID && o.ItemID == entity.ItemID && o.Lot == entity.Lot);
                        if (outBoundDetail == null)
                        {
                            outBoundDetail = new T_OutBoundDetailEntity();

                            outBoundDetail.OutBoundID = OutBoundID;
                            T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == entity.ItemID);
                            outBoundDetail.ItemCode = item.ItemCode;
                            outBoundDetail.ItemName = item.ItemName;
                            outBoundDetail.ItemID = item.F_Id;
                            outBoundDetail.Factory = item.Factory;
                            outBoundDetail.Qty = entity.Qty;
                            outBoundDetail.OutQty = 0;
                            outBoundDetail.WaveQty = 0;
                            outBoundDetail.Lot = entity.Lot;
                            outBoundDetail.Spec = entity.Spec;
                            outBoundDetail.ItemUnitText = entity.ItemUnitText;
                            outBoundDetail.OverdueDate = entity.OverdueDate;
                            outBoundDetail.SEQ = entity.SEQ;
                            outBoundDetail.SourceInOrderCode = entity.SourceInOrderCode;

                            T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
                            string containerKind = containerType.ContainerKind;
                            if (containerKind == "Rack")
                            {
                                T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                outBoundDetail.StationID = t_Station.F_Id;
                                outBoundDetail.StationCode = t_Station.StationCode;
                            }
                            else if (containerKind == "Plastic")
                            {
                                T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                outBoundDetail.StationID = t_Station.F_Id;
                                outBoundDetail.StationCode = t_Station.StationCode;
                            }
                            else if (containerKind == "Box")
                            {
                                T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                outBoundDetail.StationID = t_Station.F_Id;
                                outBoundDetail.StationCode = t_Station.StationCode;
                            }
                            else
                            {
                                return Error("物料的容器类型未知:" + item.ItemCode, "");
                            }

                            outBoundDetail.F_Id = Guid.NewGuid().ToString();
                            outBoundDetail.State = "New";
                            outBoundDetail.ActionType = "Init";
                            db.Insert<T_OutBoundDetailEntity>(outBoundDetail);
                        }
                        else
                        {
                            outBoundDetail.Qty = entity.Qty;
                            outBoundDetail.SourceInOrderCode = entity.SourceInOrderCode;
                            db.Update<T_OutBoundDetailEntity>(outBoundDetail);
                        }
                    }

                    string[] updateID = OutBoundDetailList.Select(o => o.ItemID + o.Lot).ToArray();
                    IList<T_OutBoundDetailEntity> needDelList = OutBoundDetailApp.FindList(o => (!updateID.Contains(o.ItemID + o.Lot)) && o.OutBoundID == OutBoundID).ToList();
                    foreach (T_OutBoundDetailEntity delcell in needDelList)
                    {
                        db.Delete<T_OutBoundDetailEntity>(delcell);
                    }

                    db.CommitWithOutRollBack();

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作失败。", ex.ToJson());
                }
            }
        }
        #endregion


        #region 设置强制完成出库明细
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult OutOver(string OutBoundDetailID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutBoundDetailController.ReceiveOver"; //按实际情况修改
                logObj.Parms = new { OutBoundDetailID = OutBoundDetailID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "强制完成收货"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_OutBoundDetailEntity entity = db.FindEntity<T_OutBoundDetailEntity>(OutBoundDetailID);

                    if (entity == null)
                    {
                        return Error("单据明细不存在。", "");
                    }

                    entity.State = "Over";
                    db.Update<T_OutBoundDetailEntity>(entity);
                    db.SaveChanges();

                    //收货单所有明细数据均为收货完成，则更新为收货完成
                    IList<T_OutBoundDetailEntity> notOverList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == entity.OutBoundID && o.State != "Over").ToList();
                    if (notOverList.Count == 0) //清空整个站台单据信息
                    {
                        T_OutBoundEntity OutBound = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == entity.OutBoundID);
                        OutBound.State = "Over";
                        db.Update<T_OutBoundEntity>(OutBound);

                        T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.CurOrderID == entity.OutBoundID);
                        stationEntity.OrderType = "";
                        stationEntity.CurOrderDetailID = "";
                        stationEntity.CurOrderID = "";
                        db.Update<T_StationEntity>(stationEntity);
                    }
                    db.CommitWithOutRollBack();

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作失败。", ex.ToJson());
                }
            }
        }
        #endregion


        #region 删除出库明细
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "OutBoundDetailController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "出库明细";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除出库明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_OutBoundDetailEntity data = OutBoundDetailApp.GetForm(keyValue);
                if (data.State != "New") return Error("非新建状态不可删除", "");

                OutBoundDetailApp.DeleteForm(keyValue);

                logObj.Message = "删除成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("删除成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("删除失败。", ex.ToJson());
            }
        }
        #endregion
    }
}
