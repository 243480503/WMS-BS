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

namespace MST.Web.Areas.PC_ReceiveRecordManage.Controllers
{
    public class ReceiveRecordController : ControllerBase
    {
        private T_ReceiveRecordApp receiveRecordApp = new T_ReceiveRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_InBoundDetailApp inboundDetailApp = new T_InBoundDetailApp();
        private T_InBoundApp inboundApp = new T_InBoundApp();

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult IndexForInbound()
        {
            return View();
        }

        #region 菜单中的收货明细
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult MenuRecord()
        {
            return View();
        }

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult MenuDetails()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMenuGridJson(Pagination pagination, string inBoundDetailID, string keyword)
        {
            var data = receiveRecordApp.GetList(pagination, inBoundDetailID, keyword);
            IList<ReceiveRecordModel> receiveRecordModel = new List<ReceiveRecordModel>();
            IList<SysItemsModel> detailList = itemsDetailApp.FindEnum<T_ReceiveRecordEntity>().ToList();
            foreach (T_ReceiveRecordEntity entity in data)
            {
                ReceiveRecordModel model = entity.ToObject<ReceiveRecordModel>();

                T_InBoundEntity inbound = inboundApp.FindEntity(o => o.F_Id == model.InBoundID);
                T_InBoundDetailEntity inboundDetail = inboundDetailApp.FindEntity(o => o.F_Id == model.InBoundDetailID);

                //model.ItemUnitText = new T_ItemApp().FindEntity(o => o.F_Id == model.ItemID).ItemUnitText;
                model.InBoundCode = inbound.InBoundCode;
                model.RefOrderCode = inbound.RefOrderCode;
                model.ItemName = inboundDetail.ItemName;
                model.SupplierUserName = inbound.SupplierUserName;
                model.StateName = detailList.FirstOrDefault(o => o.F_EnCode == "State").DetailList.FirstOrDefault(o => o.F_ItemCode == entity.State).F_ItemName;
                model.TransStateName = detailList.FirstOrDefault(o => o.F_EnCode == "TransState").DetailList.FirstOrDefault(o => o.F_ItemCode == entity.TransState).F_ItemName;
                model.ContainerTypeName = containerTypeApp.FindEntity(o => o.ContainerTypeCode == entity.ContainerType).ContainerTypeName;
                receiveRecordModel.Add(model);
            }

            var resultList = new
            {
                rows = receiveRecordModel,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMenuFormJson(string keyValue)
        {
            T_ReceiveRecordEntity data = receiveRecordApp.GetForm(keyValue);
            ReceiveRecordModel model = data.ToObject<ReceiveRecordModel>();
            T_InBoundEntity inbound = inboundApp.FindEntity(o => o.F_Id == model.InBoundID);
            T_InBoundDetailEntity inboundDetail = inboundDetailApp.FindEntity(o => o.F_Id == model.InBoundDetailID);

            model.InBoundCode = inbound.InBoundCode;
            model.RefOrderCode = inbound.RefOrderCode;
            model.ItemName = inboundDetail.ItemName;
            model.SupplierUserName = inbound.SupplierUserName;
            return Content(model.ToJson());
        }
        #endregion

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string inBoundDetailID, string keyword)
        {
            var data = receiveRecordApp.GetList(pagination, inBoundDetailID, keyword);
            IList<ReceiveRecordModel> receiveRecordModel = new List<ReceiveRecordModel>();
            IList<SysItemsModel> detailList = itemsDetailApp.FindEnum<T_ReceiveRecordEntity>().ToList();
            foreach (T_ReceiveRecordEntity entity in data)
            {
                ReceiveRecordModel model = entity.ToObject<ReceiveRecordModel>();
                //model.ItemUnitText = new T_ItemApp().FindEntity(o => o.F_Id == entity.ItemID).ItemUnitText;
                model.StateName = detailList.FirstOrDefault(o => o.F_EnCode == "State").DetailList.FirstOrDefault(o => o.F_ItemCode == entity.State).F_ItemName;
                model.TransStateName = detailList.FirstOrDefault(o => o.F_EnCode == "TransState").DetailList.FirstOrDefault(o => o.F_ItemCode == entity.TransState).F_ItemName;
                model.ContainerTypeName = containerTypeApp.FindEntity(o => o.ContainerTypeCode == entity.ContainerType).ContainerTypeName;
                receiveRecordModel.Add(model);
            }

            var resultList = new
            {
                rows = receiveRecordModel,
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
            T_ReceiveRecordEntity data = receiveRecordApp.GetForm(keyValue);
            ReceiveRecordModel model = data.ToObject<ReceiveRecordModel>();
            T_InBoundEntity inbound = inboundApp.FindEntity(o => o.F_Id == model.InBoundID);
            T_InBoundDetailEntity inboundDetail = inboundDetailApp.FindEntity(o => o.F_Id == model.InBoundDetailID);

            model.InBoundCode = inbound.InBoundCode;
            model.RefOrderCode = inbound.RefOrderCode;
            model.ItemName = inboundDetail.ItemName;
            model.SupplierUserName = inbound.SupplierUserName;
            return Content(model.ToJson());
        }

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult TransferForm()
        {
            return View();
        }

        #region PC保存入库单
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
                    if (string.IsNullOrEmpty(keyValue))
                    {
                        InBoundEntity.F_Id = Guid.NewGuid().ToString();
                        db.Insert<T_InBoundEntity>(InBoundEntity);
                    }
                    else
                    {
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

        #region 删除收货记录
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ReceiveRecordController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "收货记录";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除收货记录";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_ReceiveRecordEntity rec = new T_ReceiveRecordApp().FindEntityAsNoTracking(o => o.F_Id == keyValue);
                if (rec.State == "PutawayOver")
                {
                    return Error("已上架不允许删除。", "");
                }

                if (rec.State == "LockOver")
                {
                    return Error("已封箱不允许删除。", "");
                }


                receiveRecordApp.DeleteForm(keyValue);
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
