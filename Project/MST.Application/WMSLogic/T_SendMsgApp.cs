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
    public class T_SendMsgApp
    {
        private IT_SendMsgRepository service = new T_SendMsgRepository();
        public IQueryable<T_SendMsgEntity> FindList(Expression<Func<T_SendMsgEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_SendMsgEntity FindEntity(Expression<Func<T_SendMsgEntity, bool>> predicate)
        {
            T_SendMsgEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_SendMsgEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_SendMsgEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_SendMsgEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<T_SendMsgEntity>();
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            expression = expression.And(t => t.F_DeleteMark == false && t.ReceiveID == user.UserId);

            var queryParam = queryJson.ToJObject();
            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.Msg.Contains(keyword));
            }
            if (!queryParam["resultType"].IsEmpty())
            {
                string resultType = queryParam["resultType"].ToString();
                switch (resultType)
                {
                    case "1":
                        break;
                    case "2":
                        expression = expression.And(t => t.IsReadOver == "false");

                        break;
                    case "3":
                        expression = expression.And(t => t.IsReadOver == "true");
                        break;
                    default:
                        break;
                }
            }
            return service.FindList(expression, pagination).ToList();
        }

        public T_SendMsgEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_SendMsgEntity itemsEntity, string keyValue)
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

        public void Insert(T_SendMsgEntity entity)
        {
            service.Insert(entity);
        }
    }
}
