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

namespace MST.Web.Areas.PC_PhoManage.Controllers
{
    public class PhoController : ControllerBase
    {
        private T_PhoApp phoHeadApp = new T_PhoApp();
        private T_PhoDetailApp phoDetailApp = new T_PhoDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ERPWarehouseApp erpWarehouseApp = new T_ERPWarehouseApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeSelectJson()
        {
            var data = phoHeadApp.FindList(o => o.F_DeleteMark == false).ToList();
            var treeList = new List<TreeSelectModel>();
            foreach (T_PhoEntity item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.F_Id;
                treeModel.text = item.PhoCode;
                treeModel.parentId = "0";
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination,string PhoID, string keyword)
        {
            List<T_PhoDetailEntity> data = phoDetailApp.GetList(pagination, PhoID, keyword);
            IList<ItemsDetailEntity> enumCheckStateList = itemsDetailApp.FindEnum<T_PhoDetailEntity>(o => o.CheckState).ToList();
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_PhoDetailEntity>(o => o.State).ToList();

            List<PhoDetailModel> phoDetailModelList = new List<PhoDetailModel>();
            foreach (var item in data)
            {
                PhoDetailModel model = item.ToObject<PhoDetailModel>();
                model.CheckStateName = enumCheckStateList.FirstOrDefault(o => o.F_ItemCode == model.CheckState).F_ItemName;
                model.ContainerTypeName = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType).ContainerTypeName;
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;

                T_ERPWarehouseEntity t_ERPWarehouseEntity = erpWarehouseApp.FindEntity(o => o.ERPHouseCode == item.ERPWarehouseCode);
                if (t_ERPWarehouseEntity != null) model.ERPWarehouseName = t_ERPWarehouseEntity.ERPHouseName;

                phoDetailModelList.Add(model);
            }

            var resultList = new
            {
                rows = phoDetailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeJson()
        {
            var data = phoHeadApp.FindList(o => o.F_DeleteMark == false).OrderByDescending(o => o.F_CreatorTime).ToList(); 
            var treeList = new List<TreeViewModel>();
            foreach (T_PhoEntity item in data)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = data.Count(t => "0" == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.PhoCode;
                tree.value = item.PhoCode;
                tree.parentId = "0";
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = hasChildren;
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeGridJson()
        {
            var data = phoHeadApp.FindList(o => o.F_DeleteMark == false).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (T_PhoEntity item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => "0" == item.F_Id) == 0 ? false : true;
                treeModel.id = item.F_Id;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = "0";
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
            var data = phoHeadApp.GetForm(keyValue);
            return Content(data.ToJson());
        }


        #region 新增/修改快照
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_PhoEntity itemsEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "PhoController.SubmitForm";
            logObj.Parms = new { itemsEntity = itemsEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "快照管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改快照";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                int isExistsCode = phoHeadApp.FindList(o=>o.PhoCode == itemsEntity.PhoCode && o.F_Id != keyValue && o.F_DeleteMark == false).Count();
                if(isExistsCode>0)
                {
                    return Error("编码已存在","");
                }
                if(string.IsNullOrEmpty(itemsEntity.PhoName))
                {
                    itemsEntity.PhoName = itemsEntity.PhoCode;
                }
                itemsEntity.PhoTime = DateTime.Now;
                phoHeadApp.SubmitForm(itemsEntity, keyValue);

                

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


        #region 删除快照
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "PhoController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "快照管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除快照";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_PhoEntity delEntity = phoHeadApp.FindEntity(o=>o.F_Id == keyValue);
                delEntity.F_DeleteMark = true;
                phoHeadApp.Update(delEntity);

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
