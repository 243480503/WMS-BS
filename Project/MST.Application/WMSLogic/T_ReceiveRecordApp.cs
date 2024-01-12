/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_ReceiveRecordApp
    {
        private IT_ReceiveRecordRepository service = new T_ReceiveRecordRepository();
        public IQueryable<T_ReceiveRecordEntity> FindList(Expression<Func<T_ReceiveRecordEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ReceiveRecordEntity FindEntity(Expression<Func<T_ReceiveRecordEntity, bool>> predicate)
        {
            T_ReceiveRecordEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_ReceiveRecordEntity> FindListAsNoTracking(Expression<Func<T_ReceiveRecordEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_ReceiveRecordEntity FindEntityAsNoTracking(Expression<Func<T_ReceiveRecordEntity, bool>> predicate)
        {
            T_ReceiveRecordEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }
        public List<T_ReceiveRecordEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_ReceiveRecordEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public List<T_ReceiveRecordEntity> GetList(string inBoundDetailID = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_ReceiveRecordEntity>();
            if (!string.IsNullOrEmpty(inBoundDetailID))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.InBoundDetailID == inBoundDetailID && (t.ItemCode.Contains(keyword) || t.BarCode.Contains(keyword)));
                }
                else expression = expression.And(t => t.InBoundDetailID == inBoundDetailID);
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemCode.Contains(keyword) ||t.BarCode.Contains(keyword) );
                }
            }

            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }

        public List<T_ReceiveRecordEntity> GetList(Pagination pagination, string inBoundDetailID = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_ReceiveRecordEntity>();
            if (!string.IsNullOrEmpty(inBoundDetailID))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.InBoundDetailID == inBoundDetailID && (t.ItemCode.Contains(keyword) || t.BarCode.Contains(keyword) ));
                }
                else expression = expression.And(t => t.InBoundDetailID == inBoundDetailID);
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemCode.Contains(keyword) ||t.BarCode.Contains(keyword) );
                }
            }
            return service.FindList(expression, pagination).ToList();
        }

        public T_ReceiveRecordEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_ReceiveRecordEntity itemsEntity, string keyValue)
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

        public void Insert(T_ReceiveRecordEntity entity)
        {
            service.Insert(entity);
        }

        /// <summary>
        /// 正常入库站台，申请入库
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="normalStationCode"></param>
        public void ApplyIn_NormalStationIn(string barcode, string normalStationCode)
        {

        }
    }
}
