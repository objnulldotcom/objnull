﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCWeb.Model.Models
{
    public class NullUser
    {
        public NullUser()
        {
            ID = Guid.NewGuid();
            InsertDate = DateTime.Now;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// 登录类型
        /// </summary>
        public string LoginType { get; set; }
        
        /// <summary>
        /// GitHub ID
        /// </summary>
        public int GitHubID { get; set; }

        /// <summary>
        /// GitHub登录名
        /// </summary>
        public string GitHubLogin { get; set; }

        /// <summary>
        /// GitHub个人AccessToken
        /// </summary>
        public string GitHubAccessToken { get; set; }

        /// <summary>
        /// 添加日期
        /// </summary>
        public DateTime InsertDate { get; set; }

        /// <summary>
        /// 姿势集合
        /// </summary>
        public virtual ICollection<Blog> Blogs { get; set; }

        /// <summary>
        /// 姿势评论集合
        /// </summary>
        public virtual ICollection<BlogComment> BlogComments { get; set; }

        /// <summary>
        /// 姿势评论回复集合
        /// </summary>
        public virtual ICollection<BlogCommentReply> BlogCommentReplys { get; set; }

        /// <summary>
        /// 收到的姿势评论回复集合
        /// </summary>
        public virtual ICollection<BlogCommentReply> ReceivedBlogCommentReplys { get; set; }

        /// <summary>
        /// 收藏
        /// </summary>
        public virtual ICollection<UserStar> UserStars { get; set; }
    }
}
