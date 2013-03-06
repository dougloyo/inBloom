using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using inBloom.Models.Home;
using inBloomApiLibrary;

namespace inBloom.Controllers
{
    public class HomeController : Controller
    {
        private readonly GetSectionsData _sectionService = new GetSectionsData();
        private readonly GetStudentsData _studentService = new GetStudentsData();

        private string AccessToken
        {
            get
            {
                return Session["access_token"].ToString();
            }
        }

        private string UserId
        {
            get
            {
                return Session["user_ID"].ToString();
            }
        }

        public ActionResult Index()
        {
            //var data = GetSections();

            var data = _studentService.GetStudents(AccessToken);

            var x = (from d in data
                    select new StudentListModel.Student
                        {
                            Id = d.Value<string>("id"),
                            FirstName = d.Value<JToken>("name").Value<string>("firstName"),
                            LastName = d.Value<JToken>("name").Value<string>("lastSurname")
                        }).ToList();

            var model = new StudentListModel
                {
                    List = x
                };
            
            return View(model);
        }

        [HttpGet]
        public ActionResult NotifyParent()
        {
            var model = new NotificationModel();

            model.StudentId = Request.QueryString["StudentId"];

            return View(model);
        }

        [HttpPost]
        public ActionResult NotifyParent(NotificationModel model)
        {
            model.DateTime = DateTime.Now;
            
            return View(model);
        }

        private string GetSections()
        {
            // System.Web.HttpContext.Current.Response.Redirect(" mypage.aspx");
            return _sectionService.GetSections(AccessToken, UserId).ToString(Formatting.Indented);
        }

    }
}
