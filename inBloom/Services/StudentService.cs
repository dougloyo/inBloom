using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using RestSharp;
using inBloomApiLibrary;

namespace inBloom
{
    public class StudentService
    {
        GetStudentsData getStudentsData = new GetStudentsData();

        public IRestResponse SaveCustomData(RestClient client, string studentId, object data)
        {
            RestRequest request = new RestRequest(string.Format("/students/{0}/custom", studentId), Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(data);
            var studentCustomResponse = client.Post(request);

            return studentCustomResponse;
        }

        public IRestResponse<T> GetCustomData<T>(RestClient client, string studentId)
            where T : new() 
        {
            var request = new RestRequest(string.Format("/students/{0}/custom", studentId), Method.GET);
            request.RequestFormat = DataFormat.Json;

            return client.Execute<T>(request);
        }

        public JArray GetCustomData(string accessToken, string studentId)
        {
            try
            {
                var studentCustom = getStudentsData.GetStudentCustom(accessToken, studentId);
                return studentCustom;
            }
            catch
            {
                // Not production worthy... ignore errors.
                return null;
            }
        }
    }
}