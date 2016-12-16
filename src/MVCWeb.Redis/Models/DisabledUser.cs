using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class DisabledUser
    {
        public Guid UserID { get; set; }

        public DateTime AbleDate { get; set; }

        /// <summary>
        /// 封禁位置：EnumObjectType
        /// </summary>
        public int ObjectType { get; set; }
    }
}
