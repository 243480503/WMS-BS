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
    public class OrganizeController : ControllerBase
    {
        private OrganizeApp organizeApp = new OrganizeApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeSelectJson()
        {
            var data = organizeApp.GetList();
            var treeList = new List<TreeSelectModel>();
            foreach (OrganizeEntity item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.F_Id;
                treeModel.text = item.F_FullName;
                treeModel.parentId = item.F_ParentId;
                treeModel.data = item;
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeJson()
        {
            var data = organizeApp.GetList();
            var treeList = new List<TreeViewModel>();
            foreach (OrganizeEntity item in data)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = data.Count(t => t.F_ParentId == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.F_FullName;
                tree.value = item.F_EnCode;
                tree.parentId = item.F_ParentId;
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = hasChildren;
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeGridJson(string keyword)
        {
            var data = organizeApp.GetList();
            if (!string.IsNullOrEmpty(keyword))
            {
                data = data.TreeWhere(t => t.F_FullName.Contains(keyword));
            }
            var treeList = new List<TreeGridModel>();
            foreach (OrganizeEntity item in data)
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
            var data = organizeApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        #region 新增/修改机构
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(OrganizeEntity organizeEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "OrganizeController.SubmitForm";
            logObj.Parms = new { organizeEntity = organizeEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "机构管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改机构";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OrganizeEntity orgInDB = null;
                if (!string.IsNullOrEmpty(keyValue)) //修改不允许编码重复
                {
                    orgInDB = new OrganizeApp().FindEntity(o => o.F_EnCode == organizeEntity.F_EnCode && o.F_Id != keyValue);
                }
                else //新增不允许编码重复
                {
                    orgInDB = new OrganizeApp().FindEntity(o => o.F_EnCode == organizeEntity.F_EnCode);
                }

                if (orgInDB != null)
                {
                    return Error("编码已存在", "");
                }

                organizeApp.SubmitForm(organizeEntity, keyValue);
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

        #region 删除机构
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "OrganizeController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "机构管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除机构";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                string orgName = "";
                OrganizeEntity org = organizeApp.FindEntity(o => o.F_Id == keyValue);
                if (org == null)
                {
                    return Error("数据不存在", "");
                }
                //分类:Units 单位,Group 集团,Company 公司,Department 部门,WorkGroup 小组
                switch (org.F_CategoryId)
                {
                    case "Units":
                        {
                            orgName = "单位";
                        }
                        break;
                    case "Group":
                        {
                            orgName = "集团";
                        }
                        break;
                    case "Company":
                        {
                            orgName = "公司";
                        }
                        break;
                    case "Department":
                        {
                            orgName = "部门";
                        }
                        break;
                    case "WorkGroup":
                        {
                            orgName = "小组";
                        }
                        break;
                }

                List<UserEntity> userList = new UserApp().FindList(o => o.F_OrganizeId == keyValue).ToList();
                if (userList.Count > 0)
                {
                    return Error("存在用户组织属于该" + orgName, "");
                }

                userList = new UserApp().FindList(o => o.F_DepartmentId == keyValue).ToList();
                if (userList.Count > 0)
                {
                    return Error("存在用户部门属于该" + orgName, "");
                }

                List<RoleEntity> roleList = new RoleApp().FindList(o => o.F_OrganizeId == keyValue && o.F_Category == 1).ToList();
                if (roleList.Count > 0)
                {
                    return Error("存在角色属于该" + orgName, "");
                }

                List<RoleEntity> dutyList = new RoleApp().FindList(o => o.F_OrganizeId == keyValue && o.F_Category == 2).ToList();
                if (dutyList.Count > 0)
                {
                    return Error("存在岗位属于该" + orgName, "");
                }

                List<OrganizeEntity> organizeList = organizeApp.FindList(o => o.F_ParentId == keyValue).ToList();
                if (organizeList.Count > 0)
                {
                    return Error("请先删除子数据", "");
                }

                organizeApp.DeleteForm(keyValue);

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
