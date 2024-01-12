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
using static MST.Application.WMSLogic.T_TaskApp;

namespace MST.Web.Areas.PC_TaskManage.Controllers
{
    public class TaskController : ControllerBase
    {
        private T_TaskApp taskApp = new T_TaskApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            var data = taskApp.GetList(pagination, keyword);
            IList<ItemsDetailEntity> enumStatelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumInOutTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.TaskInOutType).ToList();
            IList<ItemsDetailEntity> enumOrderTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.OrderType).ToList();
            IList<ItemsDetailEntity> enumTaskTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.TaskType).ToList();

            IList<TaskModel> taskModelList = new List<TaskModel>();
            foreach (T_TaskEntity task in data)
            {
                TaskModel model = task.ToObject<TaskModel>();
                model.TaskInOutTypeName = enumInOutTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskInOutType).F_ItemName;
                model.StateName = enumStatelist.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.TaskTypeName = enumTaskTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskType).F_ItemName;
                model.OrderTypeName = enumOrderTypelist.FirstOrDefault(o => o.F_ItemCode == model.OrderType).F_ItemName;
                taskModelList.Add(model);
            }
            //return Content(taskModelList.ToJson());
            var resultList = new
            {
                rows = taskModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = taskApp.GetForm(keyValue);
            return Content(data.ToJson());
        }


        #region 新建/修改任务
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_TaskEntity taskEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.SubmitForm";
            logObj.Parms = new { taskEntity = taskEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                taskApp.SubmitForm(taskEntity, keyValue);
                if (string.IsNullOrEmpty(taskEntity.TagLocationCode))
                {
                    taskEntity.TagLocationID = "";
                }
                if (string.IsNullOrEmpty(taskEntity.SrcLocationCode))
                {
                    taskEntity.SrcLocationID = "";
                }
                logObj.Message = "操作成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("操作成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 删除任务
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                string[] keyValueArray = keyValue.Split(',');
                foreach (string key in keyValueArray)
                {
                    taskApp.DeleteForm(key);
                }


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

        #region 取消WCS任务
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult TaskReturn_Hand(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.TaskReturn_Hand";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "手动取消任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                string[] keyValueArray = keyValue.Split(',');
                foreach (string taskID in keyValueArray)
                {
                    T_TaskEntity task = new T_TaskApp().FindEntity(o => o.F_Id == keyValue);
                    WCSResult wcsRes = taskApp.TaskReturn_Hand(task.TaskNo);
                    if (!wcsRes.IsSuccess)
                    {
                        return Success("操作失败：" + wcsRes.FailMsg);
                    }
                }


                /**************************************************/

                logObj.Message = "操作成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("操作成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 暂停AGV
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult PauseDev_Hand()
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.PauseDev_Hand";
            logObj.Parms = new { };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "手动暂停AGV";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                WCSResult wcsRes = taskApp.PauseDev("");
                if (!wcsRes.IsSuccess)
                {
                    return Error("操作失败：" + wcsRes.FailMsg, "");
                }

                /**************************************************/

                logObj.Message = "操作成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("操作成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 继续AGV
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult ContinueDev_Hand(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.ContinueDev_Hand";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "手动继续AGV";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                string taskNo = keyValue;
                WCSResult wcsRes = taskApp.ContinueDev("");
                if (!wcsRes.IsSuccess)
                {
                    return Error("操作失败：" + wcsRes.FailMsg, "");
                }

                /**************************************************/

                logObj.Message = "操作成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("操作成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 模拟WCS主动申请
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult TaskApply_Ui(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.TaskApply_Ui";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "手动模拟WCS申请任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                bool hasErr = false;
                string msgErr = null;
                string[] keyValueArray = keyValue.Split(',');
                foreach (string taskID in keyValueArray)
                {
                    T_TaskEntity task = taskApp.FindEntity(o => o.F_Id == taskID);
                    if (task == null)
                    {
                        hasErr = true;
                        msgErr = "任务不存在";
                        logObj.Message = msgErr;
                        LogFactory.GetLogger().Info(logObj);
                        continue;
                    }

                    if (task.State != "New")
                    {
                        hasErr = true;
                        msgErr = "任务状态必须为新建";
                        logObj.Message = msgErr;
                        LogFactory.GetLogger().Info(logObj);
                        continue;
                    }

                    ApplyInTaskModel model = new ApplyInTaskModel();
                    T_StationEntity station = new T_StationApp().FindEntity(o => o.F_Id == task.ApplyStationID);
                    if (station == null)
                    {
                        hasErr = true;
                        msgErr = "未找到对应站台";
                        logObj.Message = msgErr;
                        LogFactory.GetLogger().Info(logObj);
                        continue;
                    }

                    model.ApplyStationCode = station.StationCode;
                    model.BarCode = task.BarCode;
                    WCSResult wcsRes = taskApp.ApplyInTask(model);
                    if (!wcsRes.IsSuccess)
                    {
                        hasErr = true;
                        msgErr = wcsRes.FailMsg;
                        continue;
                    }
                }

                if (!hasErr)
                {
                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                else
                {
                    logObj.Message = msgErr;
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    logEntity.F_Msg = msgErr;
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作完成，但存在失败。", logEntity.F_Msg);
                }

                /**************************************************/
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 下发任务
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult TaskDown_Hand(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.TaskDown_Hand";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "手动下发任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                bool hasErr = false;
                string msgErr = null;
                string[] keyValueArray = keyValue.Split(',');
                foreach (string taskID in keyValueArray)
                {
                    T_TaskEntity task = taskApp.FindEntity(o => o.F_Id == taskID);
                    if (task == null)
                    {
                        hasErr = true;
                        msgErr = "未找到对应任务ID";
                        logObj.Message = msgErr;
                        LogFactory.GetLogger().Info(logObj);
                        continue;
                    }

                    if (task.IsCanExec != "true")
                    {
                        hasErr = true;
                        msgErr = "任务尚不可执行";
                        logObj.Message = msgErr;
                        LogFactory.GetLogger().Info(logObj);
                        continue;
                    }

                    IList<string> list = new List<string>();
                    list.Add(task.TaskNo);
                    WCSResult wcsRes = taskApp.TaskDownToWCS_Hand(list);
                    if (!wcsRes.IsSuccess)
                    {
                        hasErr = true;
                        msgErr = wcsRes.FailMsg;
                        continue;
                    }

                }
                if (!hasErr)
                {
                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");

                }
                else
                {
                    return Error($"操作完成，但存在失败。{ msgErr }", "");
                }
            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 完成任务
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult TaskOver_Hand(string keyValue)
        {

            LogObj logObj = new LogObj();
            logObj.Path = "TaskController.TaskOver_Hand";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "手动完成任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                bool hasErr = false;
                string msgErr = null;
                string[] keyValueArray = keyValue.Split(',');
                foreach (string taskID in keyValueArray)
                {
                    T_TaskEntity task = taskApp.FindEntity(o => o.F_Id == taskID);
                    StateChangeTaskModel model = new StateChangeTaskModel();
                    model.State = "Over";
                    model.TaskCode = task.TaskNo;
                    WCSResult wcsRes = taskApp.StateChangeTask(model);
                    if (!wcsRes.IsSuccess)
                    {
                        hasErr = true;
                        msgErr = wcsRes.FailMsg;
                        continue;
                    }
                }

                if (!hasErr)
                {
                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                else
                {
                    logObj.Message = msgErr;
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    logEntity.F_Msg = msgErr;
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作完成，但存在失败。", logEntity.F_Msg);
                }

            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion
    }
}
