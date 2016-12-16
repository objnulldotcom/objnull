//查找用户
function Search() {
    UserPage(1);
}

//用户查询
function UserPage(index) {
    var pageSize = parseInt($("#ValUserPageSize").val());
    var condition = $("#TxtCondition").val()
    var postData = { pageSize: pageSize, pageNum: index, condition: condition }
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

//操作用户
function UserOperate(type, uid, ot) {
    var days = $("#TxtDisableDay").val();
    if (type == "启") {
        days = 0;
    }
    else if (isNaN(parseInt(days)) || parseInt(days) <= 0) {
        swal("请填写天数");
        return;
    }
    $.ajax({
        url: "/Manager/UserOperate",
        type: "Post",
        data: { type: type, uid: uid, objectType: ot, days: days },
        success: function (result) {
            if (result.msg == "done") {
                UserPage($("#UserCurrentPage").val());
            } else {
                swal("系统错误");
            }
        }
    });
}

$(function () {
    UserPage(1);
})