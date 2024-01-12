/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using System;

namespace MST.Domain
{
    public class IEntity<TEntity>
    {
        public void Create()
        {
            var entity = this as ICreationAudited;
            entity.F_Id = Common.GuId();
            var LoginInfo = OperatorProvider.Provider.GetCurrent();
            if (LoginInfo != null)
            {
                entity.F_CreatorUserId = LoginInfo.UserId;
            }
            entity.F_CreatorTime = DateTime.Now;

            CreateWMS(LoginInfo);
        }
        
        public void CreateForThread(OperatorModel LoginInfo)
        {
            var entity = this as ICreationAudited;
            entity.F_Id = Common.GuId();
            if (LoginInfo != null)
            {
                entity.F_CreatorUserId = LoginInfo.UserId;
            }
            entity.F_CreatorTime = DateTime.Now;

            CreateWMS(LoginInfo);
        }

        public void Modify(string keyValue)
        {
            var entity = this as IModificationAudited;
            entity.F_Id = keyValue;
            var LoginInfo = OperatorProvider.Provider.GetCurrent();
            if (LoginInfo != null)
            {
                entity.F_LastModifyUserId = LoginInfo.UserId;
            }
            entity.F_LastModifyTime = DateTime.Now;

            ModifyWMS(LoginInfo);
        }

        public void ModifyForThread(string keyValue,OperatorModel LoginInfo)
        {
            var entity = this as IModificationAudited;
            entity.F_Id = keyValue;
            if (LoginInfo != null)
            {
                entity.F_LastModifyUserId = LoginInfo.UserId;
            }
            entity.F_LastModifyTime = DateTime.Now;

            ModifyWMS(LoginInfo);
        }

        public void Remove()
        {
            var entity = this as IDeleteAudited;
            var LoginInfo = OperatorProvider.Provider.GetCurrent();
            if (LoginInfo != null)
            {
                entity.F_DeleteUserId = LoginInfo.UserId;
            }
            entity.F_DeleteTime = DateTime.Now;
            entity.F_DeleteMark = true;

            RemoveWMS(LoginInfo);
        }

        /// <summary>
        /// by fxx
        /// </summary>
        private void CreateWMS(OperatorModel LoginInfo)
        {
            if (this is IWMSEntity)
            {
                var entity = this as IWMSEntity;
                if (LoginInfo != null)
                {
                    entity.CreatorUserName = LoginInfo.UserName;
                }
            }
        }

        /// <summary>
        /// by fxx
        /// </summary>
        private void ModifyWMS(OperatorModel LoginInfo)
        {
            if (this is IWMSEntity)
            {
                var entity = this as IWMSEntity;
                if (LoginInfo != null)
                {
                    entity.ModifyUserName = LoginInfo.UserName;
                }
            }
        }

        /// <summary>
        /// by fxx
        /// </summary>
        private void RemoveWMS(OperatorModel LoginInfo)
        {
            if (this is IWMSEntity)
            {
                var entity = this as IWMSEntity;
                if (LoginInfo != null)
                {
                    entity.DeleteUserName = LoginInfo.UserName;
                }
            }
        }
    }
}
