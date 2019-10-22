using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcProjectManage.Util;

namespace ZcProjectManage.Controllers
{
    [LoginFilter]
    public class UserController : Controller
    {
        private zc_project_collectionEntities db = new zc_project_collectionEntities();
       /// <summary>
       /// 用户信息
       /// </summary>
       /// <param name="userid"></param>
       /// <returns></returns>
        public ActionResult Index(int userid)
        {
            var user = db.user.SingleOrDefault(t => t.id == userid);
            return PartialView(user);
        }

        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            var users = db.user.ToList();
            return PartialView(users);
        }
    }
}