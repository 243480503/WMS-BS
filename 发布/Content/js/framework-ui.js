$(function () {
    document.body.className = localStorage.getItem('config-skin');
    $("[data-toggle='tooltip']").tooltip();
})
$.reload = function () {
    location.reload();
    return false;
}
$.loading = function (bool, text) {
    var $loadingpage = top.$("#loadingPage");
    var $loadingtext = $loadingpage.find('.loading-content');
    if (bool) {
        $loadingpage.show();
    } else {
        if ($loadingtext.attr('istableloading') == undefined) {
            $loadingpage.hide();
        }
    }
    if (!!text) {
        $loadingtext.html(text);
    } else {
        $loadingtext.html("数据加载中，请稍后…");
    }
    $loadingtext.css("left", (top.$('body').width() - $loadingtext.width()) / 2 - 50);
    $loadingtext.css("top", (top.$('body').height() - $loadingtext.height()) / 2);
}
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
$.currentWindow = function () { //当前激活的tab标签页
    var iframeId = top.$(".MST_iframe:visible").attr("id");
    return top.frames[iframeId];
}
$.browser = function () {
    var userAgent = navigator.userAgent;
    var isOpera = userAgent.indexOf("Opera") > -1;
    if (isOpera) {
        return "Opera"
    };
    if (userAgent.indexOf("Firefox") > -1) {
        return "FF";
    }
    if (userAgent.indexOf("Chrome") > -1) {
        if (window.navigator.webkitPersistentStorage.toString().indexOf('DeprecatedStorageQuota') > -1) {
            return "Chrome";
        } else {
            return "360";
        }
    }
    if (userAgent.indexOf("Safari") > -1) {
        return "Safari";
    }
    if (userAgent.indexOf("compatible") > -1 && userAgent.indexOf("MSIE") > -1 && !isOpera) {
        return "IE";
    };
}

