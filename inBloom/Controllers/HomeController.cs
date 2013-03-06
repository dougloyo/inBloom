using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers;
using inBloom.Models.Home;
using inBloomApiLibrary;

namespace inBloom.Controllers
{
    public class HomeController : Controller
    {
        private readonly GetSectionsData _sectionService = new GetSectionsData();
        private readonly GetStudentsData _studentService = new GetStudentsData();
        private readonly  StudentService _studentCustomService = new StudentService();

        private RestClient _restClient;

        private RestClient RestClient
        {
            get
            {
                if (_restClient == null)
                {
                    string bearerToken = string.Format("bearer {0}", AccessToken);

                    string baseUrl = "https://api.sandbox.inbloom.org/api/rest/v1.1";

                    _restClient = new RestClient(baseUrl);
                    _restClient.AddDefaultHeader("Content-Type", "application/vnd.slc+json");
                    _restClient.AddDefaultHeader("Accept", "application/vnd.slc+json");
                    _restClient.AddDefaultHeader("Authorization", bearerToken);
                    
                }

                return _restClient;
            }
        }

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

            var model = GetStudentListModel();

            return View(model);
        }

        private StudentListModel GetStudentListModel()
        {
            var studentsData = _studentService.GetStudents(AccessToken);

            var students =
                (from d in studentsData
                 select new StudentListModel.Student
                     {
                         Id = d.Value<string>("id"),
                         FirstName = d.Value<JToken>("name").Value<string>("firstName"),
                         LastName = d.Value<JToken>("name").Value<string>("lastSurname")
                     }).ToList();

            var model = new StudentListModel
                {
                    List = students
                };
            return model;
        }

        [HttpGet]
        public ActionResult NotifyParent()
        {
            var model = new NotificationViewModel();

            model.StudentId = Request.QueryString["StudentId"];

            return View(model);
        }

        [HttpPost]
        public ActionResult NotifyParent(NotificationModel model)
        {
            model.DateTime = DateTime.Now;

            CustomStudentData customData;

            var getResponse = _studentCustomService.GetCustomData<CustomStudentData>(RestClient, model.StudentId);
            
            if (getResponse.StatusCode == HttpStatusCode.NotFound)
                customData = new CustomStudentData();
            else            
                customData = JsonConvert.DeserializeObject<CustomStudentData>(getResponse.Content);

            // Add the notification to the top of the list
            customData.Notifications.Insert(0, model);
            
            // Save the notification
            var saveResponse = _studentCustomService.SaveCustomData(RestClient, model.StudentId, customData);

            TempData["message"] = "Notification sent.";
            return RedirectToAction("Index");
        }

        public class CustomStudentData
        {
            public CustomStudentData()
            {
                Notifications = new List<NotificationModel>();
            }

            public List<NotificationModel> Notifications { get; set; }
        }

        [HttpGet]
        public ActionResult ScheduleParentTeacherConference()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ScheduleParentTeacherConference(object model)
        {
            return View(model);
        }

        private string GetSections()
        {
            // System.Web.HttpContext.Current.Response.Redirect(" mypage.aspx");
            return _sectionService.GetSections(AccessToken, UserId).ToString(Formatting.Indented);
        }

    }
}
