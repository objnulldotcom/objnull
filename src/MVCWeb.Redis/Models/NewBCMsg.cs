using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class NewBCMsg
    {
        public Guid BlogID { get; set; }

        public string Title { get; set; }

        public int Count { get; set; }

        public int Page { get; set; }

        public DateTime Date { get; set; }
    }
}
