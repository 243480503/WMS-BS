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
    public class T_PhoDetailApp
    {
        private IT_PhoDetailRepository service = new T_PhoDetailRepository();
        public IQueryable<T_PhoDetailEntity> FindList(Expression<Func<T_PhoDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_PhoDetailEntity FindEntity(Expression<Func<T_PhoDetailEntity, bool>> predicate)
        {
            T_PhoDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public List<T_PhoDetailEntity> GetList(Pagination pagination, string PhoID, string keyword)
        {
            var expression = ExtLinq.True<T_PhoDetailEntity>();
            if (!string.IsNullOrEmpty(PhoID))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.PhoID == PhoID && (t.ItemName.Contains(keyword) ||t.ItemCode.Contains(keyword) || t.LocationNo.Contains(keyword)));
                }
                else expression = expression.And(t => t.PhoID == PhoID);
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.ItemName.Contains(keyword) || t.ItemCode.Contains(keyword) || t.LocationNo.Contains(keyword));
                }
            }

            return service.FindList(expression, pagination).ToList();
        }

        public void Delete(Expression<Func<T_PhoDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_PhoDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_PhoDetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_PhoDetailEntity entity)
        {
            service.Insert(entity);
        }
    }
}
