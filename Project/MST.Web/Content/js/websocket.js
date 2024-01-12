
function InitSignalR() {

    $.connection.hub.logging = true;
    var chat = $.connection.MsgChat;//这里的MsgChat为服务器上的继承了HUB的类的HubName。
    window.top.SignalRFun = {};

    window.top.SignalRFun.Test = function () { }; //SignalR测试消息
    window.top.SignalRFun.NoReadNum = function () { };  //未读消息
    window.top.SignalRFun.EnableLogin = function () { }; //账号禁用

    chat.client.Test = function (msg) {
        window.top.SignalRFun.Test(msg)
    };

    chat.client.NoReadNum = function (msg) {
        window.top.SignalRFun.NoReadNum(msg)
    };

    chat.client.EnableLogin = function (msg) {
        window.top.SignalRFun.EnableLogin(msg)
    };


    $.connection.hub.start().done(function () {
        console.log("SignalR已连接");
    });

    $.connection.hub.disconnected(function () {
        console.log("SignalR连接断开");
        //重连
        setTimeout(function () {
            $.connection.hub.start()
                .done(function () { console.log('SignalR重连成功'); })
                .fail(function () { console.log('SignalR重连失败'); });
        }, 1000);
    });

    return chat;
}