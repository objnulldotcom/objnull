//查找用户
function Search() {
    UserPage(1);
}

//查询ip地址
function ShowIPAddr() {
    $(".IP").each(function () {
        var ip = $(this).html();
        var addrTd = $(this).parent().find("#TxtIPAddr");
        if (ip.length > 7) {
            $.ajax({
                url: "http://int.dpool.sina.com.cn/iplookup/iplookup.php?format=js&ip=" + ip,
                type: "get",
                dataType: 'jsonp',
                async: false,
                complete: function (event, xhr, options) {
                    var addr = remote_ip_info.province + remote_ip_info.city + remote_ip_info.district;
                    $(addrTd).html(addr);
                }
            });
        }
    });
}

//用户查询
function UserPage(index) {
    var pageSize = parseInt($("#ValUserPageSize").val());
    var condition = $("#TxtCondition").val()
    var postData = { pageSize: pageSize, pageNum: index, condition: condition }
    $.ajax({
        url: "/Manager/UserPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#UserData").html(result);
            var totalCount = parseInt($("#UserTotalCount").val());
            if (totalCount <= pageSize) {
                return;
            }
            $("#UserPager").pagination({
                items: totalCount,
                itemsOnPage: pageSize,
                currentPage: $("#UserCurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination-sm",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    UserPage(pageNumber);
                }
            });
        }
    });
}

//操作用户
function UserOperate(type, uid, ot, i) {
    var days = $("#TxtDisableDay" + i).val();
    if (type == "启") {
        days = 0;
    }
    else if (isNaN(parseInt(days)) || parseInt(days) <= 0) {
        swal("请填写天数");
        return;
    }
    $.ajax({
        url: "/Manager/UserOperate",
        type: "Post",
        data: { type: type, uid: uid, objectType: ot, days: days },
        success: function (result) {
            if (result.msg == "done") {
                UserPage($("#UserCurrentPage").val());
                SendNewMsg(uid);
            } else {
                swal("系统错误");
            }
        }
    });
}

//查找Blog
function SearchBlog() {
    BlogPage(1);
}

//Blog查询
function BlogPage(index) {
    var pageSize = parseInt($("#ValBlogPageSize").val());
    var condition = $("#TxtBlogCondition").val()
    var postData = { pageSize: pageSize, pageNum: index, condition: condition }
    $.ajax({
        url: "/Manager/BlogPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#BlogData").html(result);
            var totalCount = parseInt($("#BlogTotalCount").val());
            if (totalCount <= pageSize) {
                return;
            }
            $("#BlogPager").pagination({
                items: totalCount,
                itemsOnPage: pageSize,
                currentPage: $("#BlogCurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination-sm",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    BlogPage(pageNumber);
                }
            });
        }
    });
}

//删除Blog
function BlogDelete(id, uid, page) {
    swal({
        title: "确定永久删除？",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#337ab7",
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        closeOnConfirm: true
    }, function () {
        $.ajax({
            url: "/Manager/BlogDelete",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.msg == "done") {
                    SendNewMsg(uid);
                    BlogPage(page);
                } else {
                    alert(result.msg);
                }
            }
        });
    });
}

//编辑Blog
function BlogEdit(id) {
    $.ajax({
        url: "/Manager/BlogEidt",
        type: "Post",
        success: function (result) {
            window.open("/Home/BlogManagerEdit?id=" + id + "&keyId=" + result.key);
        }
    });
}

//查找NewBee
function SearchNewBee() {
    NewBeePage(1);
}

//NewBee查询
function NewBeePage(index) {
    var pageSize = parseInt($("#ValNewBeePageSize").val());
    var condition = $("#TxtNewBeeCondition").val()
    var postData = { pageSize: pageSize, pageNum: index, condition: condition }
    $.ajax({
        url: "/Manager/NewBeePage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#NewBeeData").html(result);
            var totalCount = parseInt($("#NewBeeTotalCount").val());
            if (totalCount <= pageSize) {
                return;
            }
            $("#NewBeePager").pagination({
                items: totalCount,
                itemsOnPage: pageSize,
                currentPage: $("#NewBeeCurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination-sm",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    NewBeePage(pageNumber);
                }
            });
        }
    });
}

//删除NewBee
function NewBeeDelete(id, uid, page) {
    swal({
        title: "确定永久删除？",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#337ab7",
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        closeOnConfirm: true
    }, function () {
        $.ajax({
            url: "/Manager/NewBeeDelete",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.msg == "done") {
                    SendNewMsg(uid);
                    NewBeePage(page);
                } else {
                    alert(result.msg);
                }
            }
        });
    });
}

//置顶NewBee
function NewBeeTop(id, page) {
    swal({
        title: "确定置顶？",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#337ab7",
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        closeOnConfirm: true
    }, function () {
        $.ajax({
            url: "/Manager/NewBeeTop",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.msg == "done") {
                    NewBeePage(page);
                } else {
                    alert(result.msg);
                }
            }
        });
    });
}

var MsgHub;
//发送更新消息
function SendNewMsg(id) {
    MsgHub.server.sendToUser(id, "NewMsg");
}

$(function () {
    //SignalR消息
    MsgHub = $.connection.MsgHub;
    //启动SignalR不可少
    $.connection.hub.start().done(function () {

    });

    UserPage(1);
    BlogPage(1);
    NewBeePage(1);
})