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
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.ViewModel;
using MST.Web.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace MST.Web.Areas.SystemManage.Controllers
{
    public class UserController : ControllerBase
    {
        private UserApp userApp = new UserApp();
        private UserLogOnApp userLogOnApp = new UserLogOnApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            var data = new
            {
                rows = userApp.GetList(pagination, keyword),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetUserInfo()
        {
            string userID = OperatorProvider.Provider.GetCurrent().UserId;
            UserEntity user = userApp.FindEntity(o => o.F_Id == userID);
            UserModel usermodel = user.ToObject<UserModel>();
            usermodel.GenderName = (usermodel.F_Gender != null && usermodel.F_Gender.Value) ? "男" : "女";
            return Content(usermodel.ToJson());
        }


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = userApp.GetForm(keyValue);
            return Content(data.ToJson());
        }


        #region 新增/修改账户
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(UserEntity userEntity, UserLogOnEntity userLogOnEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "UserController.SubmitForm";
            logObj.Parms = new { userEntity = userEntity, userLogOnEntity = userLogOnEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "账户管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "新增/修改账户";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                UserEntity user = null;
                if (!string.IsNullOrEmpty(keyValue))
                {
                    user = userApp.FindEntity(o => o.F_Account == userEntity.F_Account && o.F_Id != keyValue && o.F_DeleteMark == false);
                }
                else
                {
                    user = userApp.FindEntity(o => o.F_Account == userEntity.F_Account && o.F_DeleteMark == false);
                }
                if (user != null)
                {
                    return Error("账号已存在", "");
                }
                userEntity.F_NickName = string.IsNullOrEmpty(userEntity.F_NickName) ? userEntity.F_RealName : userEntity.F_NickName;
                userEntity.F_DeleteMark = false;
                userApp.SubmitForm(userEntity, userLogOnEntity, keyValue);
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


        #region 删除账户
        [HttpPost]
        [HandlerAuthorize]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "UserController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "账户管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除账户";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                UserEntity entity = userApp.FindEntity(o => o.F_Id == keyValue);

                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                entity.F_DeleteMark = true;
                userApp.Update(entity);

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

        [HttpGet]
        public ActionResult RevisePassword()
        {
            return View();
        }

        #region 重置密码
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeUserInfo(UserModel user)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "UserController.ChangeUserInfo";
            logObj.Parms = user.ToJson();

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "账户管理";
            logEntity.F_Type = DbLogType.Update.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "修改用户信息";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                string userID = OperatorProvider.Provider.GetCurrent().UserId;
                UserEntity userDb = userApp.FindEntity(o => o.F_Id == userID);

                if (!string.IsNullOrEmpty(user.NewPwd))
                {
                    UserLogOnEntity userLogOnEntity = userLogOnApp.GetForm(userDb.F_Id);
                    string password = Md5.md5(DESEncrypt.Encrypt(user.OldPwd.ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();
                    if (password != userLogOnEntity.F_UserPassword)
                    {
                        return Error("原密码错误。", "");
                    }
                    userLogOnEntity.F_ChangePasswordDate = DateTime.Now;
                    userLogOnEntity.F_UserPassword = Md5.md5(DESEncrypt.Encrypt(Md5.md5(user.NewPwd, 32).ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();
                    userLogOnApp.Update(userLogOnEntity);
                }

                userDb.F_WeChat = user.F_WeChat;
                userDb.F_MobilePhone = user.F_MobilePhone;
                userDb.F_Email = user.F_Email;
                userApp.Update(userDb);


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


        #region 重置密码
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitRevisePassword(string userPassword, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "UserController.SubmitRevisePassword";
            logObj.Parms = new { userPassword = userPassword, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "账户管理";
            logEntity.F_Type = DbLogType.Update.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "重置账户密码";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                userLogOnApp.RevisePassword(userPassword, keyValue);

                logObj.Message = "重置密码成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("重置密码成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("重置密码失败。", ex.ToJson());
            }
        }
        #endregion


        #region 账户禁用
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DisabledAccount(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "UserController.DisabledAccount";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "账户管理";
            logEntity.F_Type = DbLogType.Update.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "账户禁用";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                UserEntity userEntity = new UserEntity();
                if (OperatorProvider.Provider.GetCurrent().UserId == keyValue)
                {
                    return Error("不可禁用当前用户", "");
                }
                userEntity.F_Id = keyValue;
                userEntity.F_EnabledMark = false;
                userApp.UpdateForm(userEntity);

                logObj.Message = "账户禁用成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                //发送到前端
                new MsgHub().SendSingle(userEntity.F_Id, MsgType.EnableLogin, new WebSocketResult() { IsSuccess = true, Data = "您的账号已被禁用" });

                return Success("账户禁用成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("账户禁用失败。", ex.ToJson());
            }
        }
        #endregion


        #region 账户启用
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult EnabledAccount(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "UserController.EnabledAccount";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "账户管理";
            logEntity.F_Type = DbLogType.Update.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "账户启用";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                UserEntity userEntity = new UserEntity();
                userEntity.F_Id = keyValue;
                userEntity.F_EnabledMark = true;
                userApp.UpdateForm(userEntity);
                logObj.Message = "账户启用成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("账户启用成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("账户启用失败。", ex.ToJson());
            }
        }
        #endregion

        [HttpGet]
        public ActionResult Info()
        {
            return View();
        }
    }
}
