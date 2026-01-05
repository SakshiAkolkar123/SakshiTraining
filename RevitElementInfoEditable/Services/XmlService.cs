
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

using RevitElementInfoEditable.MVVM.Models;

namespace RevitElementInfoEditable.Services
{
    [XmlRoot("ElementInfoData")]
    public class ElementInfoData
    {
        [XmlArray("Rows")]
        [XmlArrayItem("Row")]
        public ObservableCollection<ElementParameterRow> Rows { get; set; } = new();
    }
    //two helper methods—Save and Load—to persist and restore your DataGrid rows
    public static class XmlService
    {
        public static void Save(string file, ObservableCollection<ElementParameterRow> rows)
        {
            var ser = new XmlSerializer(typeof(ElementInfoData));
            using var sw = new StreamWriter(file);
            ser.Serialize(sw, new ElementInfoData { Rows = rows });
        }

        public static ObservableCollection<ElementParameterRow> Load(string file)
        {
            var ser = new XmlSerializer(typeof(ElementInfoData));
            using var sr = new StreamReader(file);
            var data = (ElementInfoData)ser.Deserialize(sr)!;
            return data.Rows;
        }
    }
}
