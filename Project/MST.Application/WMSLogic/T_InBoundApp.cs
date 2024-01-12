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
    public class T_InBoundApp
    {
        private IT_InBoundRepository service = new T_InBoundRepository();

        public IQueryable<T_InBoundEntity> FindList(Expression<Func<T_InBoundEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_InBoundEntity FindEntity(Expression<Func<T_InBoundEntity, bool>> predicate)
        {
            T_InBoundEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public List<T_InBoundEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_InBoundEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<T_InBoundEntity>();
            var queryParam = queryJson.ToJObject();
            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.InBoundCode.Contains(keyword) || t.RefOrderCode.Contains(keyword) || t.ERPInDocCode.Contains(keyword));
            }
            if (!queryParam["resultType"].IsEmpty())
            {
                string resultType = queryParam["resultType"].ToString();
                switch (resultType)
                {
                    case "1":
                        break;
                    case "2":
                        expression = expression.And(t => t.State != "Over");

                        break;
                    case "3":
                        expression = expression.And(t => t.State == "Over");
                        break;
                    default:
                        break;
                }
            }
            return service.FindList(expression, pagination).ToList();
        }

        public void Delete(Expression<Func<T_InBoundEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public List<T_InBoundEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_InBoundEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.InBoundCode.Contains(keyword) ||  t.RefOrderCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public T_InBoundEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_InBoundEntity itemsEntity, string keyValue)
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

        public void Insert(T_InBoundEntity entity)
        {
            service.Insert(entity);
        }
    }
}
