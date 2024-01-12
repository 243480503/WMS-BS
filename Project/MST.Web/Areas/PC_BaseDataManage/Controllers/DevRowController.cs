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
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_BaseDataManage.Controllers
{
    public class DevRowController : ControllerBase
    {
        private T_EquApp equApp = new T_EquApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_DevRowApp devRowApp = new T_DevRowApp();
        private T_RowLineApp rowLineApp = new T_RowLineApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_AreaApp areaApp = new T_AreaApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination,string areaID, string keyword)
        {
            IList<T_DevRowEntity> data = devRowApp.GetList(pagination,areaID, keyword);
            IList<DevRowModel> modelList = new List<DevRowModel>();
            foreach (T_DevRowEntity dr in data)
            {
                DevRowModel model = dr.ToObject<DevRowModel>();
                modelList.Add(model);
            }

            modelList = modelList.OrderBy(o => o.Num).ToList();
            var resultList = new
            {
                rows = modelList,
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
            var data = devRowApp.GetForm(keyValue);
            DevRowModel model = data.ToObject<DevRowModel>();
            return Content(model.ToJson());
        }

        #region 新增/修改巷道
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_DevRowEntity devRowEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DevRowController.SubmitForm";
            logObj.Parms = new { devRowEntity = devRowEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "巷道";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "新增/修改巷道";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    if (!user.IsSystem)
                    {
                        T_DevRowEntity entity = devRowApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                        if (entity.IsBase == "true")
                        {
                            return Error("系统数据不允许修改。", "");
                        }
                    }
                }

                IList<T_DevRowEntity> existsList = devRowApp.FindList(o => o.Num == devRowEntity.Num && o.F_Id != keyValue).ToList();
                if (existsList.Count > 0)
                {
                    return Error("巷道序号已存在。", "");
                }

                existsList = devRowApp.FindListAsNoTracking(o => o.WayCode == devRowEntity.WayCode && o.F_Id != keyValue).ToList();
                if (existsList.Count > 0)
                {
                    return Error("巷道编码已存在。", "");
                }


                if (devRowEntity.EquID == "AGV")
                {
                    devRowEntity.EquID = null;
                }
                devRowApp.SubmitForm(devRowEntity, keyValue);

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


        #region 删除巷道
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DevRowController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "巷道";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除巷道";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    T_DevRowEntity entity = devRowApp.FindEntity(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                rowLineApp.Delete(o => o.DevRowID == keyValue);
                devRowApp.DeleteForm(keyValue);

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
