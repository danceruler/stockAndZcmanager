using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ZcProjectManage.Util
{
    public class LoginFilter : System.Web.Mvc.ActionFilterAttribute
    {
        //加载Action前调用  
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //在此也可以进行权限的验证  
            var t = GetCookieUserInfo();
            if (t == null)
            {
                SetDefaultUser();
                //filterContext.HttpContext.Response.Redirect("/login", true);
            }
        }

        public user GetCookieUserInfo()
        {
            try
            {
                string t = HttpContext.Current.Session["userinfo"].ToString();
                user result = JsonConvert.DeserializeObject<user>(t);
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void SetDefaultUser()
        {
            user defaultUser = new user()
            {
                id = 0,
                username = "游客",
                realname = "",
                password = "",
                state = 1
            };

            HttpContext.Current.Session["userinfo"] = JsonConvert.SerializeObject(defaultUser);
        }
    }

    public class LoginFilter2 : System.Web.Mvc.ActionFilterAttribute
    {
        //加载Action前调用  
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //在此也可以进行权限的验证  
            var t = GetCookieUserInfo();
            if (t != null&&t.id != 0)
            {
                filterContext.HttpContext.Response.Redirect("/Main/Index", true);
            }
        }

        public user GetCookieUserInfo()
        {
            try
            {
                string t = HttpContext.Current.Session["userinfo"].ToString();
                user result = JsonConvert.DeserializeObject<user>(t);
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}