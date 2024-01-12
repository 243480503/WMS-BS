/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MST.Web.Areas.SystemSecurity.Controllers
{
    public class FilterIPController : ControllerBase
    {
        private FilterIPApp filterIPApp = new FilterIPApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            List<FilterIPEntity> data = filterIPApp.GetList(pagination, keyword);
            IList<FilterIPModel> filterIPModels = new List<FilterIPModel>();
            foreach(FilterIPEntity cell in data)
            {
                FilterIPModel model = cell.ToObject<FilterIPModel>();
                UserEntity user = new UserApp().FindEntity(o => o.F_Id == model.F_CreatorUserId);
                if (user != null)
                {
                    model.F_CreatorUserName = user.F_RealName;
                }
                filterIPModels.Add(model);
            }
            
            var resultList = new
            {
                rows = filterIPModels,
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
            var data = filterIPApp.GetForm(keyValue);
            FilterIPModel model = data.ToObject<FilterIPModel>();
            UserEntity user = new UserApp().FindEntity(o => o.F_Id == model.F_CreatorUserId);
            if (user != null)
            {
                model.F_CreatorUserName = user.F_RealName;

            }
            UserEntity moduser = new UserApp().FindEntity(o => o.F_Id == model.F_LastModifyUserId);
            if (moduser != null)
            {
                model.F_LastModifyUserName = moduser.F_RealName;

            }
            return Content(model.ToJson());
        }

        #region 新增/修改策略
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(FilterIPEntity filterIPEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "FilterIPController.SubmitForm";
            logObj.Parms = new { filterIPEntity = filterIPEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "策略管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改策略";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                filterIPApp.SubmitForm(filterIPEntity, keyValue);

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

        #region 删除策略
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "FilterIPController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "策略管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除策略";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                filterIPApp.DeleteForm(keyValue);

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
