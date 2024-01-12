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

namespace MST.Web.Areas.RF_OtherManage.Controllers
{
    [HandlerLogin]
    public class RFTaskOverController : ControllerBase
    {
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_TaskApp taskApp = new T_TaskApp();
        private T_AreaApp areaApp = new T_AreaApp();

        #region 获取默认入库点列表
        /// <summary>
        /// 获取默认入库点列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetStationDicList()
        {
            List<T_StationEntity> stationList = new List<T_StationEntity>();
            stationList = stationApp.FindList(o => o.StationCode == FixType.Station.StationIn_Normal.ToString() || o.StationCode == FixType.Station.StationOut_Normal.ToString()).ToList();
            return Content(stationList.ToJson());
        }
        #endregion

        #region  获取录入条码的任务
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetBarTask(string barCode)
        {
            T_TaskEntity task = taskApp.FindEntity(o => o.BarCode == barCode);
            return Content(task.ToJson());
        }
        #endregion

        #region  获取所有任务
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTask()
        {

            var data = taskApp.FindList(o => o.IsCanExec == "true").ToList();
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
                model.TagLocationCodeName = task.TagLocationCode;
                taskModelList.Add(model);
            }
            return Content(taskModelList.ToJson());
        }
        #endregion

        #region 申请入库
        /// <summary>
        /// 申请入库
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult RFScanTaskOver(string TaskNo)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "RFScanController.RFScanTaskOver";
            logObj.Parms = new { TaskNo = TaskNo};

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "RF扫码手动完成任务";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "RF扫码手动完成任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                StateChangeTaskModel stateChangeTaskModel = new StateChangeTaskModel() { State = "Over", TaskCode = TaskNo };
                T_TaskApp taskApp = new T_TaskApp();
                WCSResult result = taskApp.StateChangeTask(stateChangeTaskModel);
                if (result.IsSuccess)
                {
                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);
                    return Success("操作成功。");
                }
                else
                {
                    logObj.Message = result.FailMsg;
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = result.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                    return Error(result.FailMsg, "");
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
