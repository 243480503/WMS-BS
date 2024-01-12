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
using System.Web;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_OffRackManage.Controllers
{
    public class OffRackDetailController : ControllerBase
    {
        private T_OffRackApp offRackApp = new T_OffRackApp();
        private T_OffRackDetailApp offRackDetailApp = new T_OffRackDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_LocationApp locationApp = new T_LocationApp();

        #region 获取下架单明细，维护明细右侧下架明细列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string OffRackID, string keyword)
        {
            List<T_OffRackDetailEntity> data = offRackDetailApp.GetList(pagination, OffRackID, keyword);
            List<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_OffRackDetailEntity>(o => o.State).ToList();

            List<T_StationEntity> stationList = stationApp.GetList();

            List<OffRackDetailModel> offRackDetailModelList = data.ToObject<List<OffRackDetailModel>>();
            foreach (OffRackDetailModel model in offRackDetailModelList)
            {
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                if (!string.IsNullOrEmpty(model.StationID))
                {
                    T_StationEntity station = stationApp.FindEntity(o => o.F_Id == model.StationID);
                    model.StationCode = station.StationCode;
                    model.StationName = station.StationName;
                }
                else
                {
                    model.StationCode = "";
                    model.StationName = "";
                }
            }
            var resultList = new
            {
                rows = offRackDetailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 查看下架单明细
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = offRackDetailApp.GetForm(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region 获取左侧下架货位列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLocationList(Pagination pagination,string OffRackID, string keyword)
        {
            T_OffRackEntity offRack = offRackApp.FindEntity(o=>o.F_Id == OffRackID);
            List<T_LocationEntity> data = locationApp.GetListByAreaID(pagination, offRack.AreaID, keyword);
            IList<T_ContainerEntity> conList = new T_ContainerApp().FindList(o=>o.F_DeleteMark == false).ToList();
            IList<ItemsDetailEntity> stateItemsList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> forbiddenStateItemsList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.ForbiddenState).ToList();
            IList<ItemsDetailEntity> locationTypeItemsList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.LocationType).ToList();
            IList<ItemsDetailEntity> areaTypeItemsList = itemsDetailApp.FindEnum<T_AreaEntity>(o => o.AreaType).ToList();

            data = data.Where(o => o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyOut").ToList();

            List<LocationModel> locationModelList = data.ToObject<List<LocationModel>>();
            foreach (LocationModel model in locationModelList)
            {
                model.StateName = stateItemsList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.ForbiddenStateName = forbiddenStateItemsList.FirstOrDefault(o => o.F_ItemCode == model.ForbiddenState).F_ItemName;
                model.AreaTypeName = areaTypeItemsList.FirstOrDefault(o => o.F_ItemCode == model.AreaType).F_ItemName;
                model.LocationTypeName = locationTypeItemsList.FirstOrDefault(o => o.F_ItemCode == model.LocationType).F_ItemName;
                model.BarCode = conList.FirstOrDefault(o=>o.LocationID == model.F_Id).BarCode;
            }
            return Content(locationModelList.ToJson());
        }
        #endregion

        #region 穿梭窗口
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult TransferForm()
        {
            return View();
        }
        #endregion

        #region PC保存右侧选中货位列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFormList(string OffRackDetailEntityListStr, string OffRackID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OffRackDetailController.SubmitFormList";
                logObj.Parms = new { OffRackDetailEntity = OffRackDetailEntityListStr, keyValue = OffRackID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "下架单明细";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存下架明细";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_OffRackEntity offRack = offRackApp.FindEntity(o => o.F_Id == OffRackID);
                    if (offRack.State != "New") return Error("单据不是新建状态", "");

                    List<OffRackDetailModel> OffRackDetailList = OffRackDetailEntityListStr.ToObject<List<OffRackDetailModel>>();
                    string[] arrayInPage = OffRackDetailList.Select(o => o.F_Id).ToArray();

                    List<T_OffRackDetailEntity> OffRackDetailInDB = db.FindList<T_OffRackDetailEntity>(o => o.OffRackID == OffRackID);
                    string[] arrayInDB = OffRackDetailInDB.Select(o => o.F_Id).ToArray();

                    List<T_OffRackDetailEntity> needDelList = db.FindList<T_OffRackDetailEntity>(o => (!arrayInPage.Contains(o.F_Id)) && o.OffRackID == OffRackID);
                    List<OffRackDetailModel> needInsertList = OffRackDetailList.Where(o => !arrayInDB.Contains(o.F_Id)).ToList();

                    foreach (OffRackDetailModel entity in needInsertList)
                    {
                        T_OffRackDetailEntity offRackDetail = new T_OffRackDetailEntity();
                        offRackDetail.OffRackID = OffRackID;
                        T_LocationEntity location = locationApp.FindEntity(o => o.F_Id == entity.F_Id);
                        offRackDetail.LocationID = location.F_Id;
                        offRackDetail.LocationCode = location.LocationCode;
                        offRackDetail.SEQ = entity.SEQ;
                        T_ContainerEntity con = db.FindEntityAsNoTracking<T_ContainerEntity>(o=>o.LocationID == location.F_Id && o.F_DeleteMark ==false);
                        offRackDetail.BarCode = con.BarCode;
                        offRackDetail.AreaID = location.AreaID;
                        offRackDetail.AreaCode = location.AreaCode;
                        offRackDetail.AreaName = location.AreaName;
                        offRackDetail.AreaType = location.AreaType;
                        offRackDetail.F_DeleteMark = false;

                        T_AreaEntity areaEntity = new T_AreaApp().FindEntity(o => o.F_Id == location.AreaID);
                        FixType.Area areaNum = (FixType.Area)Enum.Parse(typeof(FixType.Area), areaEntity.AreaCode);
                        T_StationEntity station = null;
                        if (areaNum == FixType.Area.BigItemArea)
                        {
                            station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString() && o.UseCode.Contains("OffRackOut"));
                            
                        }
                        else if (areaNum == FixType.Area.NormalArea)
                        {
                            station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString() && o.UseCode.Contains("OffRackOut"));
                        }
                        else
                        {
                            return Error($"货位所在区域异常。货位 [{ location.LocationCode}] 区域 [{areaEntity.AreaName}]", "");
                        }

                        if (station == null)
                        {
                            return Error($"货位所属区域未配置下架站台", "");
                        }
                        offRackDetail.StationID = station.F_Id;
                        offRackDetail.StationCode = station.StationCode;

                        offRackDetail.F_Id = Guid.NewGuid().ToString();
                        offRackDetail.State = "New";
                        db.Insert<T_OffRackDetailEntity>(offRackDetail);
                    }

                    foreach (T_OffRackDetailEntity delcell in needDelList)
                    {
                        db.Delete<T_OffRackDetailEntity>(delcell);
                    }

                    db.CommitWithOutRollBack();

                    /*************************************************/

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

        #region 删除下架明细
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "OffRackDetailController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "下架单明细";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除下架单明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_OffRackDetailEntity data = offRackDetailApp.GetForm(keyValue);
                if (data.State != "New") return Error("明细不是新建状态", "");

                offRackDetailApp.DeleteForm(keyValue);

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
