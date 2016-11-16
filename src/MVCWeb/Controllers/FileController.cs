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
        [HttpPost]
        public ActionResult JqueryUploadImg(HttpPostedFileBase upFile, int pt)
        {
            if (upFile == null || upFile.ContentLength > 1024 * 1024 * 3)
            {
                return Json(new { error = "文件太大" });
            }
            string upPath = "";
            if (pt < 1 || pt > 5)
            {
                pt = 5;
            }
            switch (pt)
            {
                case 1:
                    upPath = ConfigurationManager.AppSettings["RecruitFilePath"];
                    break;
                case 2:
                    upPath = ConfigurationManager.AppSettings["NoteFilePath"];
                    break;
                case 3:
                    upPath = ConfigurationManager.AppSettings["QuestionFilePath"];
                    break;
                case 4:
                    upPath = ConfigurationManager.AppSettings["KnowledgeFilePath"];
                    break;
                case 5:
                    upPath = ConfigurationManager.AppSettings["TreeNewBeeFilePath"];
                    break;
            }
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            upPath = upPath + date + "\\";
            if (!Directory.Exists(upPath))
            {
                Directory.CreateDirectory(upPath);
            }
            string newName = Guid.NewGuid().ToString() + Path.GetExtension(upFile.FileName);
            upFile.SaveAs(upPath + newName);

            return Json(new { error = "", path = date + ":" + newName });
        }

        public ActionResult DownloadImg(string path, int pt)
        {
            string fPath = "";
            if (pt < 1 || pt > 5)
            {
                pt = 5;
            }
            switch (pt)
            {
                case 1:
                    fPath = ConfigurationManager.AppSettings["RecruitFilePath"];
                    break;
                case 2:
                    fPath = ConfigurationManager.AppSettings["NoteFilePath"];
                    break;
                case 3:
                    fPath = ConfigurationManager.AppSettings["QuestionFilePath"];
                    break;
                case 4:
                    fPath = ConfigurationManager.AppSettings["KnowledgeFilePath"];
                    break;
                case 5:
                    fPath = ConfigurationManager.AppSettings["TreeNewBeeFilePath"];
                    break;
            }
            fPath = fPath + path.Replace(":", "\\");
            return File(fPath, "application/octet-stream", "temp");
        }
    }
}