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
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_InventoryManage.Controllers
{
    public class InventoryController : ControllerBase
    {
        private T_ContainerDetailApp inventoryApp = new T_ContainerDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_ERPWarehouseApp erpWarehouseApp = new T_ERPWarehouseApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_ItemKindApp itemKindApp = new T_ItemKindApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_InBoundApp inBoundApp = new T_InBoundApp();
        private T_LocationApp locationApp = new T_LocationApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination,string kindId, string keyword)
        {
            List<T_ContainerDetailEntity> data = inventoryApp.GetList(pagination,kindId, keyword);
            IList<ItemsDetailEntity> enumCheckStateList = itemsDetailApp.FindEnum<T_ContainerDetailEntity>(o => o.CheckState).ToList();
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_ContainerDetailEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumLocationStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.State).ToList();
            List<ContainerDetailModel> inventoryModelList = new List<ContainerDetailModel>();
            foreach (var item in data)
            {
                ContainerDetailModel model = item.ToObject<ContainerDetailModel>();
                model.CheckStateName = enumCheckStateList.FirstOrDefault(o => o.F_ItemCode == model.CheckState).F_ItemName;
                model.ContainerTypeName = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType).ContainerTypeName;
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                if (!string.IsNullOrEmpty(item.LocationID))
                {
                    T_LocationEntity loc = locationApp.FindEntity(o => o.F_Id == item.LocationID);
                    model.LocationState = loc.State;
                    model.LocationStateName = enumLocationStateList.FirstOrDefault(o => o.F_ItemCode == loc.State).F_ItemName;
                }

                T_ERPWarehouseEntity t_ERPWarehouseEntity = erpWarehouseApp.FindEntity(o => o.ERPHouseCode == item.ERPWarehouseCode);
                if (t_ERPWarehouseEntity != null) model.ERPWarehouseName = t_ERPWarehouseEntity.ERPHouseName;

                T_InBoundEntity inBoundEntity = inBoundApp.FindEntity(o => o.F_Id == model.InBoundID);
                if (inBoundEntity != null) model.RefInBoundCode = inBoundEntity.RefOrderCode;
                else model.RefInBoundCode = "";

                inventoryModelList.Add(model);
            }

            var resultList = new
            {
                rows = inventoryModelList,
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
            T_ContainerDetailEntity data = inventoryApp.GetForm(keyValue);
            ContainerDetailModel model = data.ToObject<ContainerDetailModel>();
            model.StateName = itemsDetailApp.FindEnum<T_ContainerDetailEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == data.State).F_ItemName;
            T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == data.ContainerType);
            model.ContainerTypeName = containerType.ContainerTypeName;
            return Content(model.ToJson());
        }

        #region 新建/修改库存
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_ContainerDetailEntity inventoryEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InventoryController.SubmitForm";
                logObj.Parms = new { InventoryEntity = inventoryEntity, keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "库存查询";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存新增库存";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_ItemKindEntity kind = db.FindEntity<T_ItemKindEntity>(o => o.KindCode == inventoryEntity.KindCode);
                    inventoryEntity.KindCode = kind.KindCode;
                    inventoryEntity.KindName = kind.KindName;

                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == inventoryEntity.ItemCode);
                    inventoryEntity.ItemID = item.F_Id;
                    inventoryEntity.ItemUnitText = item.ItemUnitText;
                    inventoryEntity.ItemCode = item.ItemCode;
                    inventoryEntity.ItemName = item.ItemName;
                    inventoryEntity.Spec = item.Spec;

                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == inventoryEntity.AreaCode);
                    inventoryEntity.AreaID = area.F_Id;
                    inventoryEntity.AreaName = area.AreaName;
                    inventoryEntity.AreaCode = area.AreaCode;

                    T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == inventoryEntity.LocationNo);

                    T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == inventoryEntity.BarCode && o.F_DeleteMark == false);
                    if (container == null)  /// 新容器
                    {
                        container = new T_ContainerEntity();
                        container.F_Id = Guid.NewGuid().ToString();
                        container.LocationID = location.F_Id;
                        container.LocationNo = location.LocationCode;
                        container.BarCode = inventoryEntity.BarCode;
                        T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == inventoryEntity.ContainerType);
                        container.ContainerType = containerType.ContainerTypeCode;
                        container.ContainerKind = containerType.ContainerKind;
                        container.AreaID = area.F_Id;
                        container.AreaCode = area.AreaCode;
                        container.AreaName = area.AreaName;
                        container.IsContainerVir = "0";
                        container.F_DeleteMark = false;
                        container.F_CreatorTime = DateTime.Now;
                        container.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                        container.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                        db.Insert<T_ContainerEntity>(container);
                        db.SaveChanges();

                        inventoryEntity.LocationID = location.F_Id;
                        inventoryEntity.LocationNo = location.LocationCode;
                    }
                    else
                    {
                        if (container.F_DeleteMark == true)
                        {
                            return Error("该容器已被出库", "");
                        }
                        else
                        {
                            inventoryEntity.LocationID = location.F_Id;
                            inventoryEntity.LocationNo = location.LocationCode;
                        }
                    }

                    inventoryEntity.ContainerID = container.F_Id;
                    inventoryEntity.ContainerType = container.ContainerType;
                    inventoryEntity.ContainerKind = container.ContainerKind;

                    if (string.IsNullOrEmpty(inventoryEntity.ItemBarCode))
                    {
                        inventoryEntity.ItemBarCode = container.BarCode;
                        inventoryEntity.IsItemMark = "false";
                    }
                    else inventoryEntity.IsItemMark = "true";

                    var supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierName == inventoryEntity.SupplierName);
                    inventoryEntity.SupplierID = supplier.F_Id;
                    inventoryEntity.SupplierCode = supplier.SupplierCode;
                    inventoryEntity.SupplierName = supplier.SupplierName;

                    if (string.IsNullOrEmpty(keyValue))
                    {
                        inventoryEntity.F_Id = Guid.NewGuid().ToString();
                        inventoryEntity.State = "Normal";
                        if (item.IsNeedCheck == "false") inventoryEntity.CheckState = "UnNeed";
                        else inventoryEntity.CheckState = "WaitCheck";
                        inventoryEntity.F_DeleteMark = false;

                        db.Insert<T_ContainerDetailEntity>(inventoryEntity);
                    }
                    else
                    {
                        inventoryEntity.F_Id = keyValue;
                        db.Update<T_ContainerDetailEntity>(inventoryEntity);
                    }

                    db.CommitWithOutRollBack();

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作失败。", ex.ToJson());
                }
            }
        }
        #endregion

        #region 删除库存
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InventoryController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "库存查询";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除库存";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    /// 删除库存关联的货位信息
                    var containerDetail = inventoryApp.FindEntity(o => o.F_Id == keyValue);
                    var containerDetailList = inventoryApp.FindList(o => o.LocationID == containerDetail.LocationID).ToList();
                    if (containerDetailList.Count <= 1)
                    {
                        /// 更新货位状态 Empty
                        var location = db.FindEntity<T_LocationEntity>(o => o.F_Id == containerDetail.LocationID);
                        location.State = "Empty";
                        db.Update<T_LocationEntity>(location);
                    }

                    inventoryApp.DeleteForm(keyValue);

                    /**************************************************/
                    db.CommitWithOutRollBack();

                    logObj.Message = "删除成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("删除成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("删除失败。", ex.ToJson());
                }
            }
        }
        #endregion

        #region 获取ERP仓库列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetERPWarehouseList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_ERPWarehouseEntity> areaList = new T_ERPWarehouseApp().GetList().OrderBy(o => o.ERPHouseCode).ToList();
            foreach (T_ERPWarehouseEntity cell in areaList)
            {
                object obj = new
                {
                    F_Id = cell.F_Id,
                    F_Code = cell.ERPHouseCode,
                    F_Name = cell.ERPHouseName
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion

        #region 获取物料种类
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemKindList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_ItemKindEntity> list = new T_ItemKindApp().FindList(o => o.F_DeleteMark == false).OrderBy(o => o.KindCode).ToList();
            foreach (T_ItemKindEntity cell in list)
            {
                object obj = new
                {
                    F_Id = cell.F_Id,
                    F_Code = cell.KindCode,
                    F_Name = cell.KindName
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion

        #region 获取区域下拉字典
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult AreaCodeList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_AreaEntity> areaList = new T_AreaApp().GetList().OrderBy(o => o.AreaCode).ToList();
            foreach (T_AreaEntity area in areaList)
            {
                object obj = new
                {
                    F_Id = area.F_Id,
                    F_Code = area.AreaCode,
                    F_Name = area.AreaName
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion

        #region 获取物料列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_ItemEntity> list = new T_ItemApp().FindList(o => o.F_DeleteMark == false).OrderBy(o => o.ItemCode).ToList();
            foreach (T_ItemEntity cell in list)
            {
                object obj = new
                {
                    F_Id = cell.F_Id,
                    F_Code = cell.ItemCode,
                    F_Name = cell.ItemName
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion

        #region 获取容器条码列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetContainerTypeList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_ContainerTypeEntity> list = new T_ContainerTypeApp().FindList(o => o.F_DeleteMark == false).OrderBy(o => o.ContainerTypeCode).ToList();
            foreach (T_ContainerTypeEntity cell in list)
            {
                object obj = new
                {
                    F_Id = cell.F_Id,
                    F_Code = cell.ContainerTypeCode,
                    F_Name = cell.ContainerTypeName,
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion 

        #region 获取货位编码列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLocationNoList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_LocationEntity> list = new T_LocationApp().FindList(o => o.F_DeleteMark == false).OrderBy(o => o.LocationCode).ToList();
            foreach (T_LocationEntity cell in list)
            {
                object obj = new
                {
                    F_Id = cell.F_Id,
                    F_Code = cell.LocationCode,
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion

        #region 获取货位编码列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetSupplierList()
        {
            IList<Object> objList = new List<Object>();
            IList<T_SupplierEntity> list = new T_SupplierApp().FindList(o => o.F_DeleteMark == false).OrderBy(o => o.SupplierCode).ToList();
            foreach (T_SupplierEntity cell in list)
            {
                object obj = new
                {
                    F_Id = cell.F_Id,
                    F_Code = cell.SupplierCode,
                    F_Name = cell.SupplierName,
                };
                objList.Add(obj);
            }
            return Content(objList.ToJson());
        }
        #endregion
    }
}
