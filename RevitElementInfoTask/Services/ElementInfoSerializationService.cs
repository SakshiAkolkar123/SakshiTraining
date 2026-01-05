using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitElementInfoTask.Services
{
    public static class ElementInfoSerializationService
    {
        public static void Write(string path, DataSet ds) =>
            ds.WriteXml(path, XmlWriteMode.WriteSchema);

        public static DataView? ReadView(string path)
        {
            var ds = new DataSet();
            ds.ReadXml(path, XmlReadMode.ReadSchema);
            return ds.Tables["Elements"]?.DefaultView;
        }
    }
}