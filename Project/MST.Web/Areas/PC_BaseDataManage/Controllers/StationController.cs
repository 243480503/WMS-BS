/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.APIPost;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Application.WebMsg;
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
using static MST.Application.APIPost.WCSPost;

namespace MST.Web.Areas.PC_BaseDataManage.Controllers
{
    public class StationController : ControllerBase
    {
        private T_StationApp stationApp = new T_StationApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_DevRowApp devRowApp = new T_DevRowApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_AreaApp areaApp = new T_AreaApp();


        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult BindForm()
        {
            return View();
        }

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult PrintPage()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            IList<T_StationEntity> data = stationApp.GetList(pagination, keyword);
            IList<StationModel> modelList = new List<StationModel>();
            IList<ItemsDetailEntity> detailEntityUseCodeList = itemsDetailApp.FindEnum<T_StationEntity>(o => o.UseCode);
            IList<ItemsDetailEntity> detailEntityCurOrderTypeList = itemsDetailApp.FindEnum<T_StationEntity>(o => o.OrderType);
            IList<ItemsDetailEntity> detailEntityCurModelTypeList = itemsDetailApp.FindEnum<T_StationEntity>(o => o.CurModel);
            foreach (T_StationEntity st in data)
            {
                StationModel model = st.ToObject<StationModel>();

                string useCodeName = "";
                if (!string.IsNullOrEmpty(st.UseCode))
                {
                    string[] list = st.UseCode.Split(',');
                    foreach (string use in list)
                    {
                        string useName = detailEntityUseCodeList.FirstOrDefault(o => o.F_ItemCode == use).F_ItemName;
                        useCodeName = useCodeName == "" ? useName : (useCodeName + "," + useName);
                    }
                }
                model.StationName = st.StationName;
                model.StationCode = st.StationCode;
                if (!string.IsNullOrEmpty(st.OrderType))
                {
                    model.CurOrderID = st.CurOrderID;
                    model.CurOrderTypeName = detailEntityCurOrderTypeList.FirstOrDefault(o => o.F_ItemCode == st.OrderType).F_ItemName;
                    string curOrderType = st.OrderType;
                    switch (curOrderType)
                    {
                        case "PurchaseIn": //采购入库
                            {
                                T_InBoundEntity t_InBoundEntity = new T_InBoundApp().FindEntity(o => o.F_Id == st.CurOrderID);
                                model.CurOrderCode = string.IsNullOrEmpty(t_InBoundEntity.RefOrderCode) ? t_InBoundEntity.InBoundCode : t_InBoundEntity.RefOrderCode;
                            }
                            break;
                        case "OtherOut": //其它出库
                        case "VerBackOut": //验退出库
                        case "GetItemOut": //领料出库
                        case "WarehouseBackOut": //仓退出库
                            {
                                T_OutBoundEntity t_OutBoundEntity = new T_OutBoundApp().FindEntity(o => o.F_Id == st.CurOrderID);
                                model.CurOrderCode = string.IsNullOrEmpty(t_OutBoundEntity.RefOrderCode) ? t_OutBoundEntity.OutBoundCode : t_OutBoundEntity.RefOrderCode;
                            }
                            break;
                        case "Count":  //盘点单
                            {
                                T_CountEntity t_CountEntity = new T_CountApp().FindEntity(o => o.F_Id == st.CurOrderID);
                                model.CurOrderCode = string.IsNullOrEmpty(t_CountEntity.RefOrderCode) ? t_CountEntity.CountCode : t_CountEntity.RefOrderCode;
                            }
                            break;
                        case "GetSample": //质检取样
                        case "BackSample": //质检还样
                            {
                                T_QAEntity t_QAEntity = new T_QAApp().FindEntity(o => o.F_Id == st.CurOrderID);
                                model.CurOrderCode = string.IsNullOrEmpty(t_QAEntity.QACode) ? t_QAEntity.RefOrderCode : t_QAEntity.QACode;
                            }
                            break;
                        case "OffRack"://下架
                            {
                                T_OffRackEntity offRack = new T_OffRackApp().FindEntity(o => o.F_Id == st.CurOrderID);
                                model.CurOrderCode = string.IsNullOrEmpty(offRack.RefOrderCode) ? offRack.OffRackCode : offRack.RefOrderCode;
                            }
                            break;
                        case "EmptyIn"://空容器入库(不需处理)
                            {

                            }
                            break;
                        case "LocCount": /// 货位盘点单
                            {
                                T_LocCountEntity locCount = new T_LocCountApp().FindEntity(o => o.F_Id == st.CurOrderID);
                                model.CurOrderCode = string.IsNullOrEmpty(locCount.RefOrderCode) ? locCount.LocCountCode : locCount.RefOrderCode;
                            }
                            break;
                        default:
                            {
                                return Success("未知的当前单据类型。");
                            }
                    }

                }
                model.UseCodeName = useCodeName;
                model.CurModelName = detailEntityCurModelTypeList.FirstOrDefault(o => o.F_ItemCode == st.CurModel).F_ItemName;
                modelList.Add(model);
            }

