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

namespace MST.Application.SystemManage
{
    public class UserLogOnApp
    {
        private IUserLogOnRepository service = new UserLogOnRepository();

        public UserLogOnEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void UpdateForm(UserLogOnEntity userLogOnEntity)
        {
            service.Update(userLogOnEntity);
        }

        public void Update(UserLogOnEntity userLogOnEntity)
        {
            service.Update(userLogOnEntity);
        }
        public void RevisePassword(string userPassword, string keyValue)
        {
            UserLogOnEntity userLogOnEntity = service.FindEntity(o => o.F_UserId == keyValue);
            if (userLogOnEntity == null)
            {
                userLogOnEntity = new UserLogOnEntity();
                userLogOnEntity.F_Id = keyValue;
                userLogOnEntity.F_UserId = keyValue;
                userLogOnEntity.F_UserSecretkey = Md5.md5(Common.CreateNo(), 16).ToLower();
                userLogOnEntity.F_UserPassword = Md5.md5(DESEncrypt.Encrypt(Md5.md5(userPassword, 32).ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();
                service.Insert(userLogOnEntity);
            }
            else

            {
                userLogOnEntity.F_Id = keyValue;
                userLogOnEntity.F_UserId = keyValue;
                userLogOnEntity.F_UserSecretkey = Md5.md5(Common.CreateNo(), 16).ToLower();
                userLogOnEntity.F_UserPassword = Md5.md5(DESEncrypt.Encrypt(Md5.md5(userPassword, 32).ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();
                service.Update(userLogOnEntity);
            }

        }
    }
}
