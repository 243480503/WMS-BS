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
    public class AreaController : ControllerBase
    {
        private T_AreaApp areaApp = new T_AreaApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeSelectJson()
        {
            var data = areaApp.GetList();
            var treeList = new List<TreeSelectModel>();
            foreach (T_AreaEntity item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.F_Id;
                treeModel.text = item.AreaName;
                treeModel.parentId = item.ParentID;
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeJson()
        {
            var data = areaApp.GetList();
            var treeList = new List<TreeViewModel>();
            foreach (T_AreaEntity item in data)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = data.Count(t => t.ParentID == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.AreaName;
                tree.value = item.AreaCode;
                tree.parentId = item.ParentID;
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = hasChildren;
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson());
        }

        /// <summary>
        /// 获取列表内容
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeGridJson()
        {
            var data = areaApp.GetList();
            var treeList = new List<TreeGridModel>();
            foreach (T_AreaEntity item in data)
            {
                AreaModel areaModel = item.ToObject<AreaModel>();
                areaModel.AreaTypeName = itemsDetailApp.FindEnum<T_AreaEntity>(o => o.AreaType).FirstOrDefault(o => o.F_ItemCode == item.AreaType).F_ItemName;
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentID == item.F_Id) == 0 ? false : true;
                treeModel.id = item.F_Id;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentID;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = areaModel.ToJson();
                treeList.Add(treeModel);
            }

            return Content(treeList.TreeGridJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            T_AreaEntity data = areaApp.GetForm(keyValue);
            AreaModel model = data.ToObject<AreaModel>();
            model.AreaTypeName = itemsDetailApp.FindEnum<T_AreaEntity>(o => o.AreaType).FirstOrDefault(o => o.F_ItemCode == data.AreaType).F_ItemName;
            return Content(model.ToJson());
        }

        #region 获取下拉字典数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetERPWarehouseList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_ERPWarehouseEntity> erpWarehouseList = new T_ERPWarehouseApp().GetList().OrderBy(o=>o.ERPHouseCode).ToList();
            foreach(T_ERPWarehouseEntity erpWarehouse in erpWarehouseList)
            {
                object obj = new {
                    F_Id = erpWarehouse.F_Id,
                    F_ItemCode = erpWarehouse.ERPHouseCode,
                    F_ItemName =erpWarehouse.ERPHouseName
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion


        #region 新增/修改区域列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_AreaEntity itemsEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "AreaController.SubmitForm";
            logObj.Parms = new { itemsEntity = itemsEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "区域管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改区域";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    if (!user.IsSystem)
                    {
                        T_AreaEntity entity = areaApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                        if (entity.IsBase == "true")
                        {
                            return Error("系统数据不允许修改。", "");
                        }
                    }
                    itemsEntity.F_DeleteMark = false;
                }

                itemsEntity.LocationPrefix = itemsEntity.LocationPrefix.ToUpper();

                int isExistsCode = areaApp.FindList(o => o.AreaCode == itemsEntity.AreaCode && o.F_Id != keyValue).Count();
                if(isExistsCode>0)
                {
                    return Error("编码已存在","");
                }

                int isExistsPre = areaApp.FindList(o => o.LocationPrefix == itemsEntity.LocationPrefix && o.F_Id != keyValue).Count();
                if (isExistsPre > 0)
                {
                    return Error("前缀已存在", "");
                }


                areaApp.SubmitForm(itemsEntity, keyValue);

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


        #region 删除区域列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "AreaController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "区域管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除区域列表";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    T_AreaEntity entity = areaApp.FindEntity(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                areaApp.DeleteForm(keyValue);

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
