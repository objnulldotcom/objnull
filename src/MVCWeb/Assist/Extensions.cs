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
        /// 简洁时间格式
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToBlurDate(this DateTime dateTime)
        {
            string date = "";
            if (dateTime.Date == DateTime.Now.Date)
            {
                date = dateTime.ToString("HH:mm");
            }
            else if (dateTime.Year == DateTime.Now.Year)
            {
                date = dateTime.ToString("MM-dd");
            }
            else
            {
                date = dateTime.ToString("yy-MM-dd");
            }
            return date;
        }

        /// <summary>
        /// 获取字符串字节数
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int GetByteCount(this string text)
        {
            return Encoding.Default.GetByteCount(text);
        }

        /// <summary>
        /// 按字节数从头截取字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string SubByteStr(this string text, int length)
        {
            string result = "";
            foreach(char c in text.ToCharArray())
            {
                int cbc = c.ToString().GetByteCount();
                int rbc = result.GetByteCount();
                if(rbc + cbc > length)
                {
                    break;
                }
                result += c;
            }
            return result;
        }
    }
}