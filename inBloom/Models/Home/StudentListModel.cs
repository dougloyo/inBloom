using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inBloom.Models.Home
{
    public class StudentListModel
    {
        public StudentListModel()
        {
            List = new List<Student>();
        }

        public List<Student> List { get; set; }

        public class Student
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string FullName {
                get { return FirstName + " " + LastName; }
            }
        }
    }
}