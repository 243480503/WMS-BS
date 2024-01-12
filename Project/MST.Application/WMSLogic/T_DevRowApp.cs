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
    /// <summary>
    /// 巷道
    /// </summary>
    public class T_DevRowApp
    {
        private IT_DevRowRepository service = new T_DevRowRepository();
        public IQueryable<T_DevRowEntity> FindList(Expression<Func<T_DevRowEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_DevRowEntity FindEntity(Expression<Func<T_DevRowEntity, bool>> predicate)
        {
            T_DevRowEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_DevRowEntity> FindListAsNoTracking(Expression<Func<T_DevRowEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_DevRowEntity FindEntityAsNoTracking(Expression<Func<T_DevRowEntity, bool>> predicate)
        {
            T_DevRowEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_DevRowEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_DevRowEntity> GetList(Pagination pagination, string areaID, string keyword = "")
        {
            var expression = ExtLinq.True<T_DevRowEntity>();

            expression = expression.And(t => t.AreaID == areaID);

            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.WayCode.ToString() == keyword);
            }
            return service.FindList(expression, pagination).OrderByDescending(o => o.F_CreatorTime).ToList();
        }

        public void Delete(Expression<Func<T_DevRowEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public List<T_DevRowEntity> GetListByEquID(string equID)
        {
            var expression = ExtLinq.True<T_DevRowEntity>();
            if (!string.IsNullOrEmpty(equID))
            {
                expression = expression.And(t => t.EquID == equID);
            }
            return service.IQueryable(expression).OrderBy(t => t.Num).ToList();
        }
        public T_DevRowEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_DevRowEntity itemsEntity, string keyValue)
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

        public void Insert(T_DevRowEntity entity)
        {
            service.Insert(entity);
        }
    }
}
