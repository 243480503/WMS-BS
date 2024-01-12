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
    public class T_OutRecordApp
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
                    expression = expression.And(t => t.OutBoundDetailID == outBoundDetailID && (t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword)));
                }
                else expression = expression.And(t => t.OutBoundDetailID == outBoundDetailID);
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword));
                }
            }

            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }

        public List<T_OutRecordEntity> GetList(Pagination pagination, string outBoundDetailID = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_OutRecordEntity>();
            if (!string.IsNullOrEmpty(outBoundDetailID))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.OutBoundDetailID == outBoundDetailID && (t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword) || t.BarCode.Contains(keyword)));
                }
                else expression = expression.And(t => t.OutBoundDetailID == outBoundDetailID);
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword) || t.BarCode.Contains(keyword));
                }
            }

            return service.FindList(expression, pagination).ToList();
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
        public AjaxResult WaveGen(IRepositoryBase db, string waveType, IList<ContainerDetailModel> containerDetailModelByHandList, string[] outBoundDetailIDArray, bool isMustFull)
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

            IList<T_OutBoundDetailEntity> outBoundDetailEntitiesList = db.FindList<T_OutBoundDetailEntity>(o => outBoundDetailIDArray.Contains(o.F_Id));
            string[] outBoundIDList = outBoundDetailEntitiesList.Select(o => o.OutBoundID).Distinct().ToArray();
            IList<T_OutBoundEntity> outBoundEntityList = db.FindList<T_OutBoundEntity>(o => outBoundIDList.Contains(o.F_Id));


            IList<OutBoundDetailModel> outBoundDetailModelsList = outBoundDetailEntitiesList.ToObject<IList<OutBoundDetailModel>>();
            foreach (OutBoundDetailModel model in outBoundDetailModelsList)
            {
                T_OutBoundEntity outBound = outBoundEntityList.FirstOrDefault(o => o.F_Id == model.OutBoundID);
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == model.StationID);
                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == model.ItemID);
                T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                string outBoundType = outBound.OutBoundType;
                if ("GetItemOut" == outBoundType) //领料出库 取合格库存
                {
                    model.IsUrgent = outBound.IsUrgent;
                    model.OutBoundID = outBound.F_Id;
                    model.OutBoundType = outBound.OutBoundType;
                    model.OutBoundCode = outBound.OutBoundCode;
                    model.TagAddressCode = station.TagAddress;
                    model.StationCode = station.StationCode;
                    model.StationName = station.StationName;
                    model.UnitQty = item.UnitQty;
                    model.ContainerKind = containerType.ContainerKind;
                }
                else if ("WarehouseBackOut" == outBoundType)  //仓退出库 取合格库存
                {
                    model.IsUrgent = "true"; //没有紧急出库属性，为方便计算，此处视为true
                    model.OutBoundID = outBound.F_Id;
                    model.OutBoundType = outBound.OutBoundType;
                    model.OutBoundCode = outBound.OutBoundCode;
                    model.TagAddressCode = station.TagAddress;
                    model.StationCode = station.StationCode;
                    model.StationName = station.StationName;
                    model.UnitQty = item.UnitQty;
                    model.ContainerKind = containerType.ContainerKind;
                }
                else if ("VerBackOut" == outBoundType)  //验退出库 取不合格库存
                {
                    model.IsUrgent = "true";//没有紧急出库属性，为方便计算，此处视为true
                    model.OutBoundID = outBound.F_Id;
                    model.OutBoundType = outBound.OutBoundType;
                    model.OutBoundCode = outBound.OutBoundCode;
                    model.TagAddressCode = station.TagAddress;
                    model.StationCode = station.StationCode;
                    model.StationName = station.StationName;
                    model.PointInBoundID = outBound.PointInBoundID;
                    model.UnitQty = item.UnitQty;
                    model.ContainerKind = containerType.ContainerKind;
                }
                else if ("OtherOut" == outBoundType)  //其它出库 取待检物料、免检物料、合格物料、不合格物料
                {
                    model.IsUrgent = "true";//没有紧急出库属性，为方便计算，此处视为true
                    model.OutBoundID = outBound.F_Id;
                    model.OutBoundType = outBound.OutBoundType;
                    model.OutBoundCode = outBound.OutBoundCode;
                    model.TagAddressCode = station.TagAddress;
                    model.StationCode = station.StationCode;
                    model.StationName = station.StationName;
                    model.UnitQty = item.UnitQty;
                    model.ContainerKind = containerType.ContainerKind;
                }
                else
                {
                    result.state = ResultType.error;
                    result.message = "未知的出库单据类型";
                    return result;
                }
            }

            IList<ItemGroup> itemGroupList = outBoundDetailModelsList.GroupBy(g => new { g.PointInBoundID, g.ItemID, g.ItemCode, g.Lot, g.OutBoundType, g.ItemName, g.UnitQty, g.ContainerKind, g.SourceInOrderCode })
                                                                     .Select(o => new ItemGroup
                                                                     {
                                                                         PointInBoundID = o.Key.PointInBoundID, //验退有值，其它为空，有无该字段都不影响分组
                                                                         ItemID = o.Key.ItemID,
                                                                         ItemCode = o.Key.ItemCode,
                                                                         ItemName = o.Key.ItemName,
                                                                         Lot = o.Key.Lot,
                                                                         OutBoundType = o.Key.OutBoundType,
                                                                         UnitQty = o.Key.UnitQty,
                                                                         ContainerKind = o.Key.ContainerKind,
                                                                         SourceInOrderCode = o.Key.SourceInOrderCode,
                                                                         Qty = o.Sum(i => (i.Qty ?? 0) - (i.OutQty ?? 0) - (i.WaveQty ?? 0)),
                                                                         OutDetailModelList = o.Select(k => k).OrderBy(k => k.OutBoundID).ThenBy(k => k.SEQ).ToList()
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

            Dictionary<string, decimal> readyItemOut = new Dictionary<string, decimal>();

            //出库物料分组，并校验库存是否充足
            foreach (ItemGroup itemcell in itemGroupList)
            {
                //过滤库存明细
                IQueryable<T_ContainerDetailEntity> detailIQ = db.IQueryable<T_ContainerDetailEntity>(o => o.ItemID == itemcell.ItemID && o.State == "Normal" && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false");
                string outBoundType = itemcell.OutBoundType;
                switch (outBoundType)
                {
                    case "GetItemOut": /// 领料出库：出免检、合格物料
                        {
                            detailIQ = detailIQ.Where(o => o.CheckState == "Qua" || o.CheckState == "UnNeed");
                        }
                        break;
                    case "WarehouseBackOut": /// 仓退出库：出免检、合格物料
                        {
                            detailIQ = detailIQ.Where(o => o.CheckState == "Qua" || o.CheckState == "UnNeed");
                        }
                        break;
                    case "VerBackOut": /// 验退出库：出不合格物料
                        {
                            detailIQ = detailIQ.Where(o => o.CheckState == "UnQua" && o.InBoundID == itemcell.PointInBoundID);
                        }
                        break;
                    case "OtherOut": /// 其他出库：出待检物料、免检物料、合格物料、不合格物料
                        {
                            detailIQ = detailIQ.Where(o => o.CheckState == "WaitCheck" || o.CheckState == "UnNeed"
                                                            || o.CheckState == "Qua" || o.CheckState == "UnQua");
                        }
                        break;
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
                                                                    ERPWarehouseCode = m.ERPWarehouseCode,
                                                                    ContainerKind = m.ContainerKind,
                                                                    //IsSpecial = m.IsSpecial,
                                                                    Spec = m.Spec,
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
                                                                    ValidityDayNum = m.ValidityDayNum,
                                                                    RefInBoundCode = m.RefInBoundCode,
                                                                    ERPInDocCode = m.ERPInDocCode
                                                                });

                //无关批次的库存
                itemcell.ContainerDetailModelList = conDModelIQ.ToList();

                //将手动选择的部分赋值给库存对象

                if (containerDetailModelByHandList != null)
                {
                    foreach (ContainerDetailModel handModel in containerDetailModelByHandList)
                    {
                        ContainerDetailModel inDetail = itemcell.ContainerDetailModelList.FirstOrDefault(o => o.F_Id == handModel.F_Id);
                        if (inDetail == null) //手动选择的不存在于库存集合（可能Qty-OutQty>0 或货位 被设置为待出库）
                        {
                            result.state = ResultType.error;
                            result.message = "手动选择的物料不可出库:物料" + handModel.ItemCode;
                            return result;
                        }
                        else
                        {
                            inDetail.HandQty = handModel.HandQty;
                        }
                    }
                }

                if (itemcell.OutDetailModelList.Sum(o => o.Qty ?? 0) == 0)
                {
                    result.state = ResultType.error;
                    result.message = "单据出库数量不可为0";
                    return result;
                }


                IList<ContainerDetailModel> containerDetailWithOutLot = itemcell.ContainerDetailModelList;//物料相同，但批次无关的库存

                if (string.IsNullOrEmpty(itemcell.Lot)) //无批次
                {
                    itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.Where(o => string.IsNullOrEmpty(o.Lot)).ToList();
                }
                else
                {
                    itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.Where(o => o.Lot == itemcell.Lot).ToList();
                }

                if (itemcell.ContainerDetailModelList.Count == 0)
                {
                    result.state = ResultType.error;
                    result.message = "没有可用库存:物料名称" + itemcell.ItemName + ",批号" + itemcell.Lot;
                    return result;
                }

                if (!string.IsNullOrEmpty(itemcell.SourceInOrderCode)) //出库时指定了入库单
                {
                    itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.Where(o => o.ERPInDocCode == itemcell.SourceInOrderCode).ToList();

                    if (itemcell.ContainerDetailModelList.Count == 0)
                    {
                        result.state = ResultType.error;
                        result.message = "出库单指定的入库单没有可用库存:物料名称" + itemcell.ItemName + ",批号" + itemcell.Lot;
                        return result;
                    }
                }

                //判断库存是否充足
                if (isMustFull)
                {
                    //总的可用库存(匹配批次) = 总库存 - （手动选择的容器的所有库存 - 手动选择的容器的出库数量）
                    decimal? qtyAll = itemcell.ContainerDetailModelList.Sum(o => o.Qty ?? 0) - (itemcell.ContainerDetailModelList.Where(o => o.HandQty > 0).Sum(o => o.Qty ?? 0) - itemcell.ContainerDetailModelList.Where(o => o.HandQty > 0).Sum(o => o.HandQty));
                    decimal? needOutQty = itemcell.OutDetailModelList.Sum(o => o.Qty ?? 0);
                    if (qtyAll < needOutQty)
                    {
                        result.state = ResultType.error;
                        result.message = $"物料 [ { itemcell.ItemName } ] 库存不足";
                        return result;
                    }
                }

                //领料出库的保留库存
                if (itemcell.OutBoundType == "GetItemOut")
                {
                    IList<OutBoundDetailModel> UnUrgentList = itemcell.OutDetailModelList.Where(o => o.IsUrgent != "true").ToList(); //非紧急出库
                    if (UnUrgentList.Count > 0)
                    {
                        decimal minQty = db.FindEntity<T_ItemEntity>(o => o.F_Id == itemcell.ItemID).MinQty ?? 0; //保留库存数量 
                        if (minQty > 0)
                        {
                            decimal allQty = containerDetailWithOutLot.Sum(o => o.Qty ?? 0); //同物料  全库总数量
                            decimal needOutQty = itemGroupList.Where(o => o.ItemID == itemcell.ItemID).Sum(o => o.OutDetailModelList.Sum(i => i.Qty ?? 0)); //本波次所有该物料应出总数量

                            if (allQty - minQty < needOutQty)
                            {
                                result.state = ResultType.error;
                                result.message = $"物料 [ { itemcell.ItemName } ] 库存不足,总库存{allQty}，除去需保留{minQty},实际可用{allQty - minQty},但需出库{needOutQty}";
                                return result;
                            }
                        }
                    }
                }


                if (itemcell.ContainerKind == "Box") //如果是纸箱，则整箱优先
                {

                    decimal? outQtyNoHand = itemcell.ContainerDetailModelList.Where(o => o.HandQty > 0).Sum(o => o.HandQty);//手动选择的总库存
                    decimal? autoOutQty = itemcell.Qty - outQtyNoHand; //需要自动出库的出库数量
                    if (autoOutQty % itemcell.UnitQty == 0) //可以整箱出库，则去除散箱
                    {
                        decimal? qtyWithOutScattered = itemcell.ContainerDetailModelList.Where(o => o.HandQty == 0 && o.Qty == itemcell.UnitQty).Sum(o => o.Qty); //排除手动指定后，整箱的总库存数量
                        if (qtyWithOutScattered + outQtyNoHand >= itemcell.Qty) //去除散箱的库存，依旧满足总出库需求,则全部取整箱
                        {
                            itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.Where(o => o.HandQty > 0 || o.Qty == itemcell.UnitQty).ToList();
                        }
                        itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.OrderByDescending(o => o.HandQty ?? 0).ThenBy(o => o.Lot).ThenBy(o => o.Qty ?? 0).ThenBy(o => o.F_CreatorTime).ToList();
                    }
                    else //需要出零散
                    {
                        decimal? scatteredOutQty = autoOutQty % itemcell.UnitQty; //零散需求的数量
                        decimal? qtyScattered = itemcell.ContainerDetailModelList.Where(o => o.HandQty == 0 && o.Qty < itemcell.UnitQty).Sum(o => o.Qty); //散箱总库存数量
                        if (scatteredOutQty <= qtyScattered) //散箱库存满足出库零散数量
                        {
                            IList<ContainerDetailModel> scatteredList = itemcell.ContainerDetailModelList.Where(o => o.HandQty == 0 && o.Qty < itemcell.UnitQty).OrderBy(o => o.Qty).ToList(); //散箱优先

                            decimal? readyOut = 0;
                            foreach (ContainerDetailModel cell in scatteredList)
                            {
                                if (readyOut < scatteredOutQty)
                                {
                                    if (cell.Qty <= scatteredOutQty - readyOut) //取完
                                    {
                                        cell.HandQty = cell.Qty;
                                    }
                                    else
                                    {
                                        cell.HandQty = scatteredOutQty - readyOut;
                                    }

                                    readyOut = readyOut + cell.HandQty;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            //仅保留被选中的零散库存和整箱库存 
                            itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.Where(o => o.HandQty > 0 || o.Qty == itemcell.UnitQty).ToList();
                        }

                        itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.OrderByDescending(o => o.HandQty ?? 0).ThenBy(o => o.Lot).ThenBy(o => o.Qty ?? 0).ThenBy(o => o.F_CreatorTime).ToList();
                    }
                }
                else
                {
                    //所有当前物料的可用库存,策略：手动优先，其次批号先进先出，其次散件优先
                    itemcell.ContainerDetailModelList = itemcell.ContainerDetailModelList.OrderByDescending(o => o.HandQty ?? 0).ThenBy(o => o.Lot).ThenBy(o => o.Qty ?? 0).ThenBy(o => o.F_CreatorTime).ToList();
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
                    foreach (ContainerDetailModel cdModel in itemcell.ContainerDetailModelList)
                    {
                        if (outBoundDetailModel.Qty > readyQty)
                        {
                            decimal curQty = 0;
                            bool isAuto = true;
                            if (cdModel.HandQty > 0)
                            {
                                isAuto = false;
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

                            if (itemcell.ContainerKind == "Box")
                            {
                                bool isBoxMustWhole = true; //纸箱是否只允许整箱出库，此处默认是
                                if (isBoxMustWhole)
                                {
                                    if (curQty != cdModel.Qty) //当前应拣数量不等于当前纸箱库存数量
                                    {
                                        result.state = ResultType.error;
                                        result.message = $"物料 [ { itemcell.ItemName } ] 为纸箱，但存在不为整箱的出库";
                                        return result;
                                    }
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
                            outRecord.ItemBarCode = cdModel.ItemBarCode;
                            outRecord.ERPHouseCode = cdModel.ERPWarehouseCode;
                            outRecord.OldQty = cdModel.Qty;
                            outRecord.NeedQty = curQty;
                            outRecord.IsAuto = isAuto ? "true" : "false";
                            outRecord.PickedQty = 0;
                            outRecord.AfterQty = 0;
                            outRecord.WaveID = waveEntity.F_Id;
                            outRecord.WaveCode = waveEntity.WaveCode;
                            outRecord.WaveDetailID = waveDetail.F_Id;
                            outRecord.ItemUnitText = cdModel.ItemUnitText;
                            outRecord.IsItemMark = cdModel.IsItemMark;
                            outRecord.Factory = cdModel.Factory;
                            outRecord.ProductDate = cdModel.ProductDate;
                            outRecord.ValidityDayNum = cdModel.ValidityDayNum;
                            outRecord.OverdueDate = cdModel.OverdueDate;
                            outRecord.TransState = "WaittingTrans";
                            string outBoundType = outBoundDetailModel.OutBoundType;
                            switch (outBoundType)
                            {
                                case "GetItemOut":
                                    {
                                        outRecord.OutBoundType = "GetItemOut";
                                    }
                                    break;
                                case "VerBackOut":
                                    {
                                        outRecord.OutBoundType = "VerBackOut";
                                    }
                                    break;
                                case "WarehouseBackOut":
                                    {
                                        outRecord.OutBoundType = "WarehouseBackOut";
                                    }
                                    break;
                                case "OtherOut":
                                    {
                                        outRecord.OutBoundType = "OtherOut";
                                    }
                                    break;
                            }

                            outRecord.Lot = cdModel.Lot;
                            outRecord.Spec = cdModel.Spec;
                            outRecord.Price = 0;
                            outRecord.SupplierID = cdModel.SupplierID;
                            outRecord.SupplierCode = cdModel.SupplierCode;
                            outRecord.SupplierName = cdModel.SupplierName;
                            outRecord.ReceiveRecordID = cdModel.ReceiveRecordID;
                            outRecord.IsSpecial = cdModel.IsSpecial;
                            outRecord.State = "New";
                            outRecord.OrderCode = outBoundDetailModel.OutBoundCode;
                            outRecord.PickDate = null;
                            outRecord.F_CreatorTime = DateTime.Now;
                            if (OperatorProvider.Provider.GetCurrent() != null)
                            {
                                outRecord.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                                outRecord.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                            }
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
                        outDetailBound.State = "Waved";
                        outDetailBound.ActionType = "Equ";
                        db.Update<T_OutBoundDetailEntity>(outDetailBound);
                        break;
                    }
                }
            }

            db.SaveChanges();

            IList<T_OutBoundEntity> outBoundList = db.FindList<T_OutBoundEntity>(o => outBoundIDList.Contains(o.F_Id)).ToList();
            foreach (T_OutBoundEntity ob in outBoundList)
            {
                IList<T_OutBoundDetailEntity> detailList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == ob.F_Id && o.State == "New").ToList();
                if (detailList.Count > 0)
                {
                    ob.State = "WavedPart";
                }
                else
                {
                    ob.State = "Waved";
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
                string curOrderType = "";
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

                    string outType = firstOutOrder.OutBoundType;
                    if ("GetItemOut" == outType)
                    {
                        task.TaskType = "TaskType_GetItemOut";
                        task.OrderType = "GetItemOut";
                    }
                    else if ("VerBackOut" == outType)
                    {
                        task.TaskType = "TaskType_VerBackOut";
                        task.OrderType = "VerBackOut";
                    }
                    else if ("WarehouseBackOut" == outType)
                    {
                        task.TaskType = "TaskType_WarehouseBackOut";
                        task.OrderType = "WarehouseBackOut";
                    }
                    else if ("OtherOut" == outType)
                    {
                        task.TaskType = "TaskType_OtherOut";
                        task.OrderType = "OtherOut";
                    }
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
                    task.Level = 20;
                    task.State = "New";
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

                    task.F_CreatorTime = DateTime.Now;
                    task.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                    task.F_DeleteMark = false;
                    task.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;

                    IList<T_OutRecordEntity> outReceordTempList = outRecordList.Where(o => o.OutBoundDetailID == task.OrderDetailID && o.BarCode == task.BarCode).ToList();//同单据不同明细，公用一个容器的情况
                    foreach (T_OutRecordEntity outcell in outReceordTempList)
                    {
                        outcell.TaskID = task.F_Id;
                        outcell.TaskNo = task.TaskNo;
                    }

                    taskInsertList.Add(task);

                    curOrderType = task.OrderType;
                }
                else
                {
                    curOrderType = taskInDB.OrderType;
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
                    string taskOrderType = curOrderType;
                    switch (taskOrderType)
                    {
                        case "GetItemOut":
                            {
                                taskDetailEntity.OrderType = "GetItemOut";
                            }
                            break;
                        case "VerBackOut":
                            {
                                taskDetailEntity.OrderType = "VerBackOut";
                            }
                            break;
                        case "WarehouseBackOut":
                            {
                                taskDetailEntity.OrderType = "WarehouseBackOut";
                            }
                            break;
                        case "OtherOut":
                            {
                                taskDetailEntity.OrderType = "OtherOut";
                            }
                            break;
                        default:
                            {
                                throw new Exception("任务明细类型未知");
                            }
                    }
                    taskDetailEntity.WaveCode = waveEntity.WaveCode;
                    taskDetailEntity.WaveID = waveEntity.F_Id;
                    taskDetailEntity.IsOver = "false";
                    taskDetailEntity.WaveDetailID = outcell.WaveDetailID;
                    taskDetailEntity.ContainerDetailID = outcell.ContainerDetailID;
                    taskDetailEntity.ItemBarCode = outcell.ItemBarCode;

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
                T_TaskEntity taskEntity = taskInsertList.FirstOrDefault(o => o.SrcLocationID == loccell.F_Id);
                string outType = taskEntity.OrderType;
                locStateApp.SyncLocState(db, loccell, "OutType", outType, "Stored", "Out", taskEntity.TaskNo);
            }

            int count1 = db.Insert<T_OutRecordEntity>(outRecordList);
            int count2 = db.Update<T_ContainerDetailEntity>(containerDetailListInDB);
            int count3 = db.Insert<T_TaskEntity>(taskInsertList);
            int count4 = db.Insert<T_TaskDetailEntity>(taskDetailList);
            int count5 = db.Update<T_LocationEntity>(locList);

            result.state = ResultType.success;
            return result;
        }



        private T_WaveEntity AddWave(IRepositoryBase db, string waveType, IList<T_OutBoundDetailEntity> outBoundDetailEntitiesList)
        {
            T_WaveEntity waveEntity = new T_WaveEntity();
            waveEntity.F_Id = Guid.NewGuid().ToString();
            waveEntity.WaveCode = T_CodeGenApp.GenNum("WaveRule");
            waveEntity.WaveType = waveType.ToString();
            waveEntity.State = "New";
            waveEntity.OrderType = "OutOrder";
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

        private class ItemGroup
        {
            public string ItemID { get; set; }
            public string ItemCode { get; set; }
            public string Lot { get; set; }

            public string ItemName { get; set; }

            public decimal? UnitQty { get; set; }

            public string OutBoundType { get; set; }

            public string ContainerKind { get; set; }

            /// <summary>
            /// 出库单对应的入库单编码（ERP给定）
            /// </summary>
            public string SourceInOrderCode { get; set; }
            /// <summary>
            /// 所有单据当前物料的总需求库存
            /// </summary>
            public decimal Qty { get; set; }

            /// <summary>
            /// 验退单对应的入库单ID
            /// </summary>
            public string PointInBoundID { get; set; }

            public IList<OutBoundDetailModel> OutDetailModelList { get; set; }

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

        #region 执行波次并发送任务
        /// <summary>
        /// 执行波次并发送任务
        /// </summary>
        /// <param name="db"></param>
        /// <param name="outBoundDetailIDList"></param>
        /// <returns></returns>
        public AjaxResult OutDetail_ExecTaskAndSendWCS(IRepositoryBase db, IList<string> outBoundDetailIDList)
        {
            AjaxResult rst = new AjaxResult();
            //更改单据状态(以出库单明细为基准)
            IList<string> waveIDList = db.FindList<T_WaveDetailEntity>(o => outBoundDetailIDList.Contains(o.OutBoundDetailID)).Select(o => o.WaveID).Distinct().ToArray();
            IList<T_WaveEntity> waveList = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && o.State == "New");
            IList<string> needSendWCS = new List<string>();
            foreach (T_WaveEntity wave in waveList)
            {
                IList<string> outBoundListTemp = db.FindList<T_WaveDetailEntity>(o => o.WaveID == wave.F_Id && outBoundDetailIDList.Contains(o.OutBoundDetailID)).Select(o => o.OutBoundID).Distinct().ToArray();
                IList<T_OutBoundEntity> outBoundList = db.FindList<T_OutBoundEntity>(o => outBoundListTemp.Contains(o.F_Id)).ToList();

                foreach (T_OutBoundEntity outB in outBoundList)
                {
                    string stationOrderType;
                    if (outB.OutBoundType == "GetItemOut") //领料出库
                    {
                        stationOrderType = "GetItemOut";
                    }
                    else if (outB.OutBoundType == "VerBackOut") //验退出库
                    {
                        stationOrderType = "VerBackOut";
                    }
                    else if (outB.OutBoundType == "WarehouseBackOut") //仓退出库
                    {
                        stationOrderType = "WarehouseBackOut";
                    }
                    else if (outB.OutBoundType == "OtherOut") //其它出库
                    {
                        stationOrderType = "OtherOut";
                    }
                    else
                    {
                        rst.state = ResultType.error;
                        rst.message = "未知的单据类型";
                        return rst;
                    }

                    outB.State = "Outing";
                    db.Update<T_OutBoundEntity>(outB);

                    IList<T_StationEntity> workingStationList = new List<T_StationEntity>();
                    IList<T_WaveDetailEntity> waveDetailList = db.FindList<T_WaveDetailEntity>(o => o.OutBoundID == outB.F_Id && o.WaveID == wave.F_Id && outBoundDetailIDList.Contains(o.OutBoundDetailID));
                    if (waveDetailList.Count < 1)
                    {
                        rst.state = ResultType.error;
                        rst.message = "波次明细不存在";
                        return rst;
                    }

                    foreach (T_WaveDetailEntity wDetail in waveDetailList)
                    {

                        T_OutBoundDetailEntity outDetail = db.FindEntity<T_OutBoundDetailEntity>(o => o.F_Id == wDetail.OutBoundDetailID);
                        outDetail.State = "Outing";
                        db.Update<T_OutBoundDetailEntity>(outDetail);


                        T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.F_Id == outDetail.StationID);

                        stationEntity.CurOrderID = outB.F_Id;
                        stationEntity.OrderType = stationOrderType.ToString();
                        stationEntity.WaveID = wave.F_Id;
                        db.Update<T_StationEntity>(stationEntity);
                        db.SaveChanges();

                        IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => wDetail.OutBoundDetailID == o.OrderDetailID && wDetail.F_Id == o.WaveDetailID).ToList();
                        foreach (T_TaskEntity task in taskList)
                        {
                            needSendWCS.Add(task.TaskNo);
                            task.IsCanExec = "true";
                            db.Update<T_TaskEntity>(task);

                            IList<T_OutRecordEntity> outRecList = db.FindList<T_OutRecordEntity>(o => o.TaskID == task.F_Id);
                            foreach (T_OutRecordEntity outR in outRecList)
                            {
                                outR.State = "WaitPick";
                                db.Update<T_OutRecordEntity>(outR);
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
                rst.message = "没有库存产生任务";
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
                    loc.State = "Stored";
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
                        if (OutDetail.WaveQty == 0)
                        {
                            OutDetail.State = "New";
                            OutDetail.ActionType = "Init";
                        }
                        db.Update<T_OutBoundDetailEntity>(OutDetail);
                    }
                    else
                    {
                        if (OutDetail.State != "New")
                        {
                            isNewOrder = false;
                        }
                    }
                }

                if (isNewOrder)
                {
                    outBound.State = "New";
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


        #region RF提交出库拣选信息，纸箱、料箱、料架的拣选 （纸箱关闭单据并清空站台，料箱、料架 关闭单据但不清空站台,纸箱的整箱拣选在出库任务完成时自动拣选,此处为非整箱拣选）
        /// <summary>
        /// 纸箱、料箱、料架 拣选（纸箱整箱出库，在出库任务完成时调用，纸箱散箱出库、料箱、料架 在RF上调用）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="stationID">当前站台ID</param>
        /// <param name="barCode">容器条码</param>
        /// <param name="outRecID">当前拣选ID</param>
        /// <param name="itemBarCode">子条码，不贴标情况下为空</param>
        /// <param name="needQty">需出库数量</param>
        /// <param name="NoPickTimes">当前容器剩余需要拣选的次数(同容器多次拣选)</param>
        /// <param name="OverPickQty">当前容器已拣选数量</param>
        /// <param name="AllNeedQty">当前容器总需出库数量</param>
        /// <returns></returns>
        public AjaxResult PickRecord(IRepositoryBase db, string stationID, string barCode, string outRecID, string itemBarCode, decimal? needQty, ref int NoPickTimes, ref decimal? OverPickQty, ref decimal? AllNeedQty)
        {

            AjaxResult res = new AjaxResult();
            try
            {
                /*************************************************/
                if (string.IsNullOrEmpty(barCode))
                {
                    res.state = ResultType.error;
                    res.message = "箱码不能为空";
                    return res;
                }
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == stationID);
                T_StationEntity beforeChangeStateion = station.ToObject<T_StationEntity>(); //更改前的站台
                string CurOrderID = station.CurOrderID;

                T_OutRecordEntity outRec = db.FindEntity<T_OutRecordEntity>(o => o.F_Id == outRecID);
                T_OutBoundDetailEntity outBoundDetailEntity = db.FindEntity<T_OutBoundDetailEntity>(o => o.F_Id == outRec.OutBoundDetailID);
                T_OutBoundEntity outBoundEntity = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == station.CurOrderID);

                /// 根据容器判断回库区域
                FixType.Area areaEnum;
                T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == outRec.ContainerID && o.F_DeleteMark == false);
                if (containerEntity.ContainerKind == "Rack")
                {
                    areaEnum = FixType.Area.BigItemArea;
                }
                else if (containerEntity.ContainerKind == "Box" || containerEntity.ContainerKind == "Plastic")
                {
                    areaEnum = FixType.Area.NormalArea;
                }
                else
                {
                    res.state = ResultType.error;
                    res.message = "未知的容器大类";
                    return res;
                }

                /// 非纸箱，更换标签条码
                if (containerEntity.ContainerKind != "Box")
                {
                    List<T_OutRecordEntity> outRecordList = db.FindList<T_OutRecordEntity>(o => o.BarCode == barCode && o.OutBoundID == CurOrderID);
                    List<T_OutRecordEntity> PickingList = outRecordList.Where(o => o.State == "Picking").ToList();

                    if (outRec.IsItemMark == "true") /// 必须扫码
                    {
                        if (string.IsNullOrEmpty(itemBarCode))
                        {
                            res.state = ResultType.error;
                            res.message = "子码不能为空";
                            return res;
                        }
                        if (RuleConfig.OutConfig.RFScanCode.IsItemBarCodeSame) /// 扫码必须一致
                        {
                            if (outRec.ItemBarCode != itemBarCode)
                            {
                                res.state = ResultType.error;
                                res.message = "子码不正确";
                                return res;
                            }
                            /// 是否在出库记录内
                            outRec = outRecordList.FirstOrDefault(o => o.ItemBarCode == itemBarCode); /// 存在拣选记录-----用当前子码替换拣选记录
                            if (outRec == null)
                            {
                                res.state = ResultType.error;
                                res.message = "子码不正确";
                                return res;
                            }
                        }
                        else /// 允许不一致
                        {
                            T_ContainerDetailEntity cdNew = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == barCode && o.ItemBarCode == itemBarCode);
                            if (cdNew == null)
                            {
                                res.state = ResultType.error;
                                res.message = "当前容器不存在该子码";
                                return res;
                            }

                            T_ContainerDetailEntity cdOld = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == outRec.ContainerDetailID);
                            /// 库存明细不一致，更换标签条码
                            if (cdOld.F_Id != cdNew.F_Id)
                            {
                                /// 是否在出库记录中
                                outRec = outRecordList.FirstOrDefault(o => o.ItemBarCode == cdNew.ItemBarCode); /// 存在拣选记录----用当前子码替换拣选记录
                                if (outRec == null)   /// 子码不在Picking记录表中，用库存替代
                                {
                                    /// 物料+批号 不一致
                                    if (!(cdNew.ItemID == cdOld.ItemID && (cdNew.Lot == cdOld.Lot || (string.IsNullOrEmpty(cdNew.Lot) && string.IsNullOrEmpty(cdOld.Lot)))))
                                    {
                                        res.state = ResultType.error;
                                        res.message = "子码物料不是拣选物料";
                                        return res;
                                    }
                                    if (cdNew.IsCheckFreeze == "true" || cdNew.IsCountFreeze == "true" || cdNew.State == "Freeze")
                                    {
                                        res.state = ResultType.error;
                                        res.message = "子码状态冻结";
                                        return res;
                                    }

                                    T_OutRecordEntity canRec = new T_OutRecordEntity();
                                    List<T_OutRecordEntity> canPickList = PickingList.Where(o => o.ItemID == cdNew.ItemID && (o.Lot == cdNew.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(cdNew.Lot))) && o.IsAuto == "true").ToList();
                                    if (canPickList.Count == 0)
                                    {
                                        res.state = ResultType.error;
                                        res.message = "子码不可替代拣选";
                                        return res;
                                    }
                                    else
                                    {
                                        /// 判断数量，最优拣选
                                        decimal? minQty = decimal.MaxValue;
                                        decimal? diffQty = 0;
                                        foreach (T_OutRecordEntity rec in canPickList)
                                        {
                                            if (cdNew.Qty < rec.NeedQty) continue;
                                            diffQty = cdNew.Qty - rec.NeedQty;
                                            if (minQty > diffQty)
                                            {
                                                minQty = diffQty;
                                                canRec = rec;
                                            }
                                        }
                                    }

                                    if (canRec == null)
                                    {
                                        res.state = ResultType.error;
                                        res.message = "子码标签数量不足";
                                        return res;
                                    }
                                    outRec = canRec;

                                    /// 用新库存替换原拣选信息
                                    cdNew.OutQty = (cdNew.OutQty ?? 0) + (outRec.NeedQty ?? 0);
                                    cdOld.OutQty = (cdOld.OutQty ?? 0) - (outRec.NeedQty ?? 0);

                                    outRec.ItemBarCode = cdNew.ItemBarCode;
                                    outRec.ReceiveRecordID = cdNew.ReceiveRecordID;
                                    outRec.ContainerDetailID = cdNew.F_Id;
                                    outRec.OldQty = cdNew.Qty;

                                    db.Update<T_ContainerDetailEntity>(cdNew);
                                    db.Update<T_ContainerDetailEntity>(cdOld);
                                    db.Update<T_OutRecordEntity>(outRec);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    else
                    {
                        //什么也不做
                    }
                }

                if (outRec.State == "OverPick")
                {
                    res.state = ResultType.error;
                    res.message = "当前子码已拣选";
                    return res;
                }

                if (needQty == null || needQty == 0)
                {
                    needQty = outRec.NeedQty;    /// 未返回前端本次数量时
                }
                outRec.State = "OverPick";
                outRec.PickedQty = (outRec.PickedQty ?? 0) + needQty;
                outRec.AfterQty = outRec.OldQty - outRec.PickedQty;
                outRec.PickDate = DateTime.Now;
                outRec.PickUserID = OperatorProvider.Provider.GetCurrent().UserId;
                outRec.PickUserName = OperatorProvider.Provider.GetCurrent().UserName;

                db.Update<T_OutRecordEntity>(outRec);

                outBoundDetailEntity.OutQty = (outBoundDetailEntity.OutQty ?? 0) + (needQty ?? 0);
                db.Update<T_OutBoundDetailEntity>(outBoundDetailEntity);

                T_ContainerDetailEntity containerDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == outRec.ContainerDetailID);
                containerDetail.Qty = (containerDetail.Qty ?? 0) - (needQty ?? 0);
                containerDetail.OutQty = (containerDetail.OutQty ?? 0) - (needQty ?? 0);

                string orderType;
                string transType;
                if (outBoundEntity.OutBoundType == "GetItemOut") //领料出库
                {
                    orderType = "GetItemOut";
                    transType = "GetItemOut";
                }
                else if (outBoundEntity.OutBoundType == "VerBackOut") //验退出库
                {
                    orderType = "VerBackOut";
                    transType = "VerBackOut";
                }
                else if (outBoundEntity.OutBoundType == "WarehouseBackOut") //仓退出库
                {
                    orderType = "WarehouseBackOut";
                    transType = "WarehouseBackOut";
                }
                else if (outBoundEntity.OutBoundType == "OtherOut") //其它出库
                {
                    orderType = "OtherOut";
                    transType = "OtherOut";
                }
                else
                {
                    res.state = ResultType.error;
                    res.message = "未知的单据类型";
                    return res;
                }
                db.SaveChanges();

                /// 库存流水
                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                inOutDetailApp.SyncInOutDetail(db, containerDetail, "OutType", orderType, outRec.OldQty, needQty, outRec.TaskNo);

                if (containerDetail.Qty == 0)
                {
                    db.Delete<T_ContainerDetailEntity>(containerDetail);
                }
                else
                {
                    db.Update<T_ContainerDetailEntity>(containerDetail);
                }

                db.SaveChanges();
                //根据站台单据类型，推断回库时的任务类型


                string stationOrderType = station.OrderType;
                T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == areaEnum.ToString());

                IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == containerDetail.BarCode).ToList();
                if (containerDetailList.Count() < 1) //已没有库存，空容器回库
                {
                    if (containerEntity.ContainerKind == "Box") //纸箱不用回库，但需关闭单据和清空站台
                    {
                        List<T_OutRecordEntity> noNeedBcakRecList = db.FindList<T_OutRecordEntity>(o => o.OutBoundID == outRec.OutBoundID && o.BarCode == outRec.BarCode); //纸箱，正常情况只有一条数据
                        foreach (T_OutRecordEntity cell in noNeedBcakRecList)
                        {
                            cell.IsNeedBackWare = "false";
                            db.Update<T_OutRecordEntity>(cell);

                            T_ContainerEntity con = db.FindEntity<T_ContainerEntity>(o => o.BarCode == cell.BarCode && o.F_DeleteMark == false);
                            con.F_DeleteMark = true;
                            db.Update<T_ContainerEntity>(con);
                        }

                        if (station.BarCode == barCode)
                        {
                            station.BarCode = "";
                        }

                        IList<T_OutRecordEntity> list = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == outRec.OutBoundDetailID && o.OutBoundID == outRec.OutBoundID && o.State != "OverPick");
                        if (list.Count < 1) //单据明细已拣完
                        {
                            IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完

                            if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                            {
                                //清空站台
                                station.CurOrderDetailID = "";
                                station.CurOrderID = "";
                                station.WaveID = "";
                                station.OrderType = "";
                            }
                        }
                        db.Update<T_StationEntity>(station);
                        db.SaveChanges();
                    }
                    else if (containerEntity.ContainerKind == "Plastic"
                             || containerEntity.ContainerKind == "Rack") //空容器入库（料箱 或 料架），产生任务，关闭单据，但不清空站台
                    {
                        List<T_OutRecordEntity> noNeedBcakRecList = db.FindList<T_OutRecordEntity>(o => o.OutBoundID == outRec.OutBoundID && o.BarCode == outRec.BarCode);
                        foreach (T_OutRecordEntity cell in noNeedBcakRecList)
                        {
                            cell.IsNeedBackWare = "false";
                            db.Update<T_OutRecordEntity>(cell);
                        }

                        T_TaskEntity taskBack = new T_TaskEntity();
                        taskBack.F_Id = Guid.NewGuid().ToString();
                        taskBack.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                        taskBack.TaskInOutType = "InType";
                        taskBack.ContainerID = containerEntity.F_Id;
                        taskBack.BarCode = containerEntity.BarCode;
                        taskBack.ContainerType = containerEntity.ContainerType;
                        taskBack.SrcLocationID = station.StationCode;
                        taskBack.SrcLocationCode = station.LeaveAddress;
                        taskBack.SrcWCSLocCode = station.LeaveAddress;
                        taskBack.TagAreaID = area.F_Id;
                        taskBack.TagLocationID = "";
                        taskBack.TagLocationCode = "";
                        taskBack.ApplyStationID = station.F_Id;
                        taskBack.WaveID = outRec.WaveID;
                        taskBack.WaveCode = outRec.WaveCode;
                        taskBack.SEQ = outBoundDetailEntity.SEQ;
                        taskBack.Level = 40;
                        taskBack.State = "New";
                        taskBack.IsWcsTask = "true";
                        taskBack.SendWCSTime = null;
                        taskBack.OrderType = "EmptyIn";
                        taskBack.OrderID = outBoundDetailEntity.OutBoundID;
                        taskBack.OrderDetailID = outBoundDetailEntity.F_Id;
                        taskBack.OrderCode = outBoundEntity.OutBoundCode;
                        taskBack.OverTime = null;
                        taskBack.TaskType = "TaskType_EmptyIn";
                        taskBack.IsCanExec = "true";

                        db.Insert<T_TaskEntity>(taskBack);
                    }
                    else
                    {
                        res.state = ResultType.error;
                        res.message = "容器类型未知";
                        return res;
                    }
                }
                else //还有剩余库存
                {
                    IList<T_OutRecordEntity> outRecordList = db.FindList<T_OutRecordEntity>(o => o.BarCode == outRec.BarCode && o.OutBoundDetailID == outRec.OutBoundDetailID && o.State == "Picking").ToList();
                    if (outRecordList.Count > 0) //同明细没有拣完，不回库，继续拣选(纸箱、料箱、料架均适用)
                    {
                        //什么也不做
                    }
                    else
                    {
                        IList<T_OutRecordEntity> outRecordAllList = db.FindList<T_OutRecordEntity>(o => o.BarCode == outRec.BarCode && o.OutBoundID == outRec.OutBoundID && o.State == "Picking").ToList();
                        if (outRecordList.Count > 0) //同单不同明细没有拣完，不回库，继续拣选(纸箱、料箱、料架均适用)
                        {
                            //更新站台当前单据明细信息
                            station.CurOrderDetailID = outRecordAllList.FirstOrDefault().OutBoundDetailID;
                            db.Update<T_StationEntity>(station);
                        }
                        else
                        {
                            //产生余料回库任务，(纸箱、料箱、料架均适用)
                            string taskBackType;
                            string taskOrderType;
                            switch (stationOrderType)
                            {
                                case "GetItemOut": // 领料出库
                                    {
                                        taskBackType = "TaskType_GetItemBack";
                                        taskOrderType = "GetItemOut";
                                    }
                                    break;
                                case "WarehouseBackOut"://仓退出库
                                    {
                                        taskBackType = "TaskType_WarehouseBackIn";
                                        taskOrderType = "WarehouseBackOut";
                                    }
                                    break;
                                case "VerBackOut": //验退出库
                                    {
                                        taskBackType = "TaskType_VerBackIn";
                                        taskOrderType = "VerBackOut";
                                    }
                                    break;
                                case "OtherOut": //其它出库
                                    {
                                        taskBackType = "TaskType_OtherIn";
                                        taskOrderType = "OtherOut";
                                    }
                                    break;
                                default:
                                    {
                                        res.state = ResultType.error;
                                        res.message = "单据类型未知";
                                        return res;
                                    }
                            }

                            T_TaskEntity taskBack = new T_TaskEntity();
                            taskBack.F_Id = Guid.NewGuid().ToString();
                            taskBack.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                            taskBack.TaskInOutType = "InType";
                            taskBack.ContainerID = containerEntity.F_Id;
                            taskBack.BarCode = containerEntity.BarCode;
                            taskBack.ContainerType = containerEntity.ContainerType;
                            taskBack.SrcLocationID = station.StationCode;
                            taskBack.SrcLocationCode = station.LeaveAddress;
                            taskBack.SrcWCSLocCode = station.LeaveAddress;
                            taskBack.TagAreaID = area.F_Id;
                            taskBack.TagLocationID = "";
                            taskBack.TagLocationCode = "";
                            taskBack.ApplyStationID = station.F_Id;
                            taskBack.WaveID = outRec.WaveID;
                            taskBack.WaveCode = outRec.WaveCode;
                            taskBack.SEQ = outBoundDetailEntity.SEQ;
                            taskBack.Level = 40;
                            taskBack.State = "New";
                            taskBack.IsWcsTask = "true";
                            taskBack.SendWCSTime = null;
                            taskBack.OrderType = taskOrderType.ToString();
                            taskBack.OrderID = outBoundDetailEntity.OutBoundID;
                            taskBack.OrderDetailID = outBoundDetailEntity.F_Id;
                            taskBack.OrderCode = outBoundEntity.OutBoundCode;
                            taskBack.OverTime = null;
                            taskBack.TaskType = taskBackType.ToString();
                            taskBack.IsCanExec = "true";

                            db.Insert<T_TaskEntity>(taskBack);

                            List<T_OutRecordEntity> noNeedBcakRecList = db.FindList<T_OutRecordEntity>(o => o.OutBoundID == outRec.OutBoundID && o.BarCode == outRec.BarCode);
                            foreach (T_OutRecordEntity cell in noNeedBcakRecList)
                            {
                                cell.IsNeedBackWare = "false";
                                db.Update<T_OutRecordEntity>(cell);
                            }
                        }
                    }
                }

                db.SaveChanges();



                //判断单据是否完成
                IList<T_OutRecordEntity> noPickOutRecordList = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == outRec.OutBoundDetailID && o.State != "OverPick").ToList();
                if (noPickOutRecordList.Count < 1) //当前单据明细已拣选完毕
                {
                    outBoundDetailEntity.State = "Over";
                    db.Update<T_OutBoundDetailEntity>(outBoundDetailEntity);
                    db.SaveChanges();

                    //关闭波次
                    T_WaveEntity wave = db.IQueryable<T_WaveEntity>().Join(db.IQueryable<T_WaveDetailEntity>(), m => m.F_Id, n => n.WaveID, (m, n) => new { wave = m, waveDetail = n }).Where(o => o.wave.State == "Execing" && o.waveDetail.OutBoundDetailID == outRec.OutBoundDetailID).Select(o => o.wave).FirstOrDefault();
                    if (wave == null)
                    {
                        res.state = ResultType.error;
                        res.message = "波次未找到";
                        return res;
                    }

                    IList<T_WaveDetailEntity> waveDetail = db.FindList<T_WaveDetailEntity>(o => o.WaveID == wave.F_Id);
                    if (waveDetail.Count < 1)
                    {
                        res.state = ResultType.error;
                        res.message = "波次明细未找到";
                        return res;
                    }
                    string[] allOrderDetailID = waveDetail.Select(o => o.OutBoundDetailID).ToArray();
                    IList<T_OutBoundDetailEntity> noOverOutBoundDetailList = db.FindList<T_OutBoundDetailEntity>(o => allOrderDetailID.Contains(o.F_Id) && o.State != "Over").ToList();
                    if (noOverOutBoundDetailList.Count < 1) //当前波次所有单据明细已经完成
                    {
                        T_WaveEntity wavedb = db.FindEntity<T_WaveEntity>(o => o.F_Id == wave.F_Id);
                        wavedb.State = "Over";
                        db.Update<T_WaveEntity>(wavedb);
                        db.SaveChanges();
                    }

                    IList<T_OutBoundDetailEntity> noOverDetailList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == CurOrderID && o.State != "Over").ToList();
                    if (noOverDetailList.Count < 1) /// 单据明细已全部完成
                    {
                        outBoundEntity.State = "Over";
                        db.Update<T_OutBoundEntity>(outBoundEntity);
                        db.SaveChanges();

                        /// 产生过账信息，并发送过账信息
                        bool isOutOrderTrans = false;
                        switch (transType)
                        {
                            case "PurchaseIn":
                            case "BackSample":
                            case "GetSample":
                            case "Count":
                            case "LocCount":
                                break;
                            case "GetItemOut":
                                {
                                    if (RuleConfig.OrderTransRule.OutBoundTransRule.GetItemOutTrans) isOutOrderTrans = true;
                                }
                                break;
                            case "WarehouseBackOut":
                                {
                                    if (RuleConfig.OrderTransRule.OutBoundTransRule.WarehouseBackOutTrans) isOutOrderTrans = true;
                                }
                                break;
                            case "VerBackOut":
                                {
                                    if (RuleConfig.OrderTransRule.OutBoundTransRule.VerBackOutTrans) isOutOrderTrans = true;
                                }
                                break;
                            case "OtherOut":
                                {
                                    if (RuleConfig.OrderTransRule.OutBoundTransRule.OtherOutTrans) isOutOrderTrans = true;
                                }
                                break;
                            default:
                                break;
                        }

                        if (isOutOrderTrans)
                        {
                            if (outBoundEntity.GenType == "ERP")
                            {
                                AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, outBoundEntity.F_Id, transType);
                                if ((ResultType)rst.state == ResultType.success)
                                {
                                    T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                    ERPPost post = new ERPPost();
                                    ERPResult erpRst = post.PostFactInOutQty(db, transType, trans.F_Id);
                                }
                                else
                                {
                                    res.state = ResultType.error;
                                    res.message = "过账信息产生失败";
                                    return res;
                                }
                            }

                        }
                    }
                }



                db.SaveChanges();

                IList<T_OutRecordEntity> outRecAllPickList = db.FindList<T_OutRecordEntity>(o => o.BarCode == barCode && o.WaveID == outRec.WaveID && o.OutBoundID == outRec.OutBoundID).ToList();
                IList<T_OutRecordEntity> outRecNoPickList = outRecAllPickList.Where(o => o.State == "Picking" || o.State == "WaitPick").ToList();
                NoPickTimes = outRecNoPickList.Count();    // 剩余次数
                OverPickQty = outRecAllPickList.Sum(o => o.PickedQty);
                AllNeedQty = outRecAllPickList.Sum(o => o.NeedQty);

                res.state = ResultType.success;
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
