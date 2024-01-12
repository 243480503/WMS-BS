/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_OffLineDownApp
    {
        private IT_OffLineDownRepository service = new T_OffLineDownRepository();
        public IQueryable<T_OffLineDownEntity> FindList(Expression<Func<T_OffLineDownEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_OffLineDownEntity FindEntity(Expression<Func<T_OffLineDownEntity, bool>> predicate)
        {
            T_OffLineDownEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public List<T_OffLineDownEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_OffLineDownEntity> GetList(Pagination pagination)
        {
            return service.FindList(pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }

        public List<T_OffLineDownEntity> GetList(Pagination pagination, string keyword, string resultType)
        {
            var expression = ExtLinq.True<T_OffLineDownEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.BarCode.Contains(keyword) || t.ItemBarCode.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(resultType))
            {
                switch (resultType)
                {
                    case "1": //全部
                        {
                            //什么也不干
                        }
                        break;
                    case "2": //未处理
                        {
                            expression = expression.And(o => o.State == "New");
                        }
                        break;
                    case "3": //已处理
                        {
                            expression = expression.And(o => o.State == "Over");
                        }
                        break;
                    case "4": //已忽略
                        {
                            expression = expression.And(o => o.State == "NoNeed");
                        }
                        break;
                }
            }
            return service.FindList(expression, pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }

        public T_OffLineDownEntity FindEntityAsNoTracking(Expression<Func<T_OffLineDownEntity, bool>> predicate)
        {
            T_OffLineDownEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_OffLineDownEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_OffLineDownEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }

        public void Update(T_OffLineDownEntity itemsEntity)
        {
            service.Update(itemsEntity);
        }

        public void SubmitForm(T_OffLineDownEntity itemsEntity, string keyValue)
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

        /// <summary>
        /// 判断是否有未处理的离线数据
        /// </summary>
        /// <returns></returns>
        public bool IsHaveOff()
        {
            bool isHave = false;
            IList<T_OffLineDownEntity> list = service.IQueryableAsNoTracking(o => o.F_DeleteMark == false && o.State == "New").ToList();
            if (list.Count > 0)
            {
                isHave = true;
            }
            return isHave;
        }
        
        /// <summary>
        /// 判断是否有未处理的离线数据
        /// </summary>
        /// <returns></returns>
        public bool IsHaveOff(IRepositoryBase db)
        {
            bool isHave = false;
            IList<T_OffLineDownEntity> list =db.FindList<T_OffLineDownEntity>(o => o.F_DeleteMark == false && o.State == "New").ToList();
            if (list.Count > 0)
            {
                isHave = true;
            }
            return isHave;
        }

        public void Insert(T_OffLineDownEntity entity)
        {
            service.Insert(entity);
        }

        /// <summary>
        /// 扣减库存
        /// </summary>
        /// <param name="db"></param>
        /// <param name="offLineDownIDList">需要执行扣减的离线ID</param>
        /// <returns></returns>
        public AjaxResult OffLineDownSub(IRepositoryBase db, string[] offLineDownIDList)
        {
            AjaxResult res = new AjaxResult();
            IList<T_OffLineDownEntity> entityList = db.FindList<T_OffLineDownEntity>(o => offLineDownIDList.Contains(o.F_Id)).ToList();
            foreach (T_OffLineDownEntity cell in entityList)
            {
                if (cell.State != "New")
                {
                    res.message = "非新建状态不可操作：容器编码[" + cell.BarCode + "]";
                    res.state = ResultType.error.ToString();
                    return res;
                }

                if (string.IsNullOrEmpty(cell.BarCode))
                {
                    res.message = "容器编码不可为空";
                    res.state = ResultType.error.ToString();
                    return res;
                }

                T_ContainerDetailEntity firstContainerDetail = null;
                if (string.IsNullOrEmpty(cell.ItemBarCode)) //没有扫码子条码的处理
                {
                    IList<T_ContainerDetailEntity> conDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == cell.BarCode).ToList();
                    if (conDetailList.Count > 1)
                    {
                        res.message = "容器编码对应多个库存";
                        res.state = ResultType.error.ToString();
                        return res;
                    }
                    if (conDetailList.Count < 1)
                    {
                        res.message = "库存未找到";
                        res.state = ResultType.error.ToString();
                        return res;
                    }

                    firstContainerDetail = conDetailList[0];

                    if (firstContainerDetail.Qty < cell.Qty)
                    {
                        res.message = "库存数量不足：容器[" + cell.BarCode + "]";
                        res.state = ResultType.error.ToString();
                        return res;
                    }
                }
                else
                {
                    IList<T_ContainerDetailEntity> conDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == cell.BarCode && o.ItemBarCode == cell.ItemBarCode).ToList();
                    if (conDetailList.Count > 1)
                    {
                        res.message = "容器编码与子条码对应多个库存";
                        res.state = ResultType.error.ToString();
                        return res;
                    }
                    if (conDetailList.Count < 1)
                    {
                        res.message = "库存未找到";
                        res.state = ResultType.error.ToString();
                        return res;
                    }

                    firstContainerDetail = conDetailList[0];

                    if (firstContainerDetail.Qty < cell.Qty)
                    {
                        res.message = "库存数量不足：容器[" + cell.BarCode + ",子条码[" + cell.ItemBarCode + "]";
                        res.state = ResultType.error.ToString();
                        return res;
                    }
                }

                if (firstContainerDetail.ItemCode == FixType.Item.EmptyPlastic.ToString() || firstContainerDetail.ItemCode == FixType.Item.EmptyRack.ToString())
                {
                    res.message = "空容器不可操作";
                    res.state = ResultType.error.ToString();
                    return res;
                }


                T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == firstContainerDetail.LocationID);
                if (loc == null)
                {
                    res.message = "货位未找到";
                    res.state = ResultType.error.ToString();
                    return res;
                }

                if (loc.State != "Stored")
                {
                    res.message = "货位状态不为已存储";
                    res.state = ResultType.error.ToString();
                    return res;
                }

                decimal? oldQty = firstContainerDetail.Qty;
                if (cell.Qty == null || cell.Qty == 0) //不填数量则全部出
                {
                    cell.Qty = firstContainerDetail.Qty;
                }

                cell.ConDetailAreaCode = firstContainerDetail.AreaCode;
                cell.ConDetailAreaName = firstContainerDetail.AreaName;
                cell.ConDetailContainerDetailID = firstContainerDetail.F_Id;
                cell.ConDetailContainerKind = firstContainerDetail.ContainerKind;
                cell.ConDetailContainerType = firstContainerDetail.ContainerType;
                cell.ConDetailItemCode = firstContainerDetail.ItemCode;
                cell.ConDetailItemName = firstContainerDetail.ItemName;
                cell.ConDetailLocationCode = firstContainerDetail.LocationNo;
                cell.ConDetailLocationState = loc.State.ToString();
                cell.ConDetailLot = firstContainerDetail.Lot;
                cell.ConDetailPhoTime = DateTime.Now;
                cell.ConDetailQty = firstContainerDetail.Qty;
                cell.ConDetailState = firstContainerDetail.State;
                cell.ConDetailSupplierCode = firstContainerDetail.SupplierCode;
                cell.ConDetailSupplierName = firstContainerDetail.SupplierName;
                cell.State = "Over";
                db.Update<T_OffLineDownEntity>(cell);



                if (cell.Qty < firstContainerDetail.Qty) //该库存已满足出库
                {
                    firstContainerDetail.Qty = firstContainerDetail.Qty - cell.Qty;
                    db.Update<T_ContainerDetailEntity>(firstContainerDetail);
                    db.SaveChanges();
                }
                else if (cell.Qty == firstContainerDetail.Qty)// 捡完
                {
                    firstContainerDetail.Qty = firstContainerDetail.Qty - cell.Qty; //此处等于零表示已捡完
                    if (firstContainerDetail.ContainerKind == "Box") //纸箱
                    {
                        //空纸箱需拿走
                        loc.State = "Empty";
                        loc.OrderCode = "";
                        loc.OrderType = "";
                        db.Update<T_LocationEntity>(loc);

                        T_ContainerEntity con = db.FindEntity<T_ContainerEntity>(o => o.F_Id == firstContainerDetail.ContainerID && o.F_DeleteMark == false);
                        con.F_DeleteMark = true;
                        db.Update<T_ContainerEntity>(con);

                        db.Delete<T_ContainerDetailEntity>(firstContainerDetail);
                        db.SaveChanges();
                    }
                    else if (firstContainerDetail.ContainerKind == "Plastic") //新增空料箱库存
                    {
                        T_ItemEntity item = item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());

                        if (item == null)
                        {
                            res.message = "未找到容器物料";
                            res.state = ResultType.error.ToString();
                            return res;
                        }

                        loc.OrderCode = "";
                        loc.OrderType = "";
                        db.Update<T_LocationEntity>(loc);

                        T_ContainerDetailEntity detail = new T_ContainerDetailEntity();
                        detail.Create();
                        detail.ItemID = item.F_Id;
                        detail.ContainerID = firstContainerDetail.ContainerID;
                        detail.ContainerType = firstContainerDetail.ContainerType;
                        detail.ContainerKind = firstContainerDetail.ContainerKind;
                        detail.LocationID = firstContainerDetail.LocationID;
                        detail.LocationNo = firstContainerDetail.LocationNo;
                        detail.AreaID = firstContainerDetail.AreaID;
                        detail.AreaCode = firstContainerDetail.AreaCode;
                        detail.AreaName = firstContainerDetail.AreaName;
                        detail.KindCode = item.KindCode;
                        detail.KindName = item.KindName;
                        detail.ItemName = item.ItemName;
                        detail.ItemCode = item.ItemCode;
                        detail.BarCode = firstContainerDetail.BarCode;
                        detail.ItemBarCode = "";
                        detail.Qty = 1;
                        detail.OutQty = 0;
                        detail.CheckQty = 0;
                        detail.ItemUnitText = item.ItemUnitText;
                        detail.CheckState = "UnNeed";
                        detail.CheckDetailID = "";
                        detail.CheckID = "";
                        detail.State = "Normal";
                        detail.IsCheckFreeze = "false";
                        detail.IsCountFreeze = "false";
                        detail.Lot = "";
                        detail.Spec = "";
                        detail.ERPWarehouseCode = "";
                        detail.ProductDate = null;
                        detail.OverdueDate = null;
                        detail.SupplierID = "";
                        detail.SupplierCode = "";
                        detail.SupplierName = "";
                        detail.ReceiveRecordID = "";
                        detail.IsItemMark = "";
                        detail.F_DeleteMark = false;

                        db.Insert<T_ContainerDetailEntity>(detail);

                        db.Delete<T_ContainerDetailEntity>(firstContainerDetail);

                        db.SaveChanges();
                    }
                    else if (firstContainerDetail.ContainerKind == "Rack") //新增空料架
                    {
                        T_ItemEntity item = item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());


                        if (item == null)
                        {
                            res.message = "未找到容器物料";
                            res.state = ResultType.error.ToString();
                            return res;
                        }

                        loc.OrderCode = "";
                        loc.OrderType = "";
                        db.Update<T_LocationEntity>(loc);

                        T_ContainerDetailEntity detail = new T_ContainerDetailEntity();
                        detail.Create();
                        detail.ItemID = item.F_Id;
                        detail.ContainerID = firstContainerDetail.ContainerID;
                        detail.ContainerType = firstContainerDetail.ContainerType;
                        detail.ContainerKind = firstContainerDetail.ContainerKind;
                        detail.LocationID = firstContainerDetail.LocationID;
                        detail.LocationNo = firstContainerDetail.LocationNo;
                        detail.AreaID = firstContainerDetail.AreaID;
                        detail.AreaCode = firstContainerDetail.AreaCode;
                        detail.AreaName = firstContainerDetail.AreaName;
                        detail.KindCode = item.KindCode;
                        detail.KindName = item.KindName;
                        detail.ItemName = item.ItemName;
                        detail.ItemCode = item.ItemCode;
                        detail.BarCode = firstContainerDetail.BarCode;
                        detail.ItemBarCode = "";
                        detail.Qty = 1;
                        detail.OutQty = 0;
                        detail.CheckQty = 0;
                        detail.ItemUnitText = item.ItemUnitText;
                        detail.CheckState = "UnNeed";
                        detail.CheckDetailID = "";
                        detail.CheckID = "";
                        detail.State = "Normal";
                        detail.IsCheckFreeze = "false";
                        detail.IsCountFreeze = "false";
                        detail.Lot = "";
                        detail.Spec = "";
                        detail.ERPWarehouseCode = "";
                        detail.ProductDate = null;
                        detail.OverdueDate = null;
                        detail.SupplierID = "";
                        detail.SupplierCode = "";
                        detail.SupplierName = "";
                        detail.ReceiveRecordID = "";
                        detail.IsItemMark = "";
                        detail.F_DeleteMark = false;

                        db.Insert<T_ContainerDetailEntity>(detail);
                        db.Delete<T_ContainerDetailEntity>(firstContainerDetail);
                        db.SaveChanges();
                    }
                    else
                    {
                        res.message = "容器类型未知";
                        res.state = ResultType.error.ToString();
                        return res;
                    }
                }
                else
                {
                    res.message = "该容器库存数量不足";
                    res.state = ResultType.error.ToString();
                    return res;
                }

                //库存流水
                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                inOutDetailApp.SyncInOutDetail(db, firstContainerDetail, "OutType", "OffLineDown", oldQty, cell.Qty, "");
                db.SaveChanges();
            }


            res.state = ResultType.success.ToString();
            return res;
        }
    }
}
