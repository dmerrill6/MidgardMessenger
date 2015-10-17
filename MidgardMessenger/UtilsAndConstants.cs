using System;
using System.Net;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
namespace MidgardMessenger
{
	public static class UtilsAndConstants
	{
		private static Java.IO.File _imagesDir;
		public static Java.IO.File ImagesDir{
			get{
				if(_imagesDir == null)
					CreateDirectoryForPictures();
				return _imagesDir;
			}
		}

		public static string RandomString(int length)
		{
		    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		    var random = new Random();
		    return new string(Enumerable.Repeat(chars, length)
		      .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static async Task DownloadRemoteImageFile(Uri uri, string fileNameWithPath)
		{
			using (var client = new WebClient())
			{
			    client.DownloadFileAsync(uri, fileNameWithPath);
			}
		}
		private static void CreateDirectoryForPictures ()
		{
		    _imagesDir = new Java.IO.File (
		    	
		        Android.OS.Environment.GetExternalStoragePublicDirectory (
					Android.OS.Environment.DirectoryPictures), "MidgardMessengerPhotographs");
			if (!_imagesDir.Exists ())
		    {
				_imagesDir.Mkdirs( );
		    }	
		}
	}
}

