/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.IRepository.SystemManage;
using MST.Repository.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.SystemManage
{
    public class UserApp
    {
        private IUserRepository service = new UserRepository();
        private UserLogOnApp userLogOnApp = new UserLogOnApp();

        public List<UserEntity> GetList(string keyword)
        {
            var expression = ExtLinq.True<UserEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_Account.Contains(keyword) || t.F_RealName.Contains(keyword) || t.F_MobilePhone.Contains(keyword));
            }
            expression = expression.And(t => t.F_Account != "admin" && t.IsBase != "true");
            return service.IQueryable(expression).ToList();
        }
        public List<UserEntity> GetList(Pagination pagination, string keyword)
        {
            var expression = ExtLinq.True<UserEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_Account.Contains(keyword) || t.F_RealName.Contains(keyword) || t.F_MobilePhone.Contains(keyword));
            }
            expression = expression.And(t => t.F_DeleteMark != true && t.F_Account != "admin" && t.IsBase != "true");
            return service.FindList(expression, pagination);
        }

        public UserEntity FindEntity(Expression<Func<UserEntity, bool>> predicate)
        {
            UserEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<UserEntity> FindList(Expression<Func<UserEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public UserEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.DeleteForm(keyValue);
        }
        public void SubmitForm(UserEntity userEntity, UserLogOnEntity userLogOnEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                userEntity.Modify(keyValue);
            }
            else
            {
                userEntity.Create();
            }
            service.SubmitForm(userEntity, userLogOnEntity, keyValue);
        }
        public void UpdateForm(UserEntity userEntity)
        {
            service.Update(userEntity);
        }

        public void Update(UserEntity userEntity)
        {
            service.Update(userEntity);
        }

        public UserEntity GetEntity(string loginCode)
        {
            UserEntity user = service.FindEntity(o => o.F_Account == loginCode);
            return user;
        }

        public UserEntity CheckLogin(string username, string password)
        {
            UserEntity userEntity = service.FindEntity(t => t.F_Account == username && t.F_DeleteMark == false);
            if (userEntity != null)
            {
                if (userEntity.F_EnabledMark == true)
                {
                    UserLogOnEntity userLogOnEntity = userLogOnApp.GetForm(userEntity.F_Id);
                    if (userLogOnEntity == null)
                    {
                        throw new Exception("该用户无登录权限");
                    }
                    string dbPassword = Md5.md5(DESEncrypt.Encrypt(password.ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();
                    if (dbPassword == userLogOnEntity.F_UserPassword)
                    {
                        DateTime lastVisitTime = DateTime.Now;
                        int LogOnCount = (userLogOnEntity.F_LogOnCount).ToInt() + 1;
                        if (userLogOnEntity.F_LastVisitTime != null)
                        {
                            userLogOnEntity.F_PreviousVisitTime = userLogOnEntity.F_LastVisitTime.ToDate();
                        }
                        userLogOnEntity.F_LastVisitTime = lastVisitTime;
                        userLogOnEntity.F_LogOnCount = LogOnCount;
                        userLogOnApp.UpdateForm(userLogOnEntity);
                        return userEntity;
                    }
                    else
                    {
                        throw new Exception("密码不正确，请重新输入");
                    }
                }
                else
                {
                    throw new Exception("账户被系统锁定,请联系管理员");
                }
            }
            else
            {
                throw new Exception("账户不存在，请重新输入");
            }
        }
    }
}
