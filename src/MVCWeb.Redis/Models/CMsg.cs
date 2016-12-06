using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class CMsg
    {
        /// <summary>
        /// 对象类型：EnumObjectType
        /// </summary>
        public int ObjType { get; set; }

        public Guid ObjID { get; set; }

        public string Title { get; set; }

        public int Count { get; set; }

        public int Order { get; set; }

        public DateTime Date { get; set; }
    }
}
