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
    public class T_ItemKindApp
    {
        private IT_ItemKindRepository service = new T_ItemKindRepository();
        public IQueryable<T_ItemKindEntity> FindList(Expression<Func<T_ItemKindEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ItemKindEntity FindEntity(Expression<Func<T_ItemKindEntity, bool>> predicate)
        {
            T_ItemKindEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public T_ItemKindEntity FindEntityAsNoTracking(Expression<Func<T_ItemKindEntity, bool>> predicate)
        {
            T_ItemKindEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="delType">1:全部，2已启用，3已删除</param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<T_ItemKindEntity> GetList(Pagination pagination,string delType, string keyword = "")
        {
            var expression = ExtLinq.True<T_ItemKindEntity>();
            if (delType == "2")
            {
                expression = expression.And(o => o.F_DeleteMark != true);
            }
            if(delType == "3")
            {
                expression = expression.And(o => o.F_DeleteMark == true);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.KindName.Contains(keyword) || t.KindCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public IQueryable<T_ItemKindEntity> FindListAsNoTracking(Expression<Func<T_ItemKindEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_ItemKindEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }

        public void Delete(Expression<Func<T_ItemKindEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public void SubmitForm(T_ItemKindEntity itemsEntity, string keyValue)
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

        public void Insert(T_ItemKindEntity entity)
        {
            service.Insert(entity);
        }

        public void Update(T_ItemKindEntity entity)
        {
            service.Update(entity);
        }
    }
}
