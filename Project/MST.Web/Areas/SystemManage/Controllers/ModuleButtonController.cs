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
    public class ModuleButtonController : ControllerBase
    {
        private ModuleApp moduleApp = new ModuleApp();
        private ModuleButtonApp moduleButtonApp = new ModuleButtonApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeSelectJson(string moduleId)
        {
            var data = moduleButtonApp.GetList(moduleId);
            var treeList = new List<TreeSelectModel>();
            foreach (ModuleButtonEntity item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.F_Id;
                treeModel.text = item.F_FullName;
                treeModel.parentId = item.F_ParentId;
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeGridJson(string moduleId)
        {
            var data = moduleButtonApp.GetList(moduleId);
            var treeList = new List<TreeGridModel>();
            foreach (ModuleButtonEntity item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.F_ParentId == item.F_Id) == 0 ? false : true;
                treeModel.id = item.F_Id;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.F_ParentId;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = moduleButtonApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        #region 新增/修改按钮
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(ModuleButtonEntity moduleButtonEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ModuleButtonController.SubmitForm";
            logObj.Parms = new { moduleButtonEntity = moduleButtonEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "按钮管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "新增/修改按钮";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                moduleButtonApp.SubmitForm(moduleButtonEntity, keyValue);
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

        #region 删除按钮
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ModuleButtonController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "按钮管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除按钮";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                moduleButtonApp.DeleteForm(keyValue);
                RoleApp.LastModifyRoleAuthorize = DateTime.Now;
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
        public ActionResult CloneButton()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetCloneButtonTreeJson()
        {
            var moduledata = moduleApp.GetList();
            var buttondata = moduleButtonApp.GetList();
            var treeList = new List<TreeViewModel>();
            foreach (ModuleEntity item in moduledata)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = moduledata.Count(t => t.F_ParentId == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.F_FullName;
                tree.value = item.F_EnCode;
                tree.parentId = item.F_ParentId;
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = true;
                treeList.Add(tree);
            }
            foreach (ModuleButtonEntity item in buttondata)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = buttondata.Count(t => t.F_ParentId == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.F_FullName;
                tree.value = item.F_EnCode;
                if (item.F_ParentId == "0")
                {
                    tree.parentId = item.F_ModuleId;
                }
                else
                {
                    tree.parentId = item.F_ParentId;
                }
                tree.isexpand = true;
                tree.complete = true;
                tree.showcheck = true;
                tree.hasChildren = hasChildren;
                if (item.F_Icon != "")
                {
                    tree.img = item.F_Icon;
                }
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson());
        }

        #region 克隆按钮
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SubmitCloneButton(string moduleId, string Ids)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ModuleButtonController.SubmitCloneButton";
            logObj.Parms = new { moduleId = moduleId, Ids = Ids };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "按钮管理";
            logEntity.F_Type = DbLogType.Other.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "克隆按钮";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                moduleButtonApp.SubmitCloneButton(moduleId, Ids);
                RoleApp.LastModifyRoleAuthorize = DateTime.Now;
                logObj.Message = "克隆成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("克隆成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = "克隆失败";
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("克隆失败。", ex.ToJson());
            }
        }
        #endregion
    }
}
