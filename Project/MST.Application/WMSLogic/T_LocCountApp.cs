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
    public class T_LocCountApp
    {
        private IT_LocCountRepository service = new T_LocCountRepository();
        private T_ItemApp itemApp = new T_ItemApp();

        public IQueryable<T_LocCountEntity> FindList(Expression<Func<T_LocCountEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_LocCountEntity FindEntity(Expression<Func<T_LocCountEntity, bool>> predicate)
        {
            T_LocCountEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public List<T_LocCountEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_LocCountEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<T_LocCountEntity>();
            var queryParam = queryJson.ToJObject();
            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.LocCountCode.Contains(keyword));
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
        public List<T_LocCountEntity> GetList(string keyValue)
        {
            IQueryable<T_LocCountEntity> query = service.IQueryable();
            if (!string.IsNullOrEmpty(keyValue))
            {
                query = query.Where(o => o.LocCountCode.Contains(keyValue));
            }
            return query.ToList();
        }

        public T_LocCountEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }

        public void Delete(Expression<Func<T_LocCountEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_LocCountEntity locCountEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                locCountEntity.Modify(keyValue);
                service.Update(locCountEntity);
            }
            else
            {
                locCountEntity.Create();
                service.Insert(locCountEntity);
            }
        }

        public void Insert(T_LocCountEntity entity)
        {
            service.Insert(entity);
        }
    }
}
