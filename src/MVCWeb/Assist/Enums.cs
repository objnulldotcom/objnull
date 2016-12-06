using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCWeb
{
    public enum EnumBlogType
    {
        姿势 = 0,
        宣传 = 1,
        心得 = 2,
        科普 = 3,
        搬运 = 4
    }

    public enum EnumObjectType
    {
        姿势 = 1,
        NewBee = 2,
        笔记 = 3,
        问题 = 4
    }
    
    public enum EnumRecordType
    {
        查看 = 1,
        点赞 = 2
    }
}