$.download = function (url, data, method) {
    if (url && data) {
        data = typeof data == 'string' ? data : jQuery.param(data);
        var inputs = '';
        $.each(data.split('&'), function () {
            var pair = this.split('=');
            inputs += '<input type="hidden" name="' + pair[0] + '" value="' + pair[1] + '" />';
        });
        $('<form action="' + url + '" method="' + (method || 'post') + '">' + inputs + '</form>').appendTo('body').submit().remove();
    };
};
$.modalOpen = function (options) {
    var defaults = {
        id: null,
        title: '系统窗口',
        width: "100px",
        height: "100px",
        url: '',
        shade: 0.3,
        btn: ['确认', '关闭'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
        callBack: null
    };
    var options = $.extend(defaults, options);
    var _width = top.$(window).width() > parseInt(options.width.replace('px', '')) ? options.width : top.$(window).width() + 'px';
    var _height = top.$(window).height() > parseInt(options.height.replace('px', '')) ? options.height : top.$(window).height() + 'px';
    top.layer.open({
        id: options.id,
        type: 2,
        shade: options.shade,
        title: options.title,
        fix: false,
        area: [_width, _height],
        content: options.url,
        btn: options.btn,
        btnclass: options.btnclass,
        yes: function () {
            options.callBack(options.id)
        }, cancel: function () {
            return true;
        }
    });
}

$.modalFixHightOpen = function (options) {
    var defaults = {
        id: null,
        title: '系统窗口',
        width: "100px",
        height: "100px",
        url: '',
        shade: 0.3,
        btn: ['确认', '关闭'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
        callBack: null
    };
    var options = $.extend(defaults, options);
    var _width = options.width;//top.$(window).width() > parseInt(options.width.replace('px', '')) ? options.width : top.$(window).width() + 'px';
    var _height = options.height;// top.$(window).height() > parseInt(options.height.replace('px', '')) ? options.height : top.$(window).height() + 'px';
    top.layer.open({
        id: options.id,
        type: 2,
        shade: options.shade,
        title: options.title,
        fix: false,
        area: [_width, _height],
        content: options.url,
        btn: options.btn,
        btnclass: options.btnclass,
        yes: function () {
            options.callBack(options.id)
        }, cancel: function () {
            return true;
        }
    });
}

//三个按钮
$.modalFixHightOpen3Btn = function (options) {
    var defaults = {
        id: null,
        title: '系统窗口',
        width: "100px",
        height: "100px",
        url: '',
        shade: 0.3,
        btn: ['确认', '关闭'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
        callBack: null,
        btn2: null
    };
    var options = $.extend(defaults, options);
    var _width = options.width;//top.$(window).width() > parseInt(options.width.replace('px', '')) ? options.width : top.$(window).width() + 'px';
    var _height = options.height;// top.$(window).height() > parseInt(options.height.replace('px', '')) ? options.height : top.$(window).height() + 'px';
    top.layer.open({
        id: options.id,
        type: 2,
        shade: options.shade,
        title: options.title,
        fix: false,
        area: [_width, _height],
        content: options.url,
        btn: options.btn,
        btnclass: options.btnclass,
        yes: function () {
            options.callBack(options.id)
        },
        btn2: function () {
            options.btn2(options.id);
            return false;
        },
        cancel: function () {
            return true;
        }
    });
}

$.modalConfirm = function (content, callBack) {
    top.layer.confirm(content, {
        icon: "fa-exclamation-circle",
        title: "系统提示",
        btn: ['确认', '取消'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
    }, function () {
        callBack(true);
    }, function () {
        callBack(false)
    });
}
$.modalAlert = function (content, type) {
    var icon = "";
    if (type == 'success') {
        icon = "fa-check-circle";
    }
    if (type == 'error') {
        icon = "fa-times-circle";
    }
    if (type == 'warning') {
        icon = "fa-exclamation-circle";
    }
    top.layer.alert(content, {
        icon: icon,
        title: "系统提示",
        btn: ['确认'],
        btnclass: ['btn btn-primary'],
    });
}
$.modalMsg = function (content, type) {
    if (type != undefined) {
        var icon = "";
        if (type == 'success') {
            icon = "fa-check-circle";
        }
        if (type == 'error') {
            icon = "fa-times-circle";
        }
        if (type == 'warning') {
            icon = "fa-exclamation-circle";
        }
        top.layer.msg(content, { icon: icon, time: 2000, shift: 5 });
        top.$(".layui-layer-msg").find('i.' + icon).parents('.layui-layer-msg').addClass('layui-layer-msg-' + type);
    } else {
        top.layer.msg(content);
    }
}

//关闭当前窗体
$.modalClose = function () {
    var index = top.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
    var $IsdialogClose = top.$("#layui-layer" + index).find('.layui-layer-btn').find("#IsdialogClose");
    var IsClose = $IsdialogClose.is(":checked");
    if ($IsdialogClose.length == 0) {
        IsClose = true;
    }
    if (IsClose) {
        top.layer.close(index);
    } else {
        location.reload();
    }
}


$.submitForm = function (options) {
    var defaults = {
        url: "",
        param: [],
        loading: "正在提交数据...",
        success: null,
        close: true
    };
    var options = $.extend(defaults, options);
    options.traditional == "" ? false : options.traditional;
    $.loading(true, options.loading);
    window.setTimeout(function () {
        if ($('[name=__RequestVerificationToken]').length > 0) {
            options.param["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
        }
        $.ajax({
            url: options.url,
            data: options.param,
            type: "post",
            traditional: options.traditional,
            dataType: "json",
            success: function (data) {
                $.checkLogin(data);
                if (data.state == "success") {
                    options.success(data);
                    $.modalMsg(data.message, data.state);
                    if (options.close == true) {
                        $.modalClose();
                    }
                } else {
                    if (options.error != null) {
                        options.error(data);
                    }
                    $.modalMsg(data.message, data.state);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $.loading(false);
                $.modalMsg(errorThrown, "error");
            },
            beforeSend: function () {
                $.loading(true, options.loading);
            },
            complete: function () {
                $.loading(false);
            }
        });
    }, 500);
}
$.deleteForm = function (options) {
    var defaults = {
        prompt: "注：您确定要删除该项数据吗？",
        url: "",
        param: [],
        loading: "正在删除数据...",
        success: null,
        close: true
    };
    var options = $.extend(defaults, options);
    if ($('[name=__RequestVerificationToken]').length > 0) {
        options.param["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    }
    $.modalConfirm(options.prompt, function (r) {
        if (r) {
            $.loading(true, options.loading);
            window.setTimeout(function () {
                $.ajax({
                    url: options.url,
                    data: options.param,
                    type: "post",
                    dataType: "json",
                    success: function (data) {
                        $.checkLogin(data);
                        if (data.state == "success") {
                            options.success(data);
                            $.modalMsg(data.message, data.state);
                        } else {
                            $.modalAlert(data.message, data.state);
                        }
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        $.loading(false);
                        $.modalMsg(errorThrown, "error");
                    },
                    beforeSend: function () {
                        $.loading(true, options.loading);
                    },
                    complete: function () {
                        $.loading(false);
                    }
                });
            }, 500);
        }
    });

}

$.ajaxWMS = function (options) {
    var postdata = options["data"];
    if ($('[name=__RequestVerificationToken]').length > 0) {
        postdata["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    }
    var fun = options.success;
    options.success = function (data) {
        $.checkLogin(data);
        fun(data);
    };
    $.ajax(options);
}

$.jsonWhere = function (data, action) {
    if (action == null) return;
    var reval = new Array();
    $(data).each(function (i, v) {
        if (action(v)) {
            reval.push(v);
        }
    })
    return reval;
}
$.fn.jqGridRowValue = function () {
    var $grid = $(this);
    var selectedRowIds = $grid.jqGrid("getGridParam", "selarrrow");
    if (selectedRowIds != "") {
        var json = [];
        var len = selectedRowIds.length;
        for (var i = 0; i < len; i++) {
            var rowData = $grid.jqGrid('getRowData', selectedRowIds[i]);
            json.push(rowData);
        }
        return json;
    } else {
        return $grid.jqGrid('getRowData', $grid.jqGrid('getGridParam', 'selrow'));
    }
}
//fxx
$.fn.jqGridTransferRowValue = function () {
    var $grid = $(this);
    var selectedRowIds = $grid.jqGrid("getGridParam", "selarrrow");
    var json = [];
    var len = selectedRowIds.length;
    for (var i = 0; i < len; i++) {
        var rowData = $grid.jqGrid('getRowData', selectedRowIds[i]);
        json.push(rowData);
    }
    return json;
}

$.fn.formValid = function () {
    return $(this).valid({
        errorPlacement: function (error, element) {
            element.parents('.formValue').addClass('has-error');
            element.parents('.has-error').find('i.error').remove();
            element.parents('.has-error').append('<i class="form-control-feedback fa fa-exclamation-circle error" data-placement="left" data-toggle="tooltip" title="' + error + '"></i>');
            $("[data-toggle='tooltip']").tooltip();
            if (element.parents('.input-group').hasClass('input-group')) {
                element.parents('.has-error').find('i.error').css('right', '33px')
            }
        },
        success: function (element) {
            element.parents('.has-error').find('i.error').remove();
            element.parent().removeClass('has-error');
        }
    });
}

//selectMulIDAndKey为多个多选值的配置列表，例如 [{ "id": "StoredAreaList","key":"AreaID"}]，表示控件ID为StoredAreaList的多选下拉框使用date中的StoredAreaList对象的AreaID做为初始化选中值
$.fn.formSerialize = function (formdate, selectMulIDAndKey) {
    var element = $(this);
    if (!!formdate) {
        for (var key in formdate) {
            var $id = element.find('#' + key);
            var value = $.trim(formdate[key]).replace(/&nbsp;/g, '');
            var type = $id.attr('type');
            if ($id.hasClass("select2-hidden-accessible")) {

                if ($id.attr("multiple") == "multiple") { //多选
                    type = "selectmul";
                }
                else {   //单选
                    type = "select";
                }
            }

            switch (type) {
                case "checkbox":
                    if (value == "true") {
                        $id.attr("checked", 'checked');
                    } else {
                        $id.removeAttr("checked");
                    }
                    break;
                case "select":
                    $id.val(value).trigger("change");
                    break;
                case "selectmul":
                    var valArray = [];
                    var keyForselectMul = "";
                    $.each(selectMulIDAndKey, function (i, n) {
                        if (n.id == key) {
                            keyForselectMul = n.key;
                        }
                    });
                    if (formdate[key] != null) {
                        $.each(formdate[key], function (i, n) {
                            valArray.push(n[keyForselectMul]);
                        })
                        $id.val(valArray).trigger("change");
                    }
                    break;
                default:
                    $id.val(value);
                    break;
            }
        };
        return false;
    }
    var postdata = {};
    element.find('input,select,textarea').each(function (r) {
        var $this = $(this);
        var id = $this.attr('id');
        var type = $this.attr('type');
        switch (type) {
            case "checkbox":
                postdata[id] = $this.is(":checked");
                break;
            case "select2":
                postdata[id] = $this.val();
                break;
            default:
                var value = $this.val() == "" ? "&nbsp;" : $this.val();
                if (!$.request("keyValue")) {
                    value = value.replace(/&nbsp;/g, '');
                }
                postdata[id] = value;
                break;
        }
    });
    if ($('[name=__RequestVerificationToken]').length > 0) {
        postdata["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    }
    return postdata;
};
$.fn.bindSelect = function (options) {
    var defaults = {
        id: "id",
        text: "text",
        search: false,
        url: "",
        attr: "",
        param: [],
        change: null
    };
    var options = $.extend(defaults, options);
    var $element = $(this);
    if (options.url != "") {
        $.ajax({
            url: options.url,
            data: options.param,
            dataType: "json",
            async: false,
            success: function (data) {
                $.checkLogin(data);
                $.each(data, function (i) {
                    var opt = $("<option></option>").val(data[i][options.id]).html(data[i][options.text]);
                    if (options.attr != "") {
                        opt.attr(options.attr, data[i][options.attr]);
                    }
                    $element.append(opt);
                });
                $element.select2({
                    minimumResultsForSearch: options.search == true ? 0 : -1
                });
                $element.on("change", function (e) {
                    if (options.change != null) {
                        options.change(data[$(this).find("option:selected").index()]);
                    }
                    $("#select2-" + $element.attr('id') + "-container").html($(this).find("option:selected").text().replace(/　　/g, ''));
                });
            }
        });
    } else {
        $element.select2({
            minimumResultsForSearch: -1
        });
    }
};

$.fn.clearSelect = function (options) {
    var $element = $(this);
    $element.find("option").each(function (i, e) {
        if ($(e).text().indexOf("请选择") < 0) {
            $(e).remove();
        }
    });
    $element.val("").select2({ minimumResultsForSearch: -1 });
};

$.fn.authorizeButton = function () {
    var moduleId = top.$(".MST_iframe:visible").attr("id").substr(6);
    var dataJson = top.clients.authorizeButton[moduleId];
    var $element = $(this);
    $element.find('a[authorize=yes]').attr('authorize', 'no');
    if (dataJson != undefined) {
        $.each(dataJson, function (i) {
            $element.find("#" + dataJson[i].F_EnCode).attr('authorize', 'yes');
        });
    }
    $element.find("[authorize=no]").parents('li').prev('.split').remove();
    $element.find("[authorize=no]").parents('li').remove();
    $element.find('[authorize=no]').remove();
}

$.checkLogin = function (data) {
    if (data == "ajax_overdue") {
        top.window.location.href = "/Login/Index";
    }
};

//隐藏基础数据复选框
$.hideBaseData = function () {
    $.ajax({
        url: "../../SystemManage/User/IsSysUser",
        data: {},
        dataType: "json",
        async: false,
        success: function (data) {
            $.checkLogin(data);
            if (data.isSys == "false") {
                var $element = $("table[class='form']");
                $element.find('input[sysbasedata=yes]').attr("disabled", "disabled");
            }
        }
    });
}

//单选表格
$.fn.dataGrid = function (options) {
    var defaults = {
        datatype: "json",
        autowidth: true,
        rownumbers: true,
        shrinkToFit: false,
        gridview: true
    };
    var options = $.extend(defaults, options);
    var $element = $(this);
    var selectFun = options["onSelectRow"];
    options["onSelectRow"] = function (rowid) {
        var length = $(this).jqGrid("getGridParam", "selrow") == null ? 0 : $(this).jqGrid("getGridParam", "selrow").length;
        var $operate = $(".operate");
        if (length > 0) {
            $operate.animate({ "left": 0 }, 200);
        } else {
            $operate.animate({ "left": '-100.1%' }, 200);
        }
        $operate.find('.close').click(function () {
            $operate.animate({ "left": '-100.1%' }, 200);
        })
        if (options["customEdit"] == true) {
            $element.editRow(rowid);
        }
        if (selectFun != null) {
            selectFun(rowid);
        }
    };
    $element.jqGrid(options);
};

//绑定RF界面的列表页面
$.fn.RFBindList = function (options) {
    var defaults = {
        showDel: true,
        rowHeight: 30,
    }

    options = $.extend(defaults, options);
    var doc = $(this);
    doc.addClass("RF_List_Box");
    doc.empty();
    var par = JSON.stringify(options, (k, v) => {
        if (typeof v === "function") {
            return v.toString();
        }
        else {
            return v;
        }
    });
    doc.attr("Par", par);
    if (options["datatype"] == "local") {
        var data = options["data"];
        BindRFDataRow(data, options, doc);
    }
    else {
        $.ajax({
            url: options["url"],
            data: options["data"],
            dataType: "json",
            async: false,
            success: function (data) {
                $.checkLogin(data);
                if (data != null && data.length > 0) {
                    BindRFDataRow(data, options, doc);
                }
                else {
                    doc.append("<div class='RF_Empty'>无数据</div>");
                }
            }
        });
    }

};

function BindRFDataRow(data, options, doc) {
    $.each(data, function (i, n) {
        var row = $("<div class='RF_Row' RID = '" + n.F_Id + "'></div>");
        $.each(options.colModel, function (j, k) {
            var title = k["label"];
            var val = n[k["name"]];
            row.append("<span class='RF_Title'>" + title + ":</span><span class='RF_Value'>" + (val == null ? "无" : val) + "</span>");

            var br = k["br"]; //换行
            if (br == true) {
                row.append("<br/>");
            }
        })

        if (options.delbtn == true) {
            var del = $("<span DelID='" + n.F_Id + "' class='RF_Del fa fa-remove'></span>");
            row.append(del);

            del.bind("click", function () {
                var delID = $(this).attr("DelID");
                options.delfun(delID);
            });
        }
        doc.append(row);
    });
}

$.fn.RFListReload = function () {
    var par = JSON.parse($(this).attr("Par"), function (n, v) {
        if (typeof v == "string" && v.indexOf("function") == 0) {
            return window.eval('(' + v + ')');
        }
        else {
            return v;
        }
    });
    $(this).RFBindList(par);
}