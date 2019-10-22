using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ZcProjectManage.Controllers
{
    public class UploadController : Controller
    {
        private string serverPath = "http://www.mzhdemo.xyz:2001/UploadFile/";

        [ValidateInput(false)]
        public string PostFileForPath()
        {
            //HttpResponseMessage result = null;
            string result = "";
            try
            {
                var httpRequest =  Request;
                if (httpRequest.Files.Count > 0)
                {
                    var postedFile = httpRequest.Files[0];
                    var filename = postedFile.FileName;
                    var path = Server.MapPath("~/UploadFile/");
                    var filePath = path+Guid.NewGuid() + "웃" + filename;
                    postedFile.SaveAs(filePath);

                    result = filePath.Replace(path, serverPath); ;
                }
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
