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
    public class CompanyController : Controller
    {
        private zc_project_collectionEntities db = new zc_project_collectionEntities();
        private Dictionary<CompanyTypeModel, CompanyTypeModel[]> companyTypes = new Dictionary<CompanyTypeModel, CompanyTypeModel[]>()
        {
            { new CompanyTypeModel(){ id = 0,name="承接方" },
              new CompanyTypeModel[3]{new CompanyTypeModel(){ id = 5,name="投资合作" },new CompanyTypeModel(){ id = 6,name="战略合作" },new CompanyTypeModel(){ id = 7,name="一般合作" } }
            },
            { new CompanyTypeModel(){ id = 1,name="合作方" },
              new CompanyTypeModel[3]{new CompanyTypeModel(){ id = 2,name="战略合作" },new CompanyTypeModel(){ id = 3,name="密切合作" },new CompanyTypeModel(){ id = 4,name="一般合作" } }
            },
        };

        /// <summary>
        /// 公司信息
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult Index(int companyid)
        {
            var company = db.Company.SingleOrDefault(t => t.id == companyid);
            ViewBag.TypeTags = db.projectype.Where(t => t.type == 1).ToList();
            ViewBag.AppTags = db.projectype.Where(t => t.type == 2).ToList();
            ViewBag.Models = new List<model>();
            if (!string.IsNullOrEmpty(company.modelids))
            {
                var s_modelids = company.modelids.Split(',');
                var modelids = new int[s_modelids.Length];
                for (int i = 0; i < s_modelids.Length; i++)
                {
                    modelids[i] = int.Parse(s_modelids[i]);
                }
                ViewBag.Models = db.model.Where(t => modelids.Contains(t.id)).ToList();
            }
            if (company.type == 1)
            {
                ViewBag.Projects = db.project.Where(t => t.firstcompanyid == company.id).ToList();
            }
            else
            {
                ViewBag.Projects = db.project.Where(t => t.secondcompanyid == company.id).ToList();
            }
            ViewBag.userType = JsonConvert.DeserializeObject<user>(Session["userinfo"].ToString()).id == 0 ? 0 : 1;
            ViewBag.companyTypes = companyTypes;
            return PartialView(company);
        }

        /// <summary>
        /// 公司列表
        /// </summary>
        /// <param name="companyid"></param>
        /// <returns></returns>
        public ActionResult List(string tag = "")
        {
            ViewBag.TypeTags = db.projectype.Where(t => t.type == 1).ToList();
            ViewBag.AppTags = db.projectype.Where(t => t.type == 2).ToList();
            ViewBag.companyTypes = companyTypes;
            ViewBag.tag = tag;
            return PartialView();
        }

        public ActionResult ListData(int type = -1, string key = "", string tag = "")
        {
            var companies = db.Company.ToList();
            var tags = db.projectype.ToList();
            if (!string.IsNullOrEmpty(key))
            {
                var sql = "select * from Company where connector like'%" + key + "%' or name like '%" + key + "%' or address like '%" + key + "%'";
                companies = db.Database.SqlQuery<Company>(sql).ToList();
            }
            if (type != -1)
            {
                companies = companies.Where(t => t.type == type).ToList();
            }
            var search_tags = new List<string>();
            //失败意味着是从柱形图跳转来的
            try
            {
                if (tag != null)
                {
                    var i_tags = ArrayChange.StrsToInts(tag.Split(','));
                    search_tags = tags.Where(t => i_tags.Contains(t.id)).Select(t => t.name).ToList();
                }
            }
            catch (Exception)
            {
                search_tags.Add(tag);
            }


            foreach (var cmpy in companies)
            {
                if (cmpy.tagids != null)
                {
                    var tagids = ArrayChange.StrsToInts(cmpy.tagids.Split(','));
                    var thistags = tags.Where(t => tagids.Contains(t.id)).Select(t => t.name).ToArray();
                    cmpy.tagids = ArrayChange.StrsToStr(thistags, ',');
                }
            }
            foreach (var st in search_tags)
            {
                companies = companies.Where(t => t.tagids.Contains(st)).ToList();
            }
            ViewBag.companyTypes = companyTypes;
            return PartialView(companies);
        }

        /// <summary>
        /// 新增公司
        /// </summary>
        /// <returns></returns>
        public ActionResult Add()
        {
            ViewBag.TypeTags = db.projectype.Where(t => t.type == 1).ToList();
            ViewBag.AppTags = db.projectype.Where(t => t.type == 2).ToList();
            ViewBag.companyTypes = companyTypes;
            return PartialView();
        }


        #region 新增公司
        [ValidateInput(false)]
        public ActionResult AddCompany(AddCompanyModel AddItem)
        {
            MessageModel result = new MessageModel();
            try
            {
                if (string.IsNullOrEmpty(AddItem.Company.name))
                {
                    result.State = 0;
                    result.Messgae = "公司名不能为空";
                    return Json(result);
                }
                var tagids = "";
                var modelids = "";
                //添加model
                for (var i = 0; i < AddItem.Models.Count(); i++)
                {
                    if (i > 0) modelids += ",";
                    AddItem.Models[i].typeids = AddItem.Models[i].typeids == null ? "" : AddItem.Models[i].typeids;
                    db.model.Add(AddItem.Models[i]);
                    db.SaveChanges();
                    modelids += AddItem.Models[i].id;
                    var fileArray = string.IsNullOrWhiteSpace(AddItem.Models[i].file) ? new string[0] : AddItem.Models[i].file.Split(';').Where(t => t != "").ToArray();
                    AddItem.Models[i].file = fileArray.Length > 1 ? string.Join(";", fileArray) : (fileArray.Length == 0 ? "" : fileArray[0]);
                    if (AddItem.Models[i].typeids != null)
                    {
                        tagids += (tagids == "" ? "" : ",") + AddItem.Models[i].typeids;
                    }
                }

                var item = db.Company.SingleOrDefault(t => t.name == AddItem.Company.name & t.type == AddItem.Company.type);
                if (item != null)
                {
                    result.State = 0;
                    result.Messgae = "该公司名已经存在";
                    return Json(result);
                }
                tagids = ArrayChange.StrsToStr(tagids.Split(',').Distinct().ToArray());
                AddItem.Company.tagids = tagids;
                AddItem.Company.modelids = modelids;
                var fileArray2 = string.IsNullOrWhiteSpace(AddItem.Company.description) ? new string[0] : AddItem.Company.description.Split(';').Where(t => t != "").ToArray();
                AddItem.Company.description = fileArray2.Length > 1 ? string.Join(";", fileArray2) : (fileArray2.Length == 0 ? "" : fileArray2[0]);
                db.Company.Add(AddItem.Company);
                db.SaveChanges();
                result.State = 1;
                result.Messgae = "添加成功";
                return Json(result);
            }
            catch (Exception ex)
            {
                result.State = 0;
                result.Messgae = ex.ToString();
                return Json(result);
            }
        }

        [ValidateInput(false)]
        public ActionResult SaveCompany(AddCompanyModel AddItem)
        {
            MessageModel result = new MessageModel();
            try
            {
                if (string.IsNullOrEmpty(AddItem.Company.name))
                {
                    result.State = 0;
                    result.Messgae = "公司名不能为空";
                    return Json(result);
                }

                var item = db.Company.SingleOrDefault(t => t.id == AddItem.Company.id);
                if (item == null)
                {
                    result.State = 0;
                    result.Messgae = "该公司不存在";
                    return Json(result);
                }
                //验证公司名是否合法
                var item2 = db.Company.SingleOrDefault(t => t.name == AddItem.Company.name & t.id != AddItem.Company.id);
                if (item2 != null)
                {
                    result.State = 0;
                    result.Messgae = "该公司名已经存在";
                    return Json(result);
                }

                //删除之前的model，绑定新的
                if (!string.IsNullOrEmpty(item.modelids))
                {
                    var modelids2 = item.modelids.Split(',');
                    foreach (var modelid in modelids2)
                    {
                        var i_modelid = int.Parse(modelid);
                        var model = db.model.SingleOrDefault(t => t.id == i_modelid);
                        db.model.Remove(model);
                        db.SaveChanges();
                    }
                }
                //添加model
                var tagids = "";
                var modelids = "";
                for (var i = 0; i < AddItem.Models.Count(); i++)
                {
                    if (i > 0) modelids += ",";
                    var fileArray = string.IsNullOrWhiteSpace(AddItem.Models[i].file) ? new string[0] : AddItem.Models[i].file.Split(';').Where(t => t != "").ToArray();
                    AddItem.Models[i].file = fileArray.Length > 1 ? string.Join(";", fileArray) : (fileArray.Length == 0 ? "" : fileArray[0]);
                    AddItem.Models[i].typeids = AddItem.Models[i].typeids == null ? "" : AddItem.Models[i].typeids;
                    db.model.Add(AddItem.Models[i]);
                    db.SaveChanges();
                    modelids += AddItem.Models[i].id;
                    if (AddItem.Models[i].typeids != null)
                    {
                        tagids += (tagids == "" ? "" : ",") + AddItem.Models[i].typeids;
                    }
                }
                tagids = ArrayChange.StrsToStr(tagids.Split(',').Distinct().ToArray());
                item.tagids = tagids;
                item.modelids = modelids;
                item.name = AddItem.Company.name;
                item.type = AddItem.Company.type;
                item.address = AddItem.Company.address;
                item.connector = AddItem.Company.connector;
                item.connectnum = AddItem.Company.connectnum;
                item.connectorpos = AddItem.Company.connectorpos;
                item.officiaweb = AddItem.Company.officiaweb;
                item.remark = AddItem.Company.remark;
                var fileArray2 = string.IsNullOrWhiteSpace(AddItem.Company.description) ? new string[0] : AddItem.Company.description.Split(';').Where(t => t != "").ToArray();
                item.description = fileArray2.Length > 1 ? string.Join(";", fileArray2) : (fileArray2.Length == 0 ? "" : fileArray2[0]);
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
                var item = db.Company.SingleOrDefault(t => t.id == id);
                if (item == null)
                {
                    result.State = 0;
                    result.Messgae = "该公司信息已经被删除";
                    return Json(result);
                }
                db.Company.Remove(item);
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
    }


    public class AddCompanyModel
    {
        public Company Company { get; set; }
        public model[] Models { get; set; }
    }


    public class CompanyTypeModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}