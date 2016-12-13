﻿//删除姿势
function BlogDelete(id, page) {
    swal({
        title: "确定永久删除？",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#337ab7",
        confirmButtonText: "确定",
        closeOnConfirm: true
    }, function () {
        $.ajax({
            url: "/Home/BlogDelete",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.msg == "done") {
                    UserBlogPage(page);
                } else {
                    swal(result.msg);
                }
            }
        });
    });
}

//用户姿势查询
function UserBlogPage(index) {
    var pageSize = parseInt($("#ValBlogPageSize").val());
    var postData = { uid: $("#ValUserID").val(), pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/UserBlogPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#UserData").html(result);
            $(".BlogInfo").each(function () {
                $(this).hover(function () {
                    $(this).find("#TxtModify").show();
                    $(this).css("background", "#f9f9f9")
                }, function () {
                    $(this).find("#TxtModify").hide();
                    $(this).css("background", "#ffffff")
                });
            });
            var totalCount = parseInt($("#TotalCount").val());
            if (totalCount <= pageSize) {
                return;
            }
            $("#UserPager").pagination({
                items: totalCount,
                itemsOnPage: pageSize,
                currentPage: $("#CurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination-sm",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    UserBlogPage(pageNumber);
                }
            });
        }
    });
}

//删除收藏
function StarDelete(id, page, type) {
    swal({
        title: "确定删除该收藏？",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#337ab7",
        confirmButtonText: "确定",
        closeOnConfirm: true
    }, function () {
        $.ajax({
            url: "/Home/StarDelete",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.msg == "done") {
                    UserStarPage(page, type);
                } else {
                    swal(result.msg);
                }
            }
        });
    });
}

//用户收藏查询
function UserStarPage(index, type) {
    var pageSize = parseInt($("#ValStarPageSize").val());
    var postData = { pageSize: pageSize, pageNum: index, type: type }
    $.ajax({
        url: "/Home/UserStarPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#UserData").html(result);
            $(".StarInfo").each(function () {
                $(this).hover(function () {
                    $(this).find("#TxtModify").show();
                    $(this).css("background", "#f9f9f9")
                }, function () {
                    $(this).find("#TxtModify").hide();
                    $(this).css("background", "#ffffff")
                });
            });
            var totalCount = parseInt($("#TotalCount").val());
            if (totalCount <= pageSize) {
                return;
            }
            $("#UserPager").pagination({
                items: totalCount,
                itemsOnPage: pageSize,
                currentPage: $("#CurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination-sm",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    UserStarPage(pageNumber, type);
                }
            });
        }
    });
}

//用户消息查询
function UserMsgPage(index, type) {
    var pageSize = 10;
    var postData = { pageSize: pageSize, pageNum: index, type: type }
    $.ajax({
        url: "/Home/UserMsgPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#UserData").html(result);
            $(".StarInfo").each(function () {
                $(this).hover(function () {
                    $(this).find("#TxtModify").show();
                    $(this).css("background", "#f9f9f9")
                }, function () {
                    $(this).find("#TxtModify").hide();
                    $(this).css("background", "#ffffff")
                });
            });
            var totalCount = parseInt($("#TotalCount").val());
            if (totalCount <= pageSize) {
                return;
            }
            $("#UserPager").pagination({
                items: totalCount,
                itemsOnPage: pageSize,
                currentPage: $("#CurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination-sm",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    UserMsgPage(pageNumber, type);
                }
            });
        }
    });
}

$(function () {
    //关注或取关
    $("#BtnFollow").click(function () {
        var atype = "";
        if ($("#TxtFollow").html() == "Follow") {
            atype = "put";
        } else {
            atype = "delete";
        }
        $("#BtnFollow").attr("disabled", true);
        $.ajax({
            url: "https://api.github.com/user/following/" + $("#ValGitHubLogin").val() + "?access_token=" + $("#ValUserToken").val(),
            type: atype,
            success: function (result) {
                if ($("#TxtFollow").html() == "Follow") {
                    $("#TxtFollow").html("Unfollow");
                    $("#IcoFollow").removeClass("glyphicon-plus");
                    $("#IcoFollow").addClass("glyphicon-minus");
                } else {
                    $("#TxtFollow").html("Follow");
                    $("#IcoFollow").removeClass("glyphicon-minus");
                    $("#IcoFollow").addClass("glyphicon-plus");
                }
                $("#BtnFollow").attr("disabled", false);
            }
        });
    });

    //姿势
    $("#BtnBlog").click(function () {
        $("#UserNav li").removeClass("active");
        $(this).parent().addClass("active");
        UserBlogPage(1);
    });

    //收藏
    $("#BtnStar").click(function () {
        $("#UserNav li").removeClass("active");
        $(this).parent().addClass("active");
        UserStarPage(1, 1);
    });

    //消息
    $("#BtnMyMsg").click(function () {
        $("#UserNav li").removeClass("active");
        $(this).parent().addClass("active");
        UserMsgPage(1, "replyme");
    });

    //显示姿势
    $("#BtnBlog").click();
});