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
    public class LocationController : ControllerBase
    {
        private T_LocationApp locationApp = new T_LocationApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_AreaApp areaApp = new T_AreaApp();

        #region 新建立库货位
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult CubeForm()
        {
            return View();
        }
        #endregion

        #region 新建平库货位
        [HttpGet]
        [HandlerAuthorize]
        public ActionResult FlatForm()
        {
            return View();
        }
        #endregion

        #region 区域-查询按钮
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string areaId, string queryJson)
        {
            var locEntityList = locationApp.GetList(pagination, areaId, queryJson);

            List<LocationModel> modelList = new List<LocationModel>();
            IList<ItemsDetailEntity> stateItemsList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> forbiddenStateItemsList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.ForbiddenState).ToList();
            IList<ItemsDetailEntity> locationTypeItemsList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.LocationType).ToList();
            IList<ItemsDetailEntity> areaTypeItemsList = itemsDetailApp.FindEnum<T_AreaEntity>(o => o.AreaType).ToList();

            foreach (T_LocationEntity item in locEntityList)
            {
                LocationModel model = item.ToObject<LocationModel>();
                model.StateName = stateItemsList.FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.ForbiddenStateName = forbiddenStateItemsList.FirstOrDefault(o => o.F_ItemCode == item.ForbiddenState).F_ItemName;
                model.AreaTypeName = areaTypeItemsList.FirstOrDefault(o => o.F_ItemCode == item.AreaType).F_ItemName;
                model.LocationTypeName = locationTypeItemsList.FirstOrDefault(o => o.F_ItemCode == item.LocationType).F_ItemName;
                modelList.Add(model);
            }

            var resultList = new
            {
                rows = modelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }
        #endregion

        #region 获取左侧种类树
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeJson()
        {
            var data = locationApp.GetTreeJson();
            return Content(data.TreeViewJson());
        }
        #endregion

        #region 获取存区域列表
        /// <summary>
        /// 获取存区域列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetErpHouseDicList()
        {
            List<T_ERPWarehouseEntity> list = new T_ERPWarehouseApp().FindList(o => o.F_DeleteMark != true).OrderBy(o=>o.ERPHouseCode).ToList();
            return Content(list.ToJson());
        }
        #endregion


        #region 根据ID获取货位明细
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            T_LocationEntity data = locationApp.GetForm(keyValue);
            LocationModel model = data.ToObject<LocationModel>();
            model.LocationTypeName = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.LocationType).FirstOrDefault(o=>o.F_ItemCode == data.LocationType).F_ItemName;
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            if (user != null)
            {
                model.IsCurSysUser = user.IsSystem;
            }
            return Content(model.ToJson());
        }
        #endregion

        #region 获取ERP区域是否物理区域配置
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetERPPhy(string areaId)
        {
            T_AreaEntity area = new T_AreaApp().FindEntity(o=>o.F_Id == areaId);
            var data = new { IsERPPy = area.IsERPPy == "true", IsCheckPy = area.IsCheckPy == "true" };
            return Content(data.ToJson());
        }
        #endregion

        #region 根据ID修改货位明细
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_LocationEntity itemEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "LocationController.SubmitForm";
            logObj.Parms = new { itemEntity = itemEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "货位管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "根据ID新增或修改货位明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                //OperatorModel user = OperatorProvider.Provider.GetCurrent();
                //if (!string.IsNullOrEmpty(keyValue))
                //{
                //    if (!user.IsSystem)
                //    {
                //        T_LocationEntity entity = locationApp.FindEntityAs(o => o.F_Id == keyValue);
                //        if (entity.IsBase == "true")
                //        {
                //            return Error("系统数据不允许修改。", "");
                //        }
                //    }
                //}

                T_LocationEntity entity = locationApp.FindEntity(o => o.F_Id == keyValue);
                entity.Weight = itemEntity.Weight ?? entity.Weight;
                entity.High = itemEntity.High ?? entity.High;
                entity.Width = itemEntity.Width ?? entity.Width;
                entity.State = itemEntity.State;
                entity.ForbiddenState = itemEntity.ForbiddenState;
                entity.WCSLocCode = itemEntity.WCSLocCode;
                entity.IsDampproof = itemEntity.IsDampproof;
                entity.IsLocTop = itemEntity.IsLocTop;
                entity.IsItemPriority = itemEntity.IsItemPriority;

                T_AreaEntity area = new T_AreaApp().FindEntity(o=>o.F_Id == entity.AreaID);
                if(area.IsERPPy == "true")
                {   
                    if(entity.ERPHouseCode != itemEntity.ERPHouseCode && entity.State != "Empty")
                    {
                        return Error("非空货位不能变更ERP物理区域。", "");
                    }
                    T_ERPWarehouseEntity erpWare = new T_ERPWarehouseApp().FindEntity(o => o.ERPHouseCode == itemEntity.ERPHouseCode);
                    entity.ERPHouseCode = erpWare.ERPHouseCode;
                    entity.ERPHouseName = erpWare.ERPHouseName;
                }

                locationApp.SubmitForm(entity, keyValue);

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

        #region 根据ID删除货位明细
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "LocationController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "货位管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除货位明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    T_LocationEntity entity = locationApp.FindEntity(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                locationApp.DeleteForm(keyValue);

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

        #region 批量增加立库货位
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult GenLocation(GenLocationModel model)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "LocationController.GenLocation";
            logObj.Parms = new { GenLocation = model };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "货位管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "批量增加立库货位";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            IList<T_LocationEntity> newEntityList = new List<T_LocationEntity>();
            DateTime now = DateTime.Now;
            T_AreaEntity area = areaApp.GetForm(model.AreaID);

            for (int i = model.BeginLine; i <= model.EndLine; i++)
            {
                for (int j = model.BeginColNum; j <= model.EndColNum; j++)
                {
                    for (int k = model.BeginLayer; k <= model.EndLayer; k++)
                    {
                        T_LocationEntity entity = new T_LocationEntity();
                        entity.AreaID = model.AreaID;
                        entity.AreaCode = area.AreaCode;
                        entity.AreaName = area.AreaName;
                        entity.AreaType = area.AreaType;
                        entity.IsAreaVir = area.IsAreaVir;
                        entity.LocationCode = GenLocationCode(area.LocationPrefix,i, j, k, 1);
                        entity.WCSLocCode = entity.LocationCode;
                        entity.LocationType = model.LocationType;
                        entity.Line = i;
                        entity.ColNum = j;
                        entity.Layer = k;
                        entity.Deep = 1;
                        entity.MapLine = i;
                        entity.MapColNum = j;
                        entity.MapLayer = k;
                        entity.High = model.High;
                        entity.Width = model.Width;
                        entity.State = model.State;
                        entity.IsDampproof = model.IsDampproof;
                        entity.IsLocTop = model.IsLocTop;
                        entity.IsItemPriority = model.IsItemPriority;
                        entity.ForbiddenState = model.ForbiddenState;
                        entity.F_DeleteMark = false;
                        entity.F_CreatorTime = now;
                        entity.IsBase = model.IsBase;
                        newEntityList.Add(entity);
                    }
                }
            }

            IList<T_LocationEntity> existList = locationApp.GetExistsLoc(newEntityList);
            if (existList.Count > 0)
            {
                List<string> msgCell = new List<string>();
                foreach (T_LocationEntity item in existList)
                {
                    msgCell.Add($"[{ item.Line }行{ item.ColNum }列{ item.Layer }层]");
                }
                string msg = string.Join(",", msgCell);
                if (msg.Length > 30)
                {
                    msg = msg.Substring(0, 30) + "...";
                }

                logObj.Message = $"货位已存在：{ msg }";
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = logObj.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error($"货位已存在：{ msg }", "");
            }
            else
            {
                foreach (T_LocationEntity entity in newEntityList)
                {
                    locationApp.SubmitForm(entity, null);
                }
            }

            logObj.Message = "操作成功";
            LogFactory.GetLogger().Info(logObj);

            logEntity.F_Result = true;
            new LogApp().WriteDbLog(logEntity);

            return Success("操作成功。");
        }

        /// 根据长宽高深生成货位编码
        public string GenLocationCode(string locationPrefix, int line,int colNum,int layer,int deep)
        {
            string lastCode = locationPrefix + line.ToString().PadLeft(3, '0') + colNum.ToString().PadLeft(3, '0') + layer.ToString().PadLeft(3, '0') + deep.ToString().PadLeft(2, '0');
            return lastCode;
        }
        #endregion

        #region 增加平库货位
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult GenFlatLocation(GenLocationModel model)
        {

            LogObj logObj = new LogObj();
            logObj.Path = "LocationController.GenFlatLocation";
            logObj.Parms = new { GenLocation = model };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "货位管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "增加平库货位";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            DateTime now = DateTime.Now;
            T_AreaEntity area = areaApp.GetForm(model.AreaID);

            T_LocationEntity entity = new T_LocationEntity();
            entity.AreaID = model.AreaID;
            entity.AreaCode = area.AreaCode;
            entity.AreaName = area.AreaName;
            entity.AreaType = area.AreaType;
            entity.IsAreaVir = area.IsAreaVir;
            entity.LocationCode = model.LocationCode;
            entity.LocationType = model.LocationType;
            entity.High = model.High;
            entity.Width = model.Width;
            entity.State = model.State;
            entity.ForbiddenState = model.ForbiddenState;
            entity.F_DeleteMark = false;
            entity.F_CreatorTime = now;
            entity.IsBase = model.IsBase;

            IList<T_LocationEntity> existList = locationApp.GetExistsPlatLoc(model.LocationCode);
            if (existList.Count > 0)
            {
                logObj.Message = $"货位编码已存在：{ model.LocationCode }";
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = logObj.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error($"货位编码已存在：{ model.LocationCode }", "");
            }

            locationApp.SubmitForm(entity, null);

            logObj.Message = "操作成功";
            LogFactory.GetLogger().Info(logObj);

            logEntity.F_Result = true;
            new LogApp().WriteDbLog(logEntity);

            return Success("操作成功。");
        }
        #endregion
    }
}