            var resultList = new
            {
                rows = modelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        #region 获取站台信息字典
        /// <summary>
        /// 获取站台对应的单据种类
        /// </summary>
        /// <param name="StationID"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDic_OrderType(string StationID)
        {
            //F_ItemCode
            //F_ItemName
            if (string.IsNullOrEmpty(StationID))
            {
                return Error("请选择站台", "");
            }
            IList<ItemsDetailEntity> list = new List<ItemsDetailEntity>();
            T_StationEntity station = stationApp.FindEntity(o => o.F_Id == StationID);
            if (station.StationCode == FixType.Station.StationIn_BigItem.ToString() || station.StationCode == FixType.Station.StationIn_Normal.ToString())
            {
                ItemsDetailEntity cell = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "PurchaseIn");
                list.Add(cell);
                return Content(list.ToJson());
            }
            else if (station.StationCode == FixType.Station.StationOut_BigItem.ToString() || station.StationCode == FixType.Station.StationOut_Normal.ToString())
            {
                ItemsDetailEntity cell1 = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "BackSample");
                ItemsDetailEntity cell2 = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "Count");
                ItemsDetailEntity cell3 = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "GetItemOut");
                ItemsDetailEntity cell4 = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "GetSample");
                ItemsDetailEntity cell5 = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "VerBackOut");
                ItemsDetailEntity cell6 = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "WarehouseBackOut");
                ItemsDetailEntity cell7 = new ItemsDetailApp().FindEnum<T_StationEntity>(o => o.OrderType).FirstOrDefault(o => o.F_ItemCode == "OtherOut");

                list.Add(cell1);
                list.Add(cell2);
                list.Add(cell3);
                list.Add(cell4);
                list.Add(cell5);
                list.Add(cell6);
                list.Add(cell7);
                return Content(list.ToJson());
            }
            else
            {
                return Content(list.ToJson());
            }
        }


        /// <summary>
        /// 选择站台单据种类后，获取该类型的单据
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDic_CurOrderCode(string orderType)
        {
            IList<ItemsDetailEntity> list = new List<ItemsDetailEntity>();
            string orderTypeTemp = orderType;
            switch (orderTypeTemp)
            {
                case "PurchaseIn":
                    {
                        list = new T_InBoundApp().FindList(o => o.State != "Over").ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.InBoundCode, F_ItemCode = o.InBoundCode }).ToList();
                        return Content(list.ToJson());
                    }
                case "OtherOut":
                case "WarehouseBackOut":
                case "VerBackOut":
                case "GetItemOut":
                    {
                        list = new T_OutBoundApp().FindList(o => o.State != "Over").ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.OutBoundCode, F_ItemCode = o.OutBoundCode }).ToList();
                        return Content(list.ToJson());
                    }
                case "Count":
                    {
                        list = new T_CountApp().FindList(o => o.State != "Over").ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.CountCode, F_ItemCode = o.CountCode }).ToList();
                        return Content(list.ToJson());
                    }
                case "BackSample":
                    {
                        list = new T_QAApp().FindList(o => o.State != "Over" && o.QAOrderType == "BackSample").ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.QACode, F_ItemCode = o.QACode }).ToList();
                        return Content(list.ToJson());
                    }
                case "GetSample":
                    {
                        list = new T_QAApp().FindList(o => o.State != "Over" && o.QAOrderType == "GetSample").ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.QACode, F_ItemCode = o.QACode }).ToList();
                        return Content(list.ToJson());
                    }
                default:
                    {
                        return Error("站台设置与单据类型不对应", "");
                    }
            }
        }

        /// <summary>
        /// 选定该单据后，获取单据明细SEQ
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="CurOrderCode"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDic_SEQ(string orderType, string CurOrderCode)
        {
            IList<ItemsDetailEntity> list = new List<ItemsDetailEntity>();
            string orderTypeTemp = orderType;
            switch (orderTypeTemp)
            {
                case "PurchaseIn":
                    {
                        T_InBoundEntity inbound = new T_InBoundApp().FindEntity(o => o.InBoundCode == CurOrderCode);
                        list = new T_InBoundDetailApp().FindList(o => o.State != "Over" && o.InBoundID == inbound.F_Id).ToList().Select(o => new ItemsDetailEntity { F_ItemName = (o.SEQ ?? 0).ToString(), F_ItemCode = (o.SEQ ?? 0).ToString() }).OrderBy(o => o.F_ItemCode).ToList();
                        return Content(list.ToJson());
                    }
                case "OtherOut":
                case "WarehouseBackOut":
                case "VerBackOut":
                case "GetItemOut":
                    {
                        T_OutBoundEntity outbound = new T_OutBoundApp().FindEntity(o => o.OutBoundCode == CurOrderCode);
                        list = new T_OutBoundDetailApp().FindList(o => o.State != "Over" && o.OutBoundID == outbound.F_Id).ToList().Select(o => new ItemsDetailEntity { F_ItemName = (o.SEQ ?? 0).ToString(), F_ItemCode = (o.SEQ ?? 0).ToString() }).OrderBy(o => o.F_ItemCode).ToList();
                        return Content(list.ToJson());
                    }
                case "Count":
                    {
                        T_CountEntity outbound = new T_CountApp().FindEntity(o => o.CountCode == CurOrderCode);
                        list = new T_CountDetailApp().FindList(o => o.CountState != "Over" && o.CountID == outbound.F_Id).ToList().Select(o => new ItemsDetailEntity { F_ItemName = (o.SEQ ?? 0).ToString(), F_ItemCode = (o.SEQ ?? 0).ToString() }).OrderBy(o => o.F_ItemCode).ToList();
                        return Content(list.ToJson());
                    }
                case "BackSample":
                case "GetSample":
                    {
                        T_QAEntity qa = new T_QAApp().FindEntity(o => o.QACode == CurOrderCode);
                        list = new T_QADetailApp().FindList(o => o.State != "Over" && o.QAID == qa.F_Id).ToList().Select(o => new ItemsDetailEntity { F_ItemName = (o.SEQ ?? 0).ToString(), F_ItemCode = (o.SEQ ?? 0).ToString() }).OrderBy(o => o.F_ItemCode).ToList();
                        return Content(list.ToJson());
                    }
                default:
                    {
                        return Error("站台设置与单据类型不对应", "");
                    }
            }
        }


        /// <summary>
        /// 获取单据明细SEQ后，获取容器编码
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="CurOrderCode"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDic_BarCode(string orderType, string CurOrderCode, string SEQ)
        {
            int seqInt = Convert.ToInt32(SEQ);
            IList<ItemsDetailEntity> list = new List<ItemsDetailEntity>();
            string orderTypeTemp = orderType;
            switch (orderTypeTemp)
            {
                case "PurchaseIn":
                    {
                        return Content(list.ToJson());
                    }
                case "OtherOut":
                case "WarehouseBackOut":
                case "VerBackOut":
                case "GetItemOut":
                    {
                        T_OutBoundEntity head = new T_OutBoundApp().FindEntity(o => o.OutBoundCode == CurOrderCode);
                        T_OutBoundDetailEntity detail = new T_OutBoundDetailApp().FindEntity(o => o.OutBoundID == head.F_Id && o.SEQ == seqInt);
                        list = new T_OutRecordApp().FindList(o => o.State != "OverPick" && o.OutBoundID == head.F_Id && o.OutBoundDetailID == detail.F_Id).ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.BarCode, F_ItemCode = o.BarCode }).Distinct().OrderBy(o => o.F_ItemName).ToList();
                        return Content(list.ToJson());
                    }
                case "Count":
                    {
                        T_CountEntity head = new T_CountApp().FindEntity(o => o.CountCode == CurOrderCode);
                        T_CountDetailEntity detail = new T_CountDetailApp().FindEntity(o => o.CountID == head.F_Id && o.SEQ == seqInt);
                        list = new T_CountRecordApp().FindList(o => o.CountState != "Over" && o.CountID == head.F_Id && o.CountDetailID == detail.F_Id).ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.BarCode, F_ItemCode = o.BarCode }).Distinct().OrderBy(o => o.F_ItemName).ToList();
                        return Content(list.ToJson());
                    }
                case "BackSample":
                case "GetSample":
                    {
                        T_QAEntity head = new T_QAApp().FindEntity(o => o.QACode == CurOrderCode);
                        T_QADetailEntity detail = new T_QADetailApp().FindEntity(o => o.QAID == head.F_Id && o.SEQ == seqInt);
                        list = new T_QARecordApp().FindList(o => o.State != "Over").ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.BarCode, F_ItemCode = o.BarCode }).Distinct().OrderBy(o => o.F_ItemName).ToList();
                        return Content(list.ToJson());
                    }
                default:
                    {
                        return Error("站台设置与单据类型不对应", "");
                    }
            }
        }


        /// <summary>
        /// 获取单据明细SEQ后，获取波次
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="CurOrderCode"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDic_WaveCode(string orderType, string CurOrderCode, string SEQ)
        {
            int seqInt = Convert.ToInt32(SEQ);
            IList<ItemsDetailEntity> list = new List<ItemsDetailEntity>();
            string orderTypeTemp = orderType;
            switch (orderTypeTemp)
            {
                case "OtherOut":
                case "WarehouseBackOut":
                case "VerBackOut":
                case "GetItemOut":
                    {
                        T_OutBoundEntity outHead = new T_OutBoundApp().FindEntity(o => o.OutBoundCode == CurOrderCode);
                        T_OutBoundDetailEntity outDetail = new T_OutBoundDetailApp().FindEntity(o => o.SEQ == seqInt && o.OutBoundID == outHead.F_Id);
                        IList<T_WaveDetailEntity> detailList = new T_WaveDetailApp().FindList(o => o.OutBoundDetailID == outDetail.F_Id).ToList();
                        string[] waveDetailArray = detailList.Select(o => o.WaveID).ToArray();
                        list = new T_WaveApp().FindList(o => waveDetailArray.Contains(o.F_Id) && o.State == "Execing").ToList().Select(o => new ItemsDetailEntity { F_ItemName = o.WaveCode, F_ItemCode = o.WaveCode }).Distinct().OrderBy(o => o.F_ItemName).ToList();
                        return Content(list.ToJson());
                    }
                default:
                    {
                        return Error("站台设置与单据类型不对应", "");
                    }
            }
        }
        #endregion


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = stationApp.GetForm(keyValue);
            StationModel model = data.ToObject<StationModel>();
            model.UseCodeList = new List<T_ItemEntity>();
            if (!string.IsNullOrEmpty(model.OrderType))
            {
                string orderType = model.OrderType;
                if (model.StationCode == FixType.Station.StationIn_BigItem.ToString() || model.StationCode == FixType.Station.StationIn_Normal.ToString())
                {
                    switch (orderType)
                    {
                        case "PurchaseIn":
                            {
                                T_InBoundEntity inbound = new T_InBoundApp().FindEntity(o => o.F_Id == model.CurOrderID);
                                model.CurOrderCode = inbound.InBoundCode;
                                T_InBoundDetailEntity detail = new T_InBoundDetailApp().FindEntity(o => o.F_Id == model.CurOrderDetailID);
                                model.SEQ = detail.SEQ;
                            }
                            break;
                        default:
                            {
                                return Error("站台设置与单据类型不对应", "");
                            }
                    }
                }
                else if (model.StationCode == FixType.Station.StationOut_BigItem.ToString() || model.StationCode == FixType.Station.StationOut_Normal.ToString())
                {
                    switch (orderType)
                    {
                        case "OtherOut":
                        case "WarehouseBackOut":
                        case "VerBackOut":
                        case "GetItemOut":
                            {
                                T_OutBoundEntity outbound = new T_OutBoundApp().FindEntity(o => o.F_Id == model.CurOrderID);
                                model.CurOrderCode = outbound.OutBoundCode;
                                T_OutBoundDetailEntity detail = new T_OutBoundDetailApp().FindEntity(o => o.OutBoundID == model.CurOrderID);
                                model.SEQ = detail.SEQ;
                                T_WaveEntity wave = new T_WaveApp().FindEntity(o => o.F_Id == model.WaveID);
                                model.WaveID = wave.F_Id;
                                model.WaveCode = wave.WaveCode;
                            }
                            break;
                        case "Count":
                            {
                                T_CountEntity count = new T_CountApp().FindEntity(o => o.F_Id == model.CurOrderID);
                                model.CurOrderCode = count.CountCode;
                                T_CountDetailEntity detail = new T_CountDetailApp().FindEntity(o => o.CountID == model.CurOrderID);
                                model.SEQ = detail.SEQ;
                            }
                            break;
                        case "BackSample":
                        case "GetSample":
                            {
                                T_QAEntity qa = new T_QAApp().FindEntity(o => o.F_Id == model.CurOrderID);
                                model.CurOrderCode = qa.QACode;
                                T_QADetailEntity detail = new T_QADetailApp().FindEntity(o => o.QAID == model.CurOrderID);
                                model.SEQ = detail.SEQ;
                            }
                            break;
                        default:
                            {
                                return Error("站台设置与单据类型不对应", "");
                            }
                    }
                }
                else
                {
                    return Error("未知站台", "");
                }
            }
            Array useCodeArray = data.UseCode.Split(',');
            foreach (string use in useCodeArray)
            {
                T_ItemEntity itemEntity = new T_ItemEntity() { ItemCode = use };
                model.UseCodeList.Add(itemEntity);
            }
            return Content(model.ToJson());
        }


        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_StationEntity stationEntity, string keyValue)
        {
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            if (!string.IsNullOrEmpty(keyValue))
            {
                if (!user.IsSystem)
                {
                    T_StationEntity entity = stationApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许修改。", "");
                    }
                }
            }

            int isExistsCode = stationApp.FindList(o => o.StationCode == stationEntity.StationCode && o.F_Id != keyValue).Count();
            if (isExistsCode > 0)
            {
                return Error("站台编码已存在", "");
            }

            stationApp.SubmitForm(stationEntity, keyValue);
            return Success("操作成功。");
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeModel(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "StationController.ChangeModel";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "基础数据";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "更换站台模式";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.F_Id == keyValue);

                    if (!stationEntity.UseCode.Contains("EmptyIn"))
                    {
                        return Error("站台不允许空箱入库", "");
                    }
                    if (stationEntity.CurModel == "Normal")
                    {
                        if (string.IsNullOrEmpty(stationEntity.CurOrderID))
                        {
                            stationEntity.CurModel = "Empty";
                            stationEntity.OrderType = "EmptyIn";
                        }
                        else
                        {
                            return Error("站台已被占用", "");
                        }
                    }
                    else if (stationEntity.CurModel == "Empty")
                    {
                        stationEntity.CurModel = "Normal";
                        stationEntity.OrderType = "";
                    }
                    else
                    {
                        return Error("站台模式未知", "");
                    }

                    db.Update<T_StationEntity>(stationEntity);
                    db.SaveChanges();
                    db.CommitWithOutRollBack();
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

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ClearStation(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "StationController.ClearStation";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "基础数据";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "清空站台";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.F_Id == keyValue);
                    T_StationEntity beforeChangeStation = stationEntity.ToObject<T_StationEntity>();
                    stationEntity.WaveID = "";
                    stationEntity.BarCode = "";
                    stationEntity.OrderType = "";
                    stationEntity.CurOrderDetailID = "";
                    stationEntity.CurOrderID = "";
                    stationEntity.Remark = "";
                    db.Update<T_StationEntity>(stationEntity);
                    db.CommitWithOutRollBack();

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

        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            if (!user.IsSystem)
            {
                T_StationEntity entity = stationApp.FindEntity(o => o.F_Id == keyValue);
                if (entity.IsBase == "true")
                {
                    return Error("系统数据不允许删除。", "");
                }
            }

            stationApp.DeleteForm(keyValue);
            return Success("删除成功。");
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult BindOrder(StationModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "StationController.BindOrder";
                logObj.Parms = new { model = model };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "基础数据";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "绑定单据";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    string waveID = "";
                    string barCode = "";
                    string orderType = "";
                    string curOrderDetailID = "";
                    string curOrderID = "";
                    string orderTypeTemp = model.OrderType;
                    switch (orderTypeTemp)
                    {
                        case "PurchaseIn":
                            {
                                T_InBoundEntity head = new T_InBoundApp().FindEntity(o => o.InBoundCode == model.CurOrderCode);
                                T_InBoundDetailEntity detail = new T_InBoundDetailApp().FindEntity(o => o.InBoundID == head.F_Id && o.SEQ == model.SEQ);
                                barCode = model.BarCode;
                                orderType = model.OrderType;
                                curOrderDetailID = detail.F_Id;
                                curOrderID = head.F_Id;
                            }
                            break;
                        case "OtherOut":
                        case "WarehouseBackOut":
                        case "VerBackOut":
                        case "GetItemOut":
                            {
                                T_OutBoundEntity head = new T_OutBoundApp().FindEntity(o => o.OutBoundCode == model.CurOrderCode);
                                T_OutBoundDetailEntity detail = new T_OutBoundDetailApp().FindEntity(o => o.OutBoundID == head.F_Id && o.SEQ == model.SEQ);
                                T_WaveEntity wave = new T_WaveApp().FindEntity(o => o.WaveCode == model.WaveCode);
                                waveID = wave.F_Id;
                                barCode = model.BarCode;
                                orderType = model.OrderType;
                                curOrderDetailID = detail.F_Id;
                                curOrderID = head.F_Id;
                            }
                            break;
                        case "Count":
                            {
                                T_CountEntity head = new T_CountApp().FindEntity(o => o.CountCode == model.CurOrderCode);
                                T_CountDetailEntity detail = new T_CountDetailApp().FindEntity(o => o.CountID == head.F_Id && o.SEQ == model.SEQ);
                                barCode = model.BarCode;
                                orderType = model.OrderType;
                                curOrderID = head.F_Id;
                                curOrderDetailID = detail.F_Id;
                            }
                            break;
                        case "BackSample":
                        case "GetSample":
                            {
                                T_QAEntity head = new T_QAApp().FindEntity(o => o.QACode == model.CurOrderCode);
                                T_QADetailEntity detail = new T_QADetailApp().FindEntity(o => o.QAID == head.F_Id && o.SEQ == model.SEQ);
                                barCode = model.BarCode;
                                orderType = model.OrderType;
                                curOrderID = head.F_Id;
                                curOrderDetailID = detail.F_Id;
                            }
                            break;
                        default:
                            {
                                return Error("站台设置与单据类型不对应", "");
                            }
                    }

                    T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.F_Id == model.F_Id);
                    stationEntity.WaveID = waveID;
                    stationEntity.BarCode = barCode;
                    stationEntity.OrderType = orderType;
                    stationEntity.CurOrderDetailID = curOrderDetailID;
                    stationEntity.CurOrderID = curOrderID;

                    db.Update<T_StationEntity>(stationEntity);
                    db.SaveChanges();
                    db.CommitWithOutRollBack();

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

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult GenLabel(string num)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "StationController.GenLabel";
                logObj.Parms = new { num = num };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "新容器打印";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "新容器打印";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    int printNum = Convert.ToInt32(num);
                    WCSPost post = new WCSPost();
                    for (int i = printNum; i < printNum; i++)
                    {
                        string code = T_CodeGenApp.GenNum("ContainerBarRule");
                        PrintItemBarCodePostModel model = new PrintItemBarCodePostModel();
                        model.ItemBarCode = code;
                        post.PrintItemBarCode(model);
                    }

                    db.CommitWithOutRollBack();

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
    }
}
