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
    public class T_ContainerDetailApp
    {
        private IT_ContainerDetailRepository service = new T_ContainerDetailRepository();
        public IQueryable<T_ContainerDetailEntity> FindList(Expression<Func<T_ContainerDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ContainerDetailEntity FindEntity(Expression<Func<T_ContainerDetailEntity, bool>> predicate)
        {
            T_ContainerDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public List<T_ContainerDetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_ContainerDetailEntity> GetList(Pagination pagination, string kindId, string keyword = "")
        {
            var expression = ExtLinq.True<T_ContainerDetailEntity>();
            if (kindId != "0") //为0表示全部物料类型
            {
                T_ItemKindApp kindApp = new T_ItemKindApp();
                IList<T_ItemKindEntity> itemKindAll = kindApp.FindList(o => true).ToList();
                IList<T_ItemKindEntity> lastList = new List<T_ItemKindEntity>();
                new T_ItemApp().GetChild(lastList, itemKindAll, kindId);

                string[] kindCode = lastList.Select(o => o.KindCode).ToArray();

                expression = expression.And(t => kindCode.Contains(t.KindCode));
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.KindName.Contains(keyword) || t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword) || t.BarCode.Contains(keyword) || t.LocationNo.Contains(keyword) || t.Lot.Contains(keyword) || t.ERPInDocCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public List<T_ContainerDetailEntity> GetInitList(Pagination pagination, string kindId, string keyword = "")
        {
            var expression = ExtLinq.True<T_ContainerDetailEntity>();
            if (kindId != "0") //为0表示全部物料类型
            {
                T_ItemKindApp kindApp = new T_ItemKindApp();
                IList<T_ItemKindEntity> itemKindAll = kindApp.FindList(o => true).ToList();
                IList<T_ItemKindEntity> lastList = new List<T_ItemKindEntity>();
                new T_ItemApp().GetChild(lastList, itemKindAll, kindId);

                string[] kindCode = lastList.Select(o => o.KindCode).ToArray();

                expression = expression.And(t => kindCode.Contains(t.KindCode));
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword) || t.Lot.Contains(keyword));
            }
            return service.IQueryable(expression).ToList();
        }

        /// <summary>
        /// 获取库存列表(库存统计时查看明细会用到此方法)
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="itemID">物料ID</param>
        /// <param name="Lot">批次，可为空（选填）</param>
        /// <returns></returns>
        public List<T_ContainerDetailEntity> GetReportList(Pagination pagination, string itemID, string Lot, string keyword)
        {
            var expression = ExtLinq.True<T_ContainerDetailEntity>();
            expression = expression.And(t => t.ItemID == itemID);
            if (!string.IsNullOrEmpty(Lot))
            {
                expression = expression.And(t => t.Lot == Lot);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword) || t.BarCode.Contains(keyword) || t.LocationNo.Contains(keyword) || t.Lot.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public void Delete(Expression<Func<T_ContainerDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_ContainerDetailEntity> GetCountItemList(Pagination pagination, string erpHouseCode, string kindId, string keyword)
        {
            var expression = ExtLinq.True<T_ContainerDetailEntity>();
            if (kindId != "0") //为0表示全部物料类型
            {
                T_ItemKindApp kindApp = new T_ItemKindApp();
                IList<T_ItemKindEntity> itemKindAll = kindApp.FindList(o => true).ToList();
                IList<T_ItemKindEntity> lastList = new List<T_ItemKindEntity>();
                new T_ItemApp().GetChild(lastList, itemKindAll, kindId);

                string[] kindCode = lastList.Select(o => o.KindCode).ToArray();

                expression = expression.And(t => kindCode.Contains(t.KindCode));
            }
            if (!string.IsNullOrEmpty(erpHouseCode))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ERPWarehouseCode == erpHouseCode && (t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword) || t.Lot.Contains(keyword)));
                }
                else
                {
                    expression = expression.And(t => t.ERPWarehouseCode == erpHouseCode);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword));
                }
            }
            return service.FindList(expression, pagination).GroupBy(o => new { o.ItemCode, o.Lot, o.BarCode }).Select(o => o.FirstOrDefault()).ToList();
        }

        #region 手动指定出库库存时的库存列表
        public List<T_ContainerDetailEntity> GetOutItemList(Pagination pagination, string outType, string outBoundID, string itemid, string lot, string[] areaID, string SourceInOrderCode, string keyword)
        {
            var expression = ExtLinq.True<T_ContainerDetailEntity>();
            expression = expression.And(o => areaID.Contains(o.AreaID));
            expression = expression.And(o => o.ItemID == itemid && o.State == "Normal" && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false");

            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(o => o.BarCode.Contains(keyword) || o.LocationNo.Contains(keyword) || o.ItemBarCode.Contains(keyword));
            }


            if (!string.IsNullOrEmpty(lot))
            {
                expression = expression.And(o => o.Lot == lot);
            }
            else
            {
                expression = expression.And(o => string.IsNullOrEmpty(o.Lot));
            }

            if (!string.IsNullOrEmpty(SourceInOrderCode))
            {
                expression = expression.And(o => o.ERPInDocCode == SourceInOrderCode);
            }
            else
            {
                //无需处理，库存来源单号可不为空
            }

            if (outType == "GetItemOut" || outType == "WarehouseBackOut")
            {
                expression = expression.And(o => o.CheckState == "Qua" || o.CheckState == "UnNeed");
            }
            else if (outType == "OtherOut")
            {
                expression = expression.And(o => o.CheckState == "WaitCheck" || o.CheckState == "UnNeed"
                                                || o.CheckState == "Qua" || o.CheckState == "UnQua");
            }
            else if (outType == "VerBackOut")
            {
                T_OutBoundEntity outBound = new T_OutBoundApp().FindEntity(o => o.F_Id == outBoundID);
                expression = expression.And(o => o.InBoundID == outBound.PointInBoundID && o.CheckState == "UnQua");
            }
            else
            {
                throw new Exception("出库单的单据类型未知");
            }

            return service.FindList(expression, pagination).ToList();
        }
        #endregion

        #region 手动指定质检库存时的库存列表
        public List<T_ContainerDetailEntity> GetQAItemList(Pagination pagination, string itemid, string lot, string inboundDetailID, string keyword)
        {
            var expression = ExtLinq.True<T_ContainerDetailEntity>();
            expression = expression.And(o => o.InBoundDetailID == inboundDetailID && o.ItemID == itemid
                        && o.State == "Normal" && o.IsCountFreeze == "false" && o.CheckState == "WaitCheck");

            if (!string.IsNullOrEmpty(keyword))
                expression = expression.And(o => o.BarCode.Contains(keyword) || o.LocationNo.Contains(keyword) || o.ItemBarCode.Contains(keyword));

            if (!string.IsNullOrEmpty(lot))
                expression = expression.And(o => o.Lot == lot);
            else
                expression = expression.And(o => string.IsNullOrEmpty(o.Lot));

            return service.FindList(expression, pagination).ToList();
        }
        #endregion

        public T_ContainerDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_ContainerDetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_ContainerDetailEntity entity)
        {
            service.Insert(entity);
        }

        public void Update(T_ContainerDetailEntity entity)
        {
            service.Update(entity);
        }
    }
}
