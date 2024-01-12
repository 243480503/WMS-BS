document.addEventListener('plusready', function () {
    var webview = plus.webview.currentWebview();
    plus.key.addEventListener('backbutton', function () {
        webview.canBack(function (e) {
            //首页返回键处理
            //处理逻辑：1秒内，连续两次按返回键，则退出应用；
            var first = null;
            first = new Date().getTime();
            layer.msg('再按一次退出应用', { time: 3000 }, function () {
                setTimeout(function () {
                    first = null;
                }, 1000);
            });
            plus.key.addEventListener('backbutton', function () {
                //当第二次点击时候跟第一次点击的事件做对比（小于3秒内直接退出）
                if (new Date().getTime() - first < 3000) {
                    plus.runtime.quit();
                }
            }, false);
        })
    });
});

$(function () {
    $(".RFback,#NF-Back").bind("click", function () {
        history.back();
        //window.location.href = document.referrer;
    });

    $(".RFLogout").bind("click", function () {
        $.modalConfirm("确定退出吗？", function (r) {
            if (r) {
                location.href = "../../Login/OutLogin";
            }
        })
    });

    $(".RFRef").bind("click", function () {
        location.reload();
    });
});


$.request = function (name) {
    var search = location.search.slice(1);
    var arr = search.split("&");
    for (var i = 0; i < arr.length; i++) {
        var ar = arr[i].split("=");
        if (ar[0] == name) {
            if (unescape(ar[1]) == 'undefined') {
                return "";
            } else {
                return unescape(ar[1]);
            }
        }
    }
    return "";
}
