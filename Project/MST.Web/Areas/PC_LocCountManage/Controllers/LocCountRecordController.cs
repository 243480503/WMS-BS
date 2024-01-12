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

namespace MST.Web.Areas.PC_LocCountManage.Controllers
{
    public class LocCountRecordController : ControllerBase
    {
        private T_LocCountApp locCountApp = new T_LocCountApp();
        private T_LocCountRecordApp locCountRecordApp = new T_LocCountRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ItemApp t_ItemApp = new T_ItemApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_LocationApp locationApp = new T_LocationApp();

        #region 货位校对单明细列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string locCountID, string keyword)
        {
            List<T_LocCountRecordEntity> dataList = locCountRecordApp.GetList(pagination, locCountID, keyword);

            IList<ItemsDetailEntity> enumContainerKindList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();
            IList<ItemsDetailEntity> enumCountStateList = itemsDetailApp.FindEnum<T_LocCountRecordEntity>(o => o.CountState).ToList();
            IList<ItemsDetailEntity> enumCountResultList = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.CountResult).ToList();
            IList<ItemsDetailEntity> enumForbiddenStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.ForbiddenState).ToList();
            IList<ItemsDetailEntity> enumLocStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.State).ToList();

            T_LocCountEntity locCount = locCountApp.FindEntity(o => o.F_Id == locCountID);

            IList<LocCountRecordModel> modelList = new List<LocCountRecordModel>();
            foreach (T_LocCountRecordEntity item in dataList)
            {
                LocCountRecordModel model = item.ToObject<LocCountRecordModel>();
                if (!string.IsNullOrEmpty(model.ContainerKind)) model.ContainerKindName = enumContainerKindList.FirstOrDefault(o => o.F_ItemCode == model.ContainerKind).F_ItemName;
                model.CountStateName = enumCountStateList.FirstOrDefault(o => o.F_ItemCode == model.CountState).F_ItemName;
                if (!string.IsNullOrEmpty(model.CountResult)) model.CountResultName = enumCountResultList.FirstOrDefault(o => o.F_ItemCode == model.CountResult).F_ItemName;
                model.ForbiddenStateName = enumForbiddenStateList.FirstOrDefault(o => o.F_ItemCode == model.ForbiddenState).F_ItemName;
                model.LocStateName = enumLocStateList.FirstOrDefault(o => o.F_ItemCode == model.LocState).F_ItemName;
                model.OrderState = locCount.State;
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

        #region 打开维护明细窗口
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult TransferForm()
        {
            return View();
        }
        #endregion

        #region 获取左侧列表（筛选单据所选区域）
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemList(Pagination pagination, string locCountID,string keyword)
        {
            T_LocCountEntity locCount = locCountApp.FindEntity(o => o.F_Id == locCountID);
            List<T_LocationEntity> data = locationApp.GetCountLocList(pagination, locCount.AreaCode, keyword);

            data = data.Where(o => (o.State == "Empty" || o.State == "Stored")
                            && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyOut")).ToList();

            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumForbiddenStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.ForbiddenState).ToList();

            IList<LocationModel> modelList = data.ToObject<IList<LocationModel>>();
            foreach (LocationModel model in modelList)
            {
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.ForbiddenStateName = enumForbiddenStateList.FirstOrDefault(o => o.F_ItemCode == model.ForbiddenState).F_ItemName;
            }
            return Content(modelList.ToJson());
        }
        #endregion

        #region 获取右侧列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemSelectGridJson(Pagination pagination, string locCountID, string keyword)
        {
            IList<T_LocCountRecordEntity> data = locCountRecordApp.GetList(pagination, locCountID, keyword).ToList();

            IList<ItemsDetailEntity> enumLocStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumForbiddenStateList = itemsDetailApp.FindEnum<T_LocationEntity>(o => o.ForbiddenState).ToList();

            IList<LocCountRecordModel> modelList = data.ToObject<IList<LocCountRecordModel>>();
            foreach (LocCountRecordModel model in modelList)
            {
                model.F_Id = model.LocationID;
                model.LocStateName = enumLocStateList.FirstOrDefault(o => o.F_ItemCode == model.LocState).F_ItemName;
                model.ForbiddenStateName = enumForbiddenStateList.FirstOrDefault(o => o.F_ItemCode == model.ForbiddenState).F_ItemName;
            }
            return Content(modelList.ToJson());
        }
        #endregion

        #region PC保存右侧列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFormList(string locCountRecordEntityListStr, string locCountID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "LocCountRecordController.SubmitFormList"; //按实际情况修改
                logObj.Parms = new { LocCountRecordEntityListStr = locCountRecordEntityListStr, LocCountID = locCountID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位校对单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存货位校对单明细"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /**************************************************/
                    T_LocCountEntity locCount = locCountApp.FindEntity(o => o.F_Id == locCountID);
                    if (locCount.State != "New") return Error("单据不是新建状态", "");

                    IList<T_LocCountRecordEntity> recordInDB = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == locCountID);
                    IList<T_LocCountRecordEntity> recordList = locCountRecordEntityListStr.ToObject<IList<T_LocCountRecordEntity>>();
                    IList<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => true);
                    IList<T_AreaEntity> areaList = db.FindList<T_AreaEntity>(o => true);
                    IList<T_ContainerEntity> conList = db.FindList<T_ContainerEntity>(o => o.F_DeleteMark == false);

                    IList<T_LocCountRecordEntity> bulkInsert = new List<T_LocCountRecordEntity>();
                    IList<T_LocCountRecordEntity> bulkDel = new List<T_LocCountRecordEntity>();
                    IList<T_LocCountRecordEntity> bulkUpdate = new List<T_LocCountRecordEntity>();

                    foreach (T_LocCountRecordEntity entity in recordList)
                    {
                        T_LocCountRecordEntity record = recordInDB.FirstOrDefault(o => o.LocationCode == entity.LocationCode);
                        T_LocationEntity location = locList.FirstOrDefault(o => o.LocationCode == entity.LocationCode);

                        if (record == null)
                        {
                            record = new T_LocCountRecordEntity();

                            record.F_Id = Guid.NewGuid().ToString();
                            record.LocCountID = locCountID;
                            T_AreaEntity area = areaList.FirstOrDefault(o => o.AreaCode == entity.AreaCode);
                            record.AreaID = area.F_Id;
                            record.AreaCode = area.AreaCode;
                            record.AreaName = area.AreaName;
                            record.LocationID = location.F_Id;
                            record.LocationCode = location.LocationCode;
                            record.LocState = location.State;
                            record.ForbiddenState = location.ForbiddenState;

                            /// 已存储，获取容器信息
                            T_ContainerEntity container = conList.FirstOrDefault(o => o.LocationNo == location.LocationCode && o.F_DeleteMark == false);
                            if (location.State == "Stored")  
                            {
                                if (container == null)
                                {
                                    record.ContainerID = "";
                                    record.BarCode = "";
                                    record.ContainerKind = "";
                                }
                                else
                                {
                                    record.ContainerID = container.F_Id;
                                    record.BarCode = container.BarCode;
                                    record.ContainerKind = container.ContainerKind;
                                }
                            }
                            else if (location.State == "Empty")
                            {
                                record.ContainerID = "";
                                record.BarCode = "";
                                record.ContainerKind = "";
                            }

                            record.FactBarCode = "";
                            record.CountState = "New";
                            record.CountResult = "";
                            record.GenType = "MAN";
                            record.IsConfirm = "false";
                            record.F_DeleteMark = false;
                            //db.Insert<T_LocCountRecordEntity>(record);
                            bulkInsert.Add(record);
                        }
                        else
                        {
                            /// 已存储，获取容器信息
                            T_ContainerEntity container = conList.FirstOrDefault(o => o.LocationNo == location.LocationCode && o.F_DeleteMark == false);
                            if (location.State == "Stored")
                            {
                                if (container == null)
                                {
                                    record.ContainerID = "";
                                    record.BarCode = "";
                                    record.ContainerKind = "";
                                }
                                else
                                {
                                    record.ContainerID = container.F_Id;
                                    record.BarCode = container.BarCode;
                                    record.ContainerKind = container.ContainerKind;
                                }
                            }
                            else if (location.State == "Empty")
                            {
                                record.ContainerID = "";
                                record.BarCode = "";
                                record.ContainerKind = "";
                            }
                            //db.Update<T_LocCountRecordEntity>(record);
                            bulkUpdate.Add(record);
                        }
                    }

                    string[] arrayInPage = recordList.Select(o => o.LocationCode).ToArray();
                    IList<T_LocCountRecordEntity> needDelList = db.FindList<T_LocCountRecordEntity>(o => (!arrayInPage.Contains(o.LocationCode)) && o.LocCountID == locCountID);
                    foreach (T_LocCountRecordEntity delcell in needDelList)
                    {
                        //db.Delete<T_LocCountRecordEntity>(delcell);
                        bulkDel.Add(delcell);
                    }

                    db.BulkInsert(bulkInsert);
                    db.BulkUpdate(bulkUpdate);
                    db.BulkDelete(bulkDel);
                    db.BulkSaveChanges();

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

        #region 将校对异常的数据手动更改为正常
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult CancelLocCountRec(string locCountRecID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "LocCountRecordController.CancelLocCountRec";
                logObj.Parms = new { LocCountRecID = locCountRecID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位校对单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "取消校对异常（单独处理）";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************************/
                    string msg = "";
                    bool isSuccess = CancleRecord(db, locCountRecID,"",ref msg);
                    if (!isSuccess)
                    {
                        return Error(msg, msg);
                    }
                    else
                    {
                        db.CommitWithOutRollBack();
                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLog(logEntity);

                        return Success("操作成功。");
                    }
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

        public bool CancleRecord(IRepositoryBase db,string locCountRecID, string remark, ref string msg)
        {
            T_LocCountRecordEntity locCountRec = db.FindEntity<T_LocCountRecordEntity>(o => o.F_Id == locCountRecID);
            if (locCountRec == null)
            {
                msg = "货位校对记录不存在";
                return false;
            }
            if (locCountRec.CountState == "Over")
            {
                msg = "异常已处理";
                return false;
            }
            else if (locCountRec.CountState != "WaitConfirm")
            {
                msg = "货位记录不是待异常处理状态";
                return false;
            }

            T_LocCountEntity locCount = db.FindEntity<T_LocCountEntity>(o => o.F_Id == locCountRec.LocCountID);
            if (locCount == null)
            {
                msg = "未找到货位校对单据";
                return false;
            }
            if (locCount.State != "WaitConfirm")
            {
                msg = "单据不是待异常确认状态";
                return false;
            }

            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locCountRec.LocationCode);


            /// 还原记录状态
            if (locCountRec.LocState == "Empty")
            {
                locCountRec.CountResult = "Inner_Empty";
                loc.State = "Empty";
                loc.ForbiddenState = locCountRec.ForbiddenState;
            }
            else if (locCountRec.LocState == "Stored")
            {
                locCountRec.CountResult = "Inner_SameBoxCode";
                loc.State = "Stored";
                loc.ForbiddenState = locCountRec.ForbiddenState;
            }

            locCountRec.CountState = "Over";
            locCountRec.Remark = remark;
            db.Update<T_LocCountRecordEntity>(locCountRec);
            db.Update<T_LocationEntity>(loc);
            db.SaveChanges();

            /// 判断单据结束
            List<T_LocCountRecordEntity> noConfirmRecList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == locCount.F_Id && o.CountState != "Over");
            if (noConfirmRecList.Count == 0)
            {
                locCount.State = "Over";
                db.Update<T_LocCountEntity>(locCount);
                db.SaveChanges();
            }
            return true;
        }
    }
}
