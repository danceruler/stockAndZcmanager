using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcProjectManage.Models;
using ZcProjectManage.Util;

namespace ZcProjectManage.Controllers
{
    [LoginFilter2]
    public class LoginController : Controller
    {
        private zc_project_collectionEntities db = new zc_project_collectionEntities();
        
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        #region 接口
        public ActionResult PostLogin(string username,string psw)
        {
            MessageModel result = new MessageModel();
            var user = db.user.SingleOrDefault(t => t.username == username & t.password == psw);
            if(user == null)
            {
                result.State = 0;
                result.Messgae = "用户名或者密码错误";
                return Json(result);
            }
            result.State = 1;
            result.Messgae = "登陆成功";
            Session["userinfo"] = JsonConvert.SerializeObject(user);
            return Json(result);
        }
        
        #endregion
    }
}