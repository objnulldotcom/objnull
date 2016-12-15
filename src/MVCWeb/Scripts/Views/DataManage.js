
//用户姿势查询
function UserPage(index) {
    var pageSize = parseInt($("#ValUserPageSize").val());
    var postData = { pageSize: pageSize, pageNum: index }
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
                    UserBlogPage(pageNumber);
                }
            });
        }
    });
}

$(function () {
    UserPage(1);
})