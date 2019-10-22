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
    [LoginFilter]
    public class ProjectController : Controller
    {
        private zc_project_collectionEntities db = new zc_project_collectionEntities();
        private Dictionary<int, string> states = new Dictionary<int, string>()
        {
            { 1,"待对接（默认）" },
            { 2,"正在合作" },
            { 3,"正在实施" },
            { 4,"实施完成（待交接）" },
            { 5,"项目完成" },
            { 0,"项目停滞" }
        };
        /// <summary>
        /// 项目信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int projectid)
        {
            var project = db.project.SingleOrDefault(t => t.id == projectid);
            ViewBag.TypeTags = db.projectype.Where(t => t.type == 1).ToList();
            ViewBag.AppTags = db.projectype.Where(t => t.type == 2).ToList();
            ViewBag.states = states;
            ViewBag.FirstCompanies = db.Company.Where(t => t.type == 1).ToList();
            ViewBag.SecondCompanies = db.Company.Where(t => t.type == 0).ToList();
            ViewBag.userType = JsonConvert.DeserializeObject<user>(Session["userinfo"].ToString()).id == 0 ? 0 : 1;
            return PartialView(project);
        }

        /// <summary>
        /// 项目列表
        /// </summary>
        public ActionResult List()
        {
            ViewBag.BeginDate = DateTime.Now.AddDays(-7).Date;
            ViewBag.EndDate = DateTime.Now.AddDays(1).Date;
            ViewBag.states = states;
            ViewBag.FirstComany = db.Company.Where(t => t.type == 1).ToList();
            ViewBag.SecondComany = db.Company.Where(t => t.type == 0).ToList();
            return PartialView();
        }

        /// <summary>
        /// 项目列表数据
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult ListData(DateTime beginDate, DateTime endDate, string key,int firstId = -1, int secondId = -1,int state = -1)
        {
            ViewBag.states = states;
            ViewBag.secondComs = db.Company.Where(t => t.type == 0).ToList();
            ViewBag.firstComs = db.Company.Where(t => t.type == 1).ToList();
            endDate = endDate.AddDays(1);
            List<project> result = new List<project>();
            var items = db.project.Where(t => t.createtime > beginDate & t.createtime < endDate);
            if(state > 0)
            {
                items = items.Where(t => t.state == state);
            }
            if(firstId >= 0)
            {
                items = items.Where(t => t.firstcompanyid == firstId);
            }
            if (secondId >= 0)
            {
                items = items.Where(t => t.secondcompanyid == secondId);
            }
            result = items.ToList();
            return PartialView(result);
        }

        /// <summary>
        /// 正在进行项目列表
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult RunningList(DateTime beginDate, DateTime endDate, string key)
        {
            ViewBag.BeginDate = DateTime.Now.AddDays(-7).Date;
            ViewBag.EndDate = DateTime.Now.AddDays(1).Date;
            return PartialView();
        }

        /// <summary>
        /// 正在进行项目列表数据
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult RunningListData(DateTime beginDate, DateTime endDate, string key)
        {
            List<project> result = new List<project>();
            var items = db.project.Where(t => t.createtime > beginDate & t.createtime < endDate & t.state > 0 & t.state < 5);
            if (key == "")
            {
                result = items.ToList();
                return PartialView(result);
            }
            result = items.Where(t => t.name.Contains(key)).ToList();
            return PartialView(result);
        }

        public ActionResult Add()
        {
            ViewBag.FirstCompanies = db.Company.Where(t => t.type == 1).ToList();
            ViewBag.SecondCompanies = db.Company.Where(t => t.type == 0).ToList();
            ViewBag.TypeTags = db.projectype.Where(t => t.type == 1).ToList();
            ViewBag.AppTags = db.projectype.Where(t => t.type == 2).ToList();
            return PartialView();
        }

        #region 接口
        [ValidateInput(false)]
        public ActionResult AddProject(project model)
        {
            MessageModel result = new MessageModel();
            try
            {
                if (string.IsNullOrEmpty(model.name))
                {
                    result.State = 0;
                    result.Messgae = "项目不能为空";
                    return Json(result);
                }
                if (model.firstcompanyid == null||model.firstcompanyid == 0)
                {
                    result.State = 0;
                    result.Messgae = "合作公司不能为空";
                    return Json(result);
                }
                if (model.secondcompanyid == null || model.secondcompanyid == 0)
                {
                    result.State = 0;
                    result.Messgae = "承接公司不能为空";
                    return Json(result);
                }


                var item = db.project.SingleOrDefault(p => p.name == model.name);
                if(item != null)
                {
                    result.State = 0;
                    result.Messgae = "该项目名已经存在";
                    return Json(result);
                }
                string t = Session["userinfo"].ToString();
                user now_user = JsonConvert.DeserializeObject<user>(t);
                model.createtime = DateTime.Now;
                model.userid = now_user.id;
                var fileArray1 = string.IsNullOrWhiteSpace(model.earlyfiles) ? new string[0] : model.earlyfiles.Split(';').Where(p => p != "").ToArray();
                model.earlyfiles = fileArray1.Length > 1 ? string.Join(";", fileArray1) : (fileArray1.Length == 0 ? "" : fileArray1[0]);
                var fileArray2 = string.IsNullOrWhiteSpace(model.solutionfiles) ? new string[0] : model.solutionfiles.Split(';').Where(p => p != "").ToArray();
                model.solutionfiles = fileArray2.Length > 1 ? string.Join(";", fileArray2) : (fileArray2.Length == 0 ? "" : fileArray2[0]);
                var fileArray3 = string.IsNullOrWhiteSpace(model.finalfiles) ? new string[0] : model.finalfiles.Split(';').Where(p => p != "").ToArray();
                model.finalfiles = fileArray3.Length > 1 ? string.Join(";", fileArray3) : (fileArray3.Length == 0 ? "" : fileArray3[0]);
                db.project.Add(model);
                db.SaveChanges();
                result.State = 1;
                result.Messgae = "添加成功";
                return Json(result);
            }catch(Exception ex)
            {
                result.State = 0;
                result.Messgae = ex.ToString();
                return Json(result);
            }
        }
        [ValidateInput(false)]
        public ActionResult SaveProject(project model)
        {
            MessageModel result = new MessageModel();
            try
            {
                if (string.IsNullOrEmpty(model.name))
                {
                    result.State = 0;
                    result.Messgae = "项目名不能为空";
                    return Json(result);
                }
                if (model.firstcompanyid == null || model.firstcompanyid == 0)
                {
                    result.State = 0;
                    result.Messgae = "合作公司不能为空";
                    return Json(result);
                }
                if (model.secondcompanyid == null || model.secondcompanyid == 0)
                {
                    result.State = 0;
                    result.Messgae = "承接公司不能为空";
                    return Json(result);
                }

                var item = db.project.SingleOrDefault(p => p.id == model.id);
                if (item == null)
                {
                    result.State = 0;
                    result.Messgae = "该项目已不存在";
                    return Json(result);
                }
                var item2 = db.project.SingleOrDefault(p => p.name == model.name&p.id!=model.id);
                if (item2 != null)
                {
                    result.State = 0;
                    result.Messgae = "该项目名已经存在";
                    return Json(result);
                }
                string t = Session["userinfo"].ToString();
                user now_user = JsonConvert.DeserializeObject<user>(t);
                item.name = model.name;
                item.firstcompanyid = model.firstcompanyid;
                item.secondcompanyid = model.secondcompanyid;
                item.typeids = model.typeids;
                item.state = model.state;
                item.userid = now_user.id;
                item.earlycontent = model.earlycontent;
                item.solutioncontent = model.solutioncontent;
                item.finalcontent = model.finalcontent;
                var fileArray1 = string.IsNullOrWhiteSpace(model.earlyfiles) ? new string[0] : model.earlyfiles.Split(';').Where(p => p != "").ToArray();
                item.earlyfiles = fileArray1.Length > 1 ? string.Join(";", fileArray1) : (fileArray1.Length == 0 ? "" : fileArray1[0]);
                var fileArray2 = string.IsNullOrWhiteSpace(model.solutionfiles) ? new string[0] : model.solutionfiles.Split(';').Where(p => p != "").ToArray();
                item.solutionfiles = fileArray2.Length > 1 ? string.Join(";", fileArray2) : (fileArray2.Length == 0 ? "" : fileArray2[0]);
                var fileArray3 = string.IsNullOrWhiteSpace(model.finalfiles) ? new string[0] : model.finalfiles.Split(';').Where(p => p != "").ToArray();
                item.finalfiles = fileArray3.Length > 1 ? string.Join(";", fileArray3) : (fileArray3.Length == 0 ? "" : fileArray3[0]);

                item.remark = model.remark;
                db.SaveChanges();
                result.State = 1;
                result.Messgae = "修改成功";
                return Json(result);
            }
            catch (Exception ex)
            {
                result.State = 0;
                result.Messgae = ex.ToString();
                return Json(result);
            }
        }

        public ActionResult Delete(int id)
        {
            MessageModel result = new MessageModel();
            try
            {
                var item = db.project.SingleOrDefault(t => t.id == id);
                if (item == null)
                {
                    result.State = 0;
                    result.Messgae = "该项目信息已经被删除";
                    return Json(result);
                }
                db.project.Remove(item);
                db.SaveChanges();
                result.State = 1;
                result.Messgae = "删除成功";
                return Json(result);
            }
            catch (Exception ex)
            {
                result.State = 0;
                result.Messgae = ex.ToString();
                return Json(result);
            }
        }
        #endregion

        //public class AddProjectModel
        //{
        //    public string name { get; set; }
        //    public int firstcompanyid { get; set; }
        //    public int secondcompanyid { get; set; }
        //    public DateTime createtime { get; set; }
        //    public int userid { get; set; }
        //    public string dockername { get; set; }
        //    public int type { get; set; }
        //    public string earlyfiles { get; set; }
        //    public string solutionfiles { get; set; }
        //    public string finalfiles { get; set; }
        //    public string remark { get; set; }
        //    public string holdutilname { get; set; }
        //    public string holdutilinfo { get; set; }
        //    public int state { get; set; }
        //}
    }
}