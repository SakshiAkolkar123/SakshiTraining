using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitElementInfoTask.MVVM.ViewModels
{
    public class ElementInfoViewModel
    {
        // constructor takes the path to the XML file that contains the serialized element info
        public ElementInfoViewModel(string xmlPath)
        {
            var ds = new DataSet();
            ds.ReadXml(xmlPath, XmlReadMode.ReadSchema); // read the configuration file 
            ElementTableView = ds.Tables["Elements"]?.DefaultView;
        }

        public DataView? ElementTableView { get; }
    }
}