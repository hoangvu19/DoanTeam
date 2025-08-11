using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnTeam12.Models.Department
{
    public class PositionsModel
    {
        public int PositionID { get; set; }
        public string PositionName { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}