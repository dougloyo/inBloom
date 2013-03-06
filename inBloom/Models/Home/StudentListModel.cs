using System.Collections.Generic;

namespace inBloom.Models.Home
{
    public class StudentListModel
    {
        public StudentListModel()
        {
            List = new List<Student>();
            Sections = new List<Section>();
        }

        public string SelectedSectionId { get; set; }

        public List<Section> Sections { get; set; }

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

        public class Section
        {
            public string Id { get; set; }
            public string SectionCode { get; set; }
        }
    }
}