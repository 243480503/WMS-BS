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
using MST.Code.Extend;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_InBoundManage.Controllers
{
    public class InBoundController : ControllerBase
    {
        private T_InBoundApp InBoundApp = new T_InBoundApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_MarkRuleApp markRuleApp = new T_MarkRuleApp();
        private T_MarkRecordApp markRecordApp = new T_MarkRecordApp();

        #region 获取入库单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            var data = InBoundApp.GetList(pagination, queryJson);
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_InBoundEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumGenTypeList = itemsDetailApp.FindEnum<T_InBoundEntity>(o => o.GenType).ToList();
            IList<ItemsDetailEntity> enumInBoundTypeList = itemsDetailApp.FindEnum<T_InBoundEntity>(o => o.InBoundType).ToList();

            IList<InBoundModel> inboundList = new List<InBoundModel>();
            foreach (T_InBoundEntity item in data)
            {
                InBoundModel model = item.ToObject<InBoundModel>();
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.GenTypeName = enumGenTypeList.FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;
                model.InBoundTypeName = enumInBoundTypeList.FirstOrDefault(o => o.F_ItemCode == item.InBoundType).F_ItemName;
                inboundList.Add(model);
            }
            var resultList = new
            {
                rows = inboundList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 打开入库单详情窗口
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = InBoundApp.GetForm(keyValue);
            InBoundModel model = data.ToObject<InBoundModel>();
            model.StateName = itemsDetailApp.FindEnum<T_InBoundEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == data.State).F_ItemName;
            model.GenTypeName = itemsDetailApp.FindEnum<T_InBoundEntity>(o => o.GenType).FirstOrDefault(o => o.F_ItemCode == data.GenType).F_ItemName;
            model.InBoundTypeName = itemsDetailApp.FindEnum<T_InBoundEntity>(o => o.InBoundType).FirstOrDefault(o => o.F_ItemCode == data.InBoundType).F_ItemName;
            return Content(model.ToJson());
        }
        #endregion

        #region 提交入库单信息
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_InBoundEntity InBoundEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InBoundDetailController.SubmitForm"; //按实际情况修改
                logObj.Parms = new { InBoundEntity = InBoundEntity, keyValue = keyValue }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存入库单"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/

                    InBoundEntity.GenType = "MAN";
                    T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.F_Id == InBoundEntity.SupplierUserID);
                    InBoundEntity.SupplierUserCode = supplier.SupplierCode;
                    InBoundEntity.SupplierUserName = supplier.SupplierName;
                    
                    if (string.IsNullOrEmpty(InBoundEntity.RefOrderCode))
                    {
                        InBoundEntity.RefOrderCode = InBoundEntity.InBoundCode;
                        InBoundEntity.ERPInDocCode = InBoundEntity.InBoundCode;
                    }
                    else
                    {
                        InBoundEntity.RefOrderCode = InBoundEntity.RefOrderCode;
                        InBoundEntity.ERPInDocCode = InBoundEntity.RefOrderCode;
                    }
                    if (!string.IsNullOrEmpty(InBoundEntity.Remark))
                    {
                        InBoundEntity.Remark = InBoundEntity.Remark.Replace("\n", " ");
                    }
                    if (string.IsNullOrEmpty(keyValue))
                    {
                        InBoundEntity.F_Id = Guid.NewGuid().ToString();
                        db.Insert<T_InBoundEntity>(InBoundEntity);
                    }
                    else
                    {
                        InBoundEntity.F_Id = keyValue;
                        db.Update<T_InBoundEntity>(InBoundEntity);
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

        #region 获取供应商列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetSupplierList()
        {
            var data = supplierApp.FindList(o => o.F_DeleteMark == false);
            return Content(data.ToJson());
        }
        #endregion

        #region 删除入库单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InBoundController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除入库单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == keyValue);
                    if (inBound.State != "New") return Error("单据不是新建状态。", "");

                    List<T_InBoundDetailEntity> detailList = db.FindList<T_InBoundDetailEntity>(o => o.InBoundID == inBound.F_Id);
                    foreach (T_InBoundDetailEntity detail in detailList)
                    {
                        /// 删除打印标签
                        List<T_MarkRuleEntity> markList = markRuleApp.FindList(o => o.InBoundDetailID == detail.F_Id).ToList();
                        foreach (T_MarkRuleEntity mark in markList)
                        {
                            List<T_MarkRecordEntity> recordList = markRecordApp.FindList(o => o.MarkRuleID == mark.F_Id).ToList();
                            foreach (T_MarkRecordEntity rec in recordList) markRecordApp.DeleteForm(rec.F_Id);

                            markRuleApp.DeleteForm(mark.F_Id);
                        }
                        db.Delete<T_InBoundDetailEntity>(detail);
                    }

                    db.Delete<T_InBoundEntity>(inBound);
                    db.SaveChanges();

                    db.CommitWithOutRollBack();

                    /**************************************************/

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

        #region 手动过账
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult TransByHand(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InBoundController.TransByHand";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "手动过账";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == keyValue);
                    if (inBound.State != "Over")
                    {
                        return Error("单据不是完成状态。", "");
                    }

                    if (inBound.TransState == "OverTrans" || inBound.TransState == "UnNeedTrans")
                    {
                        return Error("该单据已过账或免过账。", "");
                    }

                    string inBoundType = inBound.InBoundType;
                    string orderType;
                    switch (inBoundType)
                    {
                        case "PurchaseInType":
                            {
                                orderType = "PurchaseIn";
                            }
                            break;
                        default:
                            {

                                return Error("单据类型未知。", "");
                            }
                    }

                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, inBound.F_Id, orderType);
                    if ((ResultType)rst.state == ResultType.success)
                    {
                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                        ERPPost post = new ERPPost();
                        ERPResult erpRst = post.PostFactInOutQty(db, orderType, trans.F_Id);
                        db.CommitWithOutRollBack();
                        if (!erpRst.IsSuccess)
                        {
                            return Error(erpRst.FailMsg, "");
                        }                        
                    }
                    else
                    {
                        return Error(rst.message, "");
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
    }
}
