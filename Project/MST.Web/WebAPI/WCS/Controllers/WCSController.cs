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
using MST.Web.Areas.RF_EmptyContainerManage.Controllers;
using MST.Web.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static MST.Application.WMSLogic.T_TaskApp;
using static MST.Web.WebSocket.MsgHub;

namespace MST.Web.WebAPI.WCS.Controllers
{
    [HandlerLogin(false)]
    public class WCSController : Controller
    {
        #region 请求主方法
        [HttpPost]
        public string Ask()
        {
            string returnStr = "";
            bool logDB = true;

            UserEntity user = new UserApp().GetEntity("WCS");
            OperatorModel operatorModel = new OperatorModel();
            operatorModel.UserCode = user.F_Account;
            operatorModel.UserId = user.F_Id;
            operatorModel.UserName = user.F_RealName;
            OperatorProvider.Provider.AddCurrent(operatorModel);

            StreamReader sRead = new StreamReader(HttpContext.Request.InputStream);
            string data = sRead.ReadToEnd();
            sRead.Close();


            LogObj logObj = new LogObj();
            logObj.Path = "WCSController.Ask"; //按实际情况修改
            logObj.Parms = data; //按实际情况修改
            logObj.CurTime = DateTime.Now;

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "WCS请求Ask接口"; //按实际情况修改
            logEntity.F_Type = DbLogType.Visit.ToString(); //按实际情况修改
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = data;

            WCSResult result = new WCSResult();
            try
            {
                /*************************************************/

                WCSModel askModel = data.ToObject<WCSModel>();

                switch (askModel.Method)
                {
                    case "ApplyInTask":  //站台申请入库
                        {
                            ApplyInTaskModel applyInModel = askModel.PostData.ToObject<ApplyInTaskModel>();
                            T_TaskApp taskApp = new T_TaskApp();
                            result = taskApp.ApplyInTask(applyInModel);
                        }
                        break;
                    case "StateChangeTask": //任务状态反馈
                        {
                            StateChangeTaskModel stateChangeTaskModel = askModel.PostData.ToObject<StateChangeTaskModel>();
                            T_TaskApp taskApp = new T_TaskApp();
                            result = taskApp.StateChangeTask(stateChangeTaskModel);
                        }
                        break;
                    case "StateChangeDev": //设备状态反馈
                        {
                            
                        }
                        break;
                    case "GetCurItemInfo": //正常入库站台调用，返回当前物料打印信息与容器长宽高；正常出库站台调用，只返回容器长宽高
                        {
                            GetCurItemInfoModel curItemInfoModel = askModel.PostData.ToObject<GetCurItemInfoModel>();
                            result = GetCurItemInfo(curItemInfoModel);
                        }
                        break;
                    case "CountData":  //反馈盘点数据
                        {
                            CountDataModel countModel = askModel.PostData.ToObject<CountDataModel>();
                            result = CountData(countModel);
                        }
                        break;
                    case "EmptyPlasticOut": //空料箱抽屉出库
                        {
                            EmptyOutPlasticModel emptyOutPlasticModel = askModel.PostData.ToObject<EmptyOutPlasticModel>();
                            result = EmptyOutPlastic(emptyOutPlasticModel);

                            if (result.IsSuccess == true)
                            {
                                //呼叫空容器(不论是否成功，都继续执行)
                                T_StationEntity stationEntity = new T_StationApp().FindEntity(o => o.StationCode == FixType.Station.StationEmpty.ToString());
                                AjaxResult res = new EmptyContainerController().EmptyCallOut(stationEntity.F_Id, 1, emptyOutPlasticModel.LocationCode);
                            }

                        }
                        break;
                    case "IsOutBack": //出库口判断是否需要回库
                        {
                            IsOutBackModel inOutBackModel = askModel.PostData.ToObject<IsOutBackModel>();
                            result = IsOutBack(inOutBackModel);
                        }
                        break;
                    case "IsCanMove": //正常入库口电控按钮判断是否需可移动（电控按钮只有料箱入库时会按下）
                        {
                            IsCanMoveModel isCanMoveModel = askModel.PostData.ToObject<IsCanMoveModel>();
                            result = IsCanMove(isCanMoveModel);
                        }
                        break;
                    case "PrintCode": //贴标机打印申请
                        {
                            PrintCodeModel printCodeModel = askModel.PostData.ToObject<PrintCodeModel>();
                            result = PrintCode(printCodeModel);
                        }
                        break;
                    case "CCD": //图像自动识别（贴标前的缓存位调用）
                        {
                            CCDModel ccdModel = askModel.PostData.ToObject<CCDModel>();
                            result = CCD(ccdModel);
                        }
                        break;
                    case "Pho":
                        {
                            string barCode = HttpContext.Request.Form["BarCode"];
                            PhoModel phoModel = new PhoModel() { BarCode = barCode };
                            if (HttpContext.Request.Files.Count < 1)
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "图片不能为空";
                            }
                            else
                            {
                                HttpPostedFileBase file = HttpContext.Request.Files[0];
                                result = Pho(phoModel, file);
                            }
                        }
                        break;
                    case "ExceptionLog":
                        {
                            logDB = false;
                            ExceptionLogModel excModel = askModel.PostData.ToObject<ExceptionLogModel>();
                            result = ExceptionLog(excModel);
                        }
                        break;
                    case "PostAVGCode": //反馈当前任务所执行的AGV（扫码申请后需反馈AGV小车编码）
                        {
                            PostAVGCodeModel model = askModel.PostData.ToObject<PostAVGCodeModel>();
                            result = PostAVGCode(model);
                        }
                        break;
                    case "CallEmpty": //已作废：呼叫空容器(同时支持大件站台料架、空料箱暂存区料箱)
                        {
                            CallEmptyModel model = askModel.PostData.ToObject<CallEmptyModel>();
                            result = CallEmpty(model);
                        }
                        break;
                    case "CTULeave":
                        {
                            CTULeaveModel model = askModel.PostData.ToObject<CTULeaveModel>();
                            result = CTULeave(model);
                        }
                        break;
                    default:
                        {
                            throw new Exception("未知的方法类型");
                        };
                }

                /**************************************************/
                if (result.IsSuccess)
                {
                    logObj.Message = "操作成功";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    if (logDB)
                    {
                        logEntity.F_Result = true;
                        logEntity.F_Msg = result.ToJson();
                        logEntity.F_Description = askModel.Method;
                        new LogApp().WriteDbLog(logEntity);
                    }
                }
                else
                {
                    logObj.Message = "操作失败";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    if (logDB)
                    {
                        logEntity.F_Result = false;
                        logEntity.F_Msg = result.ToJson();
                        logEntity.F_Description = askModel.Method;
                        new LogApp().WriteDbLog(logEntity);
                    }
                }

                returnStr = result.ToJson();
                return returnStr;
            }
            catch (ArgumentNullException ex)
            {
                logObj.Message = "ArgumentNullException:" + ex.Message;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                if (logDB)
                {
                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                }

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return returnStr;
            }
            catch (EntityException ex)
            {
                logObj.Message = "EntityException:" + ex.Message;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                if (logDB)
                {
                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                }

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return returnStr;
            }

