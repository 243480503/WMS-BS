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
using MST.Application.WebMsg;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Code.Extend;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using MST.Web.Areas.PC_InventoryManage.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static MST.Application.APIPost.WCSPost;

namespace MST.Web.Areas.PC_InBoundManage.Controllers
{
    public class InBoundDetailController : ControllerBase
    {
        private T_InBoundApp InBoundApp = new T_InBoundApp();
        private T_InBoundDetailApp InBoundDetailApp = new T_InBoundDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ItemApp t_ItemApp = new T_ItemApp();
        private T_MarkRuleApp markRuleApp = new T_MarkRuleApp();
        private T_MarkRecordApp markRecordApp = new T_MarkRecordApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ERPWarehouseApp erpWavehouseApp = new T_ERPWarehouseApp();
        private T_ItemAreaApp itemAreaApp = new T_ItemAreaApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private T_ItemInStationApp itemInStationApp = new T_ItemInStationApp();

        #region 入库物料明细列表 维护明细右侧列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string InBoundID, string keyword)
        {
            var data = InBoundDetailApp.GetList(pagination, InBoundID, keyword);
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_InBoundDetailEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumCheckStateList = itemsDetailApp.FindEnum<T_InBoundDetailEntity>(o => o.CheckState).ToList();
            IList<ItemsDetailEntity> enumContainerKindList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();

            IList<T_StationEntity> stationList = stationApp.GetList();
            IList<T_ItemEntity> itemList = t_ItemApp.GetList();
            IList<T_ContainerTypeEntity> containerTypeList = containerTypeApp.GetList();

            IList<InBoundDetailModel> detailModelList = data.ToObject<IList<InBoundDetailModel>>();
            foreach (InBoundDetailModel model in detailModelList)
            {
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.CheckStateName = enumCheckStateList.FirstOrDefault(o => o.F_ItemCode == model.CheckState).F_ItemName;

                if (!string.IsNullOrEmpty(model.StationID))
                {
                    T_StationEntity station = stationList.FirstOrDefault(o => o.F_Id == model.StationID);
                    model.InPostionName = station.StationName;
                    model.InPostionCode = station.StationCode;
                }
                else
                {
                    model.InPostionName = "未设置";
                    model.InPostionCode = "未设置";
                }

                T_ItemEntity item = itemList.FirstOrDefault(o => o.F_Id == model.ItemID);

                if (!string.IsNullOrEmpty(item.ContainerType))
                {
                    T_ContainerTypeEntity containerTypeEntity = containerTypeList.FirstOrDefault(o => o.ContainerTypeCode == item.ContainerType);
                    model.ContainerKind = containerTypeEntity.ContainerKind;
                    model.ContainerKindName = enumContainerKindList.FirstOrDefault(o => o.F_ItemCode == model.ContainerKind).F_ItemName;
                }
                else
                {
                    model.ContainerKindName = "未设置";
                }
                model.UnitQty = item.UnitQty;
                model.ERPWarehouseCode = model.ERPWarehouseCode;
                model.IsItemMark = item.IsItemMark;
                model.IsMustLot = item.IsMustLot;
            }

            var resultList = new
            {
                rows = detailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }
        #endregion

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            InBoundDetailModel model = new InBoundDetailModel();
            if (!keyValue.StartsWith("New"))
            {
                T_InBoundDetailEntity data = InBoundDetailApp.GetForm(keyValue);
                model = data.ToObject<InBoundDetailModel>();
                model.IsMustLot = t_ItemApp.FindEntity(o => o.F_Id == data.ItemID).IsMustLot;

                T_ContainerDetailEntity conDetail = new T_ContainerDetailApp().FindEntity(o => o.InBoundDetailID == data.F_Id);
                if (conDetail != null)
                {
                    model.ConDetailCheckState = conDetail.CheckState;
                }
            }
            return Content(model.ToJson());
        }

        /// <summary>
        /// 料箱、料架 标签tab的明细
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetPrintJson(string keyValue)
        {
            PrintItemBarCodeModel printItem = new PrintItemBarCodeModel();
            T_InBoundDetailEntity detailEntity = InBoundDetailApp.FindEntity(o => o.F_Id == keyValue);
            T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == detailEntity.ItemID);

            //if (item.UnitQty == null || item.UnitQty <= 0)
            //{
            //    return Error("未设置物料的单位数量", "");
            //}

            //decimal printPrase = (printItem.Qty ?? 0) / (item.UnitQty ?? 0);
            //int pritnNum = 0;
            //if (!int.TryParse(printPrase.ToString(), out pritnNum))
            //{
            //    return Error("入库数量应为单位数量的整数倍", "");
            //}

            printItem.ItemCode = item.ItemCode;
            printItem.ItemName = item.ItemName;
            printItem.CreateCompany = item.Factory;
            printItem.Lot = detailEntity.Lot;
            printItem.ProductDate = detailEntity.ProductDate;
            printItem.OverdueDate = detailEntity.ValidityDayNum == 0 ? null : detailEntity.OverdueDate;
            printItem.ItemUnitText = item.ItemUnitText;
            printItem.UnitQty = item.UnitQty;
            printItem.Spec = item.Spec;
            printItem.Qty = detailEntity.Qty;
            printItem.PrintNum = Convert.ToInt32(printItem.Qty ?? 0) / Convert.ToInt32(item.UnitQty);
            return Content(printItem.ToJson());
        }

