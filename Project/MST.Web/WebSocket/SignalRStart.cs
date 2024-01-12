using Microsoft.Owin;
using MST.Web.WebSocket;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(SignalRStart.Startup))]

namespace MST.Web.WebSocket
{
    public class SignalRStart
    {
        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
                // 配置集线器
                app.MapSignalR();
            }
        }
    }
}