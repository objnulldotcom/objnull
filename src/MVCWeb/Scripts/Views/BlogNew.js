//更新MDV
function UpateMDV() {
    $("#MDValue").html("");
    $("#MDValue").html(marked($("#TxtMD").val()));
}

//textare当前位置插入
function InsertAtCaret(id, val) {
    var $txt = jQuery("#" + id);
    var caretPos = $txt[0].selectionStart;
    var textAreaTxt = $txt.val();
    $txt.val(textAreaTxt.substring(0, caretPos) + val + textAreaTxt.substring(caretPos));
}

$(function () {
    //marked选项
    marked.setOptions({
        highlight: function (code) {
            return hljs.highlightAuto(code).value;
        },
        sanitize: true,
        breaks: true
    });

    UpateMDV();

    //实时更新
    $("#TxtTitle").bind("input propertychange", function () {
        $("#MDTitle").html($("#TxtTitle").val());
    });
    $("#TxtMD").bind("input propertychange", function () {
        UpateMDV();
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
        dropZone: $("#TxtMD"),
        pasteZone: $("#TxtMD"),
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
                var link = "![](http://" + window.location.host + "/File/DownloadImg?pt="+ pt + "&path=" + data.result.path + ")";
                InsertAtCaret("TxtMD", link);
                UpateMDV();
                SetViewer("MDValue");
            }
        },
        fail: function (e, data) {
            $("#UpPercent").html("");
            swal("上传失败");
        }
    });

    //发布
    $("#BtnConfirm").click(function () {
        var title = $("#TxtTitle").val();
        if (title.length == 0) {
            swal("请填写标题");
            return;
        }
        var tlength = getBt(title);
        if (tlength > 90) {
            swal("标题最多90字节，当前" + tlength + "字节");
            return;
        }
        var mdTxt = $("#TxtMD").val();
        if (mdTxt.length == 0) {
            swal("内容不能为空");
            return;
        }
        var clength = getBt(mdTxt);
        if (clength > 50000) {
            swal("内容最多50000字节，当前" + clength + "字节");
            return;
        }
        var type = $("#SltType").val();
        $("#BtnConfirm").attr("disabled", true);
        $.ajax({
            url: "/Home/BlogNew",
            data: { type: type, title: title, mdTxt: mdTxt, mdValue: marked(mdTxt) },
            type: "post",
            success: function (result) {
                $("#BtnConfirm").attr("disabled", false);
                if (result.msg == "done") {
                    window.location.href = result.url;
                } else {
                    swal(result.msg);
                }
            }
        });
    });

    //选择类型
    $("#SltType").change(function () {
        var selectText = $("#SltType").find("option:selected").text();
        $("#LblBType").html(selectText);
        var type = $("#SltType").val();
        switch (type) {
            case "0":
                $("#LblBType").attr("class", "label label-primary");
                break;
            case "1":
                $("#LblBType").attr("class", "label label-danger");
                break;
            case "2":
                $("#LblBType").attr("class", "label label-info");
                break;
            case "3":
                $("#LblBType").attr("class", "label label-success");
                break;
            case "4":
                $("#LblBType").attr("class", "label label-warning");
                break;
        }
    });

    //五分钟自动保存草稿
    setInterval(function () {
        $("#BtnDraft").click();
    }, 1000 * 300);

    //保存草稿
    $("#BtnDraft").click(function () {
        var title = $("#TxtTitle").val();
        if (title.length > 0) {
            var tlength = getBt(title);
            if (tlength > 90) {
                swal("保存草稿失败。标题最多90字节，当前" + tlength + "字节");
                return;
            }
        }
        var mdTxt = $("#TxtMD").val();
        if (mdTxt.length > 0) {
            var clength = getBt(mdTxt);
            if (clength > 50000) {
                swal("保存草稿失败。内容最多50000字节，当前" + clength + "字节");
                return;
            }
        }
        if (title.length == 0 && mdTxt.length == 0) {
            return;
        }
        var type = $("#SltType").val();
        $("#BtnDraft").attr("disabled", true);
        $.ajax({
            url: "/Home/SaveDraft",
            data: { type: type, title: title, mdTxt: mdTxt },
            type: "post",
            success: function (result) {
                $("#BtnDraft").attr("disabled", false);
                if (result.msg == "done") {
                    $("#DraftStatus").html("保存草稿成功 at " + result.date);
                } else if (result.msg != "empty") {
                    swal("保存草稿失败。" + result.msg);
                }
            }
        });
    });

});
