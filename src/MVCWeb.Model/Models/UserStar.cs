using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCWeb.Model.Models
{
    public class UserStar
    {
        public UserStar()
        {
            ID = Guid.NewGuid();
            InsertDate = DateTime.Now;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 添加日期
        /// </summary>
        public DateTime InsertDate { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 目标ID
        /// </summary>
        public Guid TargetID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid OwnerID { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public virtual NullUser Owner { get; set; }
    }
}
