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
    public class T_TransRecordApp
    {
        private IT_TransRecordRepository service = new T_TransRecordRepository();
        public IQueryable<T_TransRecordEntity> FindList(Expression<Func<T_TransRecordEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public T_TransRecordEntity FindEntity(Expression<Func<T_TransRecordEntity, bool>> predicate)
        {
            T_TransRecordEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_TransRecordEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public void Delete(Expression<Func<T_TransRecordEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_TransRecordEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<T_TransRecordEntity>();
            var queryParam = queryJson.ToJObject();
            expression = expression.And(o => !string.IsNullOrEmpty(o.OrderCode));
            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.OrderCode.Contains(keyword));
            }
            if (!queryParam["resultType"].IsEmpty())
            {
                string resultType = queryParam["resultType"].ToString();
                switch (resultType)
                {
                    case "1":
                        break;
                    case "2":
                        expression = expression.And(t => t.State == "New");
                        break;
                    case "3":
                        expression = expression.And(t => t.State == "OK");
                        break;
                    case "4":
                        expression = expression.And(t => t.State == "Err");
                        break;
                    case "5":
                        expression = expression.And(t => t.IsIgnore == "true");
                        break;
                    default:
                        break;
                }
            }
            return service.FindList(expression, pagination).ToList();
        }
        public T_TransRecordEntity GetForm(string keyvalue)
        {
            return service.FindEntity(keyvalue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }

        public void Update(T_TransRecordEntity entity)
        {
            service.Update(entity);
        }
        public void SubmitForm(T_TransRecordEntity itemsEntity, string keyValue)
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
        public void Insert(T_TransRecordEntity entity)
        {
            service.Insert(entity);
        }

        public AjaxResult GenTransRecord(IRepositoryBase db,string orderID,string orderType)
        {
            AjaxResult rst = new AjaxResult();
            int maxTimes = 3;

            T_TransRecordEntity trans = db.FindEntity<T_TransRecordEntity>(o=>o.OrderID == orderID && o.OrderType == orderType.ToString());
            if(trans!=null)
            {
                rst.state = ResultType.success;
                rst.data = trans;
                return rst;
            }
            trans = new T_TransRecordEntity();
            trans.F_Id = Guid.NewGuid().ToString();            
            trans.OrderType = orderType.ToString();
            trans.MaxTransCount = maxTimes;
            trans.ErrCount = 0;
            trans.LastTime = null;
            trans.IsIgnore = "false";
            trans.State = "New";
            trans.Info = "";
            trans.SendText = "";
            trans.GetText = "";


            switch (orderType)
            {
                case "PurchaseIn": //采购入库单
                    {
                        T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o=>o.F_Id == orderID);
                        trans.OrderID = inbound.F_Id;
                        trans.OrderCode = inbound.InBoundCode;
                        db.Insert<T_TransRecordEntity>(trans);
                        db.SaveChanges();
                    }
                    break;
                case "GetSample": //质检取样单
                    {
                        T_QAEntity qa = db.FindEntity<T_QAEntity>(o => o.F_Id == orderID);
                        trans.OrderID = qa.F_Id;
                        trans.OrderCode = qa.QACode;
                        db.Insert<T_TransRecordEntity>(trans);
                        db.SaveChanges();
                    }
                    break;
                case "BackSample": //质检还样单
                    {
                        T_QAEntity qa = db.FindEntity<T_QAEntity>(o => o.F_Id == orderID);
                        trans.OrderID = qa.F_Id;
                        trans.OrderCode = qa.QACode;
                        db.Insert<T_TransRecordEntity>(trans);
                        db.SaveChanges();
                    }
                    break;
                case "OtherOut"://验退出库单
                case "VerBackOut"://验退出库单
                case "WarehouseBackOut"://仓退出库单
                case "GetItemOut": //领料出库单
                    {
                        T_OutBoundEntity outbound = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == orderID);
                        trans.OrderID = outbound.F_Id;
                        trans.OrderCode = outbound.OutBoundCode;
                        db.Insert<T_TransRecordEntity>(trans);
                        db.SaveChanges();
                    }
                    break;
                case "Count": /// 盘点单
                    {
                        T_CountEntity countEntity = db.FindEntity<T_CountEntity>(o => o.F_Id == orderID);
                        trans.OrderID = countEntity.F_Id;
                        trans.OrderCode = countEntity.CountCode;
                        db.Insert<T_TransRecordEntity>(trans);
                        db.SaveChanges();
                    }
                    break;
                default:
                    {
                        rst.state = ResultType.error;
                        rst.message = "单据类型不存在";
                    }
                    break;
            }

            rst.state = ResultType.success;
            rst.data = trans;
            return rst;
        }
    }
}

