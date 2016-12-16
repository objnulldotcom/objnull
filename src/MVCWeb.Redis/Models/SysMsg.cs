using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class SysMsg
    {
        public string Title { get; set; }

        public string Msg { get; set; }

        public DateTime Date { get; set; }
    }
}
