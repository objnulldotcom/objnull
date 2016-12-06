using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class UserStarCache
    {
        /// <summary>
        /// 收藏目标ID
        /// </summary>
        public Guid ObjID { get; set; }

        /// <summary>
        /// 收藏对象类型：EnumObjectType 1.姿势 2.newbee 3.笔记 4.问题
        /// </summary>
        public int ObjType { get; set; }
    }
}
