//textare当前位置插入
function InsertAtCaret(id, val) {
    var $txt = jQuery("#" + id);
    var caretPos = $txt[0].selectionStart;
    var textAreaTxt = $txt.val();
    $txt.val(textAreaTxt.substring(0, caretPos) + val + textAreaTxt.substring(caretPos));
}

var FirstLoad = true;
//NewBeeFloor分页
function NewBeeFloorPage(index) {
    var pageSize = parseInt($("#ValCPageSize").val());
    var postData = { nbID: $("#ValNewbeeID").val(), pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/NewBeeFloorPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#NewBeeFloorPage").html(result);
            $(".FloorMDV").each(function () {
                var id = $(this).children().attr("id");
                SetTempViewer(id);
            });
            if (!FirstLoad) {
                $("html,body").animate({ scrollTop: 0 }, 300)
            }
            FirstLoad = false;
            if (parseInt($("#TotalCount").val()) <= pageSize) {
                return;
            }
            $("#NBFPager").pagination({
                items: $("#TotalCount").val(),
                itemsOnPage: pageSize,
                currentPage: $("#CurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    NewBeeFloorPage(pageNumber);
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
        url: "/Home/AddNewBeeFloorReply",
        data: { floorID: cmid, toUserID: toUser, txt: txt },
        type: "post",
        success: function (result) {
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
    var postData = { floorID: cmid, corder: corder, pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/NewBeeFloorReplyPage",
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

$(function () {
    NewBeeFloorPage(1);
    $("#PreBox").hide();

    //代码高亮插件
    marked.setOptions({
        highlight: function (code) {
            return hljs.highlightAuto(code).value;
        },
        breaks: true,
        sanitize: false
    });

    //选择文件
    $("#TxtChoseFile").click(function () {
        $("#JqueryUpload").click();
    });
    //阻止全局拖拽，使自定义区域拖拽上传生效
    $(document).bind("drop dragover", function (e) {
        e.preventDefault();
    });
    //上传
    var allowExt = [".jpg", ".png", ".bmp", ".gif"];
    var pt = $("#ValPt").val();
    $("#JqueryUpload").fileupload({
        dropZone: $("#CmtTxt"),
        pasteZone: $("#CmtTxt"),
        add: function (e, data) {
            if (data.files[0].name != null) {
                var ext = data.files[0].name.substr(data.files[0].name.lastIndexOf("."));
                if (allowExt.indexOf(ext) < 0) {
                    swal("图片格式不允许");
                    return;
                }
            }
            if (data.files[0].size > 1024 * 1024 * 3) {
                swal("图片最大为3M");
                return;
            }
            data.submit();
        },
        progressall: function (e, data) {
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $("#UpPercent").html("已上传" + progress + "%");
        },
        dataType: "json",
        done: function (e, data) {
            $("#UpPercent").html("");
            if (data.result.error != "") {
                swal(data.result.error);
            } else {
                var link = "![](http://" + window.location.host + "/File/DownloadImg?pt=" + pt + "&path=" + data.result.path + ")";
                InsertAtCaret("CmtTxt", link);
            }
        },
        fail: function (e, data) {
            $("#UpPercent").html("");
            swal("上传失败");
        }
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

    //回帖
    $("#BtnConfirm").click(function () {
        var txt = $("#CmtTxt").val();
        if (txt.length == 0) {
            swal("请填写评论内容");
            return;
        }
        if (getBt(txt) > 2500) {
            swal("评论最多2500字节，当前" + getBt(txt) + "字节");
            return;
        }
        $("#BtnConfirm").attr("disabled", true);
        $.ajax({
            url: "/Home/AddNewBeeFloor",
            data: { NewBeeID: $("#ValNewbeeID").val(), mdTxt: txt, mdValue: marked(txt) },
            type: "post",
            success: function (result) {
                $("#BtnConfirm").attr("disabled", false);
                if (result.msg == "done") {
                    $("#CmtTxt").val("");
                    $("#BtnCmt").click();
                    SendNewMsg($("#ValNBOwnerID").val());
                    NewBeeFloorPage(99999);
                } else {
                    swal(result.msg);
                }
            }
        });
    });
});