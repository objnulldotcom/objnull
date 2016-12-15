using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCWeb.Model.Models
{
    public class BlogCommentReply
    {
        public BlogCommentReply()
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
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 垃圾评论
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// 发布人ID
        /// </summary>
        public Guid OwnerID { get; set; }

        /// <summary>
        /// 所属评论ID
        /// </summary>
        public Guid BlogCommentID { get; set; }

        /// <summary>
        /// 目标用户ID
        /// </summary>
        public Guid ToUserID { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public virtual NullUser Owner { get; set; }

        /// <summary>
        /// 所属评论
        /// </summary>
        public virtual BlogComment BlogComment { get; set; }

        /// <summary>
        /// 目标用户
        /// </summary>
        public virtual NullUser ToUser { get; set; }
    }
}
