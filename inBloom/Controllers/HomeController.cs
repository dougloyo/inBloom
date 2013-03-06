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
            var sectionsData = _sectionService.GetSections(AccessToken, UserId);

            var sections =
                (from d in sectionsData
                 orderby d.Value<string>("uniqueSectionCode")
                 select new StudentListModel.Section
                     {
                         Id = d.Value<string>("id"),
                         SectionCode = d.Value<string>("uniqueSectionCode"),
                     })
                     .ToList();

            sections.Insert(0, new StudentListModel.Section { Id = "", SectionCode = "All" });

            JArray studentsData;

            string sectionId = Request.QueryString["sectionId"];

            if (sectionId == null)
            {
                studentsData = _studentService.GetStudents(AccessToken);
            }
            else
            {
                //studentsData = _sectionService.GetSectionStudentAssociationStudentList(AccessToken, sectionId);
                var request = new RestRequest(string.Format("/sections/{0}/studentSectionAssociations/students", sectionId), Method.GET);

                var response = RestClient.Execute(request);
                studentsData = JArray.Parse(response.Content);
            }

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
                    Sections = sections,
                    List = students,
                    SelectedSectionId = Request.QueryString["sectionId"],
                };

            return model;
        }

        [HttpGet]
        public ActionResult NotifyParent()
        {
            string studentId = Request.QueryString["StudentId"];

            var studentCustomData = GetCustomStudentData(studentId);
            
            var model = new NotificationViewModel
                {
                    Notifications = studentCustomData.Notifications,
                    StudentId = studentId,
                };

            return View(model);
        }

        [HttpPost]
        public ActionResult NotifyParent(NotificationModel model)
        {
            model.DateTime = DateTime.Now;
            model.FeedbackPositive = GetFeedback(model);

            var customData = GetCustomStudentData(model.StudentId);

            // Add the notification to the top of the list
            customData.Notifications.Insert(0, model);
            
            // Save the notification
            var saveResponse = _studentCustomService.SaveCustomData(RestClient, model.StudentId, customData);

            TempData["message"] = "Notification sent.";
            return RedirectToAction("Index");
        }

        private bool GetFeedback(NotificationModel model)
        {
            if (model.Type.Contains("active participant")
                || model.Type.Contains("performed well")
                || model.Type.Contains("positive attitude") 
                || model.Type.Contains("helped a classmate") 
                || model.Type.Contains("turned in homework"))
                return true;

            return false;
        }

        private CustomStudentData GetCustomStudentData(string studentId)
        {
            CustomStudentData customData;

            var getResponse = _studentCustomService.GetCustomData<CustomStudentData>(RestClient, studentId);

            if (getResponse.StatusCode == HttpStatusCode.NotFound)
                customData = new CustomStudentData();
            else
                customData = JsonConvert.DeserializeObject<CustomStudentData>(getResponse.Content);
            return customData;
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

        public ActionResult BroadcastNotification()
        {
            string studentId = Request.QueryString["StudentId"];

            var studentCustomData = GetCustomStudentData(studentId);

            var model = new NotificationViewModel
            {
                Notifications = studentCustomData.Notifications,
                StudentId = studentId,
            };

            return View(model);
        }
    }
}
