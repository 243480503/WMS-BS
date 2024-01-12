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
    public class T_LocationStateDetailApp
    {
        private IT_LocationStateDetailRepository service = new T_LocationStateDetailRepository();

        public IQueryable<T_LocationStateDetailEntity> FindList(Expression<Func<T_LocationStateDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_LocationStateDetailEntity FindEntity(Expression<Func<T_LocationStateDetailEntity, bool>> predicate)
        {
            T_LocationStateDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_LocationStateDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public void Insert(T_LocationStateDetailEntity entity)
        {
            service.Insert(entity);
        }

        public List<T_LocationStateDetailEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_LocationStateDetailEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.LocationCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderBy(t => t.F_CreatorTime).ToList();
        }

        public List<T_LocationStateDetailEntity> GetList(Pagination pagination, string AreaCode = "", string queryJson = "")
        {
            var expression = ExtLinq.True<T_LocationStateDetailEntity>();
            if (!string.IsNullOrEmpty(AreaCode))
            {
                expression = expression.And(t => t.AreaCode == AreaCode);
            }
            var param = queryJson.ToJObject();
            if (!param["keyword"].IsEmpty())
            {
                string keyword = param["keyword"].ToString();
                expression = expression.And(t => t.LocationCode.Contains(keyword));
            }

            return service.FindList(expression, pagination).ToList();
        }

        public List<T_LocationStateDetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_LocationStateDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_LocationStateDetailEntity itemsEntity, string keyValue)
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

        #region 记录货位状态变更，写入货位状态变更记录表，维护货位表变更信息
        /// <summary>
        /// 记录货位状态变更，写入货位状态变更记录表，维护货位表变更信息
        /// </summary>
        /// <param name="db"></param>
        /// <param name="loc"></param>
        /// <param name="inOutType">出入库类型 </param>
        /// <param name="orderType">单据类型</param>
        /// <param name="PreState">变更前状态</param>
        /// <param name="PostState">变更后状态</param>
        /// <param name="taskNo"></param>
        public void SyncLocState(IRepositoryBase db, T_LocationEntity loc, string inOutType, string orderType, string PreState, string PostState, string taskNo)
        {
            T_LocationStateDetailEntity locStateDetail = new T_LocationStateDetailEntity();
            locStateDetail.F_Id = Guid.NewGuid().ToString();
            locStateDetail.AreaCode = loc.AreaCode;
            locStateDetail.LocationCode = loc.LocationCode;
            locStateDetail.LocationID = loc.F_Id;
            locStateDetail.PreState = PreState.ToString();
            locStateDetail.PostState = PostState.ToString();
            locStateDetail.TaskNo = taskNo;
            locStateDetail.OrderType = orderType.ToString();
            locStateDetail.InOutType = inOutType.ToString();

            T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskNo);
            if (task != null)
            {
                locStateDetail.OrderCode = task.OrderCode;
                locStateDetail.OrderDetailID = task.OrderDetailID;
                locStateDetail.OrderID = task.OrderID;
                locStateDetail.OrderSEQ = task.SEQ;
            }
            else
            {
                T_TaskHisEntity taskHis = db.FindEntity<T_TaskHisEntity>(o => o.TaskNo == taskNo);
                if (taskHis != null)
                {
                    locStateDetail.OrderID = taskHis.OrderID;
                    locStateDetail.OrderDetailID = taskHis.OrderDetailID;
                    locStateDetail.OrderCode = taskHis.OrderCode;
                    locStateDetail.OrderSEQ = taskHis.SEQ;
                }
            }

            switch (orderType)
            {
                case "PurchaseIn":
                    {
                        T_InBoundEntity order = db.FindEntity<T_InBoundEntity>(o => o.F_Id == locStateDetail.OrderID);
                        if (order != null) locStateDetail.OrderRefCode = order.RefOrderCode;
                    }
                    break;
                case "BackSample":
                case "GetSample":
                    {
                        T_QAEntity order = db.FindEntity<T_QAEntity>(o => o.F_Id == locStateDetail.OrderID);
                        if (order != null) locStateDetail.OrderRefCode = order.RefOrderCode;
                    }
                    break;
                case "Count":
                    {
                        T_CountEntity order = db.FindEntity<T_CountEntity>(o => o.F_Id == locStateDetail.OrderID);
                        if (order != null) locStateDetail.OrderRefCode = order.RefOrderCode;
                        if (inOutType == "OutType") locStateDetail.Remark = "盘点审核通过后，清除货位";
                    }
                    break;
                case "GetItemOut":
                case "WarehouseBackOut":
                case "VerBackOut":
                case "OtherOut":
                    {
                        T_OutBoundEntity order = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == locStateDetail.OrderID);
                        if (order != null) locStateDetail.OrderRefCode = order.RefOrderCode;
                    }
                    break;
                case "EmptyIn":
                case "EmptyOut":
                case "OffRack":
                    break;
                case "LocCount":
                    {
                        T_LocCountEntity order = db.FindEntity<T_LocCountEntity>(o => o.F_Id == locStateDetail.OrderID);
                        if (order != null) locStateDetail.OrderRefCode = order.RefOrderCode;
                        if (inOutType == "OutType") locStateDetail.Remark = "货位盘点异常确认后，清除货位存储信息";
                        else if (inOutType == "InType") locStateDetail.Remark = "货位盘点异常确认后，新增货位存储信息";
                    }
                    break;
                case "MoveType":
                    {
                        if (!string.IsNullOrEmpty(locStateDetail.OrderID)) //为空时则为呼叫空料箱到存储位的移库任务，而非移库单据
                        {
                            T_MoveEntity order = db.FindEntity<T_MoveEntity>(o => o.F_Id == locStateDetail.OrderID);
                            if (order != null)
                            {
                                locStateDetail.OrderRefCode = order.MoveCode;
                            }
                        }
                    }
                    break;
                case "CarryType":
                    break;
                default:
                    break;
            }

            locStateDetail.F_DeleteMark = false;
            db.Insert<T_LocationStateDetailEntity>(locStateDetail);

            /// 更新货位表
            loc.OrderType = orderType.ToString();
            loc.OrderCode = locStateDetail.OrderCode;
            db.Update<T_LocationEntity>(loc);
        }
        #endregion
    }
}
