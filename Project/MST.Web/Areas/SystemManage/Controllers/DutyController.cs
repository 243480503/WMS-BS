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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.SystemManage.Controllers
{
    public class DutyController : ControllerBase
    {
        private DutyApp dutyApp = new DutyApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(string keyword)
        {
            var data = dutyApp.GetList(keyword);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = dutyApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        #region 新增/修改岗位
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(RoleEntity roleEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DutyController.SubmitForm";
            logObj.Parms = new { roleEntity = roleEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "岗位管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新增/修改岗位";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                RoleEntity roleInDB = null;
                if (!string.IsNullOrEmpty(keyValue)) //修改不允许编码重复
                {
                    roleInDB = new RoleApp().FindEntity(o => o.F_EnCode == roleEntity.F_EnCode && o.F_Category==2  && o.F_Id != keyValue);
                }
                else //新增不允许编码重复
                {
                    roleInDB = new RoleApp().FindEntity(o => o.F_EnCode == roleEntity.F_EnCode && o.F_Category == 2);
                }

                if(roleInDB!=null)
                {
                    return Error("岗位编号重复","");
                }

                dutyApp.SubmitForm(roleEntity, keyValue);
                RoleApp.LastModifyRoleAuthorize = DateTime.Now;

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

        #region 删除岗位
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DutyController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "岗位管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除岗位";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                List<UserEntity> userList = new UserApp().FindList(o => o.F_DutyId == keyValue).ToList();
                if (userList.Count > 0)
                {
                    return Error("存在用户属于该岗位", "");
                }

                dutyApp.DeleteForm(keyValue);

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
