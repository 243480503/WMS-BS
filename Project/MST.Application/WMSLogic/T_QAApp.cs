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
    public class T_QAApp
    {
        private IT_QARepository service = new T_QARepository();

        public IQueryable<T_QAEntity> FindList(Expression<Func<T_QAEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_QAEntity FindEntity(Expression<Func<T_QAEntity, bool>> predicate)
        {
            T_QAEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_QAEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_QAEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_QAEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_QAEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.QACode.Contains(keyword) || t.RefOrderCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public List<T_QAEntity> GetList(Pagination pagination, string qaOrderType, string queryJson)
        {
            var expression = ExtLinq.True<T_QAEntity>();
            var queryParam = queryJson.ToJObject();

            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.QAOrderType == qaOrderType.ToString() && (t.QACode.Contains(keyword) || t.RefOrderCode.Contains(keyword)));
            }
            else expression = expression.And(t => t.QAOrderType == qaOrderType.ToString());

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

        public T_QAEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_QAEntity itemsEntity, string keyValue)
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

        public void Insert(T_QAEntity entity)
        {
            service.Insert(entity);
        }

        public void Update(T_QAEntity entity)
        {
            service.Update(entity);
        }
    }
}
