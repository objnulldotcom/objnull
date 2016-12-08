$(function () {
    SetViewer("MDValue");
    $("#CommentBox").hide();
    $("#PreBox").hide();
    GetCommentPage(1);
    //代码高亮插件
    marked.setOptions({
        highlight: function (code) {
            return hljs.highlightAuto(code).value;
        },
        sanitize: true
    });

    //评论
    $("#BtnComment").click(function () {
        $("#CommentBox").show();
        $(document).scrollTop(9999);
    });
    //取消
    $("#BtnCancel").click(function () {
        $("#CommentBox").hide();
    });
    //评论面板
    $("#BtnCmt").click(function () {
        $("#BtnCmt").parent().removeClass("active");
        $("#BtnPre").parent().removeClass("active");
        $("#BtnCmt").parent().addClass("active");
        $("#PreBox").hide();
        $("#EditBox").show();
    });
    //预览面板
    $("#BtnPre").click(function () {
        $("#BtnCmt").parent().removeClass("active");
        $("#BtnPre").parent().removeClass("active");
        $("#BtnPre").parent().addClass("active");
        $("#EditBox").hide();
        $("#PreBox").show();

        $("#PreBox").html("");
        $("#PreBox").html(marked($("#CmtTxt").val()));
    });
    //确定
    $("#BtnConfirm").click(function () {
        var txt = $("#CmtTxt").val();
        if (txt.length == 0) {
            swal("请填写评论内容");
            return;
        }
        if (getBt(txt) > 2000) {
            swal("评论最多2000字节，当前" + getBt(txt) + "字节");
            return;
        }
        $.ajax({
            url: "/Home/AddBlogComment",
            data: { blogID: "5e04a400-ff5e-40a6-98ad-c52d5b175e45", mdTxt: txt, mdValue: marked(txt) },
            type: "post",
            success: function (result) {
                if (result.msg == "done") {
                    GetCommentPage(99999);
                    $("#CmtTxt").val("");
                    $("#BtnCmt").click();
                    $("#CommentBox").hide();
                    SendNewMsg("0ac4a4d3-b453-4e8b-9895-0ba07e0cf59c");
                    $("#CommentCount").html(result.count);
                } else {
                    swal(result.msg);
                }
            }
        });
    });

    //点赞
    $("#BtnPro").click(function () {
        $.ajax({
            url: "/Home/ProBlog",
            data: { id: "5e04a400-ff5e-40a6-98ad-c52d5b175e45" },
            type: "post",
            success: function (result) {
                if (result.msg == "done") {
                    $("#BtnPro").hide();
                    $("#ProCount").html(result.count);
                }
            }
        });
    });

    //收藏
    $("#BtnStar").click(function () {
        $.ajax({
            url: "/Home/StarBlog",
            data: { id: "5e04a400-ff5e-40a6-98ad-c52d5b175e45" },
            type: "post",
            success: function (result) {
                if (result.msg == "done") {
                    $("#BtnStar").hide();
                }
            }
        });
    });
});

var FirstLoad = true;
//获取评论
function GetCommentPage(index) {
    var pageSize = 10;
    var postData = { blogID: "5e04a400-ff5e-40a6-98ad-c52d5b175e45", pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/BlogCommentPage",
        type: "Post",
        async: false,
        data: postData,
        success: function (result) {
            $("#Comments").html(result);
            if (!FirstLoad) {
                $("html,body").animate({ scrollTop: $("#Status").offset().top }, 300)
            }
            FirstLoad = false;
            if ($("#CommentTotalCount").val() == "0") {
                return;
            }
            $("#Pager").pagination({
                items: $("#CommentTotalCount").val(),
                itemsOnPage: pageSize,
                currentPage: $("#CommentCurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    GetCommentPage(pageNumber);
                }
            });
        }
    });
}

//显示回复
function ShowReply(corder, index) {
    if ($("#ShowReply" + corder).html() == "收起回复") {
        $("#ReplyBox" + corder).hide();
        var recount = $("#ShowReply" + corder).attr("recount");
        if (recount == "0") {
            $("#ShowReply" + corder).html("回复");
        } else {
            $("#ShowReply" + corder).html(recount + "条回复");
        }
    } else {
        $("#ReplyBox" + corder).show();
        $("#ShowReply" + corder).html("收起回复");
        if ($("#Replys" + corder).html() == "") {
            GetCommentReplyPage(index, corder);
        }
    }
}

//添加回复
function AddReply(corder) {
    var txt = $("#ReplyTxt" + corder).val();
    var toUser = $("#ReplyTxt" + corder).attr("touser");
    var cmid = $("#Comment" + corder).attr("cmid");
    if (txt.length == 0) {
        swal("请填写回复内容");
        return;
    }
    if (getBt(txt) > 400) {
        swal("评论最多400字节，当前" + getBt(txt) + "字节");
        return;
    }
    $.ajax({
        url: "/Home/AddBlogCommentReply",
        data: { commentID: cmid, toUserID: toUser, txt: txt },
        type: "post",
        success: function (result) {
            if (result.msg == "done") {
                GetCommentReplyPage(99999, corder);
                $("#ReplyTxt" + corder).val("");
                SendNewMsg(toUser);
            } else {
                swal(result.msg);
            }
        }
    });
}

//获取评论回复
function GetCommentReplyPage(index, corder) {
    var pageSize = 10;
    var cmid = $("#Comment" + corder).attr("cmid");
    var postData = { commentID: cmid, corder: corder, pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/BlogCommentReplyPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#Replys" + corder).html(result);
            if ($("#ReplyTotalCount" + corder).val() == "0") {
                return;
            }
            $(".ReplyBar").each(function () {
                $(this).hover(function () {
                    $(this).find("#BtnReply").show();
                }, function () {
                    $(this).find("#BtnReply").hide();
                });
            });
            $("#ReplyPager" + corder).pagination({
                items: $("#ReplyTotalCount" + corder).val(),
                itemsOnPage: pageSize,
                currentPage: $("#ReplyCurrentPage" + corder).val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination-sm",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    GetCommentReplyPage(pageNumber, corder);
                }
            });
        }
    });
}

//回复指定用户
function ReplyToUser(corder, userName, userID) {
    $("#ReplyToUser" + corder).html("@" + userName);
    $("#ReplyTxt" + corder).attr("touser", userID);
    $("#DefaultUser" + corder).show();
}

//回复默认用户
function ReplyDefault(corder, defaultName, defaultID) {
    $("#ReplyToUser" + corder).html("@" + defaultName);
    $("#ReplyTxt" + corder).attr("touser", defaultID);
    $("#DefaultUser" + corder).hide();
}