/*******************************************************************************
 * Copyright ? 2021 迈思特版权所有
 * Author: MST_WMS
 * Description: WMS平台
 * Website：www.maisite.com
*********************************************************************************/
using MST.Application.SystemManage;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Domain.ViewModel;
using MST.EnumType;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_OutRecordContainerApp
    {
        private IT_OutRecordRepository service = new T_OutRecordRepository();
        public IQueryable<T_OutRecordEntity> FindList(Expression<Func<T_OutRecordEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_OutRecordEntity FindEntity(Expression<Func<T_OutRecordEntity, bool>> predicate)
        {
            T_OutRecordEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_OutRecordEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_OutRecordEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_OutRecordEntity> GetList(string outBoundDetailID = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_OutRecordEntity>();
            if (!string.IsNullOrEmpty(outBoundDetailID))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.OutBoundDetailID == outBoundDetailID && t.ItemCode.Contains(keyword));
                    expression = expression.Or(t => t.OutBoundDetailID == outBoundDetailID && t.ItemName.Contains(keyword));
                }
                else expression = expression.And(t => t.OutBoundDetailID == outBoundDetailID);
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemCode.Contains(keyword));
                    expression = expression.Or(t => t.ItemName.Contains(keyword));
                }
            }
            
            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public T_OutRecordEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_OutRecordEntity itemsEntity, string keyValue)
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

        public void Insert(T_OutRecordEntity entity)
        {
            service.Insert(entity);
        }

        #region 波次运算(调用前需加锁)
        /// <summary>
        /// 波次运算(调用前需加锁)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="waveType"></param>
        /// <param name="outBoundIDArray"></param>
        /// <param name="isMustFull">是否必须满足需求数量</param>
        /// <returns></returns>
        public AjaxResult WaveGen(IRepositoryBase db, EType.T_Wave_Em.WaveType waveType, IList<ContainerModel> containerModelByHandList, string[] outBoundDetailIDArray, bool isMustFull)
        {
            AjaxResult result = new AjaxResult();

            IList<T_OutBoundDetailEntity> outBoundDetailEntitiesList = db.FindList<T_OutBoundDetailEntity>(o => outBoundDetailIDArray.Contains(o.F_Id));
            string[] outBoundIDList = outBoundDetailEntitiesList.Select(o => o.OutBoundID).ToArray();
            IList<T_OutBoundEntity> outBoundEntityList = db.FindList<T_OutBoundEntity>(o => outBoundIDList.Contains(o.F_Id));


            IList<OutBoundDetailModel> outBoundDetailModelsList = outBoundDetailEntitiesList.ToObject<IList<OutBoundDetailModel>>();
            foreach (OutBoundDetailModel model in outBoundDetailModelsList)
            {
                T_OutBoundEntity outBound = outBoundEntityList.FirstOrDefault(o => o.F_Id == model.OutBoundID);
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == model.StationID);
                EType.T_OutBound_Em.OutBoundType outBoundType = (EType.T_OutBound_Em.OutBoundType)Enum.Parse(typeof(EType.T_OutBound_Em.OutBoundType), outBound.OutBoundType);
                if (EType.T_OutBound_Em.OutBoundType.GetItemOut == outBoundType || EType.T_OutBound_Em.OutBoundType.WarehouseBackOut == outBoundType) //领料出库、仓退出库 取合格库存
                {
                    model.IsQua = true;
                    model.OutBoundID = outBound.F_Id;
                    model.OutBoundType = outBound.OutBoundType;
                    model.OutBoundCode = outBound.OutBoundCode;
                    model.TagAddressCode = station.TagAddress;
                    model.StationCode = station.StationCode;
                    model.StateName = station.StationName;
                }
                else if (EType.T_OutBound_Em.OutBoundType.VerBackOut == outBoundType)  //验退出库 取不合格库存
                {
                    model.IsQua = false;
                    model.OutBoundID = outBound.F_Id;
                    model.OutBoundType = outBound.OutBoundType;
                    model.OutBoundCode = outBound.OutBoundCode;
                    model.TagAddressCode = station.TagAddress;
                    model.StationCode = station.StationCode;
                    model.StationName = station.StationName;
                }
                else
                {
                    result.state = ResultType.error;
                    result.message = "未知的出库单据类型";
                    return result;
                }
            }

            var itemGroupList = outBoundDetailModelsList.GroupBy(g => new { g.ItemID, g.ItemCode, g.Lot, g.IsQua }).Select(o => new ItemGroup { ItemID = o.Key.ItemID, ItemCode = o.Key.ItemCode, Lot = o.Key.Lot, IsQua = o.Key.IsQua, Qty = o.Sum(i => (i.Qty ?? 0) - (i.OutQty ?? 0) - (i.WaveQty ?? 0)), OutDetailModelList = o.Select(k => k).OrderBy(k => k.OutBoundID).ThenBy(k => k.SEQ).ToList() }).ToList();

            //过滤货位
            IQueryable<T_LocationEntity> locationIQ = db.IQueryable<T_LocationEntity>(o => o.State == EType.T_Location_Em.State.Stored.ToString() && (o.ForbiddenState == EType.T_Location_Em.ForbiddenState.Normal.ToString() || o.ForbiddenState == EType.T_Location_Em.ForbiddenState.OnlyOut.ToString()));
            //过滤容器主表
            IQueryable<T_ContainerEntity> containerIQ = db.IQueryable<T_ContainerEntity>();
            //联合货位与容器
            IQueryable<LocationIDEntity> containerIDIQ = locationIQ.Join(containerIQ, m => m.F_Id, n => n.LocationID, (m, n) => new LocationIDEntity { LocationID = m.F_Id, BarCode = n.BarCode, ContainerType = n.ContainerType, LocationCode = m.LocationCode, ContainerID = n.F_Id, KindCode = n.ContainerKind });

            //出库物料分组，并校验库存是否充足
            foreach (ItemGroup itemcell in itemGroupList)
            {
                //过滤库存明细
                IQueryable<T_ContainerDetailEntity> detailIQ = db.IQueryable<T_ContainerDetailEntity>(o => o.ItemID == itemcell.ItemID && o.OutQty == 0 && o.State == EType.T_ContainerDetail_Em.State.Normal.ToString() && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false");

                if (!string.IsNullOrEmpty(itemcell.Lot))
                {
                    detailIQ = detailIQ.Where(o => o.Lot == itemcell.Lot);
                }

                if (itemcell.IsQua) //领料出库、仓退出库 取合格库存
                {
                    detailIQ = detailIQ.Where(o => o.CheckState == EType.T_ContainerDetail_Em.CheckState.Qua.ToString() || o.CheckState == EType.T_ContainerDetail_Em.CheckState.UnNeed.ToString());
                }
                else  //验退出库  取不合格库存
                {
                    detailIQ = detailIQ.Where(o => o.CheckState == EType.T_ContainerDetail_Em.CheckState.UnQua.ToString() || o.CheckState == EType.T_ContainerDetail_Em.CheckState.WaitCheck.ToString());
                }

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
                                                                    IsSpecial = m.IsSpecial,
                                                                    F_Id = m.F_Id,
                                                                    Qty = m.Qty ?? 0,
                                                                    Lot = m.Lot ?? "",
                                                                    HandQty = 0,
                                                                    F_CreatorTime = m.F_CreatorTime,
                                                                    KindCode = n.KindCode
                                                                });

                itemcell.ContainerDetailGroupByItemBarCodeList = conDModelIQ.GroupBy(o=> new
                {
                    o.ItemID,
                    o.ItemName,
                    o.ItemCode,
                    o.ItemUnitText,
                    o.Lot,
                    o.SupplierCode,
                    o.SupplierID,
                    o.SupplierName,
                    o.IsSpecial,
                    o.ContainerID,
                    o.LocationNo,
                    o.BarCode,
                    o.ContainerKind,
                    o.HandQty,
                    o.ContainerType,
                    o.KindCode,
                    o.LocationID
                }).Select(o => new ContainerDetailGroupByItemBarCode
                {
                    ItemID = o.Key.ItemID,
                    ItemName = o.Key.ItemName,
                    ItemCode = o.Key.ItemCode,
                    ItemUnitText = o.Key.ItemUnitText,
                    Lot = o.Key.Lot,
                    SupplierCode = o.Key.SupplierCode,
                    SupplierID = o.Key.SupplierID,
                    SupplierName = o.Key.SupplierName,
                    Qty = o.Sum(i => i.Qty),
                    OutQty = o.Sum(i => i.OutQty),
                    IsSpecial = o.Key.IsSpecial,
                    ContainerID = o.Key.ContainerID,
                    F_Id = o.Key.ContainerID,
                    LocationNo = o.Key.LocationNo,
                    BarCode = o.Key.BarCode,
                    ContainerKind = o.Key.ContainerKind,
                    ContainerType = o.Key.ContainerType,
                    KindCode = o.Key.KindCode,
                    LocationID = o.Key.LocationID,
                    HandQty = o.Sum(i => i.HandQty)
                }).ToList();

                //将手动选择的部分赋值给库存对象

                if (containerModelByHandList != null)
                {
                    foreach (ContainerModel handModel in containerModelByHandList)
                    {
                        ContainerDetailGroupByItemBarCode inDetail = itemcell.ContainerDetailGroupByItemBarCodeList.FirstOrDefault(o => o.ContainerID == handModel.F_Id);
                        if (inDetail == null) //手动选择的不存在于库存集合（可能Qty-OutQty>0 或货位 被设置为待出库）
                        {
                            IQueryable<T_ContainerEntity> conIQ = db.IQueryable<T_ContainerEntity>();
                            IQueryable<T_LocationEntity> locIQ = db.IQueryable<T_LocationEntity>();
                            IQueryable<T_ContainerDetailEntity> conDetailIQ = db.IQueryable<T_ContainerDetailEntity>();

                            ContainerDetailGroupByItemBarCode containerDB = conIQ.Join(locIQ, m => m.LocationID, n => n.F_Id, (m, n) => new { ContainerID = m.F_Id, n.LocationCode, m.BarCode, m.ContainerType, LocationID = n.F_Id, m.ContainerKind }).Join(conDetailIQ, j => j.ContainerID, k => k.ContainerID, (j, k) => new ContainerDetailModel()
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
                                IsSpecial = k.IsSpecial,
                                F_Id = k.F_Id,
                                Qty = k.Qty ?? 0,
                                Lot = k.Lot ?? "",
                                HandQty = handModel.HandQty,
                                F_CreatorTime = k.F_CreatorTime,
                                KindCode = j.ContainerKind
                            }).Where(o => o.ContainerID == handModel.F_Id).GroupBy(o=> new
                            {
                                o.ItemID,
                                o.ItemName,
                                o.ItemCode,
                                o.ItemUnitText,
                                o.Lot,
                                o.SupplierCode,
                                o.SupplierID,
                                o.SupplierName,
                                o.IsSpecial,
                                o.ContainerID,
                                o.LocationNo,
                                o.BarCode,
                                o.ContainerKind,
                                o.HandQty,
                                o.ContainerType,
                                o.KindCode,
                                o.LocationID
                            }).Select(o => new ContainerDetailGroupByItemBarCode
                            {
                                ItemID = o.Key.ItemID,
                                ItemName = o.Key.ItemName,
                                ItemCode = o.Key.ItemCode,
                                ItemUnitText = o.Key.ItemUnitText,
                                Lot = o.Key.Lot,
                                SupplierCode = o.Key.SupplierCode,
                                SupplierID = o.Key.SupplierID,
                                SupplierName = o.Key.SupplierName,
                                Qty = o.Sum(i => i.Qty),
                                OutQty = o.Sum(i => i.OutQty),
                                IsSpecial = o.Key.IsSpecial,
                                ContainerID = o.Key.ContainerID,
                                F_Id = o.Key.ContainerID,
                                LocationNo = o.Key.LocationNo,
                                BarCode = o.Key.BarCode,
                                ContainerKind = o.Key.ContainerKind,
                                ContainerType = o.Key.ContainerType,
                                KindCode = o.Key.KindCode,
                                LocationID = o.Key.LocationID,
                                HandQty = o.Sum(i => i.HandQty)
                            }).FirstOrDefault();

                            itemcell.ContainerDetailGroupByItemBarCodeList.Add(containerDB);
                        }
                        else
                        {
                            inDetail.HandQty = handModel.HandQty;
                        }
                    }
                }
                //所有当前物料的可用库存,策略：手动优先，其次批次先进先出，其次散件优先
                itemcell.ContainerDetailGroupByItemBarCodeList = itemcell.ContainerDetailGroupByItemBarCodeList.OrderByDescending(o => o.HandQty ?? 0).ThenBy(o => o.Lot).ThenBy(o => o.Qty ?? 0).ToList();

                decimal qtyAll = itemcell.ContainerDetailGroupByItemBarCodeList.Sum(o => o.Qty ?? 0);
                if (isMustFull)
                {
                    if (qtyAll < itemcell.Qty)
                    {
                        result.state = ResultType.error;
                        result.message = "库存不足";
                        return result;
                    }
                }
            }

            //产生波次单
            T_WaveEntity waveEntity = AddWave(db, waveType, outBoundDetailEntitiesList);

            IList<T_ContainerTypeEntity> containerTypeAll = db.FindList<T_ContainerTypeEntity>(o => true).ToList();
            ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
            IList<ItemsDetailEntity> sysitemList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();

            //产生拣选数据
            List<T_OutRecordEntity> outRecordList = new List<T_OutRecordEntity>();
            foreach (ItemGroup itemcell in itemGroupList)
            {
                foreach (OutBoundDetailModel outBoundDetailModel in itemcell.OutDetailModelList)
                {
                    decimal readyQty = 0; //已取总数量
                    T_WaveDetailEntity waveDetail = db.FindEntity<T_WaveDetailEntity>(o => o.WaveID == waveEntity.F_Id && o.OutBoundDetailID == outBoundDetailModel.F_Id);

                    IList<ContainerDetailGroupByItemBarCode> containerGroupByItemBarCode = itemcell.ContainerDetailGroupByItemBarCodeList.GroupBy(o => new
                    {
                        o.ItemID,
                        o.ItemName,
                        o.ItemCode,
                        o.ItemUnitText,
                        o.Lot,
                        o.SupplierCode,
                        o.SupplierID,
                        o.SupplierName,
                        o.IsSpecial,
                        o.ContainerID,
                        o.LocationNo,
                        o.BarCode,
                        o.ContainerKind,
                        o.HandQty,
                        o.ContainerType,
                        o.KindCode,
                        o.LocationID
                    }).Select(o => new ContainerDetailGroupByItemBarCode
                    {
                        ItemID = o.Key.ItemID,
                        ItemName = o.Key.ItemName,
                        ItemCode = o.Key.ItemCode,
                        ItemUnitText = o.Key.ItemUnitText,
                        Lot = o.Key.Lot,
                        SupplierCode = o.Key.SupplierCode,
                        SupplierID = o.Key.SupplierID,
                        SupplierName = o.Key.SupplierName,
                        Qty = o.Sum(i => i.Qty),
                        OutQty = o.Sum(i => i.OutQty),
                        IsSpecial = o.Key.IsSpecial,
                        ContainerID = o.Key.ContainerID,
                        F_Id = o.Key.ContainerID,
                        LocationNo = o.Key.LocationNo,
                        BarCode = o.Key.BarCode,
                        ContainerKind = o.Key.ContainerKind,
                        ContainerType = o.Key.ContainerType,
                        KindCode = o.Key.KindCode,
                        LocationID = o.Key.LocationID,
                        HandQty = o.Sum(i => i.HandQty)
                    }).ToList();

                    foreach (ContainerDetailGroupByItemBarCode cdModel in containerGroupByItemBarCode)
                    {
                        if (outBoundDetailModel.Qty > readyQty)
                        {
                            decimal curQty = 0;
                            if (cdModel.HandQty > 0)
                            {
                                curQty = cdModel.HandQty ?? 0;
                            }
                            else
                            {
                                if (outBoundDetailModel.Qty > (readyQty + (cdModel.Qty ?? 0))) //取完
                                {
                                    curQty = (cdModel.Qty ?? 0);
                                }
                                else  //取部分
                                {
                                    curQty = (outBoundDetailModel.Qty ?? 0) - readyQty;
                                }
                            }

                            T_OutRecordEntity outRecord = new T_OutRecordEntity();
                            outRecord.F_Id = Guid.NewGuid().ToString();
                            outRecord.OutBoundID = outBoundDetailModel.OutBoundID;
                            outRecord.OutBoundDetailID = outBoundDetailModel.F_Id;
                            outRecord.SEQ = outBoundDetailModel.SEQ;
                            outRecord.TaskID = "";
                            outRecord.TaskNo = "";
                            outRecord.BarCode = cdModel.BarCode;
                            outRecord.ContainerType = cdModel.ContainerType;
                            outRecord.ContainerTypeName = containerTypeAll.FirstOrDefault(o => o.ContainerTypeCode == cdModel.ContainerType).ContainerTypeName;
                            outRecord.ContainerKind = cdModel.KindCode;
                            outRecord.ContainerKindName = sysitemList.FirstOrDefault(o => o.F_ItemCode == cdModel.KindCode).F_ItemName;
                            outRecord.SrcLocationID = cdModel.LocationID;
                            outRecord.SrcLocationCode = cdModel.LocationNo;
                            outRecord.TagAreaID = "";
                            outRecord.TagLocationID = outBoundDetailModel.StationID;
                            outRecord.TagLocationCode = outBoundDetailModel.TagAddressCode;
                            outRecord.StationName = outBoundDetailModel.StationName;
                            outRecord.ContainerID = cdModel.ContainerID;
                            outRecord.ContainerDetailID = cdModel.F_Id;
                            outRecord.ItemID = cdModel.ItemID;
                            outRecord.ItemName = cdModel.ItemName;
                            outRecord.ItemCode = cdModel.ItemCode;
                            outRecord.OldQty = cdModel.Qty;
                            outRecord.NeedQty = curQty;
                            outRecord.PickedQty = 0;
                            outRecord.AfterQty = 0;
                            outRecord.WaveID = waveEntity.F_Id;
                            outRecord.WaveCode = waveEntity.WaveCode;
                            outRecord.WaveDetailID = waveDetail.F_Id;
                            outRecord.ItemUnitText = cdModel.ItemUnitText;
                            EType.T_OutBound_Em.OutBoundType outBoundType = (EType.T_OutBound_Em.OutBoundType)Enum.Parse(typeof(EType.T_OutBound_Em.OutBoundType), outBoundDetailModel.OutBoundType);
                            switch (outBoundType)
                            {
                                case EType.T_OutBound_Em.OutBoundType.GetItemOut:
                                    {
                                        outRecord.OutBoundType = EType.T_OutRecord_Em.OutBoundType.GetItemOut.ToString();
                                    }
                                    break;
                                case EType.T_OutBound_Em.OutBoundType.VerBackOut:
                                    {
                                        outRecord.OutBoundType = EType.T_OutRecord_Em.OutBoundType.VerBackOut.ToString();
                                    }
                                    break;
                                case EType.T_OutBound_Em.OutBoundType.WarehouseBackOut:
                                    {
                                        outRecord.OutBoundType = EType.T_OutRecord_Em.OutBoundType.WarehouseBackOut.ToString();
                                    }
                                    break;
                            }

                            outRecord.Lot = cdModel.Lot;
                            outRecord.Price = 0;
                            outRecord.SupplierID = cdModel.SupplierID;
                            outRecord.SupplierCode = cdModel.SupplierCode;
                            outRecord.SupplierName = cdModel.SupplierName;
                            outRecord.IsSpecial = cdModel.IsSpecial;
                            outRecord.State = EType.T_OutRecord_Em.State.New.ToString();
                            outRecord.OrderCode = outBoundDetailModel.OutBoundCode;
                            outRecord.PickDate = null;

                            outRecordList.Add(outRecord);

                            readyQty = readyQty + curQty;

                        }
                        else
                        {
                            break;
                        }
                    }

                    T_OutBoundDetailEntity outDetailBound = db.FindEntity<T_OutBoundDetailEntity>(o => o.F_Id == outBoundDetailModel.F_Id);
                    if (outDetailBound.WaveQty + readyQty == outDetailBound.Qty)
                    {
                        outDetailBound.WaveQty = outDetailBound.WaveQty + readyQty;
                        outDetailBound.State = EType.T_OutBoundDetail_Em.State.Waved.ToString();
                        db.Update<T_OutBoundDetailEntity>(outDetailBound);
                        break;
                    }
                }
            }

            db.SaveChanges();

            IList<T_OutBoundEntity> outBoundList = db.FindList<T_OutBoundEntity>(o => outBoundIDList.Contains(o.F_Id)).ToList();
            foreach (T_OutBoundEntity ob in outBoundList)
            {
                IList<T_OutBoundDetailEntity> detailList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == ob.F_Id && o.State == EType.T_OutBoundDetail_Em.State.New.ToString()).ToList();
                if (detailList.Count > 0)
                {
                    ob.State = EType.T_OutBound_Em.State.WavedPart.ToString();
                }
                else
                {
                    ob.State = EType.T_OutBound_Em.State.Waved.ToString();
                }
                db.Update<T_OutBoundEntity>(ob);
            }


            //更新库存待出库数量
            string[] detailID = outRecordList.Select(o => o.ContainerDetailID).ToArray();
            List<T_ContainerDetailEntity> containerDetailListInDB = db.FindList<T_ContainerDetailEntity>(o => detailID.Contains(o.F_Id));
            foreach (T_ContainerDetailEntity cdEntity in containerDetailListInDB)
            {
                cdEntity.OutQty = (cdEntity.OutQty ?? 0) + outRecordList.FirstOrDefault(o => o.ContainerDetailID == cdEntity.F_Id).NeedQty;
            }

            //产生任务,OutInfo 可能存在同一个单据主数据和不同单据明细数据
            var barCodeList = outRecordList.GroupBy(g => new { BarCode = g.BarCode }).Select(o => new { BarCode = o.Key.BarCode, OutInfo = o.Select(k => k) }).ToList();
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
                    task.TaskInOutType = EType.T_Task_Em.TaskInOutType.OutType.ToString();

                    EType.T_OutBound_Em.OutBoundType outType = (EType.T_OutBound_Em.OutBoundType)Enum.Parse(typeof(EType.T_OutBound_Em.OutBoundType), firstOutOrder.OutBoundType);
                    if (EType.T_OutBound_Em.OutBoundType.GetItemOut == outType)
                    {
                        task.TaskType = EType.T_Task_Em.TaskType.TaskType_GetItemOut.ToString();
                        task.OrderType = EType.T_Task_Em.OrderType.GetItemOut.ToString();
                    }
                    else if (EType.T_OutBound_Em.OutBoundType.VerBackOut == outType)
                    {
                        task.TaskType = EType.T_Task_Em.TaskType.TaskType_VerBackOut.ToString();
                        task.OrderType = EType.T_Task_Em.OrderType.VerBackOut.ToString();
                    }
                    else if (EType.T_OutBound_Em.OutBoundType.WarehouseBackOut == outType)
                    {
                        task.TaskType = EType.T_Task_Em.TaskType.TaskType_WarehouseBackOut.ToString();
                        task.OrderType = EType.T_Task_Em.OrderType.WarehouseBackOut.ToString();
                    }

                    task.ContainerID = firstOutOrder.ContainerID;
                    task.BarCode = firstOutOrder.BarCode;
                    task.ContainerType = firstOutOrder.ContainerType;
                    task.SrcLocationID = firstOutOrder.SrcLocationID;
                    task.SrcLocationCode = firstOutOrder.SrcLocationCode;
                    task.TagAreaID = "";
                    task.TagLocationID = firstOutOrder.TagLocationID;
                    task.TagLocationCode = firstOutOrder.TagLocationCode;
                    task.ApplyStationID = null;
                    task.Level = 4;
                    task.State = EType.T_Task_Em.State.New.ToString();
                    task.IsWcsTask = "true";
                    task.ExecEquID = null;
                    task.IsCanExec = "false";
                    task.SendWCSTime = null;
                    task.WaveCode = waveEntity.WaveCode;
                    task.WaveID = waveEntity.F_Id;
                    task.SEQ = firstOutOrder.SEQ;
                    task.WaveDetailID = firstOutOrder.WaveDetailID;
                    task.OrderID = firstOutOrder.OutBoundID;
                    task.OrderDetailID = firstOutOrder.OutBoundDetailID;
                    task.OrderCode = firstOutOrder.OrderCode;
                    task.OverTime = null;

                    IList<T_OutRecordEntity> outReceordTempList = outRecordList.Where(o => o.OutBoundDetailID == task.OrderDetailID && o.BarCode == task.BarCode).ToList();//同单据不同明细，公用一个容器的情况
                    foreach (T_OutRecordEntity outcell in outReceordTempList)
                    {
                        outcell.TaskID = task.F_Id;
                        outcell.TaskNo = task.TaskNo;
                    }

                    taskInsertList.Add(task);
                }

                //任务明细
                foreach (T_OutRecordEntity outcell in bar.OutInfo)
                {
                    T_TaskDetailEntity taskDetailEntity = new T_TaskDetailEntity();
                    taskDetailEntity.F_Id = Guid.NewGuid().ToString();
                    taskDetailEntity.ContainerID = outcell.ContainerID;
                    taskDetailEntity.BarCode = outcell.BarCode;
                    taskDetailEntity.ContainerType = outcell.ContainerType;
                    taskDetailEntity.TagLocationCode = outcell.TagLocationCode;
                    taskDetailEntity.TagLocationID = outcell.TagLocationID;
                    taskDetailEntity.SEQ = outcell.SEQ;
                    taskDetailEntity.OrderID = outcell.OutBoundID;
                    taskDetailEntity.OrderDetailID = outcell.OutBoundDetailID;
                    taskDetailEntity.OrderCode = outcell.OrderCode;
                    taskDetailEntity.WaveCode = waveEntity.WaveCode;
                    taskDetailEntity.WaveID = waveEntity.F_Id;
                    taskDetailEntity.IsOver = "false";
                    taskDetailEntity.WaveDetailID = outcell.WaveDetailID;

                    if (taskInDB == null && firstOutOrder.OutBoundDetailID == outcell.OutBoundDetailID)
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

                    taskDetailList.Add(taskDetailEntity);
                }
            }

            string[] locID = taskInsertList.Select(o => o.SrcLocationID).ToArray();
            List<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => locID.Contains(o.F_Id));
            foreach (T_LocationEntity loccell in locList)
            {
                loccell.State = EType.T_Location_Em.State.Out.ToString();
            }

            int count1 = db.Insert<T_OutRecordEntity>(outRecordList);
            int count2 = db.Update<T_ContainerDetailEntity>(containerDetailListInDB);
            int count3 = db.Insert<T_TaskEntity>(taskInsertList);
            int count4 = db.Insert<T_TaskDetailEntity>(taskDetailList);
            int count5 = db.Update<T_LocationEntity>(locList);

            result.state = ResultType.success;
            return result;
        }



        private T_WaveEntity AddWave(IRepositoryBase db, EType.T_Wave_Em.WaveType waveType, IList<T_OutBoundDetailEntity> outBoundDetailEntitiesList)
        {
            T_WaveEntity waveEntity = new T_WaveEntity();
            waveEntity.F_Id = Guid.NewGuid().ToString();
            waveEntity.WaveCode = T_CodeGenApp.GenNum("WaveRule");
            waveEntity.WaveType = waveType.ToString();
            waveEntity.State = EType.T_Wave_Em.State.New.ToString();

            db.Insert<T_WaveEntity>(waveEntity);

            foreach (T_OutBoundDetailEntity detail in outBoundDetailEntitiesList)
            {
                T_WaveDetailEntity waveDetail = new T_WaveDetailEntity();
                waveDetail.F_Id = Guid.NewGuid().ToString();
                waveDetail.WaveID = waveEntity.F_Id;
                waveDetail.OutBoundID = detail.OutBoundID;
                waveDetail.OutBoundDetailID = detail.F_Id;
                db.Insert<T_WaveDetailEntity>(waveDetail);
            }
            db.SaveChanges();
            return waveEntity;
        }

        public class ItemGroup
        {
            public string ItemID { get; set; }
            public string ItemCode { get; set; }
            public string Lot { get; set; }

            /// <summary>
            /// 是否取合格库存
            /// </summary>
            public bool IsQua { get; set; }

            /// <summary>
            /// 需求库存
            /// </summary>
            public decimal Qty { get; set; }

            public IList<OutBoundDetailModel> OutDetailModelList { get; set; }

            public IList<ContainerDetailGroupByItemBarCode> ContainerDetailGroupByItemBarCodeList { get; set; }

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

        public class ContainerDetailGroupByItemBarCode
        {
            public string F_Id { get; set; }
            public string BarCode { get; set; }
            public string ContainerKind { get; set; }

            public string ContainerKindName { get; set; }

            public string ContainerType { get; set; }

            public string ItemID { get; set; }

            public string ItemCode { get; set; }

            public string ItemName { get; set; }

            public string ItemUnitText { get; set; }

            public string LocationNo { get; set; }

            public string SupplierCode { get; set; }

            public string SupplierID { get; set; }

            public string SupplierName { get; set; }

            public string IsSpecial { get; set; }

            public string ContainerID { get; set; }

            public string KindCode { get; set; }

            public string LocationID { get; set; }

            public decimal? HandQty { get; set; }
            public string Lot { get; set; }

            public decimal? Qty { get; set; }

            public decimal? OutQty { get; set; }

            public decimal? CanUseQty { get; set; }
        }

        #endregion

        #region 波次删除
        /// <summary>
        /// 波次删除
        /// </summary>
        /// <param name="db"></param>
        /// <param name="waveID"></param>
        /// <returns></returns>
        public AjaxResult WaveDel(IRepositoryBase db, string waveID)
        {
            AjaxResult result = new AjaxResult();

            T_WaveEntity waveEntity = db.FindEntity<T_WaveEntity>(o => o.F_Id == waveID);
            if (waveEntity.State != EType.T_Wave_Em.State.New.ToString())
            {
                result.state = ResultType.error;
                result.message = "波次已执行";
                return result;
            }

            List<T_WaveDetailEntity> waveDetailList = db.FindList<T_WaveDetailEntity>(o => o.WaveID == waveID);

            List<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.WaveID == waveID);

            List<T_TaskDetailEntity> taskDetailList = db.FindList<T_TaskDetailEntity>(o => o.WaveID == waveID);

            //同波次的所有单据明细和所有容器所对应当前任务的货位
            string[] barCodeInWaveTaskDetailOrder = taskDetailList.Where(o => o.IsCurTask == "true" && o.IsOver == "false").Select(o => o.BarCode).ToArray();
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
                        newTask.TaskInOutType = EType.T_Task_Em.TaskInOutType.OutType.ToString();

                        EType.T_TaskDetail_Em.OrderType orderType = (EType.T_TaskDetail_Em.OrderType)Enum.Parse(typeof(EType.T_TaskDetail_Em.OrderType), nextDetailTask.OrderType);
                        if (EType.T_TaskDetail_Em.OrderType.GetItemOut == orderType)
                        {
                            newTask.TaskType = EType.T_Task_Em.TaskType.TaskType_GetItemOut.ToString();
                            newTask.OrderType = EType.T_Task_Em.OrderType.GetItemOut.ToString();
                        }
                        else if (EType.T_TaskDetail_Em.OrderType.VerBackOut == orderType)
                        {
                            newTask.TaskType = EType.T_Task_Em.TaskType.TaskType_VerBackOut.ToString();
                            newTask.OrderType = EType.T_Task_Em.OrderType.VerBackOut.ToString();
                        }
                        else if (EType.T_TaskDetail_Em.OrderType.WarehouseBackOut == orderType)
                        {
                            newTask.TaskType = EType.T_Task_Em.TaskType.TaskType_WarehouseBackOut.ToString();
                            newTask.OrderType = EType.T_Task_Em.OrderType.WarehouseBackOut.ToString();
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
                        newTask.Level = 4;
                        newTask.State = EType.T_Task_Em.State.New.ToString();
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


                        IList<T_OutRecordEntity> outReceordTempList = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == newTask.OrderDetailID && o.WaveCode == nextDetailTask.WaveCode && o.BarCode == newTask.BarCode).ToList();//同单据不同明细，公用一个容器的情况
                        foreach (T_OutRecordEntity outcell in outReceordTempList)
                        {
                            outcell.TaskID = newTask.F_Id;
                            outcell.TaskNo = newTask.TaskNo;
                            db.Update<T_OutRecordEntity>(outcell);
                        }
                    }
                }
                else //货位只存在于当前波次，还原货位状态
                {
                    IList<T_TaskDetailEntity> barCodeTaskDetailList = taskDetailList.Where(o => o.BarCode == barCode).ToList();
                    T_TaskDetailEntity barCodeTaskDetail = barCodeTaskDetailList.FirstOrDefault();
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == barCodeTaskDetail.SrcLocationCode);
                    loc.State = EType.T_Location_Em.State.Stored.ToString();
                    locList.Add(loc);
                }
            }

            db.Update<T_LocationEntity>(locList);

            List<T_OutRecordEntity> outRecordList = db.FindList<T_OutRecordEntity>(o => o.WaveID == waveID);

            string[] containerDetailID = outRecordList.Select(o => o.ContainerDetailID).ToArray();
            List<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => containerDetailID.Contains(o.F_Id));
            foreach (T_ContainerDetailEntity cell in containerDetailList)
            {
                decimal outQty = outRecordList.Where(o => o.ContainerDetailID == cell.F_Id && o.WaveID == waveID).Sum(o => o.NeedQty ?? 0);
                cell.OutQty = cell.OutQty - outQty;
            }

            db.Update<T_ContainerDetailEntity>(containerDetailList);


            string[] allOutBoundIDList = waveDetailList.Select(o => o.OutBoundID).ToArray();
            List<T_OutBoundEntity> outBoundList = db.FindList<T_OutBoundEntity>(o => allOutBoundIDList.Contains(o.F_Id));
            foreach (T_OutBoundEntity outBound in outBoundList)
            {
                bool isNewOrder = true;
                string[] allOutBoundDetailIDList = waveDetailList.Where(o => o.OutBoundID == outBound.F_Id).Select(o => o.OutBoundDetailID).ToArray();
                List<T_OutBoundDetailEntity> outBoundDetailList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == outBound.F_Id);
                foreach (T_OutBoundDetailEntity OutDetail in outBoundDetailList)
                {
                    //明细包含在要删除的波次中
                    if (allOutBoundDetailIDList.Contains(OutDetail.F_Id))
                    {
                        decimal? outRQty = db.FindList<T_OutRecordEntity>(o => o.WaveID == waveID && o.OutBoundDetailID == OutDetail.F_Id).Sum(o => o.NeedQty ?? 0);
                        OutDetail.WaveQty = OutDetail.WaveQty - outRQty;
                        db.Update<T_OutBoundDetailEntity>(OutDetail);
                    }
                    else
                    {
                        if (OutDetail.State != EType.T_OutBoundDetail_Em.State.New.ToString())
                        {
                            isNewOrder = false;
                        }
                    }
                }

                if (isNewOrder)
                {
                    outBound.State = EType.T_OutBound_Em.State.New.ToString();
                }
            }

            db.Update<T_OutBoundEntity>(outBoundList);



            db.Delete<T_WaveEntity>(waveEntity);
            db.Delete<T_WaveDetailEntity>(waveDetailList);
            db.Delete<T_OutRecordEntity>(outRecordList);
            db.Delete<T_TaskDetailEntity>(taskDetailList);
            db.Delete<T_TaskEntity>(taskList);

            result.state = ResultType.success;
            return result;
        }

        #endregion

    }
}
