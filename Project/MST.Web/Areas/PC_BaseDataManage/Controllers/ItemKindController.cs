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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_BaseDataManage.Controllers
{
    public class ItemKindController : ControllerBase
    {
        private T_ItemKindApp itemKindApp = new T_ItemKindApp();
        private T_ItemApp itemApp = new T_ItemApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeSelectJson()
        {
            var data = itemKindApp.FindList(o=>o.F_DeleteMark == false);
            var treeList = new List<TreeSelectModel>();
            foreach (T_ItemKindEntity item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.F_Id;
                treeModel.text = item.KindName;
                treeModel.parentId = item.ParentID;
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeJson()
        {
            var data = itemKindApp.FindList(o => o.F_DeleteMark == false);
            var treeList = new List<TreeViewModel>();
            foreach (T_ItemKindEntity item in data)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = data.Count(t => t.ParentID == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.KindName;
                tree.value = item.KindCode;
                tree.parentId = item.ParentID;
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = hasChildren;
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="delType">1:全部，2已启用，3已删除</param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeGridJson(Pagination pagination, string delType, string keyword)
        {
            var data = itemKindApp.GetList(pagination, delType, keyword);
            var treeList = new List<TreeGridModel>();
            foreach (T_ItemKindEntity item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentID == item.F_Id) == 0 ? false : true;
                treeModel.id = item.F_Id;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentID;
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
            var data = itemKindApp.GetForm(keyValue);
            return Content(data.ToJson());
        }


        #region 新增/修改品类
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_ItemKindEntity itemsEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ItemKindController.SubmitForm";
            logObj.Parms = new { itemsEntity = itemsEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "品类管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改品类";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    if (!user.IsSystem)
                    {
                        T_ItemKindEntity entity = itemKindApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                        if (entity.IsBase == "true")
                        {
                            return Error("系统数据不允许修改。", "");
                        }
                    }
                }

                int isExistsCode = itemKindApp.FindList(o=>o.KindCode == itemsEntity.KindCode && o.F_Id != keyValue).Count();
                if(isExistsCode>0)
                {
                    return Error("编码已存在","");
                }

                int isExistsItem = itemApp.FindList(o=>o.ItemKindID == itemsEntity.ParentID && o.F_DeleteMark != true).Count();
                if (isExistsItem > 0)
                {
                    return Error("选中的上级存在物料", "");
                }

                itemsEntity.F_DeleteMark = false;
                itemKindApp.SubmitForm(itemsEntity, keyValue);

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


        #region 删除品类
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ItemKindController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "品类管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除品类";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {

                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    T_ItemKindEntity entity = itemKindApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                
                T_ItemKindEntity delEntity = itemKindApp.FindEntity(o=>o.F_Id == keyValue);

                int childKind = new T_ItemKindApp().FindList(o=>o.ParentID == delEntity.F_Id && o.F_DeleteMark!=true).Count();
                if(childKind>0)
                {
                    return Error("请先删除子品类。", "");
                }

                int childCount = new T_ItemApp().FindList(o=>o.ItemKindID == delEntity.F_Id && o.F_DeleteMark != true).Count();
                if(childCount>0)
                {
                    return Error("该品类包含物料。", "");
                }

                delEntity.F_DeleteMark = true;
                itemKindApp.Update(delEntity);

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
