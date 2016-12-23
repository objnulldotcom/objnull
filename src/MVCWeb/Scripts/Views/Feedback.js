//NewBeePage
function FeedbackPage(index) {
    var pageSize = parseInt($("#FeedbackPageSize").val());
    var postData = { pageSize: pageSize, pageNum: index }
    $.ajax({
        url: "/Home/FeedbackPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#FeedbackPage").html(result);
            var total = parseInt($("#TotalCount").val());
            if (total <= pageSize) {
                return;
            }
            $("#FBPager").pagination({
                items: total,
                itemsOnPage: pageSize,
                currentPage: $("#CurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination pagination",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    FeedbackPage(pageNumber);
                }
            });
        }
    });
}
$(function () {
    FeedbackPage(1);

    //确定
    $("#BtnConfirm").click(function () {
        var content = $("#TxtContent").val();
        if (content.length == 0) {
            swal("请填写内容");
            return;
        }
        if (getBt(content) > 400) {
            swal("内容最多400字节，当前" + getBt(content) + "字节");
            return;
        }

        $("#BtnConfirm").attr("disabled", true);
        $.ajax({
            url: "/Home/AddFeedback",
            data: { txt: content },
            type: "post",
            success: function (result) {
                $("#BtnConfirm").attr("disabled", false);
                if (result.msg == "done") {
                    $("#TxtContent").val("");
                    FeedbackPage(1);
                } else {
                    swal(result.msg);
                }
            }
        });
    });

});
