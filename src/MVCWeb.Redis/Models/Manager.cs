using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCWeb.Redis.Models
{
    public class Manager
    {
        public Manager()
        {
            ID = Guid.NewGuid();
        }

        public Guid ID { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public int Role { get; set; }
    }
}
