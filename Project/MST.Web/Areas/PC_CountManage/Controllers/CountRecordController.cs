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

namespace MST.Web.Areas.PC_CountManage.Controllers
{
    public class CountRecordController : ControllerBase
    {
        private T_CountRecordApp countRecordApp = new T_CountRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string countDetailID, string keyword)
        {
            var data = countRecordApp.GetList(pagination, countDetailID, keyword);
            IList<ItemsDetailEntity> enumCountStateList = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.CountState).ToList();
            IList<ItemsDetailEntity> enumAuditStateList = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.AuditState).ToList();
            IList<ItemsDetailEntity> enumGenTypeList = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.GenType).ToList();
            IList<ItemsDetailEntity> enumCountResultList = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.CountResult).ToList();
            IList<ItemsDetailEntity> enumTransStateList = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.TransState).ToList();

            IList<CountRecordModel> countRecordModelList = new List<CountRecordModel>();
            foreach (T_CountRecordEntity entity in data)
            {
                CountRecordModel model = entity.ToObject<CountRecordModel>();
                model.StateName = enumCountStateList.FirstOrDefault(o => o.F_ItemCode == entity.CountState).F_ItemName;
                model.AuditStateName = enumAuditStateList.FirstOrDefault(o => o.F_ItemCode == entity.AuditState).F_ItemName;
                model.GenTypeName = enumGenTypeList.FirstOrDefault(o => o.F_ItemCode == entity.GenType).F_ItemName;
                model.TransStateName = enumTransStateList.FirstOrDefault(o => o.F_ItemCode == entity.TransState).F_ItemName;
                if (!string.IsNullOrEmpty(entity.CountResult))
                {
                    model.CountResultName = enumCountResultList.FirstOrDefault(o => o.F_ItemCode == entity.CountResult).F_ItemName;
                }
                //if (model.IsItemMark == "false")
                //{
                //    model.ItemBarCode = "";
                //}
                
                countRecordModelList.Add(model);
            }

            var resultList = new
            {
                rows = countRecordModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }

        #region 提交盘点结果审核
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyCount(string keyValue,string type)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountRecordController.ModifyCount";
                logObj.Parms = new { keyValue = keyValue, type= type };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "修正盘点结果";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************************/
                    T_CountRecordEntity rec = db.FindEntity<T_CountRecordEntity>(o=>o.F_Id == keyValue);

                    T_CountEntity count = db.FindEntity<T_CountEntity>(rec.CountID);
                    if (count.State == "Over") return Error("盘点单据已结束", "");
                    if (count.State == "WaitResult" || count.AuditState != "WaitAudit") return Error("盘点单据已提交审核", "");
                    if (count.State != "WaitAudit") return Error("盘点单据未结束", "");

                    List<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == count.F_Id);
                    if (detailList.Any(o => o.CountState != "WaitAudit")) return Error("盘点单据明细未全部结束", "");

                    if(type == "1")
                    {
                        rec.CountResult = "Inner_SameBoxCode";
                    }
                    else if(type =="2")
                    {
                        rec.CountResult = "Inner_NotFindBoxCode";
                    }
                    db.Update<T_CountRecordEntity>(rec);
                    db.SaveChanges();
                    db.CommitWithOutRollBack();

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功");

                    /*************************************************************/
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
    }
}