        #region 获取左侧列表物料数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemList(Pagination pagination, string kindID, string keyword)
        {
            // app已排除空料箱和空料架
            IList<T_ItemEntity> itemList = t_ItemApp.GetItemList(pagination, kindID, keyword);
            IList<ItemModel> itemModelList = itemList.ToObject<IList<ItemModel>>();
            List<ItemsDetailEntity> dicList = new ItemsDetailApp().FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind);
            foreach (ItemModel model in itemModelList)
            {
                if (!string.IsNullOrEmpty(model.ContainerType))
                {
                    T_ContainerTypeEntity conType = new T_ContainerTypeApp().FindEntity(o => o.ContainerTypeCode == model.ContainerType);
                    model.ContainerKindName = dicList.FirstOrDefault(o => o.F_ItemCode == conType.ContainerKind).F_ItemName;
                }
                else
                {
                    model.ContainerKindName = "未设置";
                }
            }

            itemModelList = itemModelList.GetPage(pagination, null).ToList();
            var resultList = new
            {
                rows = itemModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region ERP仓库下拉列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetERPList(string keyValue)
        {

            var data = erpWavehouseApp.GetList();
            return Content(data.ToJson());
        }
        #endregion

        [HttpGet]
        public virtual ActionResult TransferForm()
        {
            return View();
        }

        [HttpGet]
        public virtual ActionResult EditLotForm()
        {
            return View();
        }

        [HttpGet]
        public virtual ActionResult BarCodeListForm()
        {
            return View();
        }


        [HttpGet]
        public virtual ActionResult PrintItemBarCodeForm()
        {
            return View();
        }

        [HttpGet]
        public virtual ActionResult ChangeQA()
        {
            return View();
        }

        #region 获取打印标签的列表数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetOrderDetailInfo(string InBoundDetailID)
        {
            T_InBoundDetailEntity detail = InBoundDetailApp.FindEntity(o => o.F_Id == InBoundDetailID);
            return Content(detail.ToJson());
        }
        #endregion

        #region 获取打印标签的列表数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetPrintBarCodeList(Pagination pagination, string InBoundDetailID, string keyword)
        {
            List<T_MarkRecordEntity> recordList = new List<T_MarkRecordEntity>();
            T_MarkRuleEntity rule = markRuleApp.FindEntity(o => o.InBoundDetailID == InBoundDetailID);
            if (rule != null)
            {
                recordList = markRecordApp.GetList(pagination, rule.F_Id, keyword);
            }
            var resultList = new
            {
                rows = recordList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 切换散箱整箱模式
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult ChangeSplitModel(string InBoundDetailID)
        {
            T_InBoundDetailEntity inboundDetail = InBoundDetailApp.FindEntity(o => o.F_Id == InBoundDetailID);
            if (inboundDetail.IsSplitModel == "true")
            {
                inboundDetail.IsSplitModel = "false";
            }
            else
            {
                inboundDetail.IsSplitModel = "true";
            }
            InBoundDetailApp.Update(inboundDetail);

            return Content(inboundDetail.ToJson());
        }
        #endregion

        #region 手动新增纸箱打印标签列表数据
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult AddBarCode(string InBoundDetailID)
        {
            T_InBoundDetailEntity inboundDetail = InBoundDetailApp.FindEntity(o => o.F_Id == InBoundDetailID);
            T_InBoundEntity inbound = InBoundApp.FindEntity(o => o.F_Id == inboundDetail.InBoundID);
            T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == inboundDetail.ItemID);

            if (inboundDetail.State == "Over")
            {
                return Error("入库明细已结束", "");
            }

            if (item.IsMustLot == "true")
            {
                if (string.IsNullOrEmpty(inboundDetail.Lot))
                {
                    return Error("请先设置批号", "");
                }
            }

            string barCode = T_CodeGenApp.GenNum("ContainerBarRule");

            T_MarkRuleEntity rule = markRuleApp.FindEntity(o => o.InBoundDetailID == inboundDetail.F_Id);
            if (rule == null)
            {
                rule = new T_MarkRuleEntity();
                rule.F_Id = Guid.NewGuid().ToString();
                rule.InBoundDetailID = inboundDetail.F_Id;
                rule.SupplierCode = inbound.SupplierUserCode;
                rule.SupplierName = inbound.SupplierUserName;
                rule.ItemCode = item.ItemCode;
                rule.ItemName = item.ItemName;
                rule.Lot = inboundDetail.Lot;
                rule.Qty = inboundDetail.Qty;
                rule.PicNum = (int)((inboundDetail.Qty ?? 0) / (item.UnitQty ?? 0));
                rule.OverPicNum = 1;
                rule.IsEnable = "true";
                rule.F_DeleteMark = false;
                markRuleApp.Insert(rule);
            }
            else
            {
                rule.OverPicNum = rule.OverPicNum + 1;
                markRuleApp.Update(rule);
            }

            T_MarkRecordEntity record = new T_MarkRecordEntity();
            record.F_Id = Guid.NewGuid().ToString();
            record.MarkRuleID = rule.F_Id;
            record.BarCode = barCode;
            record.SupplierCode = inbound.SupplierUserCode;
            record.SupplierName = inbound.SupplierUserName;
            record.ItemCode = item.ItemCode;
            record.ItemName = item.ItemName;
            record.ItemID = item.F_Id;
            record.IsUsed = "false";
            record.Qty = item.UnitQty;
            record.Lot = inboundDetail.Lot;
            record.RepairPicNum = 0;
            record.PicNum = 1;
            record.IsHandPrint = "true";
            record.F_DeleteMark = false;
            record.F_CreatorTime = DateTime.Now;
            record.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
            record.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
            markRecordApp.Insert(record);

            return Success("操作成功");
        }
        #endregion

        #region 保存打印标签界面修改的数量（纸箱标签界面、料箱料架标签界面均用到）
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult ChangeBarCodeQty(string markRecordListStr)
        {
            string markRuleID = "";
            IList<T_MarkRecordEntity> markRecordList = markRecordListStr.ToObject<List<T_MarkRecordEntity>>();
            foreach (T_MarkRecordEntity markRecordCell in markRecordList)
            {
                T_MarkRecordEntity record = markRecordApp.FindEntity(o => o.F_Id == markRecordCell.F_Id);
                record.Qty = markRecordCell.Qty;
                markRecordApp.Update(record);
                markRuleID = record.MarkRuleID;
            }

            T_MarkRuleEntity rule = markRuleApp.FindEntity(o => o.F_Id == markRuleID);
            IList<T_MarkRecordEntity> recordList = markRecordApp.FindList(o => o.MarkRuleID == rule.F_Id).ToList();
            rule.Qty = recordList.Sum(o => o.Qty ?? 0);
            markRuleApp.Update(rule);
            return Success("操作成功");
        }
        #endregion

        #region 删除标签明细
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult DelBarCode(string markRecordID)
        {
            T_MarkRecordEntity record = markRecordApp.FindEntity(o => o.F_Id == markRecordID);
            if (record.IsUsed == "true")
            {
                return Error("标签已使用", "");
            }

            markRecordApp.Delete(o => o.F_Id == record.F_Id);
            return Success("操作成功");
        }
        #endregion

        #region 录入批号信息
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerLogin]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_InBoundDetailEntity InBoundDetailEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "InBoundDetailController.SubmitForm";
            logObj.Parms = new { InBoundDetailEntity = InBoundDetailEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "入库明细";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "录入批号信息";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                if (InBoundDetailEntity.ProductDate > DateTime.Now.Date) return Error("生产日期不可选择未来日期", "");

                T_InBoundDetailEntity inBoundDetailEntityDB = InBoundDetailApp.FindEntity(o => o.F_Id == keyValue);
                if (inBoundDetailEntityDB.State != "New")
                {
                    return Error("只有新建状态才可编辑。", "");
                }
                inBoundDetailEntityDB.Lot = InBoundDetailEntity.Lot;
                inBoundDetailEntityDB.ProductDate = InBoundDetailEntity.ProductDate;
                inBoundDetailEntityDB.OverdueDate = inBoundDetailEntityDB.ValidityDayNum == 0 ? null : ((DateTime?)InBoundDetailEntity.ProductDate.Value.AddDays(inBoundDetailEntityDB.ValidityDayNum.Value));
                inBoundDetailEntityDB.StoreAreaID = InBoundDetailEntity.StoreAreaID;
                inBoundDetailEntityDB.StationID = InBoundDetailEntity.StationID;

                InBoundDetailApp.Update(inBoundDetailEntityDB);
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

        #region 编辑批号时，入库站台下拉框
        [HttpGet]
        [HandlerAuthorize(false)]
        public ActionResult GetInPostionSelect(string inbounddetailID)
        {
            T_InBoundDetailEntity inBoundDetailEntityDB = InBoundDetailApp.FindEntity(o => o.F_Id == inbounddetailID);
            string[] StationIDArray = itemInStationApp.FindList(o => o.ItemID == inBoundDetailEntityDB.ItemID).Select(o => o.StationID).ToArray();
            IList<T_StationEntity> stationList = stationApp.FindList(o => StationIDArray.Contains(o.F_Id)).ToList();
            return Content(stationList.ToJson());
        }
        #endregion

        #region 编辑批号时，存储区域下拉框
        [HttpGet]
        [HandlerAuthorize(false)]
        public ActionResult GetStoreAreaSelect(string inbounddetailID)
        {
            T_InBoundDetailEntity inBoundDetailEntityDB = InBoundDetailApp.FindEntity(o => o.F_Id == inbounddetailID);
            string[] AreaIDArray = itemAreaApp.FindList(o => o.ItemID == inBoundDetailEntityDB.ItemID).Select(o => o.AreaID).ToArray();
            IList<T_AreaEntity> AreaList = areaApp.FindList(o => AreaIDArray.Contains(o.F_Id)).ToList();
            return Content(AreaList.ToJson());
        }
        #endregion

        #region 新增修改删除入库单明细，PC保存右侧选中列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFormList(string InBoundDetailEntityListStr, string InBoundID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InBoundDetailController.SubmitFormList"; //按实际情况修改
                logObj.Parms = new { InBoundEntity = InBoundDetailEntityListStr, keyValue = InBoundID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存入库单明细"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_InBoundEntity inboundEntity = InBoundApp.FindEntity(o => o.F_Id == InBoundID);
                    if (inboundEntity.State != "New")
                    {
                        return Error("单据不是新建状态。", "");
                    }

                    //if (inboundEntity.GenType == "ERP")
                    //{
                    //    return Error("该单据只能通过ERP修改。", "");
                    //}

                    IList<T_InBoundDetailEntity> InBoundDetailList = InBoundDetailEntityListStr.ToObject<IList<T_InBoundDetailEntity>>();

                    HashSet<string> hashLot = new HashSet<string>();
                    HashSet<string> hashDate = new HashSet<string>();
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    string dicterr = "";
                    foreach (T_InBoundDetailEntity entity in InBoundDetailList)
                    {
                        /// 批号必填
                        T_ItemEntity item = t_ItemApp.GetForm(entity.ItemID);
                        if (item.IsMustLot == "true")
                        {
                            if (string.IsNullOrEmpty(entity.Lot) && !hashLot.Contains(item.ItemName))
                            {
                                hashLot.Add($" {item.ItemName} ");
                            }
                            if (entity.OverdueDate == null && !hashDate.Contains(item.ItemName))
                            {
                                hashDate.Add($" {item.ItemName} ");
                            }
                        }

                        /// 物料+批号重复
                        if (dict.ContainsKey(item.ItemName))
                        {
                            if (entity.Lot == dict[item.ItemName]) dicterr += $" {item.ItemName} ";
                        }
                        else dict.Add(item.ItemName, entity.Lot);

                    }
                    if (hashLot.Count != 0)
                    {
                        string err = "批号必填：\n";
                        foreach (var item in hashLot) err += item;
                        return Error(err, "");
                    }
                    if (hashDate.Count != 0)
                    {
                        string err = "失效日期必填：\n";
                        foreach (var item in hashDate) err += item;
                        return Error(err, "");
                    }
                    if (dicterr != "") return Error($"重复物料+批号： {dicterr}", "");


                    foreach (T_InBoundDetailEntity entity in InBoundDetailList)
                    {
                        entity.InBoundID = InBoundID;
                        T_ItemEntity item = t_ItemApp.GetForm(entity.ItemID);
                        entity.ItemCode = item.ItemCode;
                        entity.ItemName = item.ItemName;
                        entity.Factory = item.Factory;
                        entity.ItemUnitText = item.ItemUnitText;
                        entity.Spec = item.Spec;
                        T_ItemInStationEntity inStation = new T_ItemInStationApp().FindEntity(o => o.ItemID == item.F_Id);
                        if (inStation == null)
                        {
                            return Error("物料未设置容器类型:" + item.ItemName, "");
                        }
                        entity.StationID = inStation.StationID;
                        IList<T_ItemAreaEntity> itemAreaList = itemAreaApp.FindList(o => o.ItemID == item.F_Id).ToList();
                        entity.StoreAreaID = itemAreaList.FirstOrDefault().AreaID; //存储区域（目前先按第一个）
                        entity.SEQ = entity.SEQ;

                        entity.ValidityDayNum = item.ValidityDayNum;
                        entity.Lot = entity.Lot;

                        if (entity.OverdueDate != null)
                        {
                            if (entity.OverdueDate < DateTime.Now.Date)
                            {
                                return Error("已过失效日期", "");
                            }
                            if (entity.ValidityDayNum == 0)
                            {
                                entity.ProductDate = null;
                            }
                            else
                            {
                                if (item.ValidityUnitType == "Year")
                                {
                                    entity.ProductDate = entity.OverdueDate.Value.AddYears(-1 * (item.ValidityDayNum ?? 0));
                                }
                                else if(item.ValidityUnitType == "Month")
                                {
                                    entity.ProductDate = entity.OverdueDate.Value.AddMonths(-1 * (item.ValidityDayNum ?? 0));
                                }
                                else if(item.ValidityUnitType == "Day")
                                {
                                    entity.ProductDate = entity.OverdueDate.Value.AddDays(-1 * (item.ValidityDayNum ?? 0));
                                }
                            }
                        }
                        else
                        {
                            entity.OverdueDate = default(DateTime?);
                        }


                        if (item.IsNeedCheck == "true")
                        {
                            entity.CheckState = "WaitCheck";
                        }
                        else
                        {
                            entity.CheckState = "UnNeed";
                        }
                        if (entity.F_Id.Contains("New_"))
                        {
                            entity.F_Id = Guid.NewGuid().ToString();
                            entity.State = "New";
                            entity.CurQty = 0;
                            entity.OverInQty = 0;
                            entity.ActionType = "Init";
                            db.Insert<T_InBoundDetailEntity>(entity);
                        }
                        else
                        {
                            db.Update<T_InBoundDetailEntity>(entity);
                        }
                    }

                    string[] updateID = InBoundDetailList.Where(o => !o.F_Id.Contains("New_")).Select(o => o.F_Id).ToArray();
                    IList<T_InBoundDetailEntity> needDelList = InBoundDetailApp.FindList(o => !updateID.Contains(o.F_Id) && o.InBoundID == InBoundID).ToList();
                    foreach (T_InBoundDetailEntity delcell in needDelList)
                    {
                        /// 删除标签
                        List<T_MarkRuleEntity> markList = db.FindList<T_MarkRuleEntity>(o => o.InBoundDetailID == delcell.F_Id).ToList();
                        foreach (T_MarkRuleEntity mark in markList)
                        {
                            List<T_MarkRecordEntity> recordList = db.FindList<T_MarkRecordEntity>(o => o.MarkRuleID == mark.F_Id);
                            foreach (T_MarkRecordEntity rec in recordList) db.Delete<T_MarkRecordEntity>(rec);

                            db.Delete<T_MarkRuleEntity>(mark);
                        }

                        db.SaveChanges();
                        db.Delete<T_InBoundDetailEntity>(delcell);
                    }

                    db.SaveChanges();
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

        #region 设置开始收货
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ReceivePlayPause(string inBoundDetailID, string action)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InBoundDetailController.ReceivePlayPause";
                logObj.Parms = new { inBoundDetailID = inBoundDetailID, action = action };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单明细";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始/暂停收货";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/


                    AjaxResult result = ReceivePlayPause(db, inBoundDetailID, action);
                    if ((ResultType)result.state == ResultType.success)
                    {
                        db.CommitWithOutRollBack();
                    }
                    else
                    {
                        db.RollBack();
                        return Error(result.message, "");
                    }

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

        public AjaxResult ReceivePlayPause(IRepositoryBase db, string inBoundDetailID, string action)
        {
            AjaxResult result = new AjaxResult();
            T_InBoundDetailEntity entity = db.FindEntity<T_InBoundDetailEntity>(inBoundDetailID);
            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == entity.ItemID);

            if (string.IsNullOrEmpty(item.ContainerType))
            {
                result.state = ResultType.error;
                result.message = "未设置物料容器类型";
                return result;
            }

            if ((item.UnitQty ?? 0) == 0)
            {
                result.state = ResultType.error;
                result.message = "未设置物料整箱数量";
                return result;
            }

            if (string.IsNullOrEmpty(item.IsItemMark))
            {
                result.state = ResultType.error;
                result.message = "未设置物料是否贴标";
                return result;
            }

            if (item.IsMustLot == "true")
            {
                if (string.IsNullOrEmpty(entity.Lot))
                {
                    result.state = ResultType.error;
                    result.message = "请先设置批号";
                    return result;
                }
            }

            if (entity == null)
            {
                result.state = ResultType.error;
                result.message = "单据明细不存在";
                return result;
            }

            if (entity.ActionType == "Hand")
            {
                result.state = ResultType.error;
                result.message = "该明细应手动入库";
                return result;
            }

            T_ItemInStationEntity itemInStation = db.FindEntity<T_ItemInStationEntity>(o => o.ItemID == item.F_Id);
            if (itemInStation == null)
            {
                result.state = ResultType.error;
                result.message = "未设置物料入库地点";
                return result;
            }

            if (string.IsNullOrEmpty(entity.StoreAreaID))
            {
                T_ItemAreaEntity itemArea = db.FindEntity<T_ItemAreaEntity>(o => o.ItemID == item.F_Id);
                if (itemArea == null)
                {
                    result.state = ResultType.error;
                    result.message = "未设置物料存储区域";
                    return result;
                }
                else
                {
                    entity.StoreAreaID = itemArea.AreaID;
                }
            }

            entity.StationID = itemInStation.StationID;
            db.Update(entity);
            db.SaveChanges();

            T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.F_Id == entity.StationID);
            string stationName = stationEntity.StationName;
            if (stationEntity == null)
            {
                result.state = ResultType.error;
                result.message = "未找到入库站台";
                return result;
            }

            if (stationEntity.CurModel == "Empty")
            {
                result.state = ResultType.error;
                result.message = stationEntity.StationName + "为空箱模式。";
                return result;
            }

            T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == entity.InBoundID);


            if (action == "Play")
            {
                if (entity.State == "Receiveing")
                {
                    result.state = ResultType.error;
                    result.message = "单据已是收货状态";
                    return result;
                }

                T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();
                bool IsHaveOffLine = offLineDownApp.IsHaveOff(db);
                if (IsHaveOffLine)
                {
                    result.state = ResultType.error;
                    result.message = "存在未处理的离线数据";
                    return result;
                }


                if (inBound.State == "New") //开始收货时，需判断库存数量+其它入库中的单据，物料数量是否低于最大库存设置
                {
                    IList<T_InBoundDetailEntity> CurInBoundDetailList = db.FindList<T_InBoundDetailEntity>(o => o.InBoundID == entity.InBoundID).ToList();
                    foreach (T_InBoundDetailEntity inBoundDetail in CurInBoundDetailList)
                    {
                        T_ItemEntity cellItem = db.FindEntity<T_ItemEntity>(o => o.F_Id == inBoundDetail.ItemID);
                        if (cellItem.MaxQty == null || cellItem.MaxQty == 0)
                        {
                            continue;
                        }
                        var inBoundWait = db.IQueryable<T_InBoundEntity>().Join(db.IQueryable<T_InBoundDetailEntity>(), m => m.F_Id, n => n.InBoundID, (m, n) => new { m, n })
                                            .Where(o => (o.m.State == "Pause" || o.m.State == "Receiveing")
                                                         && o.n.ItemID == inBoundDetail.ItemID
                                                         && (o.n.State == "Pause" || o.n.State == "Receiveing")
                                           ).GroupBy(o =>
                                           new { InBoundCode = o.m.InBoundCode }
                                           ).Select(o =>
                                           new
                                           {
                                               InBoundCode = o.Key.InBoundCode,
                                               WaitInQty = o.Sum(j => (j.n.Qty ?? 0) - (j.n.OverInQty ?? 0))
                                           }).ToList();

                        decimal? waitInQty = inBoundWait.Sum(o => o.WaitInQty);
                        decimal? allQtyInWare = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == inBoundDetail.ItemID && o.CheckState != "UnQua").Sum(o => o.Qty);
                        if (waitInQty + allQtyInWare + inBoundDetail.Qty > cellItem.MaxQty)
                        {
                            string waitInBoundMsg = $"当前 {inBound.InBoundCode}， 数量 {inBoundDetail.Qty}\n";
                            foreach (var cell in inBoundWait)
                            {
                                waitInBoundMsg += $"单据 {cell.InBoundCode}， 数量 {cell.WaitInQty}\n";
                            }

                            result.state = ResultType.error;
                            result.message = $"【{cellItem.ItemName}】 \n库存上限 {cellItem.MaxQty}，当前库存 {allQtyInWare}\n待入库单据：\n{waitInBoundMsg}";
                            return result;
                        }
                    }

                }

                if ((!string.IsNullOrEmpty(stationEntity.CurOrderID)) && (!string.IsNullOrEmpty(stationEntity.CurOrderDetailID)) && stationEntity.CurOrderDetailID != inBoundDetailID)
                {
                    result.state = ResultType.error;
                    result.message = stationName + "已绑定其它单据。";
                    return result;
                }

                if ((!string.IsNullOrEmpty(stationEntity.CurOrderDetailID)) && stationEntity.CurOrderDetailID == inBoundDetailID)
                {
                    result.state = ResultType.error;
                    result.message = stationName + "已绑定该单据。";
                    return result;
                }

                inBound.State = "Receiveing";
                db.Update<T_InBoundEntity>(inBound);


                entity.State = "Receiveing";
                entity.ActionType = "Equ"; //按自动库方式入库
                db.Update<T_InBoundDetailEntity>(entity);

                stationEntity.CurOrderID = inBound.F_Id;
                stationEntity.CurOrderDetailID = entity.F_Id;

                if (inBound.InBoundType == "PurchaseInType") //采购入库
                {
                    stationEntity.OrderType = "PurchaseIn";
                }


                db.Update<T_StationEntity>(stationEntity);

                result.state = ResultType.success;
                return result;
            }
            else if (action == "Pause")
            {
                if (entity.State == "Pause")
                {
                    result.state = ResultType.error;
                    result.message = "单据已是暂停状态";
                    return result;
                }

                if ((!string.IsNullOrEmpty(stationEntity.CurOrderID)) && stationEntity.CurOrderDetailID != inBoundDetailID)
                {
                    result.state = ResultType.error;
                    result.message = stationName + "未绑定该单据。";
                    return result;
                }

                inBound.State = "Pause";
                db.Update<T_InBoundEntity>(inBound);


                entity.State = "Pause";
                db.Update<T_InBoundDetailEntity>(entity);

                stationEntity.CurOrderID = "";
                stationEntity.CurOrderDetailID = "";
                stationEntity.OrderType = "";

                db.Update<T_StationEntity>(stationEntity);

                result.state = ResultType.success;
                return result;
            }
            else
            {
                result.state = ResultType.error;
                result.message = "参数异常";
                return result;
            }
        }
        #endregion

        /*料箱、料架 标签设置*/
        #region 批量产生子标签
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult GenPrintItem(PrintItemBarCodeModel printItem)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {

                LogObj logObj = new LogObj();
                logObj.Path = "InBoundDetailController.GenPrintItem"; //按实际情况修改
                logObj.Parms = new { printItem = printItem }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "批量产生子标签"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_InBoundDetailEntity detailEntity = InBoundDetailApp.FindEntity(o => o.F_Id == printItem.InBoundDetailID);
                    T_InBoundEntity inboundEntity = InBoundApp.FindEntity(o => o.F_Id == detailEntity.InBoundID);
                    T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == detailEntity.ItemID);

                    if (detailEntity.State == "Over")
                    {
                        return Error("单据明细已结束", "");
                    }

                    if (item.IsMustLot == "true" && string.IsNullOrEmpty(detailEntity.Lot))
                    {
                        return Error("请先设置批号", "");
                    }

                    if (item.UnitQty == null || item.UnitQty <= 0)
                    {
                        return Error("未设置物料的单位数量", "");
                    }

                    int num = printItem.PrintNum ?? 0;

                    T_MarkRuleEntity markRule = db.FindEntity<T_MarkRuleEntity>(o => o.InBoundDetailID == detailEntity.F_Id);
                    if (markRule == null)
                    {
                        markRule = new T_MarkRuleEntity();
                        markRule.F_Id = Guid.NewGuid().ToString();
                        markRule.InBoundDetailID = detailEntity.F_Id;
                        markRule.SupplierCode = inboundEntity.SupplierUserCode;
                        markRule.SupplierName = inboundEntity.SupplierUserName;
                        markRule.ItemCode = item.ItemCode;
                        markRule.ItemName = item.ItemName;
                        markRule.Lot = detailEntity.Lot;
                        markRule.Qty = printItem.Qty;
                        markRule.PicNum = num;
                        markRule.OverPicNum = 0;
                        markRule.IsEnable = "true";
                        db.Insert<T_MarkRuleEntity>(markRule);
                        db.SaveChanges();
                    }
                    else
                    {
                        markRule.PicNum = (markRule.PicNum ?? 0) + num;
                        db.Update<T_MarkRuleEntity>(markRule);
                        db.SaveChanges();
                    }
                    OperatorModel user = OperatorProvider.Provider.GetCurrent();

                    //批量产生记录

                    for (int i = 0; i < num; i++)
                    {
                        T_MarkRecordEntity markRecord = new T_MarkRecordEntity();
                        markRecord.F_Id = Guid.NewGuid().ToString();
                        markRecord.MarkRuleID = markRule.F_Id;
                        markRecord.BarCode = T_CodeGenApp.GenNum("ItemBarRule");
                        markRecord.SupplierCode = inboundEntity.SupplierUserCode;
                        markRecord.SupplierName = inboundEntity.SupplierUserName;
                        markRecord.ItemCode = detailEntity.ItemCode;
                        markRecord.ItemName = detailEntity.ItemName;
                        markRecord.Qty = item.UnitQty;
                        markRecord.IsHandPrint = "false";
                        markRecord.Lot = detailEntity.Lot;
                        markRecord.IsUsed = "false";
                        markRecord.ItemID = item.F_Id;
                        markRecord.RepairPicNum = 0;
                        markRecord.PicNum = 1;
                        markRecord.F_DeleteMark = false;
                        db.Insert<T_MarkRecordEntity>(markRecord);
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

        #region 批量打印子标签(入库单界面 料箱 或 料架 批量打印)
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult PrintAllItem(string inBoundDetailID)
        {


            LogObj logObj = new LogObj();
            logObj.Path = "InBoundDetailController.PrintItem"; //按实际情况修改
            logObj.Parms = new { inBoundDetailID = inBoundDetailID }; //按实际情况修改

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "入库单明细"; //按实际情况修改
            logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "批量打印子标签"; //按实际情况修改
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                T_MarkRuleEntity rule = markRuleApp.FindEntity(o => o.InBoundDetailID == inBoundDetailID);
                List<T_MarkRecordEntity> recordList = markRecordApp.FindList(o => o.MarkRuleID == rule.F_Id).ToList();
                if (recordList.Count < 1)
                {
                    return Error("无标签记录。", "");
                }

                AjaxResult res = null;
                foreach (T_MarkRecordEntity cell in recordList)
                {
                    res = PrintItem(cell.F_Id);
                    if (res.state.ToString() == ResultType.error.ToString())
                    {
                        break;
                    }
                }

                /**************************************************/

                if (res.state.ToString() == ResultType.error.ToString())
                {
                    logEntity.F_Result = false;
                    new LogApp().WriteDbLog(logEntity);
                    logObj.Message = res.message;
                    LogFactory.GetLogger().Info(logObj);
                    return Error("操作失败。", "");
                }
                else
                {

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);
                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);
                    return Success("操作成功。");
                }
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

        #region 单个打子标签(入库单界面 料箱 或 料架 单个打印)
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult PrintItemOne(string recordID)
        {
            try
            {
                /*************************************************/
                AjaxResult ajaxResult = PrintItem(recordID);
                /**************************************************/
                if (ajaxResult.state.ToString() == ResultType.success.ToString())
                {
                    return Success("操作成功。");
                }
                else
                {
                    return Success("操作失败:" + ajaxResult.message, ajaxResult.message);
                }

            }
            catch (Exception ex)
            {
                return Error("操作失败。", ex.ToJson());
            }
        }

        private AjaxResult PrintItem(string recordID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                AjaxResult res = new AjaxResult();
                try
                {

                    /*************************************************/
                    T_MarkRecordEntity record = markRecordApp.FindEntity(o => o.F_Id == recordID);
                    T_MarkRuleEntity markRule = markRuleApp.FindEntity(o => o.F_Id == record.MarkRuleID);
                    T_InBoundDetailEntity detailEntity = InBoundDetailApp.FindEntity(o => o.F_Id == markRule.InBoundDetailID);
                    T_InBoundEntity inboundEntity = InBoundApp.FindEntity(o => o.F_Id == detailEntity.InBoundID);
                    T_ItemEntity item = t_ItemApp.FindEntity(o => o.F_Id == detailEntity.ItemID);

                    if (detailEntity.State == "Over")
                    {
                        res.state = ResultType.error;
                        res.message = "单据明细已结束";
                        return res;
                    }

                    if (item.IsMustLot == "true" && string.IsNullOrEmpty(detailEntity.Lot))
                    {
                        res.state = ResultType.error;
                        res.message = "请先设置批号";
                        return res;
                    }

                    if (item.UnitQty == null || item.UnitQty <= 0)
                    {
                        res.state = ResultType.error;
                        res.message = "未设置物料的单位数量";
                        return res;
                    }

                    //补打一张

                    WCSPost postFirst = new WCSPost();
                    PrintItemBarCodePostModel printItemModel = new PrintItemBarCodePostModel();

                    printItemModel.ItemCode = detailEntity.ItemCode;
                    printItemModel.ItemName = detailEntity.ItemName;
                    printItemModel.UnitQty = (item.UnitQty ?? 0).ToString();
                    printItemModel.ItemUnitText = item.ItemUnitText;
                    printItemModel.Lot = detailEntity.Lot;
                    printItemModel.OverdueDate = (detailEntity.OverdueDate == null ? "" : detailEntity.OverdueDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                    printItemModel.SupplierUserName = inboundEntity.SupplierUserName;
                    printItemModel.ProductDate = (detailEntity.ProductDate == null ? "" : detailEntity.ProductDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                    printItemModel.Spec = detailEntity.Spec;
                    printItemModel.Factory = detailEntity.Factory;
                    printItemModel.User = OperatorProvider.Provider.GetCurrent();
                    printItemModel.ItemBarCode = record.BarCode;
                    WCSResult wCSResult = postFirst.PrintItemBarCode(printItemModel);
                    if (wCSResult.IsSuccess)
                    {
                        record.RepairPicNum = (record.RepairPicNum ?? 0) + 1;
                        record.PicNum = (record.PicNum ?? 0) + 1;
                        db.Update<T_MarkRecordEntity>(record);
                        db.CommitWithOutRollBack();

                        res.state = ResultType.success;
                        res.message = "操作成功";
                        return res;
                    }
                    else
                    {
                        db.RollBack();
                        res.state = ResultType.error;
                        res.message = "打印失败:" + wCSResult.FailMsg;
                        return res;
                    }

                    /**************************************************/


                }
                catch (Exception ex)
                {
                    db.RollBack();
                    res.state = ResultType.error;
                    res.message = "打印失败:" + ex.Message;
                    return res;
                }
            }
        }
        #endregion





        #region 设置强制完成收货
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ReceiveOver(string inBoundDetailID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InBoundDetailController.ReceiveOver"; //按实际情况修改
                logObj.Parms = new { inBoundDetailID = inBoundDetailID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "强制完成收货"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_InBoundDetailEntity entity = db.FindEntity<T_InBoundDetailEntity>(inBoundDetailID);
                    T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == entity.InBoundID);

                    if (entity == null)
                    {
                        return Error("单据明细不存在。", "");
                    }

                    if (entity.State == "New")
                    {
                        return Error("单据明细未开始入库", "");
                    }
                    if (entity.State == "Over")
                    {
                        return Error("单据明细已结束入库", "");
                    }

                    //IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == inBound.F_Id);
                    //if (taskList.Count > 1)
                    //{
                    //    return Error("单据任务尚未完成", "");
                    //}

                    entity.State = "Over";
                    db.Update<T_InBoundDetailEntity>(entity);
                    db.SaveChanges();

                    T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.CurOrderDetailID == entity.F_Id);

                    if (stationEntity != null)
                    {
                        stationEntity.OrderType = "";
                        stationEntity.CurOrderDetailID = "";
                        stationEntity.CurOrderID = "";
                        db.Update<T_StationEntity>(stationEntity);
                    }

                    //收货单所有明细数据均为收货完成，则更新为收货完成
                    IList<T_InBoundDetailEntity> notOverList = db.FindList<T_InBoundDetailEntity>(o => o.InBoundID == entity.InBoundID && o.State != "Over").ToList();
                    if (notOverList.Count == 0) //清空整个站台单据信息
                    {
                        inBound.State = "Over";
                        db.Update<T_InBoundEntity>(inBound);

                        /// 产生过账信息，并发送过账信息
                        if (RuleConfig.OrderTransRule.InBoundTransRule.InBoundTrans)
                        {
                            if (inBound.GenType == "ERP")
                            {
                                T_TransRecordEntity transInDB = db.FindEntity<T_TransRecordEntity>(o => o.OrderID == inBound.F_Id); //任务完成的时候也会产生过账信息，此处做个去重
                                if (transInDB == null)
                                {
                                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, inBound.F_Id, "PurchaseIn");
                                    if ((ResultType)rst.state == ResultType.success)
                                    {
                                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                        ERPPost post = new ERPPost();
                                        ERPResult erpRst = post.PostFactInOutQty(db, "PurchaseIn", trans.F_Id);
                                    }
                                    else
                                    {
                                        return Error(rst.message, "");
                                    }
                                }
                            }
                        }
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

        #region 删除入库明细
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "InBoundDetailController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "入库明细";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除入库明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_InBoundDetailEntity inBoundDetail = InBoundDetailApp.FindEntity(o => o.F_Id == keyValue);
                T_InBoundEntity inBound = InBoundApp.FindEntity(o => o.F_Id == inBoundDetail.InBoundID);
                if (inBound.State != "New")
                {
                    return Error("单据已执行，不可删除。", "");
                }

                /// 删除标签
                List<T_MarkRuleEntity> markList = markRuleApp.FindList(o => o.InBoundDetailID == inBoundDetail.F_Id).ToList();
                foreach (T_MarkRuleEntity mark in markList)
                {
                    List<T_MarkRecordEntity> recordList = markRecordApp.FindList(o => o.MarkRuleID == mark.F_Id).ToList();
                    foreach (T_MarkRecordEntity rec in recordList) markRecordApp.DeleteForm(rec.F_Id);

                    markRuleApp.DeleteForm(mark.F_Id);
                }
                InBoundDetailApp.DeleteForm(keyValue);

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

        #region 设置强制完成收货
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeConDetailCheckState(InBoundDetailModel inBoundDetail)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "InBoundDetailController.ChangeConDetailCheckState"; //按实际情况修改
                logObj.Parms = new { inBoundDetail = inBoundDetail.ToJson() }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "变更明细质检状态"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_InBoundDetailEntity entity = db.FindEntity<T_InBoundDetailEntity>(o => o.F_Id == inBoundDetail.F_Id);
                    T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == entity.InBoundID);

                    if (entity == null)
                    {
                        return Error("单据明细不存在。", "");
                    }

                    if (entity.State != "Over")
                    {
                        return Error("单据明细未完成", "");
                    }

                    IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundDetailID == inBoundDetail.F_Id);
                    foreach (T_ContainerDetailEntity cell in detailList)
                    {
                        cell.CheckState = inBoundDetail.ConDetailCheckState;
                        db.Update<T_ContainerDetailEntity>(cell);
                        db.SaveChanges();
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

    }
}
