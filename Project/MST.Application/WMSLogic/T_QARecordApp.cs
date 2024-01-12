/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.APIPost;
using MST.Application.SystemManage;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Domain.ViewModel;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_QARecordApp
    {
        private IT_QARecordRepository service = new T_QARecordRepository();
        public IQueryable<T_QARecordEntity> FindList(Expression<Func<T_QARecordEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_QARecordEntity FindEntity(Expression<Func<T_QARecordEntity, bool>> predicate)
        {
            T_QARecordEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_QARecordEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_QARecordEntity> GetList(Pagination pagination, string qaDetailID, string keyword)
        {
            var expression = ExtLinq.True<T_QARecordEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.QADetailID == qaDetailID && (t.BarCode.Contains(keyword) || t.ItemBarCode.Contains(keyword)));
            }
            else expression = expression.And(t => t.QADetailID == qaDetailID);
            return service.FindList(expression, pagination).ToList();
        }
        public T_QARecordEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_QARecordEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.Create();
                service.Insert(itemsEntity);
            }
        }

        public void Insert(T_QARecordEntity entity)
        {
            service.Insert(entity);
        }

        #region 质检出库运算,按抽样数量波次（取样质检）
        public AjaxResult WaveGen_QA(IRepositoryBase db, string waveType, IList<ContainerDetailModel> containerDetailModelByHandList, string[] qaDetailIDArray, bool isMustFull)
        {
            AjaxResult result = new AjaxResult();

            T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();
            bool IsHaveOffLine = offLineDownApp.IsHaveOff(db);
            if (IsHaveOffLine)
            {
                result.state = ResultType.error;
                result.message = "存在未处理的离线数据";
                return result;
            }

            IList<T_QADetailEntity> qaDetailEntitiesList = db.FindList<T_QADetailEntity>(o => qaDetailIDArray.Contains(o.F_Id));
            string[] qaHeadIDList = qaDetailEntitiesList.Select(o => o.QAID).Distinct().ToArray();
            IList<T_QAEntity> qaEntityList = db.FindList<T_QAEntity>(o => qaHeadIDList.Contains(o.F_Id));


            IList<QADetailModel> qaDetailModelsList = qaDetailEntitiesList.ToObject<IList<QADetailModel>>();
            foreach (QADetailModel model in qaDetailModelsList)
            {
                T_QAEntity qaHead = qaEntityList.FirstOrDefault(o => o.F_Id == model.QAID);
                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == model.ItemID);
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == model.StationID);
                T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == qaHead.RefInBoundCode);
                if (inBound == null)
                {
                    result.state = ResultType.error;
                    result.message = "入库来源单不存在:" + qaHead.RefInBoundCode;
                    return result;
                }
                model.QAID = qaHead.F_Id;
                model.QACode = qaHead.QACode;
                model.RefInBoundCode = qaHead.RefInBoundCode;
                model.RefInBoundID = inBound.F_Id;
                model.TagAddressCode = station.TagAddress;
                model.StationCode = station.StationCode;
                model.StationName = station.StationName;
            }

            IList<ItemGroup> itemGroupList = qaDetailModelsList.GroupBy(g => new { g.ItemID, g.ItemCode, g.Lot, g.RefInBoundCode })
                                                               .Select(o => new ItemGroup
                                                               {
                                                                   ItemID = o.Key.ItemID,
                                                                   ItemCode = o.Key.ItemCode,
                                                                   RefInBoundCode = o.Key.RefInBoundCode,
                                                                   ItemName = o.FirstOrDefault().ItemName,
                                                                   Lot = o.Key.Lot,
                                                                   Qty = o.Sum(i => (i.SampleSumNum ?? 0)),
                                                                   QADetailModelList = o.Select(k => k).OrderBy(k => k.QAID).ThenBy(k => k.SEQ).ToList()
                                                               }).ToList();


            //过滤区域、巷道、行
            IQueryable<T_AreaEntity> areaQuery = db.IQueryable<T_AreaEntity>(o => o.IsEnable == "true"); //启用的区域
            IQueryable<T_DevRowEntity> wayQuery = db.IQueryable<T_DevRowEntity>(o => o.IsEnable == "true");//启用的巷道ID
            IQueryable<T_RowLineEntity> lineQuery = db.IQueryable<T_RowLineEntity>(o => o.IsEnable == "true"); //启用的行

            var wayAndLineQuery = areaQuery.Join(wayQuery, m => m.F_Id, n => n.AreaID, (m, n) => new { AreaID = n.AreaID, WayID = n.F_Id }).Join(lineQuery, m => m.WayID, n => n.DevRowID, (m, n) => new { m.AreaID, n.Line });

            //过滤货位
            IQueryable<T_LocationEntity> locationIQ = db.IQueryable<T_LocationEntity>(o =>
            o.State == "Stored"
            && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyOut")
            ).Join(wayAndLineQuery, m => new { m.AreaID, m.Line }, n => new { n.AreaID, n.Line }, (m, n) => m);


            //过滤容器主表
            IQueryable<T_ContainerEntity> containerIQ = db.IQueryable<T_ContainerEntity>();
            //联合货位与容器
            IQueryable<LocationIDEntity> containerIDIQ = locationIQ.Join(containerIQ, m => m.F_Id, n => n.LocationID, (m, n) => new LocationIDEntity { LocationID = m.F_Id, BarCode = n.BarCode, ContainerType = n.ContainerType, LocationCode = m.LocationCode, ContainerID = n.F_Id, KindCode = n.ContainerKind });

            //出库物料分组，并校验库存是否充足
            foreach (ItemGroup itemcell in itemGroupList)
            {
                T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == itemcell.RefInBoundCode);
                if (inbound == null)
                {
                    result.state = ResultType.error;
                    result.message = "入库来源单不存在:" + itemcell.RefInBoundCode;
                    return result;
                }
                T_InBoundDetailEntity inboundDetail = null;
                if (string.IsNullOrEmpty(itemcell.Lot))
                {
                    inboundDetail = db.FindEntity<T_InBoundDetailEntity>(o => o.ItemID == itemcell.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == inbound.F_Id);
                }
                else
                {
                    inboundDetail = db.FindEntity<T_InBoundDetailEntity>(o => o.ItemID == itemcell.ItemID && o.Lot == itemcell.Lot && o.InBoundID == inbound.F_Id);
                }

                if (inboundDetail == null)
                {
                    result.state = ResultType.error;
                    result.message = "入库来源单明细不存在:物料编码" + itemcell.ItemCode;
                    return result;
                }

                //过滤库存明细
                IQueryable<T_ContainerDetailEntity> detailIQ = db.IQueryable<T_ContainerDetailEntity>(o => o.InBoundDetailID == inboundDetail.F_Id && o.Qty > 0 && o.ItemID == itemcell.ItemID && o.OutQty == 0 && o.State == "Normal" && o.CheckState == "WaitCheck" && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false");

                if (string.IsNullOrEmpty(itemcell.Lot))
                {
                    detailIQ = detailIQ.Where(o => string.IsNullOrEmpty(o.Lot));
                }
                else
                {
                    detailIQ = detailIQ.Where(o => o.Lot == itemcell.Lot);
                }

                detailIQ = detailIQ.Where(o => o.CheckState == "WaitCheck");


                IQueryable<ContainerDetailModel> conDModelIQ = detailIQ.Join(containerIDIQ, m => m.ContainerID, n => n.ContainerID,
                                                                (m, n) => new ContainerDetailModel
                                                                {
                                                                    BarCode = n.BarCode,
                                                                    ContainerType = n.ContainerType,
                                                                    LocationID = n.LocationID,
                                                                    LocationNo = n.LocationCode,
                                                                    ContainerID = n.ContainerID,
                                                                    ItemID = m.ItemID,
                                                                    ItemName = m.ItemName,
                                                                    ItemCode = m.ItemCode,
                                                                    ItemBarCode = m.ItemBarCode,
                                                                    ItemUnitText = m.ItemUnitText,
                                                                    SupplierID = m.SupplierID,
                                                                    SupplierCode = m.SupplierCode,
                                                                    SupplierName = m.SupplierName,
                                                                    ReceiveRecordID = m.ReceiveRecordID,
                                                                    ERPWarehouseCode = m.ERPWarehouseCode,
                                                                    Spec = m.Spec,
                                                                    //IsSpecial = m.IsSpecial,
                                                                    F_Id = m.F_Id,
                                                                    Qty = m.Qty ?? 0,
                                                                    Lot = m.Lot ?? "",
                                                                    HandQty = 0,
                                                                    F_CreatorTime = m.F_CreatorTime,
                                                                    KindCode = n.KindCode,
                                                                    IsItemMark = m.IsItemMark,
                                                                    Factory = m.Factory,
                                                                    ProductDate = m.ProductDate,
                                                                    OverdueDate = m.OverdueDate,
                                                                    ValidityDayNum = m.ValidityDayNum
                                                                });

                itemcell.ContainerDetailModelList = conDModelIQ.ToList();

                //将手动选择的部分赋值给库存对象
                if (containerDetailModelByHandList != null)
                {
                    foreach (ContainerDetailModel handModel in containerDetailModelByHandList)
                    {
                        ContainerDetailModel inDetail = itemcell.ContainerDetailModelList.FirstOrDefault(o => o.F_Id == handModel.F_Id);
                        if (inDetail == null) //手动选择的不存在于库存集合（可能Qty-OutQty>0 或货位 被设置为待出库）
                        {
                            IQueryable<T_ContainerEntity> conIQ = db.IQueryable<T_ContainerEntity>();
                            IQueryable<T_LocationEntity> locIQ = db.IQueryable<T_LocationEntity>();
                            IQueryable<T_ContainerDetailEntity> conDetailIQ = db.IQueryable<T_ContainerDetailEntity>();

                            ContainerDetailModel containerDB = conIQ.Join(locIQ, m => m.LocationID, n => n.F_Id,
                                (m, n) => new { ContainerID = m.F_Id, n.LocationCode, m.BarCode, m.ContainerType, LocationID = n.F_Id, m.ContainerKind })
                                .Join(conDetailIQ, j => j.ContainerID, k => k.ContainerID, (j, k) => new ContainerDetailModel()
                                {
                                    BarCode = j.BarCode,
                                    ContainerType = j.ContainerType,
                                    LocationID = j.LocationID,
                                    LocationNo = j.LocationCode,
                                    ContainerID = j.ContainerID,
                                    ItemID = k.ItemID,
                                    ItemName = k.ItemName,
                                    ItemCode = k.ItemCode,
                                    ItemBarCode = k.ItemBarCode,
                                    ItemUnitText = k.ItemUnitText,
                                    SupplierID = k.SupplierID,
                                    SupplierCode = k.SupplierCode,
                                    SupplierName = k.SupplierName,
                                    ReceiveRecordID = k.ReceiveRecordID,
                                    ERPWarehouseCode = k.ERPWarehouseCode,
                                    Spec = k.Spec,
                                    //IsSpecial = k.IsSpecial,
                                    F_Id = k.F_Id,
                                    Qty = k.Qty ?? 0,
                                    Lot = k.Lot ?? "",
                                    HandQty = handModel.HandQty,
                                    F_CreatorTime = k.F_CreatorTime,
                                    KindCode = j.ContainerKind,
                                    IsItemMark = k.IsItemMark,
                                    Factory = k.Factory,
                                    ProductDate = k.ProductDate,
                                    OverdueDate = k.OverdueDate,
                                    ValidityDayNum = k.ValidityDayNum
                                }).Where(o => o.F_Id == handModel.F_Id).FirstOrDefault();

                            itemcell.ContainerDetailModelList.Add(containerDB);
                        }
                        else
                        {
                            inDetail.HandQty = handModel.HandQty;
                        }
                    }
                }
                //所有当前物料的可用库存,策略：手动优先，其次随机
                itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.OrderByDescending(o => o.HandQty ?? 0).ThenBy(o => Guid.NewGuid()).ToList();

                decimal qtyAll = itemcell.ContainerDetailModelList.Sum(o => o.Qty ?? 0);
                if (isMustFull)
                {
                    if (qtyAll < itemcell.Qty)
                    {
                        result.state = ResultType.error;
                        result.message = $"[ { itemcell.ItemName } ] 库存不足";
                        return result;
                    }
                }
            }

            //产生波次单
            T_WaveEntity waveEntity = AddWave_QA(db, waveType, qaDetailEntitiesList);

            IList<T_ContainerTypeEntity> containerTypeAll = db.FindList<T_ContainerTypeEntity>(o => true).ToList();
            ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
            IList<ItemsDetailEntity> sysitemList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();

            //产生拣选数据
            List<T_QARecordEntity> qaRecordList = new List<T_QARecordEntity>();
            foreach (ItemGroup itemcell in itemGroupList)
            {
                foreach (QADetailModel qaDetailModel in itemcell.QADetailModelList)
                {
                    decimal readyQty = 0; //已取总数量
                    T_WaveDetailEntity waveDetail = db.FindEntity<T_WaveDetailEntity>(o => o.WaveID == waveEntity.F_Id && o.OutBoundDetailID == qaDetailModel.F_Id);
                    foreach (ContainerDetailModel cdModel in itemcell.ContainerDetailModelList)
                    {
                        if (qaDetailModel.SampleSumNum > readyQty)
                        {
                            decimal curQty = 0;
                            bool isAuto = true;
                            if (cdModel.HandQty > 0)
                            {
                                curQty = cdModel.HandQty ?? 0;
                                isAuto = false;
                            }
                            else
                            {
                                if (qaDetailModel.SampleSumNum > (readyQty + (cdModel.Qty ?? 0))) //取完
                                {
                                    curQty = (cdModel.Qty ?? 0);
                                }
                                else  //取部分
                                {
                                    curQty = (qaDetailModel.SampleSumNum ?? 0) - readyQty;
                                }
                            }

                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == cdModel.ItemID);

                            T_QARecordEntity qaRecord = new T_QARecordEntity();
                            qaRecord.F_Id = Guid.NewGuid().ToString();
                            qaRecord.QAID = qaDetailModel.QAID;
                            qaRecord.QADetailID = qaDetailModel.F_Id;
                            qaRecord.SEQ = qaDetailModel.SEQ;
                            qaRecord.ERPHouseCode = cdModel.ERPWarehouseCode;
                            qaRecord.ReturnQty = 0;
                            qaRecord.IsNeedBack = qaDetailModel.IsBroken == "true" ? "false" : "true";
                            qaRecord.IsReturnOver = qaDetailModel.IsBroken == "true" ? "true" : "false";
                            qaRecord.TaskID = "";
                            qaRecord.TaskNo = "";
                            qaRecord.BarCode = cdModel.BarCode;
                            qaRecord.ContainerType = cdModel.ContainerType;
                            qaRecord.ContainerTypeName = containerTypeAll.FirstOrDefault(o => o.ContainerTypeCode == cdModel.ContainerType).ContainerTypeName;
                            qaRecord.ContainerKind = cdModel.KindCode;
                            qaRecord.ContainerKindName = sysitemList.FirstOrDefault(o => o.F_ItemCode == cdModel.KindCode).F_ItemName;
                            qaRecord.SrcLocationID = cdModel.LocationID;
                            qaRecord.SrcLocationCode = cdModel.LocationNo;
                            qaRecord.TagLocationID = qaDetailModel.StationID;
                            qaRecord.TagLocationCode = qaDetailModel.TagAddressCode;
                            qaRecord.TagLocationName = qaDetailModel.StationName;
                            qaRecord.ContainerID = cdModel.ContainerID;
                            qaRecord.ContainerDetailID = cdModel.F_Id;
                            qaRecord.ItemID = cdModel.ItemID;
                            qaRecord.ItemName = cdModel.ItemName;
                            qaRecord.ItemCode = cdModel.ItemCode;
                            qaRecord.ERPHouseCode = cdModel.ERPWarehouseCode;
                            qaRecord.ItemBarCode = cdModel.ItemBarCode;
                            qaRecord.OldQty = cdModel.Qty;
                            qaRecord.Qty = curQty;
                            qaRecord.PickedQty = 0;
                            qaRecord.AfterQty = 0;
                            qaRecord.IsAuto = isAuto ? "true" : "false";
                            qaRecord.ERPHouseCode = cdModel.ERPWarehouseCode;
                            qaRecord.WaveID = waveEntity.F_Id;
                            qaRecord.WaveCode = waveEntity.WaveCode;
                            qaRecord.WaveDetailID = waveDetail.F_Id;
                            qaRecord.ItemUnitText = cdModel.ItemUnitText;
                            qaRecord.TransState = "WaittingTrans";
                            qaRecord.Lot = cdModel.Lot;
                            qaRecord.Spec = cdModel.Spec;
                            qaRecord.SupplierUserID = cdModel.SupplierID;
                            qaRecord.SupplierUserCode = cdModel.SupplierCode;
                            qaRecord.SupplierUserName = cdModel.SupplierName;
                            qaRecord.ReceiveRecordID = cdModel.ReceiveRecordID;
                            qaRecord.IsSpecial = cdModel.IsSpecial;
                            qaRecord.State = "New";
                            qaRecord.QACode = qaDetailModel.QACode;
                            qaRecord.PickDate = null;
                            qaRecord.PickUserName = null;
                            qaRecord.IsItemMark = cdModel.IsItemMark;
                            qaRecord.Factory = cdModel.Factory;
                            qaRecord.ValidityDayNum = cdModel.ValidityDayNum ?? 0;
                            qaRecord.ProductDate = cdModel.ProductDate;
                            qaRecord.OverdueDate = cdModel.OverdueDate;
                            qaRecord.QAOrderType = "GetSample";
                            qaRecord.IsAppearQA = "false";
                            qaRecord.IsArrive_Get = "false";
                            qaRecord.IsNeedBackWare_Get = "true";
                            qaRecord.IsScanBack_Get = "false";
                            qaRecord.IsArrive_Back = "false";
                            qaRecord.IsNeedBackWare_Back = "true";
                            qaRecord.IsScanBack_Back = "false";
                            qaRecord.F_DeleteMark = false;
                            qaRecord.F_CreatorTime = DateTime.Now;
                            if (OperatorProvider.Provider.GetCurrent() != null)
                            {
                                qaRecord.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                                qaRecord.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                            }
                            qaRecordList.Add(qaRecord);
                            readyQty = readyQty + curQty;
                        }
                        else
                        {
                            break;
                        }
                    }
                    T_QADetailEntity qaDetail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == qaDetailModel.F_Id);
                    if (qaDetail.WaveQty + readyQty == qaDetail.SampleSumNum)
                    {
                        qaDetail.SampleSumCnt = qaRecordList.Count();
                        qaDetail.WaveQty = qaDetail.WaveQty + readyQty;
                        qaDetail.State = "Waved";
                        qaDetail.ActionType = "Equ";
                        db.Update<T_QADetailEntity>(qaDetail);
                        break;
                    }
                }
            }

            db.SaveChanges();

            IList<T_QAEntity> qaList = db.FindList<T_QAEntity>(o => qaHeadIDList.Contains(o.F_Id)).ToList();
            foreach (T_QAEntity ob in qaList)
            {
                IList<T_QADetailEntity> detailList = db.FindList<T_QADetailEntity>(o => o.QAID == ob.F_Id && o.State == "New").ToList();
                if (detailList.Count > 0)
                {
                    ob.State = "WavedPart";
                }
                else
                {
                    ob.State = "Waved";
                }
                db.Update<T_QAEntity>(ob);
            }


            //更新库存待出库数量
            string[] detailID = qaRecordList.Select(o => o.ContainerDetailID).ToArray();
            List<T_ContainerDetailEntity> containerDetailListInDB = db.FindList<T_ContainerDetailEntity>(o => detailID.Contains(o.F_Id));
            foreach (T_ContainerDetailEntity cdEntity in containerDetailListInDB)
            {
                cdEntity.OutQty = (cdEntity.OutQty ?? 0) + qaRecordList.FirstOrDefault(o => o.ContainerDetailID == cdEntity.F_Id).Qty;
            }
            db.SaveChanges();
            //冻结库存
            foreach (QADetailModel model in qaDetailModelsList)
            {
                List<T_ContainerDetailEntity> detailList = new List<T_ContainerDetailEntity>();
                if (string.IsNullOrEmpty(model.Lot))
                {
                    detailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == model.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == model.RefInBoundID);
                }
                else
                {
                    detailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == model.ItemID && o.Lot == model.Lot && o.InBoundID == model.RefInBoundID);
                }

                foreach (T_ContainerDetailEntity cd in detailList)
                {
                    cd.IsCheckFreeze = "true";
                    cd.CheckDetailID = model.F_Id;
                    cd.CheckID = model.QAID;
                    db.Update<T_ContainerDetailEntity>(cd);
                }
            }


            //产生任务,OutInfo 可能存在同一个单据主数据和不同单据明细数据
            var barCodeList = qaRecordList.GroupBy(g => new { BarCode = g.BarCode }).Select(o => new { BarCode = o.Key.BarCode, OutInfo = o.Select(k => k) }).ToList();
            string[] taskBarCode = barCodeList.Select(o => o.BarCode).ToArray();
            IList<T_TaskEntity> taskInDBList = db.FindList<T_TaskEntity>(o => taskBarCode.Contains(o.BarCode)).ToList();


            List<T_TaskEntity> taskInsertList = new List<T_TaskEntity>();
            //任务明细
            List<T_TaskDetailEntity> taskDetailList = new List<T_TaskDetailEntity>();

            foreach (var bar in barCodeList)
            {
                string curTaskID = "";
                string curTaskNo = "";
                var firstOutOrder = bar.OutInfo.FirstOrDefault(); //先取第一单据，后续可能一个容器多个单据
                T_TaskEntity taskInDB = taskInDBList.FirstOrDefault(o => o.BarCode == bar.BarCode);
                if (taskInDB == null) //容器任务不存在
                {

                    T_TaskEntity task = new T_TaskEntity();
                    curTaskID = Guid.NewGuid().ToString();
                    task.F_Id = curTaskID;
                    curTaskNo = T_CodeGenApp.GenNum("TaskRule");
                    task.TaskNo = curTaskNo;
                    task.TaskInOutType = "OutType";

                    task.TaskType = "TaskType_CheckPickOut";
                    task.OrderType = "GetSample";

                    task.ContainerID = firstOutOrder.ContainerID;
                    task.BarCode = firstOutOrder.BarCode;
                    task.ContainerType = firstOutOrder.ContainerType;
                    task.SrcLocationID = firstOutOrder.SrcLocationID;
                    task.SrcLocationCode = firstOutOrder.SrcLocationCode;
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == firstOutOrder.SrcLocationID);
                    task.SrcWCSLocCode = loc.WCSLocCode;
                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.StationLoc.ToString());
                    task.TagAreaID = area.F_Id;
                    task.TagLocationID = firstOutOrder.TagLocationID;
                    task.TagLocationCode = firstOutOrder.TagLocationCode;
                    task.TagWCSLocCode = firstOutOrder.TagLocationCode;
                    task.ApplyStationID = firstOutOrder.TagLocationID;
                    task.Level = 30;
                    task.State = "New";
                    task.IsWcsTask = "true";
                    task.ExecEquID = null;
                    task.IsCanExec = "false";
                    task.SendWCSTime = null;
                    task.WaveCode = waveEntity.WaveCode;
                    task.WaveID = waveEntity.F_Id;
                    task.SEQ = firstOutOrder.SEQ;
                    task.WaveDetailID = firstOutOrder.WaveDetailID;
                    task.OrderID = firstOutOrder.QAID;
                    task.OrderDetailID = firstOutOrder.QADetailID;
                    task.OrderCode = firstOutOrder.QACode;
                    task.OverTime = null;

                    task.F_CreatorTime = DateTime.Now;
                    task.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                    task.F_DeleteMark = false;
                    task.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;

                    IList<T_QARecordEntity> qaReceordTempList = qaRecordList.Where(o => o.QADetailID == task.OrderDetailID && o.BarCode == task.BarCode).ToList();//同单据不同明细，公用一个容器的情况
                    foreach (T_QARecordEntity outcell in qaReceordTempList)
                    {
                        outcell.TaskID = task.F_Id;
                        outcell.TaskNo = task.TaskNo;
                    }

                    taskInsertList.Add(task);
                }

                //任务明细
                foreach (T_QARecordEntity outcell in bar.OutInfo)
                {
                    T_TaskDetailEntity taskDetailEntity = new T_TaskDetailEntity();
                    taskDetailEntity.F_Id = Guid.NewGuid().ToString();
                    taskDetailEntity.ContainerID = outcell.ContainerID;
                    taskDetailEntity.BarCode = outcell.BarCode;
                    taskDetailEntity.ContainerType = outcell.ContainerType;
                    taskDetailEntity.TagLocationCode = outcell.TagLocationCode;
                    taskDetailEntity.TagLocationID = outcell.TagLocationID;
                    taskDetailEntity.SEQ = outcell.SEQ;
                    taskDetailEntity.OrderID = outcell.QAID;
                    taskDetailEntity.OrderDetailID = outcell.QADetailID;
                    taskDetailEntity.OrderCode = outcell.QACode;
                    taskDetailEntity.OrderType = "GetSample";
                    taskDetailEntity.WaveCode = waveEntity.WaveCode;
                    taskDetailEntity.WaveID = waveEntity.F_Id;
                    taskDetailEntity.IsOver = "false";
                    taskDetailEntity.WaveDetailID = outcell.WaveDetailID;
                    taskDetailEntity.ContainerDetailID = outcell.ContainerDetailID;
                    taskDetailEntity.ItemBarCode = outcell.ItemBarCode;


                    if (taskInDB == null && firstOutOrder.QADetailID == outcell.QADetailID)
                    {
                        taskDetailEntity.SrcLocationCode = outcell.SrcLocationCode;
                        taskDetailEntity.SrcLocationID = outcell.SrcLocationID;
                        taskDetailEntity.IsCurTask = "true";
                        taskDetailEntity.TaskID = curTaskID;
                        taskDetailEntity.TaskNo = curTaskNo;
                    }
                    else
                    {
                        taskDetailEntity.SrcLocationCode = null;
                        taskDetailEntity.SrcLocationID = null;
                        taskDetailEntity.IsCurTask = "false";
                    }
                    taskDetailEntity.F_DeleteMark = false;
                    taskDetailEntity.F_CreatorTime = DateTime.Now;
                    if (OperatorProvider.Provider.GetCurrent() != null)
                    {
                        taskDetailEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                        taskDetailEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserName;
                    }
                    taskDetailList.Add(taskDetailEntity);
                }
            }

            string[] locID = taskInsertList.Select(o => o.SrcLocationID).ToArray();
            List<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => locID.Contains(o.F_Id));

            T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();
            foreach (T_LocationEntity loccell in locList)
            {
                loccell.State = "Out";

                /// 货位状态变更记录
                string taskNo = taskInsertList.FirstOrDefault(o => o.SrcLocationID == loccell.F_Id).TaskNo;
                locStateApp.SyncLocState(db, loccell, "OutType", "GetSample", "Stored", "Out", taskNo);
            }

            int count1 = db.Insert<T_QARecordEntity>(qaRecordList);
            int count2 = db.Update<T_ContainerDetailEntity>(containerDetailListInDB);
            int count3 = db.Insert<T_TaskEntity>(taskInsertList);
            int count4 = db.Insert<T_TaskDetailEntity>(taskDetailList);
            int count5 = db.Update<T_LocationEntity>(locList);

            result.state = ResultType.success;
            return result;
        }


        private T_WaveEntity AddWave_QA(IRepositoryBase db, string waveType, IList<T_QADetailEntity> qaDetailEntitiesList)
        {
            T_WaveEntity waveEntity = new T_WaveEntity();
            waveEntity.F_Id = Guid.NewGuid().ToString();
            waveEntity.WaveCode = T_CodeGenApp.GenNum("WaveRule");
            waveEntity.WaveType = waveType.ToString();
            waveEntity.State = "New";
            waveEntity.OrderType = "QAOrder";
            db.Insert<T_WaveEntity>(waveEntity);

            foreach (T_QADetailEntity detail in qaDetailEntitiesList)
            {
                T_WaveDetailEntity waveDetail = new T_WaveDetailEntity();
                waveDetail.F_Id = Guid.NewGuid().ToString();
                waveDetail.WaveID = waveEntity.F_Id;
                waveDetail.OutBoundID = detail.QAID;
                waveDetail.OutBoundDetailID = detail.F_Id;
                db.Insert<T_WaveDetailEntity>(waveDetail);
            }
            db.SaveChanges();
            return waveEntity;
        }

        private class ItemGroup
        {
            public string ItemID { get; set; }
            public string ItemCode { get; set; }
            public string Lot { get; set; }
            public string ItemName { get; set; }
            public string RefInBoundCode { get; set; }
            /// <summary>
            /// 是否取合格库存
            /// </summary>
            public bool IsQua { get; set; }

            /// <summary>
            /// 需求库存(按数量波次)
            /// </summary>
            public decimal Qty { get; set; }

            /// <summary>
            /// 需求个数（按标签个数波次）
            /// </summary>
            public decimal Cnt { get; set; }
            public IList<QADetailModel> QADetailModelList { get; set; }

            public IList<ContainerDetailModel> ContainerDetailModelList { get; set; }

        }

        public class LocationIDEntity
        {
            public string LocationID { get; set; }
            public string BarCode { get; set; }
            public string ContainerType { get; set; }
            public string LocationCode { get; set; }
            public string ContainerID { get; set; }
            public string KindCode { get; set; }

        }
        #endregion

        #region 质检出库运算,按标签波次(外观质检)
        public AjaxResult WaveGen_QA_CntBar(IRepositoryBase db, string waveType, IList<ContainerDetailModel> containerDetailModelByHandList, string[] qaDetailIDArray, bool isMustFull)
        {
            AjaxResult result = new AjaxResult();

            T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();
            bool IsHaveOffLine = offLineDownApp.IsHaveOff(db);
            if (IsHaveOffLine)
            {
                result.state = ResultType.error;
                result.message = "存在未处理的离线数据";
                return result;
            }

            IList<T_QADetailEntity> qaDetailEntitiesList = db.FindList<T_QADetailEntity>(o => qaDetailIDArray.Contains(o.F_Id));
            string[] qaHeadIDList = qaDetailEntitiesList.Select(o => o.QAID).Distinct().ToArray();
            IList<T_QAEntity> qaEntityList = db.FindList<T_QAEntity>(o => qaHeadIDList.Contains(o.F_Id));


            IList<QADetailModel> qaDetailModelsList = qaDetailEntitiesList.ToObject<IList<QADetailModel>>();
            foreach (QADetailModel model in qaDetailModelsList)
            {
                T_QAEntity qaHead = qaEntityList.FirstOrDefault(o => o.F_Id == model.QAID);
                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == model.ItemID);
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == model.StationID);
                T_InBoundEntity inBound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == qaHead.RefInBoundCode);
                if (inBound == null)
                {
                    result.state = ResultType.error;
                    result.message = "入库来源单不存在:" + qaHead.RefInBoundCode;
                    return result;
                }
                model.QAID = qaHead.F_Id;
                model.QACode = qaHead.QACode;
                model.TagAddressCode = station.TagAddress;
                model.StationCode = station.StationCode;
                model.StationName = station.StationName;
                model.RefInBoundCode = qaHead.RefInBoundCode;
                model.RefInBoundID = inBound.F_Id;
            }

            IList<ItemGroup> itemGroupList = qaDetailModelsList.GroupBy(g => new { g.ItemID, g.ItemCode, g.Lot, g.RefInBoundCode })
                                                               .Select(o => new ItemGroup
                                                               {
                                                                   ItemID = o.Key.ItemID,
                                                                   ItemCode = o.Key.ItemCode,
                                                                   RefInBoundCode = o.Key.RefInBoundCode,
                                                                   ItemName = o.FirstOrDefault().ItemName,
                                                                   Lot = o.Key.Lot,
                                                                   Cnt = o.Sum(i => (i.SampleSumCnt ?? 0)), //按标签个数波次
                                                                   QADetailModelList = o.Select(k => k).OrderBy(k => k.QAID).ThenBy(k => k.SEQ).ToList()
                                                               }).ToList();

            //过滤区域、巷道、行
            IQueryable<T_AreaEntity> areaQuery = db.IQueryable<T_AreaEntity>(o => o.IsEnable == "true"); //启用的区域
            IQueryable<T_DevRowEntity> wayQuery = db.IQueryable<T_DevRowEntity>(o => o.IsEnable == "true");//启用的巷道ID
            IQueryable<T_RowLineEntity> lineQuery = db.IQueryable<T_RowLineEntity>(o => o.IsEnable == "true"); //启用的行

            var wayAndLineQuery = areaQuery.Join(wayQuery, m => m.F_Id, n => n.AreaID, (m, n) => new { AreaID = n.AreaID, WayID = n.F_Id }).Join(lineQuery, m => m.WayID, n => n.DevRowID, (m, n) => new { m.AreaID, n.Line });

            //过滤货位
            IQueryable<T_LocationEntity> locationIQ = db.IQueryable<T_LocationEntity>(o =>
            o.State == "Stored"
            && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyOut")
            ).Join(wayAndLineQuery, m => new { m.AreaID, m.Line }, n => new { n.AreaID, n.Line }, (m, n) => m);

            //过滤容器主表
            IQueryable<T_ContainerEntity> containerIQ = db.IQueryable<T_ContainerEntity>();
            //联合货位与容器
            IQueryable<LocationIDEntity> containerIDIQ = locationIQ.Join(containerIQ, m => m.F_Id, n => n.LocationID, (m, n) => new LocationIDEntity { LocationID = m.F_Id, BarCode = n.BarCode, ContainerType = n.ContainerType, LocationCode = m.LocationCode, ContainerID = n.F_Id, KindCode = n.ContainerKind });

            //出库物料分组，并校验库存是否充足
            foreach (ItemGroup itemcell in itemGroupList)
            {
                T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == itemcell.RefInBoundCode);
                if (inbound == null)
                {
                    result.state = ResultType.error;
                    result.message = "入库来源单不存在:" + itemcell.RefInBoundCode;
                    return result;
                }
                T_InBoundDetailEntity inboundDetail = null;
                if (string.IsNullOrEmpty(itemcell.Lot))
                {
                    inboundDetail = db.FindEntity<T_InBoundDetailEntity>(o => o.ItemID == itemcell.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == inbound.F_Id);
                }
                else
                {
                    inboundDetail = db.FindEntity<T_InBoundDetailEntity>(o => o.ItemID == itemcell.ItemID && o.Lot == itemcell.Lot && o.InBoundID == inbound.F_Id);
                }

                if (inboundDetail == null)
                {
                    result.state = ResultType.error;
                    result.message = "入库来源单明细不存在:物料编码" + itemcell.ItemCode;
                    return result;
                }

                //过滤库存明细
                IQueryable<T_ContainerDetailEntity> detailIQ = db.IQueryable<T_ContainerDetailEntity>(o => o.InBoundDetailID == inboundDetail.F_Id && o.Qty > 0 && o.ItemID == itemcell.ItemID && o.OutQty == 0 && o.State == "Normal" && o.CheckState == "WaitCheck" && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false");

                if (string.IsNullOrEmpty(itemcell.Lot))
                {
                    detailIQ = detailIQ.Where(o => string.IsNullOrEmpty(o.Lot));
                }
                else
                {
                    detailIQ = detailIQ.Where(o => o.Lot == itemcell.Lot);
                }

                detailIQ = detailIQ.Where(o => o.CheckState == "WaitCheck");


                IQueryable<ContainerDetailModel> conDModelIQ = detailIQ.Join(containerIDIQ, m => m.ContainerID, n => n.ContainerID,
                                                                (m, n) => new ContainerDetailModel
                                                                {
                                                                    BarCode = n.BarCode,
                                                                    ContainerType = n.ContainerType,
                                                                    LocationID = n.LocationID,
                                                                    LocationNo = n.LocationCode,
                                                                    ContainerID = n.ContainerID,
                                                                    ItemID = m.ItemID,
                                                                    ItemName = m.ItemName,
                                                                    ItemCode = m.ItemCode,
                                                                    ItemBarCode = m.ItemBarCode,
                                                                    ItemUnitText = m.ItemUnitText,
                                                                    SupplierID = m.SupplierID,
                                                                    SupplierCode = m.SupplierCode,
                                                                    SupplierName = m.SupplierName,
                                                                    ReceiveRecordID = m.ReceiveRecordID,
                                                                    ERPWarehouseCode = m.ERPWarehouseCode,
                                                                    Spec = m.Spec,
                                                                    //IsSpecial = m.IsSpecial,
                                                                    F_Id = m.F_Id,
                                                                    Qty = m.Qty ?? 0,
                                                                    Lot = m.Lot ?? "",
                                                                    HandQty = 0,
                                                                    F_CreatorTime = m.F_CreatorTime,
                                                                    KindCode = n.KindCode,
                                                                    IsItemMark = m.IsItemMark,
                                                                    Factory = m.Factory,
                                                                    ProductDate = m.ProductDate,
                                                                    OverdueDate = m.OverdueDate,
                                                                    ValidityDayNum = m.ValidityDayNum
                                                                });

                itemcell.ContainerDetailModelList = conDModelIQ.ToList();

                //将手动选择的部分赋值给库存对象
                if (containerDetailModelByHandList != null)
                {
                    foreach (ContainerDetailModel handModel in containerDetailModelByHandList)
                    {
                        ContainerDetailModel inDetail = itemcell.ContainerDetailModelList.FirstOrDefault(o => o.F_Id == handModel.F_Id);
                        if (inDetail == null) //手动选择的不存在于库存集合,则将手动选择的追加到库存集合（可能Qty-OutQty>0 或货位 被设置为待出库）
                        {
                            IQueryable<T_ContainerEntity> conIQ = db.IQueryable<T_ContainerEntity>();
                            IQueryable<T_LocationEntity> locIQ = db.IQueryable<T_LocationEntity>();
                            IQueryable<T_ContainerDetailEntity> conDetailIQ = db.IQueryable<T_ContainerDetailEntity>();

                            ContainerDetailModel containerDB = conIQ.Join(locIQ, m => m.LocationID, n => n.F_Id,
                                (m, n) => new { ContainerID = m.F_Id, n.LocationCode, m.BarCode, m.ContainerType, LocationID = n.F_Id, m.ContainerKind })
                                .Join(conDetailIQ, j => j.ContainerID, k => k.ContainerID, (j, k) => new ContainerDetailModel()
                                {
                                    BarCode = j.BarCode,
                                    ContainerType = j.ContainerType,
                                    LocationID = j.LocationID,
                                    LocationNo = j.LocationCode,
                                    ContainerID = j.ContainerID,
                                    ItemID = k.ItemID,
                                    ItemName = k.ItemName,
                                    ItemCode = k.ItemCode,
                                    ItemBarCode = k.ItemBarCode,
                                    ItemUnitText = k.ItemUnitText,
                                    SupplierID = k.SupplierID,
                                    SupplierCode = k.SupplierCode,
                                    SupplierName = k.SupplierName,
                                    ReceiveRecordID = k.ReceiveRecordID,
                                    ERPWarehouseCode = k.ERPWarehouseCode,
                                    Spec=k.Spec,
                                    //IsSpecial = k.IsSpecial,
                                    F_Id = k.F_Id,
                                    Qty = k.Qty ?? 0,
                                    Lot = k.Lot ?? "",
                                    HandQty = 1, //按标签波次，则每个手动选择的库存，只代表1个
                                    F_CreatorTime = k.F_CreatorTime,
                                    KindCode = j.ContainerKind,
                                    IsItemMark = k.IsItemMark,
                                    Factory = k.Factory,
                                    ProductDate = k.ProductDate,
                                    OverdueDate = k.OverdueDate,
                                    ValidityDayNum = k.ValidityDayNum
                                }).Where(o => o.F_Id == handModel.F_Id).FirstOrDefault();

                            itemcell.ContainerDetailModelList.Add(containerDB);
                        }
                        else
                        {
                            inDetail.HandQty = 1; //按标签波次，则每个手动选择的库存，只代表1个
                        }
                    }
                }
                //所有当前物料的可用库存,策略：手动优先，其次随机
                itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.OrderByDescending(o => o.HandQty ?? 0).ThenBy(o => Guid.NewGuid()).ToList();

                decimal qtyAll = itemcell.ContainerDetailModelList.Count();
                if (isMustFull)
                {
                    if (qtyAll < itemcell.Cnt)
                    {
                        result.state = ResultType.error;
                        result.message = $"[ { itemcell.ItemName } ] 库存不足";
                        return result;
                    }
                }
            }

            //产生波次单
            T_WaveEntity waveEntity = AddWave_QA(db, waveType, qaDetailEntitiesList);

            IList<T_ContainerTypeEntity> containerTypeAll = db.FindList<T_ContainerTypeEntity>(o => true).ToList();
            ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
            IList<ItemsDetailEntity> sysitemList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();

            //产生拣选数据
            List<T_QARecordEntity> qaRecordList = new List<T_QARecordEntity>();
            foreach (ItemGroup itemcell in itemGroupList)
            {
                foreach (QADetailModel qaDetailModel in itemcell.QADetailModelList) //循环该物料的单据
                {
                    decimal readyCnt = 0; //已取总个数
                    decimal readyQty = 0; //已取总数量
                    T_WaveDetailEntity waveDetail = db.FindEntity<T_WaveDetailEntity>(o => o.WaveID == waveEntity.F_Id && o.OutBoundDetailID == qaDetailModel.F_Id);
                    foreach (ContainerDetailModel cdModel in itemcell.ContainerDetailModelList) //循环该物料的库存
                    {
                        if (qaDetailModel.SampleSumCnt > readyCnt)
                        {
                            decimal curQty = cdModel.Qty ?? 0; //全部取完
                            bool isAuto = true;
                            if (cdModel.HandQty > 0)
                            {
                                isAuto = false;
                            }

                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == cdModel.ItemID);

                            T_QARecordEntity qaRecord = new T_QARecordEntity();
                            qaRecord.F_Id = Guid.NewGuid().ToString();
                            qaRecord.QAID = qaDetailModel.QAID;
                            qaRecord.QADetailID = qaDetailModel.F_Id;
                            qaRecord.SEQ = qaDetailModel.SEQ;
                            qaRecord.ERPHouseCode = cdModel.ERPWarehouseCode;
                            qaRecord.ReturnQty = 0;
                            qaRecord.IsNeedBack = "false";
                            qaRecord.IsReturnOver = "true";
                            qaRecord.TaskID = "";
                            qaRecord.TaskNo = "";
                            qaRecord.BarCode = cdModel.BarCode;
                            qaRecord.ContainerType = cdModel.ContainerType;
                            qaRecord.ContainerTypeName = containerTypeAll.FirstOrDefault(o => o.ContainerTypeCode == cdModel.ContainerType).ContainerTypeName;
                            qaRecord.ContainerKind = cdModel.KindCode;
                            qaRecord.ContainerKindName = sysitemList.FirstOrDefault(o => o.F_ItemCode == cdModel.KindCode).F_ItemName;
                            qaRecord.SrcLocationID = cdModel.LocationID;
                            qaRecord.SrcLocationCode = cdModel.LocationNo;
                            qaRecord.TagLocationID = qaDetailModel.StationID;
                            qaRecord.TagLocationCode = qaDetailModel.TagAddressCode;
                            qaRecord.TagLocationName = qaDetailModel.StationName;
                            qaRecord.ContainerID = cdModel.ContainerID;
                            qaRecord.ContainerDetailID = cdModel.F_Id;
                            qaRecord.ItemID = cdModel.ItemID;
                            qaRecord.ItemName = cdModel.ItemName;
                            qaRecord.ItemCode = cdModel.ItemCode;
                            qaRecord.ERPHouseCode = cdModel.ERPWarehouseCode;
                            qaRecord.ItemBarCode = cdModel.ItemBarCode;
                            qaRecord.OldQty = cdModel.Qty;
                            qaRecord.Qty = curQty;
                            qaRecord.PickedQty = 0;
                            qaRecord.AfterQty = 0;
                            qaRecord.IsAuto = isAuto ? "true" : "false";
                            qaRecord.ERPHouseCode = cdModel.ERPWarehouseCode;
                            qaRecord.WaveID = waveEntity.F_Id;
                            qaRecord.WaveCode = waveEntity.WaveCode;
                            qaRecord.WaveDetailID = waveDetail.F_Id;
                            qaRecord.ItemUnitText = cdModel.ItemUnitText;
                            qaRecord.TransState = "WaittingTrans";
                            qaRecord.Lot = cdModel.Lot;
                            qaRecord.Spec = cdModel.Spec;
                            qaRecord.SupplierUserID = cdModel.SupplierID;
                            qaRecord.SupplierUserCode = cdModel.SupplierCode;
                            qaRecord.SupplierUserName = cdModel.SupplierName;
                            qaRecord.ReceiveRecordID = cdModel.ReceiveRecordID;
                            qaRecord.IsSpecial = cdModel.IsSpecial;
                            qaRecord.State = "New";
                            qaRecord.QACode = qaDetailModel.QACode;
                            qaRecord.PickDate = null;
                            qaRecord.PickUserName = null;
                            qaRecord.IsItemMark = cdModel.IsItemMark;
                            qaRecord.Factory = cdModel.Factory;
                            qaRecord.ValidityDayNum = cdModel.ValidityDayNum ?? 0;
                            qaRecord.ProductDate = cdModel.ProductDate;
                            qaRecord.OverdueDate = cdModel.OverdueDate;
                            qaRecord.QAOrderType = "GetSample";
                            qaRecord.IsAppearQA = "true";
                            qaRecord.IsArrive_Get = "false";
                            qaRecord.IsNeedBackWare_Get = "true";
                            qaRecord.IsScanBack_Get = "false";
                            qaRecord.IsArrive_Back = "false";
                            qaRecord.IsNeedBackWare_Back = "true";
                            qaRecord.IsScanBack_Back = "false";
                            qaRecord.F_DeleteMark = false;
                            qaRecord.F_CreatorTime = DateTime.Now;
                            if (OperatorProvider.Provider.GetCurrent() != null)
                            {
                                qaRecord.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                                qaRecord.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                            }
                            qaRecordList.Add(qaRecord);
                            readyCnt = readyCnt + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    T_QADetailEntity qaDetail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == qaDetailModel.F_Id);
                    if (readyCnt == qaDetail.SampleSumCnt)
                    {
                        qaDetail.SampleSumNum = readyQty;
                        qaDetail.WaveQty = qaDetail.WaveQty + readyQty;
                        qaDetail.State = "Waved";
                        qaDetail.ActionType = "Equ";
                        db.Update<T_QADetailEntity>(qaDetail);
                        break;
                    }
                }
            }

            db.SaveChanges();

            IList<T_QAEntity> qaList = db.FindList<T_QAEntity>(o => qaHeadIDList.Contains(o.F_Id)).ToList();
            foreach (T_QAEntity ob in qaList)
            {
                IList<T_QADetailEntity> detailList = db.FindList<T_QADetailEntity>(o => o.QAID == ob.F_Id && o.State == "New").ToList();
                if (detailList.Count > 0)
                {
                    ob.State = "WavedPart";
                }
                else
                {
                    ob.State = "Waved";
                }
                db.Update<T_QAEntity>(ob);
            }


            //更新库存待出库数量
            string[] detailID = qaRecordList.Select(o => o.ContainerDetailID).ToArray();
            List<T_ContainerDetailEntity> containerDetailListInDB = db.FindList<T_ContainerDetailEntity>(o => detailID.Contains(o.F_Id));
            foreach (T_ContainerDetailEntity cdEntity in containerDetailListInDB)
            {
                cdEntity.OutQty = (cdEntity.OutQty ?? 0) + qaRecordList.FirstOrDefault(o => o.ContainerDetailID == cdEntity.F_Id).Qty;
            }
            db.SaveChanges();
            //冻结库存
            foreach (QADetailModel model in qaDetailModelsList)
            {
                List<T_ContainerDetailEntity> detailList = new List<T_ContainerDetailEntity>();
                if (string.IsNullOrEmpty(model.Lot))
                {
                    detailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == model.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == model.RefInBoundID);
                }
                else
                {
                    detailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == model.ItemID && o.Lot == model.Lot && o.InBoundID == model.RefInBoundID);
                }

                foreach (T_ContainerDetailEntity cd in detailList)
                {
                    cd.IsCheckFreeze = "true";
                    cd.CheckDetailID = model.F_Id;
                    cd.CheckID = model.QAID;
                    db.Update<T_ContainerDetailEntity>(cd);
                }
            }


            //产生任务,OutInfo 可能存在同一个单据主数据和不同单据明细数据
            var barCodeList = qaRecordList.GroupBy(g => new { BarCode = g.BarCode }).Select(o => new { BarCode = o.Key.BarCode, OutInfo = o.Select(k => k) }).ToList();
            string[] taskBarCode = barCodeList.Select(o => o.BarCode).ToArray();
            IList<T_TaskEntity> taskInDBList = db.FindList<T_TaskEntity>(o => taskBarCode.Contains(o.BarCode)).ToList();


            List<T_TaskEntity> taskInsertList = new List<T_TaskEntity>();
            //任务明细
            List<T_TaskDetailEntity> taskDetailList = new List<T_TaskDetailEntity>();

            foreach (var bar in barCodeList)
            {
                string curTaskID = "";
                string curTaskNo = "";
                var firstOutOrder = bar.OutInfo.FirstOrDefault(); //先取第一单据，后续可能一个容器多个单据
                T_TaskEntity taskInDB = taskInDBList.FirstOrDefault(o => o.BarCode == bar.BarCode);
                if (taskInDB == null) //容器任务不存在
                {

                    T_TaskEntity task = new T_TaskEntity();
                    curTaskID = Guid.NewGuid().ToString();
                    task.F_Id = curTaskID;
                    curTaskNo = T_CodeGenApp.GenNum("TaskRule");
                    task.TaskNo = curTaskNo;
                    task.TaskInOutType = "OutType";

                    task.TaskType = "TaskType_CheckPickOut";
                    task.OrderType = "GetSample";

                    task.ContainerID = firstOutOrder.ContainerID;
                    task.BarCode = firstOutOrder.BarCode;
                    task.ContainerType = firstOutOrder.ContainerType;
                    task.SrcLocationID = firstOutOrder.SrcLocationID;
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == firstOutOrder.SrcLocationID);
                    task.SrcWCSLocCode = loc.WCSLocCode;
                    task.SrcLocationCode = firstOutOrder.SrcLocationCode;
                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.StationLoc.ToString());
                    task.TagAreaID = area.F_Id;
                    task.TagLocationID = firstOutOrder.TagLocationID;
                    task.TagLocationCode = firstOutOrder.TagLocationCode;
                    task.TagWCSLocCode = firstOutOrder.TagLocationCode;
                    task.ApplyStationID = firstOutOrder.TagLocationID;
                    task.Level = 30;
                    task.State = "New";
                    task.IsWcsTask = "true";
                    task.ExecEquID = null;
                    task.IsCanExec = "false";
                    task.SendWCSTime = null;
                    task.WaveCode = waveEntity.WaveCode;
                    task.WaveID = waveEntity.F_Id;
                    task.SEQ = firstOutOrder.SEQ;
                    task.WaveDetailID = firstOutOrder.WaveDetailID;
                    task.OrderID = firstOutOrder.QAID;
                    task.OrderDetailID = firstOutOrder.QADetailID;
                    task.OrderCode = firstOutOrder.QACode;
                    task.OverTime = null;

                    task.F_CreatorTime = DateTime.Now;
                    task.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                    task.F_DeleteMark = false;
                    task.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;

                    IList<T_QARecordEntity> qaReceordTempList = qaRecordList.Where(o => o.QADetailID == task.OrderDetailID && o.BarCode == task.BarCode).ToList();//同单据不同明细，公用一个容器的情况
                    foreach (T_QARecordEntity outcell in qaReceordTempList)
                    {
                        outcell.TaskID = task.F_Id;
                        outcell.TaskNo = task.TaskNo;
                    }

                    taskInsertList.Add(task);
                }

                //任务明细
                foreach (T_QARecordEntity outcell in bar.OutInfo)
                {
                    T_TaskDetailEntity taskDetailEntity = new T_TaskDetailEntity();
                    taskDetailEntity.F_Id = Guid.NewGuid().ToString();
                    taskDetailEntity.ContainerID = outcell.ContainerID;
                    taskDetailEntity.BarCode = outcell.BarCode;
                    taskDetailEntity.ContainerType = outcell.ContainerType;
                    taskDetailEntity.TagLocationCode = outcell.TagLocationCode;
                    taskDetailEntity.TagLocationID = outcell.TagLocationID;
                    taskDetailEntity.SEQ = outcell.SEQ;
                    taskDetailEntity.OrderID = outcell.QAID;
                    taskDetailEntity.OrderDetailID = outcell.QADetailID;
                    taskDetailEntity.OrderCode = outcell.QACode;
                    taskDetailEntity.OrderType = "GetSample";
                    taskDetailEntity.WaveCode = waveEntity.WaveCode;
                    taskDetailEntity.WaveID = waveEntity.F_Id;
                    taskDetailEntity.IsOver = "false";
                    taskDetailEntity.WaveDetailID = outcell.WaveDetailID;
                    taskDetailEntity.ContainerDetailID = outcell.ContainerDetailID;
                    taskDetailEntity.ItemBarCode = outcell.ItemBarCode;


                    if (taskInDB == null && firstOutOrder.QADetailID == outcell.QADetailID)
                    {
                        taskDetailEntity.SrcLocationCode = outcell.SrcLocationCode;
                        taskDetailEntity.SrcLocationID = outcell.SrcLocationID;
                        taskDetailEntity.IsCurTask = "true";
                        taskDetailEntity.TaskID = curTaskID;
                        taskDetailEntity.TaskNo = curTaskNo;
                    }
                    else
                    {
                        taskDetailEntity.SrcLocationCode = null;
                        taskDetailEntity.SrcLocationID = null;
                        taskDetailEntity.IsCurTask = "false";
                    }
                    taskDetailEntity.F_DeleteMark = false;
                    taskDetailEntity.F_CreatorTime = DateTime.Now;
                    if (OperatorProvider.Provider.GetCurrent() != null)
                    {
                        taskDetailEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                        taskDetailEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserName;
                    }
                    taskDetailList.Add(taskDetailEntity);
                }
            }

            string[] locID = taskInsertList.Select(o => o.SrcLocationID).ToArray();
            List<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => locID.Contains(o.F_Id));

            T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();
            foreach (T_LocationEntity loccell in locList)
            {
                loccell.State = "Out";

                /// 货位状态变更记录
                string taskNo = taskInsertList.FirstOrDefault(o => o.SrcLocationID == loccell.F_Id).TaskNo;
                locStateApp.SyncLocState(db, loccell, "OutType", "GetSample", "Stored", "Out", taskNo);
            }

            int count1 = db.Insert<T_QARecordEntity>(qaRecordList);
            int count2 = db.Update<T_ContainerDetailEntity>(containerDetailListInDB);
            int count3 = db.Insert<T_TaskEntity>(taskInsertList);
            int count4 = db.Insert<T_TaskDetailEntity>(taskDetailList);
            int count5 = db.Update<T_LocationEntity>(locList);

            result.state = ResultType.success;
            return result;
        }
        #endregion

        #region 执行波次并发送任务
        /// <summary>
        /// 执行波次并发送任务
        /// </summary>
        /// <param name="db"></param>
        /// <param name="qaDetailIDList"></param>
        /// <returns></returns>
        public AjaxResult QADetail_ExecTaskAndSendWCS(IRepositoryBase db, IList<string> qaDetailIDList)
        {
            AjaxResult rst = new AjaxResult();
            //更改单据状态(以出库单明细为基准)
            IList<string> waveIDList = db.FindList<T_WaveDetailEntity>(o => qaDetailIDList.Contains(o.OutBoundDetailID)).Select(o => o.WaveID).Distinct().ToArray();
            IList<T_WaveEntity> waveList = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && o.State == "New");
            IList<string> needSendWCS = new List<string>();
            foreach (T_WaveEntity wave in waveList)
            {
                IList<string> qaListTemp = db.FindList<T_WaveDetailEntity>(o => o.WaveID == wave.F_Id && qaDetailIDList.Contains(o.OutBoundDetailID)).Select(o => o.OutBoundID).Distinct().ToArray();
                IList<T_QAEntity> qaList = db.FindList<T_QAEntity>(o => qaListTemp.Contains(o.F_Id)).ToList();

                foreach (T_QAEntity outB in qaList)
                {
                    string stationOrderType = "GetSample";
                    outB.State = "Outing";
                    db.Update<T_QAEntity>(outB);

                    IList<T_StationEntity> workingStationList = new List<T_StationEntity>();
                    IList<T_WaveDetailEntity> waveDetailList = db.FindList<T_WaveDetailEntity>(o => o.OutBoundID == outB.F_Id && o.WaveID == wave.F_Id && qaDetailIDList.Contains(o.OutBoundDetailID));
                    if (waveDetailList.Count < 1)
                    {
                        rst.state = ResultType.error;
                        rst.message = "波次明细不存在";
                        return rst;
                    }

                    foreach (T_WaveDetailEntity wDetail in waveDetailList)
                    {

                        T_QADetailEntity qaDetail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == wDetail.OutBoundDetailID);
                        qaDetail.State = "Outing";
                        db.Update<T_QADetailEntity>(qaDetail);


                        T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.F_Id == qaDetail.StationID);
                        if (string.IsNullOrEmpty(stationEntity.CurOrderID) || stationEntity.CurOrderID == outB.F_Id)
                        {
                            stationEntity.CurOrderID = outB.F_Id;
                            stationEntity.OrderType = stationOrderType.ToString();
                            stationEntity.WaveID = wave.F_Id;
                            db.Update<T_StationEntity>(stationEntity);
                            db.SaveChanges();

                            IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => wDetail.OutBoundDetailID == o.OrderDetailID && wDetail.F_Id == o.WaveDetailID).ToList();
                            foreach (T_TaskEntity task in taskList)
                            {
                                needSendWCS.Add(task.TaskNo);

                                //task.State = "Execing";
                                task.IsCanExec = "true";
                                //task.SendWCSTime = DateTime.Now;
                                db.Update<T_TaskEntity>(task);

                                IList<T_QARecordEntity> qaRecList = db.FindList<T_QARecordEntity>(o => o.TaskID == task.F_Id);
                                foreach (T_QARecordEntity outR in qaRecList)
                                {
                                    outR.State = "WaitPick";
                                    db.Update<T_QARecordEntity>(outR);
                                }
                            }
                        }
                    }

                }
                wave.State = "Execing";
                db.Update<T_WaveEntity>(wave);
            }

            db.SaveChanges();

            if (needSendWCS.Count < 1)
            {
                rst.state = ResultType.error;
                rst.message = "无可用库存";
                return rst;
            }

            //发送任务到WCS
            WCSResult wcsRst = new WCSResult();
            wcsRst = new WCSPost().SendTask(db, needSendWCS);
            if (!wcsRst.IsSuccess)
            {
                rst.state = ResultType.error;
                rst.message = wcsRst.FailMsg;
            }
            else
            {
                rst.state = ResultType.success;
            }
            return rst;
        }
        #endregion

        #region 波次删除(质检)
        /// <summary>
        /// 波次删除(质检)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="waveID"></param>
        /// <returns></returns>
        public AjaxResult WaveDel_QA(IRepositoryBase db, string waveID)
        {
            AjaxResult result = new AjaxResult();

            T_WaveEntity waveEntity = db.FindEntity<T_WaveEntity>(o => o.F_Id == waveID);
            if (waveEntity.State != "New")
            {
                result.state = ResultType.error;
                result.message = "波次已执行";
                return result;
            }

            List<T_WaveDetailEntity> waveDetailList = db.FindList<T_WaveDetailEntity>(o => o.WaveID == waveID);

            List<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.WaveID == waveID);

            List<T_TaskDetailEntity> taskDetailList = db.FindList<T_TaskDetailEntity>(o => o.WaveID == waveID);

            //同波次的所有单据明细和所有容器所对应当前任务的货位
            string[] barCodeInWaveTaskDetailOrder = taskDetailList.Where(o => o.IsCurTask == "true" && o.IsOver == "false").Select(o => o.BarCode).Distinct().ToArray();
            //即存在于当前波次，又存在于其它波次 的库位
            string[] barCodeInOtherTaskDetailOrder = db.FindList<T_TaskDetailEntity>(o => barCodeInWaveTaskDetailOrder.Contains(o.BarCode) && o.IsOver == "false" && o.WaveID != waveID && o.IsCurTask == "true").Select(o => o.BarCode).ToArray();

            List<T_LocationEntity> locList = new List<T_LocationEntity>();
            foreach (string barCode in barCodeInWaveTaskDetailOrder)
            {
                if (barCodeInOtherTaskDetailOrder.Contains(barCode)) //容器同时存在多个波次，不用更新货位状态,如果是当前任务，则需启用下一个波次任务
                {
                    T_TaskDetailEntity curTaskDetail = taskDetailList.FirstOrDefault(o => o.BarCode == barCode && o.IsCurTask == "true");
                    if (curTaskDetail != null) //删除的任务明细存在主任务，生成新的主任务
                    {
                        List<T_TaskDetailEntity> taskNextList = db.FindList<T_TaskDetailEntity>(o => o.WaveID != waveID && o.BarCode == barCode && o.IsOver == "false");
                        T_TaskDetailEntity nextDetailTask = taskNextList.FirstOrDefault();

                        T_TaskEntity newTask = new T_TaskEntity();
                        newTask.F_Id = Guid.NewGuid().ToString();
                        newTask.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                        newTask.TaskInOutType = "OutType";

                        string orderType = nextDetailTask.OrderType;
                        if ("GetItemOut" == orderType)
                        {
                            newTask.TaskType = "TaskType_GetItemOut";
                            newTask.OrderType = "GetItemOut";
                            newTask.Level = 20;
                        }
                        else if ("VerBackOut" == orderType)
                        {
                            newTask.TaskType = "TaskType_VerBackOut";
                            newTask.OrderType = "VerBackOut";
                            newTask.Level = 20;
                        }
                        else if ("WarehouseBackOut" == orderType)
                        {
                            newTask.TaskType = "TaskType_WarehouseBackOut";
                            newTask.OrderType = "WarehouseBackOut";
                            newTask.Level = 20;
                        }
                        else if ("OtherOut" == orderType)
                        {
                            newTask.TaskType = "TaskType_OtherOut";
                            newTask.OrderType = "OtherOut";
                            newTask.Level = 20;
                        }
                        else if ("GetSample" == orderType)
                        {
                            newTask.TaskType = "TaskType_CheckPickOut";
                            newTask.OrderType = "GetSample";
                            newTask.Level = 30;
                        }
                        else
                        {
                            result.state = ResultType.error;
                            result.message = "任务明细单据类型错误";
                            return result;
                        }

                        newTask.ContainerID = nextDetailTask.ContainerID;
                        newTask.BarCode = nextDetailTask.BarCode;
                        newTask.ContainerType = nextDetailTask.ContainerType;
                        newTask.SrcLocationID = curTaskDetail.SrcLocationID;   // nextDetailTask.SrcLocationID;
                        newTask.SrcLocationCode = curTaskDetail.SrcLocationCode; // nextDetailTask.SrcLocationCode;
                        newTask.TagAreaID = "";
                        newTask.TagLocationID = nextDetailTask.TagLocationID;
                        newTask.TagLocationCode = nextDetailTask.TagLocationCode;
                        newTask.ApplyStationID = null;
                        
                        newTask.State = "New";
                        newTask.IsWcsTask = "true";
                        newTask.ExecEquID = null;
                        newTask.IsCanExec = "true";
                        newTask.SendWCSTime = null;
                        newTask.WaveCode = nextDetailTask.WaveCode;
                        newTask.WaveID = nextDetailTask.WaveID;
                        newTask.SEQ = nextDetailTask.SEQ;

                        newTask.OrderID = nextDetailTask.OrderID;
                        newTask.OrderDetailID = nextDetailTask.OrderDetailID;
                        newTask.OrderCode = nextDetailTask.OrderCode;
                        newTask.OverTime = null;

                        db.Insert<T_TaskEntity>(newTask);


                        nextDetailTask.IsCurTask = "true";
                        nextDetailTask.TaskID = newTask.F_Id;
                        nextDetailTask.TaskNo = newTask.TaskNo;
                        db.Update<T_TaskDetailEntity>(nextDetailTask);


                        IList<T_QARecordEntity> qaReceordTempList = db.FindList<T_QARecordEntity>(o => o.QADetailID == newTask.OrderDetailID && o.WaveCode == nextDetailTask.WaveCode && o.BarCode == newTask.BarCode).ToList();//同单据不同明细，公用一个容器的情况
                        foreach (T_QARecordEntity outcell in qaReceordTempList)
                        {
                            outcell.TaskID = newTask.F_Id;
                            outcell.TaskNo = newTask.TaskNo;
                            db.Update<T_QARecordEntity>(outcell);
                        }
                    }
                }
                else //货位只存在于当前波次，还原货位状态
                {
                    IList<T_TaskDetailEntity> barCodeTaskDetailList = taskDetailList.Where(o => o.BarCode == barCode).ToList();
                    T_TaskDetailEntity barCodeTaskDetail = barCodeTaskDetailList.FirstOrDefault();
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == barCodeTaskDetail.SrcLocationCode);
                    loc.State = "Stored";
                    locList.Add(loc);
                }
            }

            db.Update<T_LocationEntity>(locList);

            List<T_QARecordEntity> qaRecordList = db.FindList<T_QARecordEntity>(o => o.WaveID == waveID);

            string[] containerDetailID = qaRecordList.Select(o => o.ContainerDetailID).ToArray();
            List<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => containerDetailID.Contains(o.F_Id));
            foreach (T_ContainerDetailEntity cell in containerDetailList)
            {
                decimal outQty = qaRecordList.Where(o => o.ContainerDetailID == cell.F_Id && o.WaveID == waveID).Sum(o => o.Qty ?? 0);
                cell.OutQty = cell.OutQty - outQty;
            }

            db.Update<T_ContainerDetailEntity>(containerDetailList);



            string[] allQAIDList = waveDetailList.Select(o => o.OutBoundID).ToArray();
            List<T_QAEntity> qaList = db.FindList<T_QAEntity>(o => allQAIDList.Contains(o.F_Id));
            foreach (T_QAEntity qaHead in qaList)
            {
                bool isNewOrder = true;
                string[] allOutBoundDetailIDList = waveDetailList.Where(o => o.OutBoundID == qaHead.F_Id).Select(o => o.OutBoundDetailID).ToArray();
                List<T_QADetailEntity> qaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qaHead.F_Id);
                foreach (T_QADetailEntity qaDetail in qaDetailList)
                {
                    //解结库存
                    List<T_ContainerDetailEntity> detailList = new List<T_ContainerDetailEntity>();
                    if (string.IsNullOrEmpty(qaDetail.Lot))
                    {
                        detailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == qaDetail.ItemID && string.IsNullOrEmpty(o.Lot));
                    }
                    else
                    {
                        detailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == qaDetail.ItemID && o.Lot == qaDetail.Lot);
                    }
                    foreach (T_ContainerDetailEntity cd in detailList)
                    {
                        cd.IsCheckFreeze = "false";
                        db.Update<T_ContainerDetailEntity>(cd);
                    }


                    //明细包含在要删除的波次中
                    if (allOutBoundDetailIDList.Contains(qaDetail.F_Id))
                    {
                        decimal? outRQty = db.FindList<T_QARecordEntity>(o => o.WaveID == waveID && o.QADetailID == qaDetail.F_Id).Sum(o => o.Qty ?? 0);
                        qaDetail.WaveQty = qaDetail.WaveQty - outRQty;
                        db.Update<T_QADetailEntity>(qaDetail);
                    }
                    else
                    {
                        if (qaDetail.State != "New")
                        {
                            isNewOrder = false;
                        }
                    }
                }

                if (isNewOrder)
                {
                    qaHead.State = "New";
                }
            }

            db.Update<T_QAEntity>(qaList);



            db.Delete<T_WaveEntity>(waveEntity);
            db.Delete<T_WaveDetailEntity>(waveDetailList);
            db.Delete<T_QARecordEntity>(qaRecordList);
            db.Delete<T_TaskDetailEntity>(taskDetailList);
            db.Delete<T_TaskEntity>(taskList);

            result.state = ResultType.success;
            return result;
        }

        #endregion
    }
}
