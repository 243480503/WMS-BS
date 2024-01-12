/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using MST.Application.SystemSecurity;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Application.APIPost
{
    public class WCSPost
    {

        #region 请求主方法

        public enum PostWCSFunType
        {
            /// <summary>
            /// 任务推送
            /// </summary>
            [Description("任务推送")]
            SendToWCSTask,

            /// <summary>
            /// 标签打印
            /// </summary>
            [Description("标签打印")]
            PrintItemBarCode,

            /// <summary>
            /// 异常容器任务推送
            /// </summary>
            [Description("异常容器任务推送")]
            SendExceptionTask,

            /// <summary>
            /// 取消WCS任务
            /// </summary>
            [Description("取消WCS任务")]
            TaskCancel,

            /// <summary>
            /// 继续WCS任务
            /// </summary>
            [Description("继续WCS任务")]
            ContinueDev,

            /// <summary>
            /// 暂停WCS任务
            /// </summary>
            [Description("暂停WCS任务")]
            PauseDev
        }
        /// <summary>
        /// 请求主方法
        /// </summary>
        /// <param name="funType"></param>
        /// <param name="postParam"></param>
        /// <returns></returns>
        private WCSResult PostWCS(PostWCSFunType funType, object postParam)
        {
            string returnStr = "";

            LogObj logObj = new LogObj();
            logObj.Path = "APIPost.PostWCS"; //按实际情况修改

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "WMS请求WCS接口"; //按实际情况修改
            logEntity.F_Type = DbLogType.Visit.ToString(); //按实际情况修改

            logEntity.F_Description = "WMS请求接口"; //按实际情况修改
            logEntity.F_Path = logObj.Path;

            OperatorModel user = null;
            WCSResult result = new WCSResult();
            try
            {
                /*************************************************/
                string wcs_url = RuleConfig.Intface.WCS.URL ?? "";

                WCSModel wcsRequest = new WCSModel();
                wcsRequest.MsgID = Guid.NewGuid().ToString();
                wcsRequest.AskTime = DateTime.Now;
                wcsRequest.PostData = postParam;
                switch (funType)
                {
                    case PostWCSFunType.SendToWCSTask:
                        {
                            user = OperatorProvider.Provider.GetCurrent();
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                            wcsRequest.Method = "WMSSendTask";
                        }
                        break;
                    case PostWCSFunType.SendExceptionTask:
                        {
                            user = OperatorProvider.Provider.GetCurrent();
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                            wcsRequest.Method = "SendExceptionTask";
                        }
                        break;
                    case PostWCSFunType.PrintItemBarCode:
                        {
                            PrintItemBarCodePostModel postModel = (PrintItemBarCodePostModel)postParam;
                            user = postModel.User;
                            postModel.User = null;
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                            wcsRequest.Method = "WMSPrint";
                        }
                        break;
                    case PostWCSFunType.PauseDev:
                        {
                            user = OperatorProvider.Provider.GetCurrent();
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                            wcsRequest.Method = "PauseDev";
                        }
                        break;
                    case PostWCSFunType.ContinueDev:
                        {
                            user = OperatorProvider.Provider.GetCurrent();
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                            wcsRequest.Method = "ContinueDev";
                        }
                        break;

                    case PostWCSFunType.TaskCancel:
                        {
                            user = OperatorProvider.Provider.GetCurrent();
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                            wcsRequest.Method = "TaskCancel";
                        }
                        break;
                    default:
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "未知的方法名";
                            return result;
                        }
                }

                logObj.Parms = new { funType = funType, postParam = wcsRequest }; //按实际情况修改
                logEntity.F_Param = wcsRequest.ToJson();

                string param = wcsRequest.ToJson();

                string responseStr = "";
                if ((!string.IsNullOrEmpty(wcs_url)) && wcs_url != "http://")
                {
                    wcs_url = wcs_url + "/" + funType.ToString() + "/";
                    responseStr = HttpMethods.HttpPost(wcs_url, param);
                    LogFactory.GetLogger().Info(new LogObj() { CurTime=DateTime.Now, Message="接口消息", Parms= param, Path= "APIPost.PostWCS", ReturnData= responseStr });
                    result = responseStr.ToObject<WCSResult>();
                    if (result.IsSuccess)
                    {
                        logObj.Message = "操作成功";
                        logObj.ReturnData = responseStr;
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        logEntity.F_Msg = responseStr;
                        new LogApp().WriteDbLogThread(logEntity, user);
                    }
                    else
                    {
                        logObj.Message = "操作失败";
                        logObj.ReturnData = responseStr;
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = false;
                        logEntity.F_Msg = responseStr;
                        new LogApp().WriteDbLogThread(logEntity, user);
                    }
                    
                }
                else
                {
                    result = new WCSResult() { IsSuccess = true, Data = "成功，但未指定接口地址" };
                    responseStr = result.ToJson();

                }
                /**************************************************/
                

                returnStr = result.ToJson();
                return result;
            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLogThread(logEntity, user);

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return result;
            }
        }
        #endregion

        #region 发送任务到WCS

        public class SendTaskModel
        {
            public string StationCode { get; set; }
            public string TaskNo { get; set; }
            public string TaskInOutType { get; set; }
            public string TaskType { get; set; }
            public string SrcLocationCode { get; set; }
            public string TagLocationCode { get; set; }
            public string SrcWCSLocCode { get; set; }
            public string TagWCSLocCode { get; set; }
            public int? Level { get; set; }
            public string ContainerType { get; set; }

            public string BarCode { get; set; }

            public string ContainerKind { get; set; }
            public string OrderType { get; set; }
            public string OrderCode { get; set; }

            public string ContainerBarCode { get; set; }

            /// <summary>
            /// 对应的AGV系统的箱型编码
            /// </summary>
            public string AGVContainerTypeCode { get; set; }

            /// <summary>
            /// 容器宽度
            /// </summary>
            public decimal? ConWidth { get; set; }

            /// <summary>
            /// 容器高度
            /// </summary>
            public decimal? ConHeight { get; set; }

            /// <summary>
            /// 容器深度
            /// </summary>
            public decimal? ConLength { get; set; }

            /// <summary>
            /// 指定执行设备
            /// </summary>
            public string PointExecRobotCode { get; set; }

            public IList<TaskContaienrItem> TaskContainerItems { get; set; }
        }

        public class TaskContaienrItem
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }

            public string Lot { get; set; }

            public decimal? Qty { get; set; }

            public decimal? OutQty { get; set; }

            public string ItemBarCode { get; set; }
        }


        /// <summary>
        /// 发送任务到WCS,并将任务更新为执行中(适合小任务量)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="taskNoList"></param>
        /// <returns></returns>
        public WCSResult SendTask(IRepositoryBase db, IList<string> taskNoList)
        {
            WCSResult wcsRes = new WCSResult();
            try
            {
                IList<SendTaskModel> taskwcsList = new List<SendTaskModel>();
                IList<T_TaskEntity> taskList = new List<T_TaskEntity>();
                foreach (string taskNo in taskNoList)
                {
                    T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskNo);
                    if (taskEntity == null)
                    {
                        wcsRes.IsSuccess = false;
                        wcsRes.FailCode = "0001";
                        wcsRes.FailMsg = "任务未找到：TaskNo=" + taskNo;
                        return wcsRes;
                    }

                    if (taskEntity.State != "New")
                    {
                        wcsRes.IsSuccess = false;
                        wcsRes.FailCode = "0001";
                        wcsRes.FailMsg = "任务状态必须为新建：TaskNo=" + taskNo;
                        return wcsRes;
                    }

                    IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == taskEntity.BarCode).ToList();
                    IList<TaskContaienrItem> taskContainerItem = detailList.Select(o => new TaskContaienrItem { ItemBarCode = o.ItemBarCode, ItemName = o.ItemName, ItemCode = o.ItemCode, Lot = o.Lot, Qty = o.Qty, OutQty = o.OutQty }).ToList();


                    T_ContainerTypeEntity containerTypeEntity = null; //空货位校对没有容器类型

                    if (!string.IsNullOrEmpty(taskEntity.BarCode))
                    {
                        containerTypeEntity = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskEntity.ContainerType);
                    }

                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == taskEntity.ApplyStationID);

                    SendTaskModel sendTaskModel = new SendTaskModel
                    {
                        TaskNo = taskEntity.TaskNo,
                        TaskInOutType = taskEntity.TaskInOutType,
                        TaskType = taskEntity.TaskType,
                        SrcLocationCode = taskEntity.SrcLocationCode,
                        TagLocationCode = taskEntity.TagLocationCode,
                        SrcWCSLocCode = taskEntity.SrcWCSLocCode,
                        TagWCSLocCode = taskEntity.TagWCSLocCode,
                        Level = taskEntity.Level,
                        ContainerType = containerTypeEntity == null ? null : containerTypeEntity.ContainerTypeCode,
                        ContainerKind = containerTypeEntity == null ? null : containerTypeEntity.ContainerKind,
                        OrderType = taskEntity.OrderType,
                        OrderCode = taskEntity.OrderCode,
                        TaskContainerItems = taskContainerItem,
                        ConLength = containerTypeEntity == null ? null : containerTypeEntity.BorderLong,
                        ConHeight = containerTypeEntity == null ? null : containerTypeEntity.BorderHeight,
                        ConWidth = containerTypeEntity == null ? null : containerTypeEntity.BorderWidth,
                        AGVContainerTypeCode = containerTypeEntity == null ? null : containerTypeEntity.AGVContainerTypeCode,
                        BarCode = taskEntity.BarCode,
                        ContainerBarCode = taskEntity.BarCode,
                        PointExecRobotCode = taskEntity.PointExecRobotCode,
                        StationCode = station == null ? "" : station.StationCode, //移库没有站台
                    };
                    taskwcsList.Add(sendTaskModel);
                    taskList.Add(taskEntity);
                }

                wcsRes = PostWCS(PostWCSFunType.SendToWCSTask, taskwcsList);
                if (wcsRes.IsSuccess)
                {
                    foreach (T_TaskEntity task in taskList)
                    {
                        task.State = "Execing";
                        task.SendWCSTime = DateTime.Now;
                        db.Update<T_TaskEntity>(task);
                    }
                    return wcsRes;
                }
                else
                {
                    return wcsRes;
                }
            }
            catch (Exception ex)
            {
                wcsRes.IsSuccess = false;
                wcsRes.FailCode = "0002";
                wcsRes.FailMsg = "错误：" + ex.Message;
                return wcsRes;
            }
        }


        /// <summary>
        /// 发送任务到WCS,并将任务更新为执行中（适合大数据量）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="taskNoList"></param>
        /// <returns></returns>
        public WCSResult SendTaskBulk(IRepositoryBase db, IList<string> taskNoList)
        {
            WCSResult wcsRes = new WCSResult();
            try
            {
                IList<SendTaskModel> taskwcsList = new List<SendTaskModel>();
                IList<T_TaskEntity> taskInDBList = db.FindList<T_TaskEntity>(o => true);
                IList<T_ContainerTypeEntity> conTypeInDBList = db.FindList<T_ContainerTypeEntity>(o => true);
                IList<T_StationEntity> stationInDBList = db.FindList<T_StationEntity>(o => true);
                IList<T_ContainerDetailEntity> conDetailInDBList = db.FindList<T_ContainerDetailEntity>(o => true);

                IList<T_TaskEntity> taskList = new List<T_TaskEntity>();


                foreach (string taskNo in taskNoList)
                {
                    T_TaskEntity taskEntity = taskInDBList.FirstOrDefault(o => o.TaskNo == taskNo);
                    if (taskEntity == null)
                    {
                        wcsRes.IsSuccess = false;
                        wcsRes.FailCode = "0001";
                        wcsRes.FailMsg = "任务未找到：TaskNo=" + taskNo;
                        return wcsRes;
                    }

                    if (taskEntity.State != "New")
                    {
                        wcsRes.IsSuccess = false;
                        wcsRes.FailCode = "0001";
                        wcsRes.FailMsg = "任务状态必须为新建：TaskNo=" + taskNo;
                        return wcsRes;
                    }

                    IList<T_ContainerDetailEntity> detailList = conDetailInDBList.Where(o => o.BarCode == taskEntity.BarCode).ToList();
                    IList<TaskContaienrItem> taskContainerItem = detailList.Select(o => new TaskContaienrItem { ItemBarCode = o.ItemBarCode, ItemName = o.ItemName, ItemCode = o.ItemCode, Lot = o.Lot, Qty = o.Qty, OutQty = o.OutQty }).ToList();


                    T_ContainerTypeEntity containerTypeEntity = null; //空货位校对没有容器类型

                    if (!string.IsNullOrEmpty(taskEntity.BarCode))
                    {
                        containerTypeEntity = conTypeInDBList.FirstOrDefault(o => o.ContainerTypeCode == taskEntity.ContainerType);// db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskEntity.ContainerType);
                    }

                    T_StationEntity station = stationInDBList.FirstOrDefault(o => o.F_Id == taskEntity.ApplyStationID);

                    SendTaskModel sendTaskModel = new SendTaskModel
                    {
                        TaskNo = taskEntity.TaskNo,
                        TaskInOutType = taskEntity.TaskInOutType,
                        TaskType = taskEntity.TaskType,
                        SrcLocationCode = taskEntity.SrcLocationCode,
                        TagLocationCode = taskEntity.TagLocationCode,
                        SrcWCSLocCode = taskEntity.SrcWCSLocCode,
                        TagWCSLocCode = taskEntity.TagWCSLocCode,
                        Level = taskEntity.Level,
                        ContainerType = containerTypeEntity == null ? null : containerTypeEntity.ContainerTypeCode,
                        ContainerKind = containerTypeEntity == null ? null : containerTypeEntity.ContainerKind,
                        OrderType = taskEntity.OrderType,
                        OrderCode = taskEntity.OrderCode,
                        TaskContainerItems = taskContainerItem,
                        ConLength = containerTypeEntity == null ? null : containerTypeEntity.BorderLong,
                        ConHeight = containerTypeEntity == null ? null : containerTypeEntity.BorderHeight,
                        ConWidth = containerTypeEntity == null ? null : containerTypeEntity.BorderWidth,
                        AGVContainerTypeCode = containerTypeEntity == null ? null : containerTypeEntity.AGVContainerTypeCode,
                        BarCode = taskEntity.BarCode,
                        ContainerBarCode = taskEntity.BarCode,
                        PointExecRobotCode = taskEntity.PointExecRobotCode,
                        StationCode = station == null ? "" : station.StationCode, //移库没有站台
                    };
                    taskwcsList.Add(sendTaskModel);
                    taskList.Add(taskEntity);
                }

                wcsRes = PostWCS(PostWCSFunType.SendToWCSTask, taskwcsList);
                if (wcsRes.IsSuccess)
                {
                    IList<T_TaskEntity> needUpdateTaskList = new List<T_TaskEntity>();
                    foreach (T_TaskEntity task in taskList)
                    {
                        task.State = "Execing";
                        task.SendWCSTime = DateTime.Now;
                        needUpdateTaskList.Add(task);
                    }
                    db.BulkUpdate(needUpdateTaskList);
                    db.BulkSaveChanges();
                    return wcsRes;
                }
                else
                {
                    return wcsRes;
                }
            }
            catch (Exception ex)
            {
                wcsRes.IsSuccess = false;
                wcsRes.FailCode = "0002";
                wcsRes.FailMsg = "错误：" + ex.Message;
                return wcsRes;
            }
        }

        #endregion

        #region 发送需打印的物料标签到WCS

        public class PrintItemBarCodePostModel
        {
            public string ItemBarCode { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string UnitQty { get; set; }
            public string ItemUnitText { get; set; }
            public string Lot { get; set; }
            public string OverdueDate { get; set; }
            public string SupplierUserName { get; set; }
            public string ProductDate { get; set; }
            public string Spec { get; set; }
            public string Factory { get; set; }

            public OperatorModel User { get; set; }

        }

        /// <summary>
        /// 发送需打印的物料标签到WCS
        /// </summary>
        /// <param name="db"></param>
        /// <param name="printItemModel"></param>
        /// <returns></returns>
        public WCSResult PrintItemBarCode(PrintItemBarCodePostModel printItemModel)
        {
            WCSResult wcsRes = new WCSResult();
            try
            {
                wcsRes = PostWCS(PostWCSFunType.PrintItemBarCode, printItemModel);
                return wcsRes;
            }
            catch (Exception ex)
            {
                wcsRes.IsSuccess = false;
                wcsRes.FailCode = "0002";
                wcsRes.FailMsg = "错误：" + ex.Message;
                return wcsRes;
            }
        }
        #endregion

        #region 异常容器手动PDA发送任务到WCS

        /// <summary>
        /// 异常申请的报文（取值为任务的SendMes字段，该字段的存入类型为WCSApplayResult，此处结构与WCSApplayResult相同）
        /// </summary>
        private class WCSExceptionApplayResult
        {
            public string TaskNo { get; set; }
            public string TaskInOutType { get; set; }
            public string TaskType { get; set; }
            public string SrcLocationCode { get; set; }
            public string TagLocationCode { get; set; }

            public string SrcWCSLocCode { get; set; }
            public string TagWCSLocCode { get; set; }
            public int? Level { get; set; }
            public string OrderType { get; set; }
            public string OrderCode { get; set; }

            public string StationCode { get; set; }

            /// <summary>
            /// 容器宽度
            /// </summary>
            public decimal? ConWidth { get; set; }

            /// <summary>
            /// 容器高度
            /// </summary>
            public decimal? ConHeight { get; set; }

            /// <summary>
            /// 容器深度
            /// </summary>
            public decimal? ConLength { get; set; }

            /// <summary>
            /// 容器大类
            /// </summary>
            public string ContainerKind { get; set; }

            /// <summary>
            /// 容器类型
            /// </summary>
            public string ContainerType { get; set; }

            public string ContainerBarCode { get; set; }
            public IList<string> ItemCodes { get; set; }
        }

        /// <summary>
        /// 异常容器PDA手动推送任务
        /// </summary>
        /// <param name="db"></param>
        /// <param name="taskNoList"></param>
        /// <returns></returns>
        public WCSResult ExceptionSendTask(IRepositoryBase db, string taskNo)
        {
            WCSResult wcsRes = new WCSResult();
            try
            {

                T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskNo);
                if (taskEntity == null)
                {
                    wcsRes.IsSuccess = false;
                    wcsRes.FailCode = "0001";
                    wcsRes.FailMsg = "任务未找到：TaskNo=" + taskNo;
                    return wcsRes;
                }

                if (taskEntity.State != "Execing")
                {
                    wcsRes.IsSuccess = false;
                    wcsRes.FailCode = "0001";
                    wcsRes.FailMsg = "任务状态必须为执行中：TaskNo=" + taskNo;
                    return wcsRes;
                }

                if (string.IsNullOrEmpty(taskEntity.SendMsg))
                {
                    wcsRes.IsSuccess = false;
                    wcsRes.FailCode = "0001";
                    wcsRes.FailMsg = "任务报文不存在：TaskNo=" + taskNo;
                    return wcsRes;
                }

                WCSExceptionApplayResult sendTaskModel = taskEntity.SendMsg.ToObject<WCSExceptionApplayResult>();

                wcsRes = PostWCS(PostWCSFunType.SendExceptionTask, sendTaskModel);

                if (wcsRes.IsSuccess)
                {

                    taskEntity.State = "Execing";
                    taskEntity.SendWCSTime = DateTime.Now;
                    db.Update<T_TaskEntity>(taskEntity);
                    return wcsRes;
                }
                else
                {
                    return wcsRes;
                }
            }
            catch (Exception ex)
            {
                wcsRes.IsSuccess = false;
                wcsRes.FailCode = "0002";
                wcsRes.FailMsg = "错误：" + ex.Message;
                return wcsRes;
            }
        }

        #endregion

        #region 取消WCS任务

        /// <summary>
        /// 取消WCS任务
        /// </summary>
        private class TaskCancelPostModel
        {
            public string TaskNo { get; set; }
        }

        /// <summary>
        ///  取消WCS任务
        /// </summary>
        /// <param name="db"></param>
        /// <param name="taskNo"></param>
        /// <returns></returns>
        public WCSResult TaskCancel(IRepositoryBase db, string taskNo)
        {
            WCSResult wcsRes = new WCSResult();
            try
            {

                T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskNo);
                if (taskEntity == null)
                {
                    wcsRes.IsSuccess = false;
                    wcsRes.FailCode = "0001";
                    wcsRes.FailMsg = "任务未找到：TaskNo=" + taskNo;
                    return wcsRes;
                }

                if (taskEntity.State != "Execing")
                {
                    wcsRes.IsSuccess = false;
                    wcsRes.FailCode = "0001";
                    wcsRes.FailMsg = "任务状态必须为执行中：TaskNo=" + taskNo;
                    return wcsRes;
                }

                TaskCancelPostModel data = new TaskCancelPostModel();
                data.TaskNo = taskNo;

                wcsRes = PostWCS(PostWCSFunType.TaskCancel, data);

                if (wcsRes.IsSuccess)
                {
                    taskEntity.State = "New";
                    taskEntity.SendWCSTime = DateTime.Now;
                    db.Update<T_TaskEntity>(taskEntity);
                    return wcsRes;
                }
                else
                {
                    return wcsRes;
                }
            }
            catch (Exception ex)
            {
                wcsRes.IsSuccess = false;
                wcsRes.FailCode = "0002";
                wcsRes.FailMsg = "错误：" + ex.Message;
                return wcsRes;
            }
        }

        #endregion

        #region 暂停AGV

        /// <summary>
        /// 取消WCS任务
        /// </summary>
        private class PauseDevPostModel
        {
            public string AgvCode { get; set; }
        }

        /// <summary>
        /// 暂停AGV
        /// </summary>
        /// <param name="db"></param>
        /// <param name="agvCode">不指定则暂停全部AGV</param>
        /// <returns></returns>
        public WCSResult PauseDev(IRepositoryBase db, string agvCode)
        {
            WCSResult wcsRes = new WCSResult();
            try
            {

                IList<T_EquEntity> equList = new List<T_EquEntity>();
                if (string.IsNullOrEmpty(agvCode))
                {
                    equList = db.FindList<T_EquEntity>(o => o.EquType == "AGV").ToList();
                }
                else
                {
                    equList = db.FindList<T_EquEntity>(o => o.EquType == "AGV" && o.WCSCode == agvCode).ToList();
                }

                foreach (T_EquEntity cell in equList)
                {
                    PauseDevPostModel data = new PauseDevPostModel();
                    data.AgvCode = cell.WCSCode;
                    wcsRes = PostWCS(PostWCSFunType.PauseDev, data);
                }
                return wcsRes;
            }
            catch (Exception ex)
            {
                wcsRes.IsSuccess = false;
                wcsRes.FailCode = "0002";
                wcsRes.FailMsg = "错误：" + ex.Message;
                return wcsRes;
            }
        }

        #endregion

        #region 继续AGV

        /// <summary>
        /// 继续AGV
        /// </summary>
        private class ContinueDevPostModel
        {
            public string AgvCode { get; set; }
        }

        /// <summary>
        /// 继续AGV
        /// </summary>
        /// <param name="db"></param>
        /// <param name="agvCode">不指定则继续全部AGV</param>
        /// <returns></returns>
        public WCSResult ContinueDev(IRepositoryBase db, string agvCode)
        {
            WCSResult wcsRes = new WCSResult();
            try
            {

                IList<T_EquEntity> equList = new List<T_EquEntity>();
                if (string.IsNullOrEmpty(agvCode))
                {
                    equList = db.FindList<T_EquEntity>(o => o.EquType == "AGV").ToList();
                }
                else
                {
                    equList = db.FindList<T_EquEntity>(o => o.EquType == "AGV" && o.WCSCode == agvCode).ToList();
                }

                foreach (T_EquEntity cell in equList)
                {
                    ContinueDevPostModel data = new ContinueDevPostModel();
                    data.AgvCode = cell.WCSCode;
                    wcsRes = PostWCS(PostWCSFunType.ContinueDev, data);
                }

                return wcsRes;
            }
            catch (Exception ex)
            {
                wcsRes.IsSuccess = false;
                wcsRes.FailCode = "0002";
                wcsRes.FailMsg = "错误：" + ex.Message;
                return wcsRes;
            }
        }

        #endregion

    }
}
