//全局调用-------------------------------------------------
var viewer;
//设置图片查看器
function SetViewer(id) {
    if (viewer != null) {
        viewer.destroy();
    }
    viewer = new Viewer(document.getElementById(id), null);
}
//设置临时图片查看器
function SetTempViewer(id) {
   var v = new Viewer(document.getElementById(id), null);
}

//获取字节数
function getBt(str) {
    var char = str.replace(/[^\x00-\xff]/g, '**');
    return char.length;
}

var MsgHub;
//发送更新消息
function SendNewMsg(id) {
    MsgHub.server.sendToUser(id, "NewMsg");
}

//初始化-----------------------------------------------------
$(function () {
    //未登录不初始化signalr及消息等事件
    if ($("#CurrentUserID").val() == "") {
        return;
    }
    //SignalR消息
    MsgHub = $.connection.MsgHub;
    MsgHub.client.addNewMessage = function (message) {
        if (message == "NewMsg") {
            UpdateMsgCount();
        }
    };
    $.connection.hub.start().done(function () {

    });

    //显示或隐藏消息
    $("#BtnMsg").click(function () {
        if ($("#MsgBox").is(":hidden")) {
            ShowMsgBox(true);
            UpdateMsgCount();
            if ($("#MsgCount").html() == "") {
                $("#MsgBox").html($("#NoMsgHtml").html());
                return;
            }
            $("#MsgBox").html($("#LoadingHtml").html());
            $.ajax({
                url: "/Home/GetMsg",
                type: "get",
                success: function (result) {
                    $("#MsgBox").html(result);
                    $(".MsgBar").each(function () {
                        $(this).hover(function () {
                            $(this).find("#BtnDelMsg").show();
                        }, function () {
                            $(this).find("#BtnDelMsg").hide();
                        });
                    });
                }
            });
        } else {
            ShowMsgBox(false);
        }
    });

    //点击消息框外隐藏消息框
    $(document).mouseup(function (e) {
        var container = $("#MsgBox");
        if (!container.is(e.target) && container.has(e.target).length === 0 && !$("#BtnMsg").is(e.target)) {
            ShowMsgBox(false);
        }
    });

    //更新账号
    $("#BtnUpdateInfo").click(function () {
        $.ajax({
            url: "/OAuth/UpdateInfo",
            type: "post",
            success: function (result) {
                if (result.msg == "done") {
                    window.location.reload();
                } else {
                    swal("更新失败");
                }
            }
        });
    });
});

//ajax消息页面调用---------------------------------------------
//切换显示消息框
function ShowMsgBox(type) {
    if (type) {
        $("#MsgLi").css("background-color", "#e7e7e7");
        $("#MsgBox").slideDown(60);
    } else {
        $("#MsgLi").css("background-color", "");
        $("#MsgBox").slideUp(60);
    }
}

//更新消息数量
function UpdateMsgCount() {
    $.ajax({
        url: "/Home/GetMsgCount",
        type: "get",
        success: function (result) {
            result = result == "0" ? "" : result;
            $("#MsgCount").html(result);
        }
    });
}

//删除评论消息
function DelCMsg(objID, order) {
    $.ajax({
        url: "/Home/DeleteCMsg",
        type: "post",
        data: { objID: objID },
        success: function (result) {
            UpdateMsgCount();
            if (order > 0) {
                $("#CMsgBar" + order).remove();
            }
        }
    });
}

//查看评论消息
function CheckCMsg(url, objID) {
    DelCMsg(objID, 0);
    window.location.href = url;
}

//删除回复消息
function DelRMsg(objID, co, ro, order) {
    $.ajax({
        url: "/Home/DeletRMsg",
        type: "post",
        data: { objID: objID, co: co, ro: ro },
        success: function (result) {
            UpdateMsgCount();
            if (order > 0) {
                $("#RMsgBar" + order).remove();
            }
        }
    });
}

//查看回复消息
function CheckRMsg(url, objID, co, ro) {
    DelRMsg(objID, co, ro, 0);
    window.location.href = url;
}

//清空消息
function ClearMsg() {
    $.ajax({
        url: "/Home/ClearMsg",
        type: "post",
        success: function (result) {
            if (result.msg == "done") {
                ShowMsgBox(false);
                UpdateMsgCount();
            } else {
                swal(result.msg)
            }
        }
    });
}
