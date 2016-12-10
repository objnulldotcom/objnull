
var FirstLoad = true;
//Ajax页面确保图片加载完成
// Fn to allow an event to fire after all images are loaded
$.fn.imagesLoaded = function () {
    // get all the images (excluding those with no src attribute)
    var $imgs = this.find('img[src!=""]');
    // if there's no images, just return an already resolved promise
    if (!$imgs.length) { return $.Deferred().resolve().promise(); }
    // for each image, add a deferred object to the array which resolves when the image is loaded (or if loading fails)
    var dfds = [];
    $imgs.each(function () {
        var dfd = $.Deferred();
        dfds.push(dfd);
        var img = new Image();
        img.onload = function () { dfd.resolve(); }
        img.onerror = function () { dfd.resolve(); }
        img.src = this.src;

    });
    // return a master promise object which will resolve when all the deferred objects have resolved
    // IE - when all the images are loaded
    return $.when.apply($, dfds);

}
//获取评论
function GetCommentPage(index) {
    var pageSize = parseInt($("#ValCPageSize").val());
    var postData = { blogID: $("#ValBlogID").val(), pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/BlogCommentPage",
        type: "Post",
        async: false,
        data: postData,
        success: function (result) {
            $("#Comments").html(result).imagesLoaded().then(function () {
                //等待图片加载完成跳转至消息
                var co = parseInt($("#ValCOrder").val());
                var ro = parseInt($("#ValROrder").val());
                if (co > 0) {
                    $("html,body").animate({ scrollTop: $("#Comment" + co).offset().top - 100 }, 500)
                    if (ro > 0) {
                        $("html,body").animate({ scrollTop: $("#Replys" + co).offset().top - 100 }, 300)
                    }
                }
            });
            if (!FirstLoad) {
                $("html,body").animate({ scrollTop: $("#Status").offset().top }, 300)
            }
            FirstLoad = false;
            if (parseInt($("#CommentTotalCount").val()) <= pageSize) {
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
        if ($("#ReplyTotalCount" + corder).val() == null) {
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
    $("#BtnAddReply" + corder).attr("disabled", true);
    $.ajax({
        url: "/Home/AddBlogCommentReply",
        data: { commentID: cmid, toUserID: toUser, txt: txt },
        type: "post",
        success: function (result) {
            $("#BtnAddReply" + corder).attr("disabled", false);
            if (result.msg == "done") {
                GetCommentReplyPage(99999, corder);
                $("#ReplyTxt" + corder).val("");
                var recount = parseInt($("#ShowReply" + corder).attr("recount"));
                $("#ShowReply" + corder).attr("recount", recount + 1);
                SendNewMsg(toUser);
            } else {
                swal(result.msg);
            }
        }
    });
}

//获取评论回复
function GetCommentReplyPage(index, corder) {
    var pageSize = parseInt($("#ValRPageSize").val());
    var cmid = $("#Comment" + corder).attr("cmid");
    var postData = { commentID: cmid, corder: corder, pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/BlogCommentReplyPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#Replys" + corder).html(result);
            $(".ReplyBar").each(function () {
                $(this).hover(function () {
                    $(this).find("#BtnReply").show();
                }, function () {
                    $(this).find("#BtnReply").hide();
                });
            });
            var totalCount = parseInt($("#ReplyTotalCount" + corder).val());
            if (totalCount <= pageSize) {
                return;
            }
            $("#ReplyPager" + corder).pagination({
                items: totalCount,
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

$(function () {
    SetViewer("MDValue");
    $(".CommentBox").hide();
    $("#PreBox").hide();

    var co = parseInt($("#ValCOrder").val());
    var ro = parseInt($("#ValROrder").val());
    var cpsize = parseInt($("#ValCPageSize").val());
    var rpsize = parseInt($("#ValRPageSize").val());

    //消息查看定位页码
    if (co > 0) {
        var ct = Math.floor(co / cpsize);
        var cindex = co % cpsize == 0 ? ct : ct + 1;
        GetCommentPage(cindex);
        if (ro > 0) {
            var rt = Math.floor(ro / rpsize);
            var rindex = ro % rpsize == 0 ? rt : rt + 1;
            ShowReply(co, rindex);
        }
    } else {
        GetCommentPage(1);
    }

    //marked
    marked.setOptions({
        highlight: function (code) {
            return hljs.highlightAuto(code).value;
        },
        sanitize: true,
        breaks: true
    });

    //评论
    $("#BtnComment").click(function () {
        $(".CommentBox").show();
        $(document).scrollTop(9999);
    });
    //取消
    $("#BtnCancel").click(function () {
        $(".CommentBox").hide();
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

    //添加评论
    $("#BtnConfirm").click(function () {
        var txt = $("#CmtTxt").val();
        if (txt.length == 0) {
            swal("请填写评论内容");
            return;
        }
        if (getBt(txt) > 5000) {
            swal("评论最多5000字节，当前" + getBt(txt) + "字节");
            return;
        }
        $("#BtnConfirm").attr("disabled", true);
        $.ajax({
            url: "/Home/AddBlogComment",
            data: { blogID: $("#ValBlogID").val(), mdTxt: txt, mdValue: marked(txt) },
            type: "post",
            success: function (result) {
                $("#BtnConfirm").attr("disabled", false);
                if (result.msg == "done") {
                    GetCommentPage(99999);
                    $("#CmtTxt").val("");
                    $("#BtnCmt").click();
                    $(".CommentBox").hide();
                    SendNewMsg($("#ValBlogOwnerID").val());
                    $("#CommentCount").html(result.count);
                } else {
                    swal(result.msg);
                }
            }
        });
    });

    //点赞
    $("#BtnPro").click(function () {
        $("#BtnPro").attr("disabled", true);
        $.ajax({
            url: "/Home/ProBlog",
            data: { id: $("#ValBlogID").val() },
            type: "post",
            success: function (result) {
                $("#BtnPro").attr("disabled", false);
                if (result.msg == "done") {
                    $("#BtnPro").hide();
                    $("#ProCount").html(result.count);
                }
            }
        });
    });

    //收藏
    $("#BtnStar").click(function () {
        $("#BtnStar").attr("disabled", true);
        $.ajax({
            url: "/Home/StarBlog",
            data: { id: $("#ValBlogID").val() },
            type: "post",
            success: function (result) {
                $("#BtnStar").attr("disabled", false);
                if (result.msg == "done") {
                    $("#BtnStar").hide();
                }
            }
        });
    });

});
