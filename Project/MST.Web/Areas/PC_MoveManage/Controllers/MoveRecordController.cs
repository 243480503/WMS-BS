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
using MST.Code.Extend;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_MoveManage.Controllers
{
    public class MoveRecordController : ControllerBase
    {
        private T_MoveApp moveApp = new T_MoveApp();
        private T_MoveRecordApp moveRecordApp = new T_MoveRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_ItemAreaApp itemAreaApp = new T_ItemAreaApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_RowLineApp rowLineApp = new T_RowLineApp();
        private T_DevRowApp devRowApp = new T_DevRowApp();

        #region 移库单明细列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string moveID, string keyword)
        {
            List<T_MoveRecordEntity> dataList = moveRecordApp.GetList(pagination, moveID, keyword);

            IList<MoveRecordModel> list = new List<MoveRecordModel>();
            foreach (T_MoveRecordEntity item in dataList)
            {
                IList<T_ContainerDetailEntity> containerDetailEntityList = containerDetailApp.FindList(o => o.BarCode == item.BarCode).ToList();
                MoveRecordModel model = item.ToObject<MoveRecordModel>();
                model.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == item.ContainerKind).F_ItemName;
                model.StateName = itemsDetailApp.FindEnum<T_MoveRecordEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.Qty = containerDetailEntityList.Where(o => o.ItemID == item.ItemID && o.Lot == item.Lot).Sum(o => o.Qty);
                list.Add(model);
            }

            var resultList = new
            {
                rows = list,
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

        #region 获取新建移库单明细弹窗右侧明细列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemSelectGridJson(Pagination pagination, string moveID, string keyword)
        {
            IList<T_MoveRecordEntity> data = moveRecordApp.GetList(pagination, moveID, keyword).OrderBy(o => o.SrcLocationCode).ToList();
            IList<MoveRecordModel> detailModelList = data.ToObject<IList<MoveRecordModel>>();
            foreach (MoveRecordModel model in detailModelList)
            {
                model.F_Id = model.SrcLocationID;//对应左侧的ID
                model.LocationNo = model.SrcLocationCode;
                model.BarCode = model.BarCode;
                model.ContainerID = model.ContainerID;
                model.TagLocationID = string.IsNullOrEmpty(model.TagLocationID) ? "0" : model.TagLocationID;
            }
            return Content(detailModelList.ToJson());
        }
        #endregion

        #region 获取左侧列表货位数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemList(Pagination pagination, string moveID, string kindID, string keyword)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                T_MoveEntity move = db.FindEntity<T_MoveEntity>(o => o.F_Id == moveID);
                IQueryable<T_LocationEntity> LocQuery = db.IQueryable<T_LocationEntity>(o => o.AreaID == move.AreaID
                    && o.State == "Stored"
                    && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyOut"));

                string[] kindCode = new string[0];
                if (kindID != "0") //为0表示全部物料类型
                {
                    T_ItemKindApp kindApp = new T_ItemKindApp();
                    IList<T_ItemKindEntity> itemKindAll = kindApp.FindList(o => true).ToList();
                    IList<T_ItemKindEntity> lastList = new List<T_ItemKindEntity>();
                    new T_ItemApp().GetChild(lastList, itemKindAll, kindID);

                    kindCode = lastList.Select(o => o.KindCode).ToArray();
                }
                IQueryable<T_ContainerDetailEntity> conDetailQuery = db.IQueryable<T_ContainerDetailEntity>(o => o.AreaID == move.AreaID && (kindCode.Count() == 0 ? true : kindCode.Contains(o.KindCode)) && (string.IsNullOrEmpty(keyword) ? true : (o.LocationNo.Contains(keyword) || o.ItemCode.Contains(keyword) || o.BarCode.Contains(keyword) || o.ItemName.Contains(keyword) || o.Lot.Contains(keyword))));
                List<LeftItem> LeftItemList = LocQuery.Join(conDetailQuery, m => m.F_Id, n => n.LocationID, (m, n) => new { Loc = m, ConDetail = n }).GroupBy(o => new { F_Id = o.Loc.F_Id, LocationNo = o.Loc.LocationCode, ContainerID = o.ConDetail.ContainerID, BarCode = o.ConDetail.BarCode }).Select(o => new LeftItem
                {
                    ContainerID = o.Key.ContainerID,
                    BarCode = o.Key.BarCode,
                    F_Id = o.Key.F_Id,
                    LocationNo = o.Key.LocationNo,
                    ItemID = o.FirstOrDefault().ConDetail.ItemID,
                    ItemCode = o.FirstOrDefault().ConDetail.ItemCode,
                    ItemName = o.FirstOrDefault().ConDetail.ItemName,
                    Factory = o.FirstOrDefault().ConDetail.Factory,
                    Lot = o.FirstOrDefault().ConDetail.Lot,
                    Spec = o.FirstOrDefault().ConDetail.Spec,
                    ItemUnitText = o.FirstOrDefault().ConDetail.ItemUnitText,
                    OverdueDate = o.FirstOrDefault().ConDetail.OverdueDate,
                    Qty = o.Sum(m => m.ConDetail.Qty)
                }).ToList();

                LeftItemList = LeftItemList.GetPage(pagination, null).ToList();
                var resultList = new
                {
                    rows = LeftItemList,
                    total = pagination.total,
                    page = pagination.page,
                    records = pagination.records
                };
                return Content(resultList.ToJson());
            }
        }

        public class LeftItem
        {
            public string ItemID { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Factory { get; set; } /// 生产厂家
            public decimal? Qty { get; set; }
            public string Lot { get; set; }
            public string Spec { get; set; }
            public string ItemUnitText { get; set; }
            public DateTime? OverdueDate { get; set; }
            //public IList<T_ContainerDetailEntity> ContainerDetailList { get; set; }
            public string LocationNo { get; set; }
            public string F_Id { get; set; }

            /// <summary>
            /// 容器ID
            /// </summary>
            public string ContainerID { get; set; }

            /// <summary>
            /// 容器条码
            /// </summary>
            public string BarCode { get; set; }

        }
        #endregion

        #region 获取右侧目标货位下拉数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLocList(string srcLocID, string itemID, string TagLocationID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == itemID);
                T_ContainerTypeEntity con = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                T_ContainerDetailEntity detail = db.FindEntity<T_ContainerDetailEntity>(o => o.LocationID == srcLocID);
                T_LocationEntity locEntity = db.FindEntity<T_LocationEntity>(o => o.F_Id == srcLocID);
                T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == locEntity.AreaID);

                IList<T_LocationEntity> locList = new List<T_LocationEntity>();

                bool isEmptyIn = false;
                if (item.ItemCode == FixType.Item.EmptyPlastic.ToString() || item.ItemCode == FixType.Item.EmptyRack.ToString())
                {
                    isEmptyIn = true;
                }
                string errMsg = "";
                string pathMsg = "";
                locList = locationApp.EnableUseLocList(ref errMsg, ref pathMsg, db, area, con, isEmptyIn, detail.CheckState, item);

                if ((!string.IsNullOrEmpty(TagLocationID)) && TagLocationID != "0") // 目标货位不为自动
                {
                    T_LocationEntity entity = db.FindEntity<T_LocationEntity>(o => o.F_Id == TagLocationID);
                    if (locList.Where(o => o.F_Id == entity.F_Id).Count() < 1)
                    {
                        locList.Add(entity);
                    }
                }

                var data = locList.OrderBy(o => o.AreaCode).ThenBy(o => o.Line).ThenBy(o => o.ColNum).ThenBy(o => o.Layer).ThenBy(o => o.Deep).Select(o => new { o.F_Id, o.LocationCode }).ToList();
                return Content(data.ToJson());
            }
        }
        #endregion

        #region PC保存右侧选中列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFormList(string moveRecordEntityListStr, string moveID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "MoveRecordController.SubmitFormList"; //按实际情况修改
                logObj.Parms = new { moveRecordEntityListStr = moveRecordEntityListStr, moveID = moveID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "移库单明细"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存移库单明细"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_MoveEntity moveEntity = moveApp.FindEntity(o => o.F_Id == moveID);
                    if (moveEntity.State != "New")
                    {
                        return Error("单据不是新建状态。", "");
                    }

                    if (moveEntity.IsAuto == "true")
                    {
                        return Error("移库为自动不能手动维护明细。", "");
                    }

                    IList<MoveRecordModel> moveRecordList = moveRecordEntityListStr.ToObject<IList<MoveRecordModel>>();

                    foreach (MoveRecordModel entity in moveRecordList)
                    {
                        if (entity.IsNewAdd == "true")
                        {
                            T_MoveRecordEntity moveRecord = new T_MoveRecordEntity();

                            moveRecord.MoveID = moveID;
                            T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == entity.ItemID);
                            moveRecord.ItemCode = item.ItemCode;
                            moveRecord.ItemName = item.ItemName;
                            moveRecord.Factory = item.Factory;
                            moveRecord.ExpireDate = null;
                            moveRecord.ItemID = item.F_Id;
                            moveRecord.Qty = entity.Qty;
                            moveRecord.Lot = entity.Lot;
                            moveRecord.Spec = entity.Spec;
                            moveRecord.ItemUnitText = entity.ItemUnitText;
                            moveRecord.OverdueDate = entity.OverdueDate;
                            moveRecord.SrcLocationID = entity.F_Id;
                            moveRecord.ContainerID = entity.ContainerID;
                            moveRecord.BarCode = entity.BarCode;
                            T_LocationEntity locationEntity = new T_LocationApp().FindEntity(o => o.F_Id == entity.F_Id);
                            moveRecord.SrcLocationCode = locationEntity.LocationCode;
                            T_ContainerDetailEntity conDetail = containerDetailApp.FindEntity(o => o.LocationID == entity.F_Id);
                            moveRecord.ContainerKind = conDetail.ContainerKind;
                            moveRecord.F_Id = Guid.NewGuid().ToString();
                            moveRecord.State = "New";
                            if (entity.TagLocationID != "0")//手动指定目标货位
                            {
                                moveRecord.GenType = "PointTag";
                                T_LocationEntity loc = locationApp.FindEntity(o => o.F_Id == entity.TagLocationID);
                                moveRecord.TagLocationID = loc.F_Id;
                                moveRecord.TagLocationCode = loc.LocationCode;
                            }
                            else //手动指定源货位
                            {
                                moveRecord.GenType = "PointSource";
                                moveRecord.TagLocationID = "";
                                moveRecord.TagLocationCode = "";
                            }
                            db.Insert<T_MoveRecordEntity>(moveRecord);
                        }
                        else
                        {
                            T_MoveRecordEntity moveRecord = moveRecordApp.FindEntity(o => o.MoveID == moveID && o.F_Id == entity.F_Id); //entity.F_Id 新建时为空，修改时表示T_MoveRecordEntity的ID
                            moveRecord.Qty = entity.Qty;
                            if (entity.TagLocationID != "0")//手动指定目标货位
                            {
                                moveRecord.GenType = "PointTag";
                                T_LocationEntity loc = locationApp.FindEntity(o => o.F_Id == entity.TagLocationID);
                                moveRecord.TagLocationID = loc.F_Id;
                                moveRecord.TagLocationCode = loc.LocationCode;
                            }
                            else //手动指定源货位
                            {
                                moveRecord.GenType = "PointSource";
                                moveRecord.TagLocationID = "";
                                moveRecord.TagLocationCode = "";
                            }
                            db.Update<T_MoveRecordEntity>(moveRecord);
                        }
                    }

                    string[] existsUI = moveRecordList.Where(o => o.IsNewAdd == "false").Select(o => o.F_Id).ToArray();
                    IList<T_MoveRecordEntity> needDelList = moveRecordApp.FindList(o => (!existsUI.Contains(o.F_Id)) && o.MoveID == moveID).ToList();
                    foreach (T_MoveRecordEntity delcell in needDelList)
                    {
                        db.Delete<T_MoveRecordEntity>(delcell);
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