            catch (DbEntityValidationException ex)
            {
                logObj.Message = "DbEntityValidationException:" + ex.Message;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                if (logDB)
                {
                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                }

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return returnStr;
            }
            catch (Exception ex)
            {

                logObj.Message = "Exception:" + ex.Message;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                if (logDB)
                {
                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                }

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return returnStr;
            }
        }
        #endregion

        #region 上传照片方法（前端UI）
        [HttpPost]
        public string UploadPho()
        {
            string returnStr = "";

            UserEntity user = new UserApp().GetEntity("WCS");
            OperatorModel operatorModel = new OperatorModel();
            operatorModel.UserCode = user.F_Account;
            operatorModel.UserId = user.F_Id;
            operatorModel.UserName = user.F_RealName;
            OperatorProvider.Provider.AddCurrent(operatorModel);

            StreamReader sRead = new StreamReader(HttpContext.Request.InputStream);
            string data = sRead.ReadToEnd();
            sRead.Close();


            LogObj logObj = new LogObj();
            logObj.Path = "WCSController.UploadPho"; //按实际情况修改
            logObj.Parms = new { data = data }; //按实际情况修改

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "WCS请求Ask接口"; //按实际情况修改
            logEntity.F_Type = DbLogType.Visit.ToString(); //按实际情况修改
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            WCSResult result = new WCSResult();
            try
            {
                /*************************************************/
                string barCode = HttpContext.Request.Form["BarCode"];
                PhoModel phoModel = new PhoModel() { BarCode = barCode };
                if (HttpContext.Request.Files.Count < 1)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "图片不能为空";
                }
                else
                {
                    HttpPostedFileBase file = HttpContext.Request.Files[0];
                    result = Pho(phoModel, file);
                }

                /**************************************************/
                if (result.IsSuccess)
                {
                    logObj.Message = "操作成功";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    logEntity.F_Msg = result.ToJson();
                    logEntity.F_Description = "UploadPho";
                    new LogApp().WriteDbLog(logEntity);
                }
                else
                {
                    logObj.Message = "操作失败";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = result.ToJson();
                    logEntity.F_Description = "UploadPho";
                    new LogApp().WriteDbLog(logEntity);
                }

                returnStr = result.ToJson();
                return returnStr;
            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return returnStr;
            }
        }
        #endregion

        #region 设备状态反馈（WCS调用）
        public class StateChangeDevModel
        {
            /// <summary>
            /// 设备类型：CTU,LMR,Runner
            /// </summary>
            public string DevType { get; set; } /// 设备类型
            public object Info { get; set; }   /// 设备消息
        }

        #endregion

        #region 获取当前站台绑定的物料信息（WCS调用）
        public class GetCurItemInfoModel
        {
            public string ApplyStationCode { get; set; }    /// 申请入库站台编码（正常入库口:StationIn_Normal , 正常出库口:StationOut_Normal）

        }

        public class CurItemInfo
        {
            /// <summary>
            /// 是否空料箱
            /// </summary>
            public string IsEmpty { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string UnitQty { get; set; }
            public string ItemUnitText { get; set; }
            public string Lot { get; set; }
            public string SupplierUserName { get; set; }
            public string ProductDate { get; set; }
            public string Spec { get; set; }
            public string Factory { get; set; }
            public string ContainerKind { get; set; }

            public string ContainerType { get; set; }
            public string BorderLong { get; set; }
            public string BorderWidth { get; set; }
            public string BorderHeigh { get; set; }
            public string OrderCode { get; set; }
            public string SEQ { get; set; }
            public string OrderType { get; set; }
        }

        /// <summary>
        /// ApplyStationCode=StationIn_Normal：正常入库站台调用，返回当前物料打印信息与容器长宽高；ApplyStationCode=StationOut_Normal：正常出库站台调用，只返回容器长宽高
        /// </summary>
        /// <param name="curItemInfoModel"></param>
        /// <returns></returns>
        public WCSResult GetCurItemInfo(GetCurItemInfoModel curItemInfoModel)
        {
            WCSResult result = new WCSResult();
            T_StationEntity stationEntity = new T_StationApp().FindEntity(o => o.StationCode == curItemInfoModel.ApplyStationCode);
            if (stationEntity == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "未找到此站台编号";
            }
            else
            {
                if (string.IsNullOrEmpty(stationEntity.CurOrderID) && stationEntity.OrderType != "EmptyIn")
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "此站台无单据";
                }
                else
                {
                    T_ItemEntity itemEntity = new T_ItemEntity();
                    T_InBoundEntity inBoundEntity = null;
                    T_InBoundDetailEntity inBoundDetailEntity = null;
                    T_ContainerTypeEntity containerTypeEntity = new T_ContainerTypeEntity();
                    string orderType = stationEntity.OrderType;
                    CurItemInfo itemInfo = new CurItemInfo();
                    switch (orderType)
                    {
                        case "PurchaseIn": /// 采购入库(与杨工讨论，仅入库时需请求，出库口的入库WCS自己已记录长宽高，2021-08-11)
                            {
                                inBoundEntity = new T_InBoundApp().FindEntity(o => o.F_Id == stationEntity.CurOrderID);
                                inBoundDetailEntity = new T_InBoundDetailApp().FindEntity(o => o.F_Id == stationEntity.CurOrderDetailID);
                                itemEntity = new T_ItemApp().FindEntity(o => o.F_Id == inBoundDetailEntity.ItemID);
                                containerTypeEntity = new T_ContainerTypeApp().FindEntity(o => o.ContainerTypeCode == itemEntity.ContainerType);

                                itemInfo.IsEmpty = "false";
                                itemInfo.ItemCode = ((itemEntity == null || string.IsNullOrEmpty(itemEntity.ItemCode)) ? "" : itemEntity.ItemCode);
                                itemInfo.ItemName = ((itemEntity == null || string.IsNullOrEmpty(itemEntity.ItemName)) ? "" : itemEntity.ItemName);
                                itemInfo.UnitQty = (itemEntity == null || itemEntity.UnitQty == null) ? "" : itemEntity.UnitQty.ToString();
                                itemInfo.ItemUnitText = ((itemEntity == null || string.IsNullOrEmpty(itemEntity.ItemUnitText)) ? "" : itemEntity.ItemUnitText);
                                itemInfo.Lot = ((inBoundDetailEntity == null || string.IsNullOrEmpty(inBoundDetailEntity.Lot)) ? "" : inBoundDetailEntity.Lot);
                                itemInfo.SupplierUserName = ((inBoundEntity == null || string.IsNullOrEmpty(inBoundEntity.SupplierUserName)) ? "" : inBoundEntity.SupplierUserName);
                                itemInfo.ProductDate = ((inBoundDetailEntity == null || inBoundDetailEntity.ProductDate == null)) ? "" : inBoundDetailEntity.ProductDate.Value.ToString("yyyy-MM-dd");
                                itemInfo.Spec = ((itemEntity == null || string.IsNullOrEmpty(itemEntity.Spec)) ? "" : itemEntity.Spec);
                                itemInfo.Factory = ((itemEntity == null || string.IsNullOrEmpty(itemEntity.Factory)) ? "" : itemEntity.Factory);
                                itemInfo.ContainerKind = containerTypeEntity.ContainerKind;
                                itemInfo.BorderLong = (containerTypeEntity.BorderLong == null) ? "" : containerTypeEntity.BorderLong.ToString();
                                itemInfo.BorderWidth = (containerTypeEntity.BorderWidth == null) ? "" : (containerTypeEntity.BorderWidth - 7).ToString();//-7因为纸箱夹紧夹不紧
                                itemInfo.BorderHeigh = (containerTypeEntity.BorderHeight == null) ? "" : containerTypeEntity.BorderHeight.ToString();
                                itemInfo.OrderCode = inBoundEntity.InBoundCode;
                                itemInfo.SEQ = (inBoundDetailEntity.SEQ ?? 0).ToString();
                                itemInfo.OrderType = orderType.ToString();
                                itemInfo.ContainerType = containerTypeEntity.ContainerTypeCode;
                            }
                            break;
                        case "EmptyIn": /// 空料箱入库
                            {
                                T_ItemEntity itemEmpty = new T_ItemApp().FindEntity(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                containerTypeEntity = new T_ContainerTypeApp().FindEntity(o => o.ContainerTypeCode == FixType.ContainerType.StandardPlastic.ToString());
                                itemInfo.IsEmpty = "true";
                                itemInfo.ItemCode = itemEmpty.ItemCode;
                                itemInfo.ItemName = itemEntity.ItemName;
                                itemInfo.ContainerKind = containerTypeEntity.ContainerKind;
                                itemInfo.BorderLong = (containerTypeEntity.BorderLong == null) ? "" : containerTypeEntity.BorderLong.ToString();
                                itemInfo.BorderWidth = (containerTypeEntity.BorderWidth == null) ? "" : containerTypeEntity.BorderWidth.ToString();
                                itemInfo.BorderHeigh = (containerTypeEntity.BorderHeight == null) ? "" : containerTypeEntity.BorderHeight.ToString();
                                itemInfo.OrderType = orderType.ToString();
                                itemInfo.ContainerType = containerTypeEntity.ContainerTypeCode;
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "此站台单据类型无效";
                            }
                            break;
                    }

                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    result.Data = itemInfo;
                }
            }
            return result;
        }
        #endregion

        #region 盘点数据反馈（WCS调用，货位盘与物料盘公用）
        public class CountDataModel
        {
            /// <summary>
            /// 任务编号
            /// </summary>
            public string TaskNo { get; set; }
            /// <summary>
            /// 盘点结果（Find：找到箱码，NoFind：未找到箱码）
            /// </summary>
            public string State { get; set; }
            /// <summary>
            /// 盘点箱码（盘点结果为Find时必填）
            /// </summary>
            public string BarCode { get; set; }
            /// <summary>
            /// 货位码(必填,正常情况应由WCS判断是否一致)
            /// </summary>
            public string LocationCode { get; set; }
        }

        private static object CountDataObj = new object();
        public WCSResult CountData(CountDataModel countDataModel)
        {
            lock (CountDataObj)
            {
                using (var db = new RepositoryBase().BeginTrans())
                {
                    return CountDataLogic(db, countDataModel);
                }
            }
        }

        public WCSResult CountDataLogic(IRepositoryBase db, CountDataModel countDataModel)
        {
            WCSResult result = new WCSResult();
            T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();

            try
            {
                T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.TaskNo == countDataModel.TaskNo);
                if (taskEntity == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "未找到此任务编码";
                    return result;
                }
                else
                {
                    string curOrderType = taskEntity.OrderType;
                    switch (curOrderType)
                    {
                        case "Count":
                            {
                                bool isSuccess = true;

                                T_CountRecordEntity countRecord = db.FindEntity<T_CountRecordEntity>(o => o.CountDetailID == taskEntity.OrderDetailID);
                                /// 货位码不一致
                                if (taskEntity.TagLocationCode != countDataModel.LocationCode)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "指定货位与实际扫描的货位码不相符";
                                    return result;
                                }
                                else
                                {
                                    List<T_CountRecordEntity> countRecordList = db.FindList<T_CountRecordEntity>(o => o.CountID == taskEntity.OrderID && o.BarCode == taskEntity.BarCode).ToList();
                                    /// 找到容器条码
                                    if (countDataModel.State.ToLower() == "find")
                                    {
                                        if (string.IsNullOrEmpty(countDataModel.BarCode))   /// 找到容器情况，容器条码必填
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "找到容器情况，容器条码必填";
                                            return result;
                                        }

                                        if (taskEntity.BarCode == countDataModel.BarCode) /// 箱码一致
                                        {
                                            /// 记录盘点，结束在库盘点任务
                                            foreach (var item in countRecordList)
                                            {
                                                item.CountResult = "Inner_SameBoxCode";
                                                item.CountState = "Over";
                                                item.CountQty = item.Qty;
                                                db.Update<T_CountRecordEntity>(item);

                                                /// 更新盘点单明细数量
                                                T_CountDetailEntity detail = db.FindEntity<T_CountDetailEntity>(o => o.F_Id == item.CountDetailID);
                                                if (detail.CountQty == null) detail.CountQty = 0;
                                                detail.CountQty += item.CountQty;
                                                db.Update<T_CountDetailEntity>(detail);
                                            }
                                        }
                                        else
                                        {
                                            /// 记录不一致箱码
                                            if (string.IsNullOrEmpty(taskEntity.BarCode))
                                            {
                                                foreach (var item in countRecordList)
                                                {
                                                    item.CountResult = "Inner_MoreBoxCode"; /// 多余箱码：多余的库存（审核通过后需要新增库存）
                                                    item.CountState = "Over";
                                                    db.Update<T_CountRecordEntity>(item);
                                                }
                                            }
                                            else
                                            {
                                                foreach (var item in countRecordList)
                                                {
                                                    item.CountResult = "Inner_DiffBoxCode"; /// 箱码不一致：审核通过后需要更换箱码？
                                                    item.FactBarCode = countDataModel.BarCode;
                                                    item.CountState = "Over";
                                                    db.Update<T_CountRecordEntity>(item);
                                                }
                                            }
                                        }

                                        result.IsSuccess = true;
                                        result.FailCode = "0000";
                                        isSuccess = true;
                                    }
                                    else if (countDataModel.State.ToLower() == "nofind") /// 未找到箱码
                                    {
                                        if (!string.IsNullOrEmpty(countDataModel.BarCode))   /// 未找到的情况，容器条码应为空
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "未找到容器情况，容器条码应为空";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(taskEntity.BarCode))
                                        {
                                            foreach (var item in countRecordList)
                                            {
                                                item.CountResult = "Inner_Empty";   /// 正常空货位
                                                item.CountState = "Over";
                                                db.Update<T_CountRecordEntity>(item);
                                            }
                                        }
                                        else
                                        {
                                            foreach (var item in countRecordList)
                                            {
                                                item.CountResult = "Inner_NotFindBoxCode";  /// 未找到箱码：货不见了
                                                item.CountState = "Over";
                                                db.Update<T_CountRecordEntity>(item);
                                            }
                                        }

                                        result.IsSuccess = true;
                                        result.FailCode = "0000";
                                        isSuccess = true;
                                    }
                                    else
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "未知的状态编码";
                                        return result;
                                    }

                                    if (isSuccess)
                                    {
                                        taskEntity.State = "Over";
                                        taskEntity.OverTime = DateTime.Now;
                                        db.Update<T_TaskEntity>(taskEntity);

                                        T_TaskApp taskApp = new T_TaskApp();
                                        taskApp.MoveToHis(db, taskEntity.TaskNo);
                                        db.SaveChanges();

                                        /// 更新货位状态 纸箱 Out -> Stored
                                        T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.F_Id == taskEntity.TagLocationID);
                                        location.State = "Stored";
                                        db.Update<T_LocationEntity>(location);

                                        /// 货位状态变更记录
                                        locStateApp.SyncLocState(db, location, "InType", "Count", "Out", "Stored", taskEntity.TaskNo);

                                        List<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == taskEntity.OrderID);
                                        foreach (T_CountDetailEntity detail in detailList)
                                        {
                                            List<T_CountRecordEntity> recordList = db.FindList<T_CountRecordEntity>(o => o.CountID == taskEntity.OrderID && o.ItemID == detail.ItemID
                                                                    && (o.Lot == detail.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(detail.Lot)))
                                                                    && (o.CountState == "Outing" || o.CountState == "Counting"));
                                            if (recordList.Count == 0)
                                            {
                                                /// 更新容器物料所属明细状态
                                                detail.CountState = "WaitAudit";
                                                db.Update<T_CountDetailEntity>(detail);
                                                db.SaveChanges();
                                            }
                                        }

                                        if (detailList.All(o => o.CountState == "WaitAudit"))
                                        {
                                            T_CountEntity count = db.FindEntity<T_CountEntity>(o => o.F_Id == taskEntity.OrderID);
                                            count.State = "WaitAudit";
                                            db.Update<T_CountEntity>(count);
                                        }

                                        db.SaveChanges();
                                        db.CommitWithOutRollBack();
                                    }
                                }
                                return result;
                            }
                        case "LocCount":
                            {
                                bool isSuccess = true;
                                bool isNormalResult = false;
                                bool isErrResult = false;

                                T_LocCountRecordEntity locCountRecord = db.FindEntity<T_LocCountRecordEntity>(o => o.LocCountID == taskEntity.OrderID && o.LocationCode == taskEntity.TagLocationCode);
                                T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.F_Id == taskEntity.TagLocationID);

                                if (taskEntity.TagLocationCode != countDataModel.LocationCode) /// 货位码不一致
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "指定货位与实际扫描的货位码不相符";
                                    return result;
                                }
                                else
                                {
                                    if (countDataModel.State.ToLower() == "find") /// 找到容器条码
                                    {
                                        if (string.IsNullOrEmpty(countDataModel.BarCode))   /// 找到的情况，容器条码不能为空
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "找到容器情况，容器条码必填";
                                            return result;
                                        }

                                        if (taskEntity.BarCode == countDataModel.BarCode) /// 箱码一致
                                        {
                                            isNormalResult = true;

                                            locCountRecord.CountResult = "Inner_SameBoxCode";    /// 箱码一致
                                            locCountRecord.CountState = "Over";
                                            db.Update<T_LocCountRecordEntity>(locCountRecord);
                                        }
                                        else
                                        {
                                            /// 记录不一致箱码
                                            if (string.IsNullOrEmpty(taskEntity.BarCode))
                                            {
                                                isErrResult = true;

                                                locCountRecord.CountResult = "Inner_MoreBoxCode"; /// 多余箱码：多余的库存   处理：直接出库
                                                locCountRecord.CountState = "WaitConfirm";
                                                locCountRecord.FactBarCode = countDataModel.BarCode;
                                                db.Update<T_LocCountRecordEntity>(locCountRecord);
                                            }
                                            else
                                            {
                                                isErrResult = true;

                                                locCountRecord.CountResult = "Inner_DiffBoxCode"; /// 箱码不一致    处理：出库再重新分配货位
                                                locCountRecord.CountState = "WaitConfirm";
                                                locCountRecord.FactBarCode = countDataModel.BarCode;
                                                db.Update<T_LocCountRecordEntity>(locCountRecord);
                                            }
                                        }

                                        result.IsSuccess = true;
                                        result.FailCode = "0000";
                                        isSuccess = true;
                                    }
                                    else if (countDataModel.State.ToLower() == "nofind")
                                    {
                                        if (!string.IsNullOrEmpty(countDataModel.BarCode))   /// 未找到的情况，容器条码应为空
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "未找到容器情况，容器条码应为空";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(taskEntity.BarCode))
                                        {
                                            isNormalResult = true;

                                            locCountRecord.CountResult = "Inner_Empty";   /// 正常空货位
                                            locCountRecord.CountState = "Over";
                                            db.Update<T_LocCountRecordEntity>(locCountRecord);
                                        }
                                        else
                                        {
                                            isErrResult = true;

                                            locCountRecord.CountResult = "Inner_NotFindBoxCode";  /// 未找到箱码   处理：锁定库存
                                            locCountRecord.CountState = "WaitConfirm";
                                            db.Update<T_LocCountRecordEntity>(locCountRecord);
                                        }

                                        result.IsSuccess = true;
                                        result.FailCode = "0000";
                                        isSuccess = true;
                                    }
                                    else
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "未知的状态编码";
                                        return result;
                                    }

                                    if (isSuccess)
                                    {
                                        taskEntity.State = "Over";
                                        taskEntity.OverTime = DateTime.Now;
                                        db.Update<T_TaskEntity>(taskEntity);

                                        T_TaskApp taskApp = new T_TaskApp();
                                        taskApp.MoveToHis(db, taskEntity.TaskNo);
                                        db.SaveChanges();

                                        /// 更新货位状态 纸箱 Out -> Stored / Empty
                                        if (isNormalResult) /// 正常空货位 or 箱码一致
                                        {
                                            if (string.IsNullOrEmpty(taskEntity.BarCode)) location.State = "Empty";
                                            else location.State = "Stored";
                                            db.Update<T_LocationEntity>(location);
                                            db.SaveChanges();

                                            /// 货位状态变更记录
                                            string locState = location.State;
                                            locStateApp.SyncLocState(db, location, "InType", "Count", "Out", locState, taskEntity.TaskNo);
                                        }

                                        /// 禁用异常货位
                                        if (isErrResult)
                                        {
                                            location.ForbiddenState = "Lock";
                                            db.Update<T_LocationEntity>(location);
                                            db.SaveChanges();
                                        }

                                        /// 更新货位盘点单状态
                                        T_LocCountEntity locCount = db.FindEntity<T_LocCountEntity>(o => o.F_Id == taskEntity.OrderID);

                                        List<T_LocCountRecordEntity> recordList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == taskEntity.OrderID);
                                        if (recordList.All(o => o.CountState == "Over"))
                                        {
                                            /// 全部 完成
                                            locCount.State = "Over";
                                        }
                                        else if (recordList.All(o => o.CountState == "Over" || o.CountState == "WaitConfirm"))
                                        {
                                            /// 全部 完成/待处理异常
                                            locCount.State = "WaitConfirm";
                                        }

                                        db.Update<T_LocCountEntity>(locCount);

                                        db.SaveChanges();
                                        db.CommitWithOutRollBack();
                                    }
                                }
                                return result;
                            }
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "不是AGV在库盘点的单据";
                                return result;
                            }
                    }
                }
            }
            catch (Exception)
            {
                db.RollBack();
                throw;
            }
        }
        #endregion

        #region 空料箱抽屉出库（WCS调用）
        public class EmptyOutPlasticModel
        {
            /// <summary>
            /// 货位码
            /// </summary>
            public string LocationCode { get; set; }
        }


        public WCSResult EmptyOutPlastic(EmptyOutPlasticModel emptyOutPlasticModel)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                WCSResult result = new WCSResult();
                try
                {
                    if (string.IsNullOrEmpty(emptyOutPlasticModel.LocationCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "货位编码不可为空";
                        return result;
                    }
                    else
                    {
                        T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == emptyOutPlasticModel.LocationCode);
                        if (loc == null)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "该货位不存在";
                            return result;
                        }
                        if (loc.AreaCode != FixType.Area.EmptyArea.ToString())
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "货位不属于空箱暂存区";
                            return result;
                        }

                        if (loc.State != "Stored")
                        {
                            result.IsSuccess = false;
                            return result;
                        }
                        loc.State = "Empty";
                        db.Update<T_LocationEntity>(loc);

                        T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.LocationNo == loc.LocationCode && o.F_DeleteMark == false);
                        container.LocationNo = "";
                        container.LocationID = "";
                        container.AreaCode = "";
                        container.AreaName = "";
                        container.AreaID = "";
                        container.F_DeleteMark = true; //空料箱暂存区在箱子拿走后，需假删除容器信息,料架不在此处删除，而是在空容器出库任务完成时做假删除
                        db.Update<T_ContainerEntity>(container);

                        T_ContainerDetailEntity containerDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.LocationNo == loc.LocationCode); //空托盘只有1个明细
                        T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                        inOutDetailApp.SyncInOutDetail(db, containerDetail, "OutType", "EmptyOut", 1, containerDetail.Qty, null);
                        db.Delete<T_ContainerDetailEntity>(containerDetail);
                        db.SaveChanges();
                        db.CommitWithOutRollBack();

                        result.IsSuccess = true;
                        result.FailCode = "0000";
                        return result;
                    }
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region 出库判断是否回库（WCS调用）
        public class IsOutBackModel
        {
            public string BarCode { get; set; } /// 容器编码
            public string ApplyStationCode { get; set; }    /// 申请站台
        }


        public WCSResult IsOutBack(IsOutBackModel isOutBackModel)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                WCSResult result = new WCSResult();
                try
                {
                    if (string.IsNullOrEmpty(isOutBackModel.BarCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "容器编码不可为空";
                        return result;
                    }

                    if (string.IsNullOrEmpty(isOutBackModel.ApplyStationCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "申请站台不可为空";
                        return result;
                    }
                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == isOutBackModel.ApplyStationCode);

                    if (station == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "申请站台不存在";
                        return result;
                    }

                    if (string.IsNullOrEmpty(station.OrderType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "申请站台不存在单据";
                        return result;
                    }

                    bool isCanBack = false;
                    bool isCanDown = false;
                    string orderType = station.OrderType;
                    switch (orderType)
                    {
                        case "OtherOut"://其它出库
                        case "VerBackOut": //验退出库
                        case "WarehouseBackOut": //仓退出库
                        case "GetItemOut": //领料出库
                            {
                                IList<T_OutRecordEntity> outRecordList = db.FindList<T_OutRecordEntity>(o => o.BarCode == isOutBackModel.BarCode && o.WaveID == station.WaveID && o.OutBoundID == station.CurOrderID).ToList();
                                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == isOutBackModel.BarCode && o.F_DeleteMark == false);
                                if (container == null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "容器不存在";
                                    return result;
                                }
                                if (outRecordList.Where(o => o.State == "Picking").Count() > 0) //未拣完，不允许回库,不允许拿下
                                {
                                    isCanBack = false;
                                    isCanDown = false;
                                }
                                else
                                {
                                    IList<T_ContainerDetailEntity> outBoundDetailsList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == isOutBackModel.BarCode);
                                    if (outBoundDetailsList.Sum(o => o.Qty ?? 0) == 0) //已没有余料
                                    {
                                        if (container.ContainerKind == "Box")
                                        {
                                            isCanBack = false;
                                            isCanDown = true;
                                        }
                                        else if (container.ContainerKind == "Plastic")
                                        {
                                            isCanBack = true;
                                            isCanDown = false;
                                        }
                                    }
                                    else
                                    {
                                        isCanBack = true;
                                        isCanDown = false;
                                    }
                                }
                            }
                            break;
                        case "GetSample": //取样出库
                            {
                                IList<T_QARecordEntity> recordList = db.FindList<T_QARecordEntity>(o => o.BarCode == isOutBackModel.BarCode && o.QAID == station.CurOrderID && o.State != "Picked");
                                if (recordList.Count > 0) //未取样完毕
                                {
                                    isCanBack = false;
                                    isCanDown = false;
                                }
                                else
                                {
                                    isCanBack = true;
                                    isCanDown = false;
                                }
                            }
                            break;
                        case "LocCount"://货位盘点出库(盘点异常会产生此异常出库)
                            {
                                T_LocCountRecordEntity record = db.FindEntity<T_LocCountRecordEntity>(o=>o.FactBarCode == isOutBackModel.BarCode && o.LocCountID == station.CurOrderID);
                                if(record.CountState !="Over")
                                {
                                    isCanBack = false;
                                    isCanDown = false;
                                }
                                else
                                {
                                    string res = record.CountResult;
                                    if (res == "Inner_DiffBoxCode" )
                                    {
                                        T_TaskEntity task = db.FindEntity<T_TaskEntity>(o=>o.BarCode == isOutBackModel.BarCode);
                                        if(task==null)
                                        {
                                            isCanBack = false;
                                            isCanDown = true;
                                        }
                                        else
                                        {
                                            isCanBack = true;
                                            isCanDown = false;
                                        }
                                        
                                    }
                                    else if (res == "Inner_MoreBoxCode")
                                    {
                                        isCanBack = false;
                                        isCanDown = true;
                                    }
                                }
                            }
                            break;
                        case "Count": //盘点出库
                            {
                                IList<T_CountRecordEntity> recordList = db.FindList<T_CountRecordEntity>(o => o.BarCode == isOutBackModel.BarCode && o.CountID == station.CurOrderID && o.CountState != "Over");
                                if (recordList.Count > 0) //未盘点完毕
                                {
                                    isCanBack = false;
                                    isCanDown = false;
                                }
                                else
                                {
                                    isCanBack = true;
                                    isCanDown = false;
                                }
                            }
                            break;
                        case "BackSample"://还样出库
                            {
                                IList<T_QARecordEntity> recordList = db.FindList<T_QARecordEntity>(o => o.BarCode == isOutBackModel.BarCode && o.QAID == station.CurOrderID && o.State != "Over");
                                if (recordList.Count > 0) //未还样完毕
                                {
                                    isCanBack = false;
                                    isCanDown = false;
                                }
                                else
                                {
                                    isCanBack = true;
                                    isCanDown = false;
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "单据类型不支持";
                                return result;
                            }
                    }


                    result.Data = new { IsBack = isCanBack, IsDown = isCanDown };
                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region 正常入库口电控按钮判断是否需可移动（WCS调用）
        public class IsCanMoveModel
        {
            /// <summary>
            /// 申请站台
            /// </summary>
            public string ApplyStationCode { get; set; }
        }


        public WCSResult IsCanMove(IsCanMoveModel isCanMoveModel)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                WCSResult result = new WCSResult();
                try
                {
                    if (string.IsNullOrEmpty(isCanMoveModel.ApplyStationCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "申请站台不可为空";
                        return result;
                    }
                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == isCanMoveModel.ApplyStationCode);
                    if (string.IsNullOrEmpty(station.OrderType) || station.CurModel == "Empty") //暂停、未开始、空料箱入库 等不可点击此按钮，即便点击也返回false
                    {
                        result.Data = new { IsMove = false };
                    }
                    else  //其它纸箱、料箱 返回true 
                    {
                        result.Data = new { IsMove = true };
                    }

                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region 贴标机打印（WCS调用）
        public class PrintCodeModel
        {
            public string ApplyStationCode { get; set; }    /// 申请站台
        }

        public WCSResult PrintCode(PrintCodeModel printModel)
        {
            WCSResult result = new WCSResult();
            if (string.IsNullOrEmpty(printModel.ApplyStationCode))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "申请站台不能为空";
                return result;
            }
            if (printModel.ApplyStationCode != FixType.Station.StationIn_Normal.ToString())
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "申请站台不存在";
                return result;
            }

            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_Normal.ToString());
                    if (stationEntity == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "站台未找到";
                        return result;
                    }


                    if (string.IsNullOrEmpty(stationEntity.CurOrderDetailID))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "站台没有单据";
                        return result;
                    }

                    T_InBoundDetailEntity inboundDetail = db.FindEntity<T_InBoundDetailEntity>(o => o.F_Id == stationEntity.CurOrderDetailID);
                    T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == inboundDetail.InBoundID);
                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == inboundDetail.ItemID);
                    T_MarkRuleEntity rule = db.FindEntity<T_MarkRuleEntity>(o => o.InBoundDetailID == inboundDetail.F_Id);

                    string barCode;
                    object obj = null;
                    if (inboundDetail.IsSplitModel == "true")
                    {
                        //获取第一个未使用的手动生成的标签
                        T_MarkRecordEntity record = db.FindList<T_MarkRecordEntity>(o => o.MarkRuleID == rule.F_Id && o.IsHandPrint == "true" && o.IsUsed == "false").OrderBy(o => o.BarCode).FirstOrDefault();
                        if (record == null)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "散箱没有任何可用标签";
                            return result;
                        }

                        barCode = record.BarCode;
                        obj = new
                        {
                            IsNeedPrint = true,
                            BarCode = record.BarCode,
                            ItemCode = item.ItemCode,
                            ItemName = item.ItemName,
                            UnitQty = item.UnitQty,
                            Qty = record.Qty,
                            ItemUnitText = item.ItemUnitText,
                            Lot = inboundDetail.Lot,
                            ValidityOutDate = inboundDetail.ProductDate == null ? "" : inboundDetail.ProductDate.Value.AddDays(item.ValidityDayNum ?? 0).ToString("yyyy-MM-dd"),
                            SupplierUserName = inbound.SupplierUserName,
                            ProductDate = inboundDetail.ProductDate,
                            Spec = item.Spec,
                            Factory = item.Factory
                        };

                    }
                    else
                    {

                        if (item.UnitQty == null || item.UnitQty == 0)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "物料单位数量未设置:ItemCode=" + item.ItemCode + ",ItemName=" + item.ItemName;
                            return result;
                        }
                        barCode = T_CodeGenApp.GenNum("ContainerBarRule");

                        if (rule == null)
                        {
                            rule = new T_MarkRuleEntity();
                            rule.F_Id = Guid.NewGuid().ToString();
                            rule.InBoundDetailID = inboundDetail.F_Id;
                            rule.SupplierCode = inbound.SupplierUserCode;
                            rule.SupplierName = inbound.SupplierUserName;
                            rule.ItemCode = item.ItemCode;
                            rule.ItemName = item.ItemName;
                            rule.Lot = inboundDetail.Lot;
                            rule.Qty = inboundDetail.Qty;
                            rule.PicNum = (int)((inboundDetail.Qty ?? 0) / (item.UnitQty ?? 0));
                            rule.OverPicNum = 1;
                            rule.IsEnable = "true";
                            rule.F_DeleteMark = false;
                            db.Insert<T_MarkRuleEntity>(rule);
                            db.SaveChanges();
                        }
                        else
                        {
                            rule.OverPicNum = rule.OverPicNum + 1;
                            db.Update<T_MarkRuleEntity>(rule);
                        }

                        T_MarkRecordEntity record = new T_MarkRecordEntity();
                        record.F_Id = Guid.NewGuid().ToString();
                        record.MarkRuleID = rule.F_Id;
                        record.BarCode = barCode;
                        record.SupplierCode = inbound.SupplierUserCode;
                        record.SupplierName = inbound.SupplierUserName;
                        record.ItemCode = item.ItemCode;
                        record.ItemName = item.ItemName;
                        record.Qty = item.UnitQty;
                        record.Lot = inboundDetail.Lot;
                        record.IsUsed = "false";
                        record.ItemID = item.F_Id;
                        record.RepairPicNum = 0;
                        record.PicNum = 1;
                        record.IsHandPrint = "false";
                        record.F_DeleteMark = false;
                        db.Insert<T_MarkRecordEntity>(record);

                        obj = new
                        {
                            IsNeedPrint = true,
                            BarCode = barCode,
                            ItemCode = item.ItemCode,
                            ItemName = item.ItemName,
                            UnitQty = item.UnitQty,
                            Qty = item.UnitQty,
                            ItemUnitText = item.ItemUnitText,
                            Lot = inboundDetail.Lot,
                            ValidityOutDate = inboundDetail.ProductDate == null ? "" : inboundDetail.ProductDate.Value.AddDays(item.ValidityDayNum ?? 0).ToString("yyyy-MM-dd"),
                            SupplierUserName = inbound.SupplierUserName,
                            ProductDate = inboundDetail.ProductDate,
                            Spec = item.Spec,
                            Factory = item.Factory
                        };

                    }

                    db.SaveChanges();
                    db.CommitWithOutRollBack();

                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    result.Data = obj;
                    return result;

                }
                catch (Exception e)
                {
                    db.RollBack();
                    throw e;
                }
            }
        }
        #endregion

        #region CCD自动识别（WCS调用）
        public class CCDModel
        {
            public string ItemCode { get; set; }    /// 物料编码
            public string Lot { get; set; }   /// 批号
            public DateTime? PorductDate { get; set; } //生产日期

            public string ContaienrKind { get; set; } //Box纸箱，Plastic料箱

            public string Long { get; set; } //长(mm)
            public string Width { get; set; }//宽(mm)
            public string Heigh { get; set; }//高(mm)
        }

        public WCSResult CCD(CCDModel ccdModel)
        {
            WCSResult result = new WCSResult();
            if (string.IsNullOrEmpty(ccdModel.ItemCode))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "物料编码不能为空";
                return result;
            }
            if (string.IsNullOrEmpty(ccdModel.Lot))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "批号不能为空";
                return result;
            }
            if (string.IsNullOrEmpty(ccdModel.ContaienrKind))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "容器种类不能为空";
                return result;
            }

            if (ccdModel.PorductDate == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "生产日期不能为空";
                return result;
            }

            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_Normal.ToString());
                    if (stationEntity == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "站台未找到";
                        return result;
                    }


                    T_InBoundDetailEntity entity = db.FindEntity<T_InBoundDetailEntity>(o => o.ItemCode == ccdModel.ItemCode && o.State != "Over");
                    if (entity == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "没有对应的单据明细";
                        return result;
                    }

                    T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == entity.InBoundID);

                    if ((!string.IsNullOrEmpty(stationEntity.CurOrderID)) && stationEntity.CurOrderID != inBound.F_Id)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "站台已被其它单据占用";
                        return result;
                    }

                    inBound.State = "Receiveing";
                    db.Update<T_InBoundEntity>(inBound);

                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == entity.ItemID);
                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                    if (containerType.ContainerKind != "Box")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "只能入纸箱";
                        return result;
                    }

                    if (item.IsMustLot == "true")
                    {
                        if (string.IsNullOrEmpty(ccdModel.Lot)) //CCD未传入
                        {
                            if (string.IsNullOrEmpty(entity.Lot)) //单据未设置
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "物料批号必填，但即没有CCD传入批号，也没有设置单据批号";
                                return result;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(entity.Lot)) // CCD传入，单据未设置
                            {
                                entity.Lot = ccdModel.Lot;
                            }
                            else
                            {
                                if (ccdModel.Lot != entity.Lot)//CCD传入，单据已设置，但两者不一致
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "批号不一致";
                                    return result;
                                }
                            }
                        }
                    }

                    entity.ProductDate = ccdModel.PorductDate;
                    entity.State = "Receiveing";
                    db.Update<T_InBoundDetailEntity>(entity);

                    stationEntity.CurOrderID = inBound.F_Id;
                    stationEntity.CurOrderDetailID = entity.F_Id;

                    if (inBound.InBoundType == "PurchaseInType") //采购入库
                    {
                        stationEntity.OrderType = "PurchaseIn";
                    }


                    db.Update<T_StationEntity>(stationEntity);

                    db.CommitWithOutRollBack();

                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region Pho（WCS调用,流道UI）
        public class PhoModel
        {
            public string BarCode { get; set; }
        }

        public WCSResult Pho(PhoModel phoModel, HttpPostedFileBase file)
        {
            WCSResult result = new WCSResult();
            if (string.IsNullOrEmpty(phoModel.BarCode))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "容器编码不能为空";
                return result;
            }

            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    string datePath = DateTime.Now.ToString("yyyy-MM-dd");
                    string target = Server.MapPath("/PhoImg/" + datePath + "/");
                    if (!Directory.Exists(target))
                    {
                        Directory.CreateDirectory(target);
                    }
                    string fileName = phoModel.BarCode + "_" + Guid.NewGuid().ToString() + ".jpg";
                    file.SaveAs(target + fileName);

                    LogObj logObj = new LogObj();
                    logObj.Path = "WCSController.Pho"; //按实际情况修改

                    LogFactory.GetLogger().Info(logObj);

                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region 异常报警日志
        public class ExceptionLogModel
        {
            //系统 = 1,
            //错误 = 2,
            //调试 = 4,
            //PLC = 8,
            //SCANNER = 16,
            //WMS = 32,
            //AGV = 64,
            //SCADA =128

            public string Type { get; set; }

            //Info = 1,
            //Error = 2,
            //Debug = 3,
            //Warn = 4,
            //Fatal = 5

            public string Level { get; set; }
            public string Content { get; set; }
        }

        public WCSResult ExceptionLog(ExceptionLogModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    WCSResult result = new WCSResult();
                    MsgHub hub = new MsgHub();
                    foreach (SocketUser socketUser in MsgHub.SocketUserList)
                    {
                        string uId = socketUser.UserID;
                        T_SendMsgEntity msgEntity = new T_SendMsgEntity();
                        msgEntity.F_Id = Guid.NewGuid().ToString();
                        msgEntity.MsgType = "DevErr";
                        msgEntity.Msg = model.Content;
                        msgEntity.ReceiveID = uId;
                        msgEntity.SendTime = DateTime.Now;
                        msgEntity.IsReadOver = "false";
                        msgEntity.F_DeleteMark = false;

                        db.Insert<T_SendMsgEntity>(msgEntity);
                        db.SaveChanges();

                        int notRead = db.FindList<T_SendMsgEntity>(o => o.ReceiveID == uId && o.IsReadOver == "false").Count();
                        hub.SendSingle(uId, MsgType.NoReadNum, new WebSocketResult() { IsSuccess = true, Data = notRead });
                    }

                    db.CommitWithOutRollBack();


                    result.IsSuccess = true;
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }
        #endregion

        #region AGV编码反馈
        public class PostAVGCodeModel
        {
            public string TaskNo { get; set; }
            /// <summary>
            /// AVG的WCS编码
            /// </summary>
            public string AGVCode { get; set; }
        }

        public WCSResult PostAVGCode(PostAVGCodeModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                WCSResult result = new WCSResult();
                try
                {
                    if (string.IsNullOrEmpty(model.TaskNo))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "任务编码不可为空";
                        return result;
                    }
                    else if (string.IsNullOrEmpty(model.AGVCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "AGV编码不可为空";
                        return result;
                    }
                    else
                    {
                        T_EquEntity equ = db.FindEntity<T_EquEntity>(o => o.WCSCode == model.AGVCode);
                        if (equ == null)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "AGV不存在";
                            return result;
                        }
                        T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.TaskNo == model.TaskNo);
                        if (task == null)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "任务不存在";
                            return result;
                        }

                        task.ExecEquID = equ.F_Id;
                        db.Update<T_TaskEntity>(task);
                        db.CommitWithOutRollBack();

                        result.IsSuccess = true;
                        result.FailCode = "0000";
                        return result;
                    }
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region 呼叫空容器(已作废)
        public class CallEmptyModel
        {
            /// <summary>
            /// 呼叫的站台(1=空料箱暂存区站台，2=大件入库站台)
            /// </summary>
            public string StationType { get; set; }
            /// <summary>
            /// 呼叫的数量(对于大件站台，没次数量应为1，对于空料箱站台，呼叫数量大于等于16，则每次填满为止)
            /// </summary>
            public string Num { get; set; }
        }

        public WCSResult CallEmpty(CallEmptyModel model)
        {
            WCSResult result = new WCSResult();
            if (string.IsNullOrEmpty(model.Num))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "数量不正确";
                return result;
            }

            int num = -1;
            if (!int.TryParse(model.Num, out num))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "数量不正确";
                return result;
            }

            T_StationEntity stationEntity = null;
            if (model.StationType == "1")
            {
                stationEntity = new T_StationApp().FindEntity(o => o.StationCode == FixType.Station.StationEmpty.ToString());
            }
            else if (model.StationType == "2")
            {
                stationEntity = new T_StationApp().FindEntity(o => o.StationCode == FixType.Station.StationIn_BigItem.ToString());
            }
            else
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "站台类型不正确";
                return result;
            }

            AjaxResult res = new EmptyContainerController().EmptyCallOut(stationEntity.F_Id, num);
            if ((ResultType)res.state == ResultType.success)
            {
                result.IsSuccess = true;
                result.FailCode = "0000";
                return result;
            }
            else
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = res.message;
                return result;
            }
        }
        #endregion

        #region CTU未满料离开(非必须，仅为提高效率)
        public class CTULeaveModel
        {
            /// <summary>
            /// 离开的站台(StationIn_Normal：入库站台(正常)，StationOut_Normal：出库站台(正常))
            /// </summary>
            public string StationType { get; set; }
            /// <summary>
            /// 接驳位开始(索引为0)，到扫码器截止(索引为最大)，中间的所有容器条码
            /// </summary>
            public string ContainerBar { get; set; }
        }

        public WCSResult CTULeave(CTULeaveModel model)
        {
            WCSResult result = new WCSResult();

            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    if (string.IsNullOrEmpty(model.StationType) || (model.StationType != FixType.Station.StationIn_Normal.ToString() && model.StationType != FixType.Station.StationOut_Normal.ToString()))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "站台不正确";
                        return result;
                    }

                    if (string.IsNullOrEmpty(model.ContainerBar)) //有容器
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "容器条码不正确";
                        return result;
                    }
                    int? line = db.IQueryable<T_TaskEntity>(o => model.ContainerBar == o.BarCode).Join(db.IQueryable<T_LocationEntity>(o => true), m => m.TagLocationID, n => n.F_Id, (m, n) => n.Line).Distinct().ToList().FirstOrDefault();
                    if (line == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "容器条码无任务";
                        return result;
                    }

                    T_RowLineEntity lineEntity = db.FindEntity<T_RowLineEntity>(o => o.Line == line);
                    lineEntity.CurOverCount_In = lineEntity.ContinuityCount_In; //计数满
                    db.Update<T_RowLineEntity>(lineEntity);
                    db.CommitWithOutRollBack();

                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    return result;
                }
                catch (Exception e)
                {
                    db.RollBack();
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = e.Message;
                    return result;
                }

            }

        }
        #endregion

    }
}
