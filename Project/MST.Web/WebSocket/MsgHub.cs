using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MST.Application;
using MST.Application.WebMsg;
using MST.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MST.Web.WebSocket
{
    [HubName("MsgChat")]
    public class MsgHub : Hub
    {
        #region 用户的连接
        public class SocketUser
        {
            public string UserID { get; set; }

            public string ConnectionId { get; set; }
        }

        public static List<SocketUser> SocketUserList = new List<SocketUser>();

        public override Task OnConnected()
        {
            string userID;
            OperatorModel operatorUser = OperatorProvider.Provider.GetCurrent();
            if (operatorUser == null) //未登录
            {
                userID = Guid.NewGuid().ToString();
            }
            else
            {
                userID = operatorUser.UserId;
            }
            SocketUser socketUser = new SocketUser();
            socketUser.ConnectionId = Context.ConnectionId;
            socketUser.UserID = userID;

            SocketUserList.Add(socketUser);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            IList<SocketUser> userList = SocketUserList.Where(o=>o.ConnectionId == Context.ConnectionId).ToList();
            SocketUser socketUser = userList.FirstOrDefault();
            if (socketUser != null)
            {
                SocketUserList.Remove(socketUser);
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
        #endregion

        /// <summary>
        /// 异步推送
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> SendAllAsync(MsgType type, WebSocketResult webSocketResult)
        {
            IHubContext hub = GlobalHost.ConnectionManager.GetHubContext<MsgHub>();

            switch (type)
            {
                case MsgType.Test:
                    {
                        await hub.Clients.All.Test(webSocketResult);
                    }
                    break;
                case MsgType.NoReadNum:
                    {
                        await hub.Clients.All.NoReadNum(webSocketResult);
                    }
                    break;
                case MsgType.EnableLogin:
                    {
                        await hub.Clients.All.EnableLogin(webSocketResult);
                    }
                    break;                
            }
            return true;
        }

        /// <summary>
        /// 同步推送
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public void SendAll(MsgType type, WebSocketResult webSocketResult)
        {
            IHubContext hub = GlobalHost.ConnectionManager.GetHubContext<MsgHub>();

            switch (type)
            {
                case MsgType.Test:
                    {
                        hub.Clients.All.Test(webSocketResult);
                    }
                    break;
                case MsgType.NoReadNum:
                    {
                        hub.Clients.All.NoReadNum(webSocketResult);
                    }
                    break;
                case MsgType.EnableLogin:
                    {
                        hub.Clients.All.EnableLogin(webSocketResult);
                    }
                    break;
            }
        }

        /// <summary>
        /// 单用户推送
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public void SendSingle(string userID, MsgType type, WebSocketResult webSocketResult)
        {
            IHubContext hub = GlobalHost.ConnectionManager.GetHubContext<MsgHub>();
            SocketUser socUser = SocketUserList.FirstOrDefault(o => o.UserID == userID);
            if (socUser == null)
            {
                return;
            }

            switch (type)
            {
                case MsgType.Test:
                    {
                        hub.Clients.Client(socUser.ConnectionId).Test(webSocketResult);
                    }
                    break;
                case MsgType.NoReadNum:
                    {
                        hub.Clients.Client(socUser.ConnectionId).NoReadNum(webSocketResult);
                    }
                    break;
                case MsgType.EnableLogin:
                    {
                        hub.Clients.Client(socUser.ConnectionId).EnableLogin(webSocketResult);
                    }
                    break;               
            }
        }
    }
}