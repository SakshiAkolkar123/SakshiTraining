//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;

//namespace Assignment2.helper
//{
//    internal class ImageUtils
//    {
//    }
//}


using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Assignment2.helper
{
    public static class ImageUtils
    {
        // Loads PNG as ImageSource from a relative file path next to the DLL
        public static ImageSource LoadPngImage(string relativePath)
        {
            // Resolve path relative to the assembly location
            var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(assemblyPath);
            var fullPath = Path.Combine(dir ?? "", relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Icon file not found: {fullPath}");

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // allow file to be released after load
            image.UriSource = new Uri(fullPath, UriKind.Absolute);
            image.EndInit();
            image.Freeze(); // make it cross-thread safe
            return image;
        }
    }
}

