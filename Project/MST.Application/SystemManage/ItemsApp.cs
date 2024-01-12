/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain;
using MST.Domain.Entity.SystemManage;
using MST.Domain.IRepository.SystemManage;
using MST.Repository.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.SystemManage
{
    public class ItemsApp
    {
        private IItemsRepository service = new ItemsRepository();

        public List<ItemsEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        /// <summary>
        /// 获取指定节点下的值
        /// </summary>
        /// <param name="rootCode"></param>
        /// <param name="firstCode"></param>
        /// <param name="SconedCode"></param>
        /// <param name="ItemName">字段名称</param>
        /// <returns></returns>
        public ItemsDetailEntity GetDetailByPath(string rootCode,string firstCode,string secondCode, string itemCode)
        {
            ItemsEntity root = service.IQueryable(o=>o.F_EnCode == rootCode).FirstOrDefault();
            if (root != null)
            {
                ItemsEntity firstEntity = service.IQueryable(o => o.F_ParentId == root.F_Id && o.F_EnCode == firstCode).FirstOrDefault();
                if (firstEntity != null)
                {
                    ItemsEntity SecondEntity = service.IQueryable(o => o.F_ParentId == firstEntity.F_Id && o.F_EnCode == secondCode).FirstOrDefault();
                    if(SecondEntity!=null)
                    {
                        ItemsDetailEntity detailEntity = new ItemsDetailRepository().IQueryable(o => o.F_ItemId == SecondEntity.F_Id && o.F_ItemCode == itemCode).FirstOrDefault();
                        return detailEntity;
                    }
                }
            }
            return null;
        }

        public ItemsEntity FindEntity(Expression<Func<ItemsEntity, bool>> predicate)
        {
            ItemsEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public ItemsEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            if (service.IQueryable().Count(t => t.F_ParentId.Equals(keyValue)) > 0)
            {
                throw new Exception("删除失败！操作的对象包含了下级数据。");
            }
            else
            {
                service.Delete(t => t.F_Id == keyValue);
            }
        }
        public void SubmitForm(ItemsEntity itemsEntity, string keyValue)
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
    }
}
