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
        public Guid TargetID { get; set; }

        /// <summary>
        /// 收藏类型：1.姿势 2.newbee 3.笔记 4.问题
        /// </summary>
        public int type { get; set; }
    }
}
