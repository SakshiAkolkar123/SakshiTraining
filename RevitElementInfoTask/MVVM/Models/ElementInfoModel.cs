using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitElementInfoTask.MVVM.Models
{
    public class ElementInfoModel
    {
        public int ElementId { get; set; }
        public string Category { get; set; } = "";
        public string Family { get; set; } = "";
        public string Type { get; set; } = "";
        public string Level { get; set; } = "";
        public string Name { get; set; } = "";
    }
}