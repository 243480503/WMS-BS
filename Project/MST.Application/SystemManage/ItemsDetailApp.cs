/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.IRepository.SystemManage;
using MST.Domain.ViewModel;
using MST.Repository.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.SystemManage
{
    public class ItemsDetailApp
    {
        private IItemsDetailRepository service = new ItemsDetailRepository();

        public List<ItemsDetailEntity> GetList(string itemId = "", string keyword = "")
        {
            var expression = ExtLinq.True<ItemsDetailEntity>();
            if (!string.IsNullOrEmpty(itemId))
            {
                expression = expression.And(t => t.F_ItemId == itemId);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_ItemName.Contains(keyword) || t.F_ItemCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderBy(t => t.F_SortCode).ToList();
        }
        public List<ItemsDetailEntity> GetItemList(string enCode)
        {
            return service.GetItemList(enCode);
        }
        public ItemsDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(ItemsDetailEntity itemsDetailEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsDetailEntity.Modify(keyValue);
                service.Update(itemsDetailEntity);
            }
            else
            {
                itemsDetailEntity.Create();
                service.Insert(itemsDetailEntity);
            }
        }

        public ItemsDetailEntity FindEntity(Expression<Func<ItemsDetailEntity, bool>> predicate)
        {
            ItemsDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }

        /// <summary>
        /// 查找实体指定字段的字典数据
        /// </summary>
        /// <typeparam name="T">表实体类型</typeparam>
        /// <param name="exp">字段字符串</param>
        /// <returns></returns>
        public List<ItemsDetailEntity> FindEnum<T>(Expression<Func<T,object>> exp) where T:new()
        {
            var expression = exp.Body as System.Linq.Expressions.MemberExpression;
            string name = expression.Member.Name;
            T myT = new T();
            Type typeT = myT.GetType();
            string tableStr = typeT.Name.Replace("Entity","");
            return service.GetEnumList(tableStr, name);
        }



        /// <summary>
        /// 查找实体所有字典数据
        /// </summary>
        /// <typeparam name="T">表实体类型</typeparam>
        /// <returns></returns>
        public IList<SysItemsModel> FindEnum<T>() where T : new()
        {
            T myT = new T();
            Type typeT = myT.GetType();
            string tableStr = typeT.Name.Replace("Entity", "");
            return service.GetEnumList(tableStr);
        }

        /// <summary>
        /// 查找实体指定字段的字典数据
        /// </summary>
        /// <param name="root"></param>
        /// <param name="entityName">表名</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public IList<ItemsDetailEntity> FindEnum(string entityName, string fieldName)
        {
            return service.GetEnumList(entityName, fieldName);
        }

    }
}
