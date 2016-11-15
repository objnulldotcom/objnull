using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using PagedList.Mvc;

namespace MVCWeb
{
    public static class Extensions
    {
        #region HttpContextBase扩展
        
        /// <summary>
        /// 写cookie（扩展）
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="expireDate">过期日期可为空</param>
        public static void WriteCookie(this HttpContextBase httpContext, string name, string value, DateTime? expireDate = null)
        {
            HttpCookie cookie = new HttpCookie(name, value);
            if (expireDate != null)
            {
                cookie.Expires = expireDate.Value;
            }
            if (httpContext.Request.Cookies[cookie.Name] == null)
            {
                httpContext.Response.Cookies.Add(cookie);
            }
            else
            {
                httpContext.Response.Cookies.Set(cookie);
            }
        }

        /// <summary>
        /// 读cookie（扩展）
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ReadCookie(this HttpContextBase httpContext, string name)
        {
            if (httpContext.Request.Cookies[name] == null)
            {
                return "";
            }
            else
            {
                return httpContext.Request.Cookies[name].Value;
            }
        }

        /// <summary>
        /// 写编码cookie（扩展）
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="expireDate">写cookie扩展</param>
        public static void WriteEncodeCookie(this HttpContextBase httpContext, string name, string value, DateTime? expireDate = null)
        {
            value = Convert.ToBase64String(Encoding.ASCII.GetBytes(value));
            httpContext.WriteCookie(name, value, expireDate);
        }

        /// <summary>
        /// 读编码cookie（扩展）
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ReadEncodeCookie(this HttpContextBase httpContext, string name)
        {
            string value = httpContext.ReadCookie(name);
            if (!string.IsNullOrEmpty(value))
            {
                return Encoding.ASCII.GetString(Convert.FromBase64String(value));
            }
            else
            {
                return "";
            }
        }

        #endregion

        /// <summary>
        /// 生成PagedListRenderOptions（扩展）
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="btnCount">分页按钮数量</param>
        /// <returns></returns>
        public static PagedListRenderOptions GetPagedListOption(this System.Web.Mvc.HtmlHelper htmlHelper, int btnCount = 5)
        {
            PagedListRenderOptions option = new PagedListRenderOptions();
            option.MaximumPageNumbersToDisplay = btnCount;
            option.DisplayLinkToFirstPage = PagedListDisplayMode.Always;
            option.DisplayLinkToLastPage = PagedListDisplayMode.Always;
            option.DisplayLinkToPreviousPage = PagedListDisplayMode.Never;
            option.DisplayLinkToNextPage = PagedListDisplayMode.Never;
            return option;
        }
    }
}