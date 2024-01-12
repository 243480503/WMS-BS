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
using MST.Web.WebAPI.RFOffLine.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static MST.Web.WebAPI.RFOffLine.Controllers.RFOffLineController;

namespace MST.Web.Areas.PC_InventoryManage.Controllers
{
    public class OffLineDownController : ControllerBase
    {
        private T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            T_OffLineDownEntity data = offLineDownApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            var queryParam = queryJson.ToJObject();
            string keyword = "";
            string resultType = "";

            if (!queryParam["keyword"].IsEmpty())
            {
                keyword = queryParam["keyword"].ToString();
            }

            if (!queryParam["resultType"].IsEmpty())
            {
                resultType = queryParam["resultType"].ToString();
            }

            ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
            List<ItemsDetailEntity> itemsDetailConStateList = itemsDetailApp.FindEnum<T_OffLineDownEntity>(o => o.ConDetailState);
            List<ItemsDetailEntity> itemsDetailStateList = itemsDetailApp.FindEnum<T_OffLineDownEntity>(o => o.State);
            List<ItemsDetailEntity> itemsDetailLocList = itemsDetailApp.FindEnum<T_OffLineDownEntity>(o => o.ConDetailLocationState);

            List<T_OffLineDownEntity> data = new T_OffLineDownApp().GetList(pagination, keyword, resultType);


            List<OffLineDownModel> dataModel = new List<OffLineDownModel>();
            foreach (var item in data)
            {
                OffLineDownModel model = item.ToObject<OffLineDownModel>();
                model.ConDetailStateName = itemsDetailConStateList.FirstOrDefault(o => o.F_ItemCode == model.ConDetailState).F_ItemName;
                model.StateName = itemsDetailStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.ConDetailLocationStateName = itemsDetailLocList.FirstOrDefault(o => o.F_ItemCode == model.ConDetailLocationState).F_ItemName;
                dataModel.Add(model);
            }

            var resultList = new
            {
                rows = dataModel,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }


        #region 新增/修改离线
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_OffLineDownEntity offlineEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "OffLineDownController.SubmitForm";
            logObj.Parms = new { offlineEntity = offlineEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "离线出库";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新增/修改离线";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                if (!string.IsNullOrEmpty(keyValue))  //修改
                {
                    T_OffLineDownEntity entity = offLineDownApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);

                    if (entity.State == "Over")
                    {
                        return Error("已结束状态不可修改", "");
                    }

                    entity.Qty = offlineEntity.Qty;
                    entity.BarCode = offlineEntity.BarCode;
                    entity.ItemBarCode = offlineEntity.ItemBarCode;
                    entity.State = offlineEntity.State;
                    offLineDownApp.Update(entity);
                }
                else //新增
                {
                    IList<RFUploadDownModel> rfUploadDownModelList = new List<RFUploadDownModel>();
                    RFUploadDownModel model = new RFUploadDownModel();
                    model.BarCode = offlineEntity.BarCode;
                    model.ItemBarCode = offlineEntity.ItemBarCode;
                    model.OutQty = offlineEntity.Qty;
                    rfUploadDownModelList.Add(model);

                    RFOffLineResult res = new RFOffLineController().RFUploadDown(rfUploadDownModelList, OperatorProvider.Provider.GetCurrent(), "MAN", false);
                    if (!res.IsSuccess)
                    {
                        return Error(res.FailMsg, "");
                    }
                }


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


        #region 确定处理离线
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult OffLineDownOK(string keyValueListStr)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "OffLineDownController.OffLineDownOK";
            logObj.Parms = new { keyValue = keyValueListStr };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "离线出库";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "确定处理离线";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    string[] offLineDownIDList = keyValueListStr.ToObject<string[]>();

                    AjaxResult res = offLineDownApp.OffLineDownSub(db, offLineDownIDList);
                    if (res.state.ToString() == ResultType.success.ToString())
                    {
                        db.CommitWithOutRollBack();

                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLog(logEntity);

                        return Success("操作成功。");
                    }
                    else
                    {
                        return Error("操作失败。",res.message);
                    }
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


        #region 删除离线
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "OffLineDownController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "离线管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除离线";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {

                T_OffLineDownEntity entity = offLineDownApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);

                if (entity.State == "Over")
                {
                    return Error("已结束状态不可删除", "");
                }

                new T_OffLineDownApp().DeleteForm(keyValue);

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
