using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class UserRecord
    {
        /// <summary>
        /// 操作对象ID
        /// </summary>
        public Guid ObjID { get; set; }

        /// <summary>
        /// 操作类型：EnumRecordType 1.查看 2.点赞
        /// </summary>
        public int type { get; set; }
    }
}
