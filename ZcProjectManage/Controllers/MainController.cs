using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcProjectManage.Util;

namespace ZcProjectManage.Controllers
{
    [LoginFilter]
    public class MainController : Controller
    {
        private Dictionary<CompanyTypeModel, CompanyTypeModel[]> companyTypes = new Dictionary<CompanyTypeModel, CompanyTypeModel[]>()
        {
            { new CompanyTypeModel(){ id = 0,name="承接方" },
              new CompanyTypeModel[3]{new CompanyTypeModel(){ id = 5,name="投资合作" },new CompanyTypeModel(){ id = 6,name="战略合作" },new CompanyTypeModel(){ id = 7,name="一般合作" } }
            },
            { new CompanyTypeModel(){ id = 1,name="合作方" },
              new CompanyTypeModel[3]{new CompanyTypeModel(){ id = 2,name="战略合作" },new CompanyTypeModel(){ id = 3,name="密切合作" },new CompanyTypeModel(){ id = 4,name="一般合作" } }
            },
        };
        public ActionResult Index(string cname = "", string aname = "", int id = 0, string tag = "")
        {
            ViewBag.cname = cname;
            ViewBag.aname = aname;
            ViewBag.id = id;
            ViewBag.userType = JsonConvert.DeserializeObject<user>(Session["userinfo"].ToString()).id == 0 ? 0 : 1;
            ViewBag.companyTypes = companyTypes;
            ViewBag.tag = tag;
            return View();
        }

        public ActionResult Sum()
        {
            return PartialView();
        }

        public ActionResult SumData()
        {
            var db = new zc_project_collectionEntities();
            var cooperateSum = db.Company.Where(t => t.type == 1 || t.type == 2 || t.type == 3 || t.type == 4).Count();
            var strategyCooperateSum = db.Company.Where(t => t.type == 2).Count();
            var closeCooperateSum = db.Company.Where(t => t.type == 3).Count();
            var normalCooperateSum = db.Company.Where(t => t.type == 4).Count();
            var noCooperateSum = cooperateSum - normalCooperateSum - strategyCooperateSum - closeCooperateSum;
            var undertakeSum = db.Company.Where(t => t.type == 0 || t.type == 5 || t.type == 6 || t.type == 7).Count();
            var investUndertakeSum = db.Company.Where(t => t.type == 5).Count();
            var strategyUndertakeSum = db.Company.Where(t => t.type == 6).Count();
            var normalUndertakeSum = db.Company.Where(t => t.type == 7).Count();


            var result = new List<SumModel>();

            SumModel allSumModel = new SumModel();
            allSumModel.type = -1;
            allSumModel.models = new List<SumSonModel>();
            SumSonModel cooperateModel = new SumSonModel();
            cooperateModel.name = "合作方";
            cooperateModel.sum = cooperateSum;
            cooperateModel.color = "";
            allSumModel.models.Add(cooperateModel);
            SumSonModel undertakeModel = new SumSonModel();
            undertakeModel.name = "承接方";
            undertakeModel.sum = undertakeSum;
            undertakeModel.color = "";
            allSumModel.models.Add(undertakeModel);
            result.Add(allSumModel);

            SumModel undertakeSumModel = new SumModel();
            undertakeSumModel.type = 0;
            undertakeSumModel.models = new List<SumSonModel>();
            SumSonModel investUndertakeModel = new SumSonModel();
            investUndertakeModel.name = "资源合作";
            investUndertakeModel.sum = investUndertakeSum;
            undertakeSumModel.models.Add(investUndertakeModel);
            SumSonModel strategyUndertakeModel = new SumSonModel();
            strategyUndertakeModel.name = "战略合作";
            strategyUndertakeModel.sum = strategyUndertakeSum;
            undertakeSumModel.models.Add(strategyUndertakeModel);
            SumSonModel normalUndertakeModel = new SumSonModel();
            normalUndertakeModel.name = "一般合作";
            normalUndertakeModel.sum = normalUndertakeSum;
            undertakeSumModel.models.Add(normalUndertakeModel);
            result.Add(undertakeSumModel);

            SumModel cooperateSumModel = new SumModel();
            cooperateSumModel.type = 1;
            cooperateSumModel.models = new List<SumSonModel>();
            SumSonModel strategyCooperateModel = new SumSonModel();
            strategyCooperateModel.name = "战略合作";
            strategyCooperateModel.sum = strategyCooperateSum;
            strategyCooperateModel.color = "";
            cooperateSumModel.models.Add(strategyCooperateModel);
            SumSonModel closeCooperateModel = new SumSonModel();
            closeCooperateModel.name = "密切合作";
            closeCooperateModel.sum = closeCooperateSum;
            closeCooperateModel.color = "";
            cooperateSumModel.models.Add(closeCooperateModel);
            SumSonModel normalCooperateModel = new SumSonModel();
            normalCooperateModel.name = "一般合作";
            normalCooperateModel.sum = normalCooperateSum;
            normalCooperateModel.color = "";
            cooperateSumModel.models.Add(normalCooperateModel);
            SumSonModel noCooperateModel = new SumSonModel();
            noCooperateModel.name = "未合作";
            noCooperateModel.sum = noCooperateSum;
            noCooperateModel.color = "";
            cooperateSumModel.models.Add(noCooperateModel);
            result.Add(cooperateSumModel);
            return Json(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult SumData2(int type)
        {
            var db = new zc_project_collectionEntities();
            List<SumSonModel> result = new List<SumSonModel>();
            var companies = db.Company.ToList();
            if (type == 1)
            {
                companies = companies.Where(t => t.type == 1 || t.type == 2 || t.type == 3 || t.type == 4).ToList();
            }
            else if (type == 0)
            {
                companies = companies.Where(t => t.type == 0 || t.type == 5 || t.type == 6 || t.type == 7).ToList();
            }
            Dictionary<string, int> TagSum = new Dictionary<string, int>();
            foreach (var company in companies)
            {
                var tags = company.tagids.Split(',').Where(t => t != "").ToArray();
                var tagids = new List<int>();
                foreach (var tag in tags)
                {
                    tagids.Add(int.Parse(tag));
                }
                var tagItems = db.projectype.Where(t => tagids.Contains(t.id)).ToList();
                foreach (var tagItem in tagItems)
                {
                    if (TagSum.Keys.Contains(tagItem.name))
                    {
                        TagSum[tagItem.name] += 1;
                    }
                    else
                    {
                        TagSum.Add(tagItem.name, 1);
                    }
                }

            }
            foreach (var tagsumKey in TagSum.Keys)
            {
                SumSonModel sumSonModel = new SumSonModel();
                sumSonModel.name = tagsumKey;
                sumSonModel.sum = TagSum[tagsumKey];
                sumSonModel.color = "";
                result.Add(sumSonModel);
            }
            result = result.OrderByDescending(t => t.sum).Take(20).ToList();
            return Json(result);
        }

    }

    public class SumModel
    {
        /// <summary>
        /// -1全部，0承接方，1合作方
        /// </summary>
        public int type { get; set; }
        public List<SumSonModel> models { get; set; }
    }

    public class SumSonModel
    {
        /// <summary>
        /// 比例
        /// </summary>
        public int sum { get; set; }
        public string name { get; set; }
        public string color { get; set; }
    }

}