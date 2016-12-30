//删除姿势
function BlogDelete(id, page) {
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
            url: "/Home/BlogDelete",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.msg == "done") {
                    UserBlogPage(page);
                } else {
                    alert(result.msg);
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

//删除NewBee
function NewBeeDelete(id, page) {
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
            url: "/Home/NewBeeDelete",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.msg == "done") {
                    UserNewBeePage(page);
                } else {
                    alert(result.msg);
                }
            }
        });
    });
}

//用户NewBee查询
function UserNewBeePage(index) {
    var pageSize = parseInt($("#ValNewBeePageSize").val());
    var postData = { uid: $("#ValUserID").val(), pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/UserNewBeePage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#UserData").html(result);
            $(".NewBeeInfo").each(function () {
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
        cancelButtonText: "取消",
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
    var pageSize = parseInt($("#ValMsgPageSize").val());
    var postData = { pageSize: pageSize, pageNum: index, type: type }
    $.ajax({
        url: "/Home/UserMsgPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#UserData").html(result);
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

//修改邮箱
function EmailEdit() {
    $("#DivEmail").hide();
    $("#DivEmailEdit").show();
}

//取消修改邮箱
function EmailCancelEdit() {
    $("#DivEmail").show();
    $("#DivEmailEdit").hide();
}

//设置邮箱
function SetNewEmail() {
    $("#BtnSetEmail").attr("disabled", true);
    $.ajax({
        url: "/Home/SetEmail",
        type: "Post",
        data: { email: $("#TxtNewEmail").val() },
        success: function (result) {
            $("#BtnSetEmail").attr("disabled", false);
            if (result.msg == "done") {
                window.location.reload();
            } else {
                swal(result.msg);
            }
        }
    });
}

$(function () {
    //检查是否已关注
    if ($("#ValOwner").val() == "0") {
        $.ajax({
            url: "https://api.github.com/user/following/" + $("#ValGitHubLogin").val() + "?access_token=" + $("#ValUserToken").val(),
            type: "Get",
            complete: function (xhr, textStatus) {
                $("#BtnFollow").show();
                if (xhr.status == "204") {
                    $("#TxtFollow").html("Unfollow");
                    $("#IcoFollow").removeClass("glyphicon-plus");
                    $("#IcoFollow").addClass("glyphicon-minus");
                } else {
                    $("#TxtFollow").html("Follow");
                    $("#IcoFollow").removeClass("glyphicon-minus");
                    $("#IcoFollow").addClass("glyphicon-plus");
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                if (xhr.status == 404) {
                    console.log("↑这个错误说明你没有关注该用户，不要问我为啥不能去掉这个错误，因为github返回的就是404。");
                }
            }
        });
    }

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

    //点赞
    $("#BtnPro").click(function () {
        $("#BtnPro").attr("disabled", true);
        $.ajax({
            url: "/Home/UserPro",
            type: "Post",
            data: { id: $("#ValUserID").val() },
            success: function (result) {
                $("#BtnPro").attr("disabled", false);
                if (result.msg == "done") {
                    $("#BtnPro").hide();
                    $("#TxtProCount").html(parseInt($("#TxtProCount").html()) + 1);
                }else {
                    swal(result.msg);
                }
            }
        });
    });

    //姿势
    $("#BtnBlog").click(function () {
        $("#UserNav li").removeClass("active");
        $(this).parent().addClass("active");
        UserBlogPage(1);
    });

    //NewBee
    $("#BtnNewBee").click(function () {
        $("#UserNav li").removeClass("active");
        $(this).parent().addClass("active");
        UserNewBeePage(1);
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
        UserMsgPage(1, "Breply");
    });

    //显示姿势
    $("#BtnBlog").click();

    //悬停显示修改邮箱
    $("#DivEmail").hover(function () {
        $("#DivEmail").find("#BtnEdit").show();
    }, function () {
        $("#DivEmail").find("#BtnEdit").hide();
    });
});