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
using MST.Domain.ViewModel;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_ItemApp
    {
        private IT_ItemRepository service = new T_ItemRepository();
        private T_ItemKindApp itemKindApp = new T_ItemKindApp();
        private T_ItemAreaApp itemAreaApp = new T_ItemAreaApp();
        private T_ItemInStationApp itemInStationApp = new T_ItemInStationApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private T_StationApp stationApp = new T_StationApp();

        public IQueryable<T_ItemEntity> FindList(Expression<Func<T_ItemEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ItemEntity FindEntity(Expression<Func<T_ItemEntity, bool>> predicate)
        {
            T_ItemEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_ItemEntity> FindListAsNoTracking(Expression<Func<T_ItemEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_ItemEntity FindEntityAsNoTracking(Expression<Func<T_ItemEntity, bool>> predicate)
        {
            T_ItemEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="delType">1全部，2正常，3已删除</param>
        /// <param name="kindId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<T_ItemEntity> GetList(Pagination pagination, string delType, string kindId = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_ItemEntity>();
            if (string.IsNullOrEmpty(kindId) || kindId == "0")
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => (t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword)));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemKindID == kindId && (t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword)));
                }
                else
                {
                    expression = expression.And(t => t.ItemKindID == kindId);
                }
            }

            if (delType == "2") //正常
            {
                expression = expression.And(t => t.F_DeleteMark != true);
            }
            else if (delType == "3")//删除
            {
                expression = expression.And(t => t.F_DeleteMark == true);
            }

            return service.FindList(expression, pagination).ToList();
        }

        public List<T_ItemEntity> GetItemList(Pagination pagination, string kindID, string keyword)
        {
            var expression = ExtLinq.True<T_ItemEntity>();
            //排除空料箱和空料架
            expression = expression.And(t => t.ItemCode != FixType.Item.EmptyPlastic.ToString() && t.ItemCode != FixType.Item.EmptyRack.ToString() && t.F_DeleteMark == false);

            if (kindID != "0") //为0表示全部物料类型
            {
                T_ItemKindApp kindApp = new T_ItemKindApp();
                IList<T_ItemKindEntity> itemKindAll = kindApp.FindList(o => true).ToList();
                IList<T_ItemKindEntity> lastList = new List<T_ItemKindEntity>();
                GetChild(lastList, itemKindAll, kindID);

                string[] kindCode = lastList.Select(o => o.KindCode).ToArray();

                expression = expression.And(t => kindCode.Contains(t.KindCode));
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => (t.ItemCode.Contains(keyword) || t.ItemName.Contains(keyword) || t.Factory.Contains(keyword)));
            }
            //return service.FindList(expression, pagination).ToList();
            return service.IQueryable(expression).ToList();
        }

        public void GetChild(IList<T_ItemKindEntity> lastList, IList<T_ItemKindEntity> allList, string id)
        {
            T_ItemKindEntity parent = allList.FirstOrDefault(o => o.F_Id == id);
            IList<T_ItemKindEntity> childList = allList.Where(o => o.ParentID == parent.F_Id).ToList();

            if (childList.Count == 0)
            {
                lastList.Add(parent);
            }
            else
            {
                foreach (T_ItemKindEntity cell in childList)
                {
                    GetChild(lastList, allList, cell.F_Id);
                }
            }
        }

        public List<TreeViewModel> GetTreeJson()
        {
            var data = itemKindApp.FindList(o => o.F_DeleteMark == false).OrderBy(o=>o.KindCode).ToList();
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            if (!user.IsSystem)
            {
                data = data.Where(o => o.IsBase != "true").ToList();
            }

            var treeList = new List<TreeViewModel>();
            foreach (T_ItemKindEntity item in data)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = data.Count(t => t.ParentID == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.KindName;
                tree.value = item.KindCode;
                tree.parentId = item.ParentID;
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = hasChildren;
                treeList.Add(tree);
            }

            TreeViewModel top = new TreeViewModel();
            top.id = "0";
            top.text = "全部";
            top.value = "0";
            top.parentId = "-1";
            top.isexpand = true;
            top.complete = true;
            top.hasChildren = treeList.Count > 0;

            treeList.Add(top);

            return treeList;
        }

        public int Update(T_ItemEntity item)
        {
            return service.Update(item);
        }

        public List<T_ItemEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_ItemEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }

        public void Delete(Expression<Func<T_ItemEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public void Insert(T_ItemEntity entity)
        {
            service.Insert(entity);
        }

        /// <summary>
        /// 获取默认存储区域
        /// </summary>
        /// <returns></returns>
        public T_ItemAreaEntity GetStoreArea(string itemID)
        {
            T_ItemAreaEntity t_ItemAreaEntity = itemAreaApp.FindEntity(o => o.ItemID == itemID && o.IsDefault == "true");
            return t_ItemAreaEntity;
        }

        /// <summary>
        /// 获取默认入库站台
        /// </summary>
        /// <returns></returns>
        public T_ItemInStationEntity GetInStation(string itemID)
        {
            T_ItemInStationEntity t_ItemInStationEntity = itemInStationApp.FindEntity(o => o.ItemID == itemID && o.IsDefault == "true");
            return t_ItemInStationEntity;
        }



        /// <summary>
        /// 提交物料明细
        /// </summary>
        /// <param name="itemsEntity"></param>
        /// <param name="keyValue"></param>
        public void SubmitForm(IRepositoryBase db, ItemModel itemsModel, string keyValue)
        {
            T_ItemEntity itemsEntity = itemsModel.ToObject<T_ItemEntity>();
            if (!string.IsNullOrEmpty(keyValue))
            {
                T_ItemEntity entity = db.FindEntity<T_ItemEntity>(o => o.F_Id == keyValue);
                itemsEntity.F_Id = entity.F_Id;
                entity.ItemKindID = itemsEntity.ItemKindID; /// 物料种类ID
                entity.ItemCode = itemsEntity.ItemCode;   /// 物料编码
                entity.ItemName = itemsEntity.ItemName;   /// 物料名称
                entity.ItemUnitText = itemsEntity.ItemUnitText==null?"": itemsEntity.ItemUnitText;   /// 物料单位,为确保报表统计时，避免空字符串和空产生不一样的结果，此处默认为空字符串
                T_ItemKindEntity kind = db.FindEntity<T_ItemKindEntity>(o => o.F_Id == itemsEntity.ItemKindID);
                entity.KindCode = kind.KindCode;
                entity.KindName = kind.KindName;
                entity.WarningQty = itemsEntity.WarningQty;   /// 预警数量
                entity.IsBase = itemsEntity.IsBase; /// 是否基础数据
                entity.Spec = itemsEntity.Spec == null ? "" : itemsEntity.Spec;  /// 规格,为确保报表统计时，避免空字符串和空产生不一样的结果，此处默认为空字符串
                entity.ERPWarehouseCode = itemsEntity.ERPWarehouseCode;   /// ERP仓位编码
                entity.IsItemMark = itemsEntity.IsItemMark; //是否需贴标
                entity.IsMustLot = itemsEntity.IsMustLot;  /// 强制批号控制
                entity.IsMixLot = itemsEntity.IsMixLot;   /// 是否允许混批
                entity.IsMixItem = itemsEntity.IsMixItem;   /// 是否允许混料
                entity.IsMixQA = itemsEntity.IsMixQA;/// 是否允许合格不合格混放
                entity.Factory = itemsEntity.Factory == null ? "" : itemsEntity.Factory;/// 生产厂家,为确保报表统计时，避免空字符串和空产生不一样的结果，此处默认为空字符串
                entity.ContainerType = itemsEntity.ContainerType;  /// 入库容器类型
                entity.ValidityUnitType = itemsEntity.ValidityUnitType;
                entity.ValidityDayNum = itemsEntity.ValidityDayNum;   /// 有效期天数
                entity.UnitQty = itemsEntity.UnitQty;  /// 单位数量(包装数量)
                entity.CheckPerc = itemsEntity.CheckPerc;/// 来料抽检百分比
                entity.CheckBoxPerc = itemsEntity.CheckBoxPerc; /// 每箱抽检百分比
                entity.IsNeedCheck = itemsEntity.IsNeedCheck;/// 是否需要质检
                entity.StackType = itemsEntity.StackType;  /// 码垛类型
                entity.IsDampproof = itemsEntity.IsDampproof;//防潮
                entity.IsSpecial = itemsEntity.IsSpecial;  /// 是否特殊物料
                entity.IsBroken = itemsEntity.IsBroken;   /// 是否破坏性质检
                entity.IsPriority = itemsEntity.IsPriority; //区域内细分货位优先
                entity.ValidityWarning = itemsEntity.ValidityWarning; //有效期预警天数
                entity.MaxQty = itemsEntity.MaxQty;//最大库存量
                entity.MinQty = itemsEntity.MinQty;//保留库存
                entity.F_DeleteMark = false;
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = OperatorProvider.Provider.GetCurrent().UserId;
                entity.ModifyUserName = OperatorProvider.Provider.GetCurrent().UserName;
                db.Update<T_ItemEntity>(entity);
            }
            else
            {
                T_ItemKindEntity kind = db.FindEntity<T_ItemKindEntity>(o => o.F_Id == itemsEntity.ItemKindID);
                itemsEntity.KindCode = kind.KindCode;
                itemsEntity.KindName = kind.KindName;
                itemsEntity.F_DeleteMark = false;
                itemsEntity.Spec = itemsEntity.Spec == null ? "" : itemsEntity.Spec;//为确保报表统计时，避免空字符串和空产生不一样的结果，此处默认为空字符串
                itemsEntity.Factory = itemsEntity.Factory == null ? "" : itemsEntity.Factory;//为确保报表统计时，避免空字符串和空产生不一样的结果，此处默认为空字符串
                itemsEntity.ItemUnitText = itemsEntity.ItemUnitText == null ? "" : itemsEntity.ItemUnitText;   // 物料单位，避免空字符串和空产生不一样的结果，此处默认为空字符串
                itemsEntity.Create();
                db.Insert<T_ItemEntity>(itemsEntity);
            }
            db.SaveChanges();

            //处理默认存储区域

            string[] areaIDList_InModel = itemsModel.StoredAreaList.Select(o => o.AreaID).ToArray();
            string[] areaIDList_InDB = db.FindList<T_ItemAreaEntity>(o => o.ItemID == itemsEntity.F_Id).Select(o => o.AreaID).ToArray();

            string[] needDel = areaIDList_InDB.Where(o => !areaIDList_InModel.Contains(o)).ToArray(); //需删除
            string[] needIns = areaIDList_InModel.Where(o => !areaIDList_InDB.Contains(o)).ToArray(); //需插入

            foreach (string areaid in needDel) //删除前的验证
            {
                T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == areaid);
                IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.AreaID == areaid && o.ItemID == itemsEntity.F_Id).ToList();
                if (containerDetailList.Count > 0)
                {
                    throw new Exception("该物料在" + area.AreaName + "中还存在库存");
                }
                db.SaveChanges();
            }

            foreach (string areaid in needDel) //删除
            {
                db.Delete<T_ItemAreaEntity>(o => o.ItemID == itemsEntity.F_Id && o.AreaID == areaid);
                db.SaveChanges();
            }

            int index = 0;
            foreach (string areaid in needIns) //插入
            {
                T_ItemAreaEntity itemAreaEntity = new T_ItemAreaEntity();
                itemAreaEntity.Create();
                itemAreaEntity.AreaID = areaid;
                itemAreaEntity.ItemID = itemsEntity.F_Id;
                itemAreaEntity.F_DeleteMark = false;
                if (index == 0)
                {
                    T_ItemAreaEntity defalueArea = db.FindEntity<T_ItemAreaEntity>(o => o.ItemID == itemsEntity.F_Id && o.IsDefault == "true");// itemAreaApp.FindEntity(o => o.ItemID == itemsEntity.F_Id && o.IsDefault == "true");
                    if (defalueArea == null)
                    {
                        itemAreaEntity.IsDefault = "true";
                    }
                }
                db.Insert<T_ItemAreaEntity>(itemAreaEntity);
                db.SaveChanges();
                index++;
            }

            //处理默认入库站台

            string[] StationIDList_InModel = itemsModel.InStationList.Select(o => o.StationID).ToArray();
            string[] StationIDList_InDB = db.FindList<T_ItemInStationEntity>(o => o.ItemID == itemsEntity.F_Id).Select(o => o.StationID).ToArray();

            string[] StationNeedDel = StationIDList_InDB.Where(o => !StationIDList_InModel.Contains(o)).ToArray(); //需删除
            string[] StationNeedIns = StationIDList_InModel.Where(o => !StationIDList_InDB.Contains(o)).ToArray(); //需插入

            foreach (string stationId in StationNeedDel) //删除
            {
                db.Delete<T_ItemInStationEntity>(o => o.ItemID == itemsEntity.F_Id && o.StationID == stationId);
                db.SaveChanges();
            }

            int _index = 0;
            foreach (string stationid in StationNeedIns) //插入
            {
                T_ItemInStationEntity itemStationEntity = new T_ItemInStationEntity();
                itemStationEntity.Create();
                itemStationEntity.StationID = stationid;
                itemStationEntity.ItemID = itemsEntity.F_Id;
                itemStationEntity.F_DeleteMark = false;
                if (_index == 0)
                {
                    T_ItemInStationEntity defalueStation = db.FindEntity<T_ItemInStationEntity>(o => o.ItemID == itemsEntity.F_Id && o.IsDefault == "true");
                    if (defalueStation == null)
                    {
                        itemStationEntity.IsDefault = "true";
                    }
                }

                db.Insert<T_ItemInStationEntity>(itemStationEntity);
                db.SaveChanges();
                _index++;
            }
        }
    }
}
