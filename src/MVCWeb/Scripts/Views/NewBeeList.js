//NewBeePage
function NewBeePage(index) {
    var pageSize = parseInt($("#ValPageSize").val());
    var postData = { pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/NewBeePage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#NewBeePage").html(result);
            var total = parseInt($("#TotalCount").val());
            if (total <= pageSize) {
                return;
            }
            $("#NBPager").pagination({
                items: total,
                itemsOnPage: pageSize,
                currentPage: $("#CurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    NewBeePage(pageNumber);
                }
            });
        }
    });
}

//textare当前位置插入
function InsertAtCaret(id, val) {
    var $txt = jQuery("#" + id);
    var caretPos = $txt[0].selectionStart;
    var textAreaTxt = $txt.val();
    $txt.val(textAreaTxt.substring(0, caretPos) + val + textAreaTxt.substring(caretPos));
}

$(function () {
    $("#PreBox").hide();
    NewBeePage(1);

    //代码高亮插件
    marked.setOptions({
        highlight: function (code) {
            return hljs.highlightAuto(code).value;
        },
        breaks: true,
        sanitize: true
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

    //内容面板
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
        var title = $("#TxtTitle").val();
        if (title.length == 0) {
            swal("请填写标题");
            return;
        }
        if (getBt(title) > 100) {
            swal("标题最多100字节，当前" + getBt(title) + "字节");
            return;
        }

        var txt = $("#CmtTxt").val();
        if (txt.length == 0) {
            swal("请填写评论内容");
            return;
        }
        if (getBt(txt) > 2500) {
            swal("内容最多2500字节，当前" + getBt(txt) + "字节");
            return;
        }
        $("#BtnConfirm").attr("disabled", true);
        $.ajax({
            url: "/Home/NewNewBee",
            data: { title: title, mdTxt: txt, mdValue: marked(txt) },
            type: "post",
            success: function (result) {
                $("#BtnConfirm").attr("disabled", false);
                if (result.msg == "done") {
                    $("#TxtTitle").val("");
                    $("#CmtTxt").val("");
                    NewBeePage(1);
                } else {
                    swal(result.msg);
                }
            }
        });
    });

});
