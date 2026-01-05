using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;

namespace RevitWPFParameterInfo4
{
    //Utility helper to load icon
    public static class Util
    {
        //returns a BitmapSource ready for WPF controls and Revit ribbon images.
        public static BitmapSource LoadBitmapSource(string path)
        {
            if (!File.Exists(path)) return null;
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new System.Uri(path);
            //Finalizes initn & loads the image.
            img.EndInit();
            img.Freeze(); // for cross-thread
            return img;
        }
    }
}
