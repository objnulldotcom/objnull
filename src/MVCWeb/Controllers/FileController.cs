using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;

namespace MVCWeb.Controllers
{
    public class FileController : BaseController
    {
        //上传图片
        [HttpPost]
        public ActionResult JqueryUploadImg(HttpPostedFileBase upFile, int pt)
        {
            if (upFile == null || upFile.ContentLength > 1024 * 1024 * 3)
            {
                return Json(new { error = "文件太大" });
            }
            string upPath = "";
            switch (pt)
            {
                case (int)EnumObjectType.姿势:
                    upPath = ConfigurationManager.AppSettings["BlogFilePath"];
                    break;
                case (int)EnumObjectType.NewBee:
                    upPath = ConfigurationManager.AppSettings["NewBeeFilePath"];
                    break;
                case (int)EnumObjectType.问题:
                    upPath = ConfigurationManager.AppSettings["QuestionFilePath"];
                    break;
                case (int)EnumObjectType.笔记:
                    upPath = ConfigurationManager.AppSettings["NoteFilePath"];
                    break;
                default:
                    upPath = ConfigurationManager.AppSettings["NewBeeFilePath"];
                    break;
            }
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            upPath = upPath + date + "\\";
            if (!Directory.Exists(upPath))
            {
                Directory.CreateDirectory(upPath);
            }
            string ext = upFile.FileName == "blob" ? ".png" : Path.GetExtension(upFile.FileName);
            string newName = Guid.NewGuid().ToString() + ext;
            upFile.SaveAs(upPath + newName);

            return Json(new { error = "", path = date + ":" + newName });
        }

        //下载图片
        public ActionResult DownloadImg(string path, int pt)
        {
            string fPath = "";
            switch (pt)
            {
                case (int)EnumObjectType.姿势:
                    fPath = ConfigurationManager.AppSettings["BlogFilePath"];
                    break;
                case (int)EnumObjectType.NewBee:
                    fPath = ConfigurationManager.AppSettings["NewBeeFilePath"];
                    break;
                case (int)EnumObjectType.问题:
                    fPath = ConfigurationManager.AppSettings["QuestionFilePath"];
                    break;
                case (int)EnumObjectType.笔记:
                    fPath = ConfigurationManager.AppSettings["NoteFilePath"];
                    break;
                default:
                    fPath = ConfigurationManager.AppSettings["NewBeeFilePath"];
                    break;
            }
            fPath = fPath + path.Replace(":", "\\");
            return File(fPath, "application/octet-stream", "temp");
        }
    }
}