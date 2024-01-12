/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.SystemSecurity;
using MST.Code;
using MST.Domain.Entity.SystemSecurity;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MST.Web.Areas.SystemSecurity.Controllers
{
    public class DbBackupController : ControllerBase
    {
        private DbBackupApp dbBackupApp = new DbBackupApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            var data = dbBackupApp.GetList(pagination, queryJson);

            var resultList = new
            {
                rows = data,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDBName()
        {
            string dbName = dbBackupApp.GetDBName();
            List<object> dbList = new List<object>();
            dbList.Add(new { dbName = dbName, dbDescription = "当前数据库" });
            return Content(dbList.ToJson());
        }

        #region 备份数据
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(DbBackupEntity dbBackupEntity)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DbBackupController.SubmitForm";
            logObj.Parms = new { dbBackupEntity = dbBackupEntity };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "备份数据";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "备份数据";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                dbBackupEntity.F_FilePath = Server.MapPath("~/Resource/DbBackup/" + dbBackupEntity.F_FileName + ".bak");
                dbBackupEntity.F_FileName = dbBackupEntity.F_FileName + ".bak";
                dbBackupEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                if (!string.IsNullOrEmpty(dbBackupEntity.F_Description)) dbBackupEntity.F_Description = dbBackupEntity.F_Description.Replace("\n", " ");
                dbBackupApp.SubmitForm(dbBackupEntity);

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

        #region 删除备份
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DbBackupController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "备份数据";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除备份";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                dbBackupApp.DeleteForm(keyValue);

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

        #region 下载备份
        [HttpPost]
        [HandlerAuthorize]
        public void DownloadBackup(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DbBackupController.DownloadBackup";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "备份数据";
            logEntity.F_Type = DbLogType.Other.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "下载备份";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                var data = dbBackupApp.GetForm(keyValue);
                //string filename = Server.UrlDecode(data.F_FileName);
                string filepath = Server.MapPath(data.F_FilePath);
                if (FileDownHelper.FileExists(filepath))
                {
                    FileDownHelper.DownLoad(data.F_FilePath);
                }

                logObj.Message = "下载成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);
            }
        }
        #endregion
    }
}
