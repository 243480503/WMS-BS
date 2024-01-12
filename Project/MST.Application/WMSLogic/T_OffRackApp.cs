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
    public class T_OffRackApp
    {
        private IT_OffRackRepository service = new T_OffRackRepository();
        public IQueryable<T_OffRackEntity> FindList(Expression<Func<T_OffRackEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }
        public T_OffRackEntity FindEntity(Expression<Func<T_OffRackEntity, bool>> predicate)
        {
            T_OffRackEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_OffRackEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public void Delete(Expression<Func<T_OffRackEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_OffRackEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_OffRackEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.OffRackCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public List<T_OffRackEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<T_OffRackEntity>();
            var queryParam = queryJson.ToJObject();
            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.OffRackCode.Contains(keyword) || t.RefOrderCode.Contains(keyword));
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
        public T_OffRackEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_OffRackEntity itemsEntity, string keyValue)
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
        public void Insert(T_OffRackEntity entity)
        {
            service.Insert(entity);
        }
    }
}
