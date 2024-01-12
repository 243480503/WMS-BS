/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.APIPost;
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

namespace MST.Web.Areas.PC_TransManage.Controllers
{
    public class TransController : ControllerBase
    {
        private T_TransRecordApp transApp = new T_TransRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            var data = transApp.GetList(pagination, queryJson);
            IList<ItemsDetailEntity> enumOrderTypelist = itemsDetailApp.FindEnum<T_TransRecordEntity>(o => o.OrderType).ToList();
            IList<ItemsDetailEntity> enumStatelist = itemsDetailApp.FindEnum<T_TransRecordEntity>(o => o.State).ToList();
            IList<TransRecordModel> transModelList = new List<TransRecordModel>();
            foreach (T_TransRecordEntity trans in data)
            {
                TransRecordModel model = trans.ToObject<TransRecordModel>();
                model.OrderTypeName = enumOrderTypelist.FirstOrDefault(o => o.F_ItemCode == trans.OrderType).F_ItemName;
                model.StateName = enumStatelist.FirstOrDefault(o => o.F_ItemCode == trans.State).F_ItemName;
                transModelList.Add(model);
            }

            var resultList = new
            {
                rows = transModelList,
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
            var data = transApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        #region 手动过账
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult SendByHand(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "TransController.SendByHand";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "过账";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "手动过账";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_TransRecordEntity trans = db.FindEntity<T_TransRecordEntity>(o => o.F_Id == keyValue);
                    if(trans.IsIgnore == "true")
                    {
                        return Error("该账目已被忽略","");
                    }

                    if(trans.State == "OK")
                    {
                        return Error("该账目已完成", "");
                    }

                    string orderType = trans.OrderType;
                    ERPPost post = new ERPPost();
                    ERPResult erpRst = post.PostFactInOutQty(db, orderType, trans.F_Id);
                    db.CommitWithOutRollBack();
                    if (!erpRst.IsSuccess)
                    {
                        return Error(erpRst.FailMsg, "");
                    }

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

        #region 新增/修改过账
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_TransRecordEntity transRecord, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TransController.SubmitForm";
            logObj.Parms = new { transRecord = transRecord, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "过账";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "修改过账";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_TransRecordEntity transInDB = transApp.FindEntity(o => o.F_Id == keyValue);
                transInDB.State = transRecord.State;
                transInDB.IsIgnore = transRecord.IsIgnore;
                transApp.Update(transInDB);
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

    }
}
