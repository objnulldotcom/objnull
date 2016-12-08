//获取分页
function JqueryPage(index) {
    var pageSize = $("#ValPageSize").val();
    var order = $("#ValOrder").val();
    var days = $("#SltDays").val();
    var postData = { order: order, pageSize: pageSize, pageNum: index, days: days }
    $.ajax({
        url: "/Home/BlogPage",
        type: "Post",
        data: postData,
        success: function (result) {
            $("#JTable").html(result);
            $("#HtmlEmpty").hide();
            var totalCount = parseInt($("#TotalCount").val());
            if (totalCount == 0 && order != "new") {
                $("#TxtDays").html(days);
                $("#TxtOrder").html(order == "view" ? "热门" : "好评");
                $("#HtmlEmpty").show();
            }
            if(totalCount <= parseInt(pageSize)){
                return;
            }
            $("#JPager").pagination({
                items: totalCount,
                itemsOnPage: pageSize,
                currentPage: $("#CurrentPage").val(),
                prevText: "<",
                nextText: ">",
                listStyle: "pagination",
                hrefTextPrefix: "javascript:;",
                onPageClick: function (pageNumber, event) {
                    JqueryPage(pageNumber, order);
                }
            });
        }
    });
}
//初始化
$(function () {
    JqueryPage(1, "new");
    //切换排序
    $("#BtnNew").click(function () {
        $("#NavOrder li").removeClass("active");
        $(this).parent().addClass("active");
        $("#RowDays").hide();
        $("#ValOrder").val("new");
        JqueryPage(1);
    });
    $("#BtnView").click(function () {
        $("#NavOrder li").removeClass("active");
        $(this).parent().addClass("active");
        $("#RowDays").show();
        $("#ValOrder").val("view");
        JqueryPage(1);
    });
    $("#BtnPro").click(function () {
        $("#NavOrder li").removeClass("active");
        $(this).parent().addClass("active");
        $("#RowDays").show();
        $("#ValOrder").val("pro");
        JqueryPage(1);
    });
    //选择天数
    $("#SltDays").change(function () {
        JqueryPage(1);
    });
});

