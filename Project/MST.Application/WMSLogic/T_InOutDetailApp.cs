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
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_InOutDetailApp
    {
        private IT_InOutDetailRepository service = new T_InOutDetailRepository();
        public IQueryable<T_InOutDetailEntity> FindList(Expression<Func<T_InOutDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public List<T_InOutDetailEntity> FindList(Pagination pagination, string keyword)
        {
            var expression = ExtLinq.True<T_InOutDetailEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(o => o.ItemCode.Contains(keyword) || o.ItemName.Contains(keyword) || o.BarCode.Contains(keyword) || o.OrderCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }
        public T_InOutDetailEntity FindEntity(Expression<Func<T_InOutDetailEntity, bool>> predicate)
        {
            T_InOutDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_InOutDetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_InOutDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_InOutDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_InOutDetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_InOutDetailEntity entity)
        {
            service.Insert(entity);
        }

        /// <summary>
        /// 将库存插入库存表，并产生流水
        /// </summary>
        /// <param name="detail"></param>
        public void SyncInOutDetail(IRepositoryBase db, T_ContainerDetailEntity cDetail, string inOutType, string orderType, decimal? beforeQty, decimal? changeQty, string taskNo)
        {
            T_InOutDetailEntity inOutDetail = new T_InOutDetailEntity();
            inOutDetail.F_Id = Guid.NewGuid().ToString();

            inOutDetail.ContainerDetailID = cDetail.F_Id;
            inOutDetail.AreaID = cDetail.AreaID;
            inOutDetail.AreaCode = cDetail.AreaCode;
            inOutDetail.AreaName = cDetail.AreaName;
            inOutDetail.LocationID = cDetail.LocationID;
            inOutDetail.LocationNo = cDetail.LocationNo;
            inOutDetail.ContainerID = cDetail.ContainerID;
            inOutDetail.ContainerType = cDetail.ContainerType;
            inOutDetail.BarCode = cDetail.BarCode;
            inOutDetail.ItemBarCode = cDetail.ItemBarCode;
            inOutDetail.KindCode = cDetail.KindCode;
            inOutDetail.KindName = cDetail.KindName;
            inOutDetail.ItemID = cDetail.ItemID;
            inOutDetail.ItemName = cDetail.ItemName;
            inOutDetail.ItemCode = cDetail.ItemCode;
            inOutDetail.Lot = cDetail.Lot;
            inOutDetail.Spec = cDetail.Spec;
            inOutDetail.SupplierID = cDetail.SupplierID;
            inOutDetail.SupplierCode = cDetail.SupplierCode;
            inOutDetail.SupplierName = cDetail.SupplierName;
            inOutDetail.ItemUnitText = cDetail.ItemUnitText;
            inOutDetail.CheckID = cDetail.CheckID;
            inOutDetail.CheckDetailID = cDetail.CheckDetailID;
            inOutDetail.CheckState = cDetail.CheckState;
            inOutDetail.ProductDate = cDetail.ProductDate;
            inOutDetail.OverdueDate = cDetail.OverdueDate;
            //inOutDetail.IsSpecial = cDetail.IsSpecial;
            inOutDetail.State = cDetail.State;
            inOutDetail.IsCheckFreeze = cDetail.IsCheckFreeze;
            inOutDetail.IsCountFreeze = cDetail.IsCountFreeze;
            inOutDetail.ContainerKind = cDetail.ContainerKind;
            inOutDetail.Factory = cDetail.Factory;
            inOutDetail.ERPWarehouseCode = cDetail.ERPWarehouseCode;
            inOutDetail.ValidityDayNum = cDetail.ValidityDayNum;
            inOutDetail.IsItemMark = cDetail.IsItemMark;
            inOutDetail.Qty = beforeQty;
            inOutDetail.ChangeQty = changeQty;
            inOutDetail.OutQty = cDetail.OutQty;
            inOutDetail.CheckQty = cDetail.CheckQty;
            inOutDetail.InBoundRefCode = cDetail.RefInBoundCode;
            inOutDetail.ReceiveRecordID = cDetail.ReceiveRecordID;
            inOutDetail.InBoundSEQ = cDetail.SEQ;
            inOutDetail.InBoundCode = cDetail.InBoundCode;
            inOutDetail.ERPInDocCode = cDetail.ERPInDocCode;
            inOutDetail.IsBroken = "true";

            if (inOutType == "InType")
            {
                inOutDetail.AfterChangeQty = beforeQty + changeQty;   /// 入库，增加库存
            }
            else if (inOutType == "OutType")
            {
                inOutDetail.AfterChangeQty = beforeQty - changeQty; /// 出库，减少库存
            }

            inOutDetail.InOutType = inOutType.ToString();
            inOutDetail.OrderType = orderType.ToString();
            inOutDetail.TaskNo = taskNo;

            T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskNo);
            if (task != null)
            {
                inOutDetail.OrderID = task.OrderID;
                inOutDetail.OrderDetailID = task.OrderDetailID;
                inOutDetail.OrderCode = task.OrderCode;
                inOutDetail.OrderSEQ = task.SEQ;
            }
            else
            {
                T_TaskHisEntity taskHis = db.FindEntity<T_TaskHisEntity>(o => o.TaskNo == taskNo);
                if(taskHis!=null)
                {
                    inOutDetail.OrderID = taskHis.OrderID;
                    inOutDetail.OrderDetailID = taskHis.OrderDetailID;
                    inOutDetail.OrderCode = taskHis.OrderCode;
                    inOutDetail.OrderSEQ = taskHis.SEQ;
                }
            }

            switch (orderType)
            {
                case "PurchaseIn":
                    {
                        T_InBoundEntity order = db.FindEntity<T_InBoundEntity>(o => o.F_Id == inOutDetail.OrderID);
                        if (order != null) inOutDetail.OrderRefCode = order.RefOrderCode;
                    }
                    break;
                case "BackSample":
                case "GetSample":
                    {
                        T_QAEntity order = db.FindEntity<T_QAEntity>(o => o.F_Id == inOutDetail.OrderID);
                        if (order != null) inOutDetail.OrderRefCode = order.RefOrderCode;

                        T_QADetailEntity qaDetail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == inOutDetail.OrderDetailID);
                        if (qaDetail != null) inOutDetail.IsBroken = qaDetail.IsBroken;
                    }
                    break;
                case "Count":
                    {
                        T_CountEntity order = db.FindEntity<T_CountEntity>(o => o.F_Id == inOutDetail.OrderID);
                        if (order != null) inOutDetail.OrderRefCode = order.RefOrderCode;
                        if (inOutType == "InType") inOutDetail.Remark = "盘点审核通过后，增加库存";
                        else if (inOutType == "OutType") inOutDetail.Remark = "盘点审核通过后，减少库存";
                    }
                    break;
                case "GetItemOut":
                case "WarehouseBackOut":
                case "VerBackOut":
                case "OtherOut":
                    {
                        T_OutBoundEntity order = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == inOutDetail.OrderID);
                        if (order != null) inOutDetail.OrderRefCode = order.RefOrderCode;
                    }
                    break;
                case "EmptyIn":
                    {
                        if (string.IsNullOrEmpty(taskNo)) inOutDetail.Remark = "盘点审核通过后，更新库存产生空容器";
                    }
                    break;
                default:
                    break;
            }

            inOutDetail.F_DeleteMark = false;
            db.Insert<T_InOutDetailEntity>(inOutDetail);
        }
    }
}
