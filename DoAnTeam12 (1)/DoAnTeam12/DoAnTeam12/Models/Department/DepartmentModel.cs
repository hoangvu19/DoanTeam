using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnTeam12.Models.Department
{
    public class DepartmentModel
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}