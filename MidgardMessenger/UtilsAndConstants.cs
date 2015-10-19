using System;
using System.Net;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Android.App;
using System.Collections.Generic;
using Android.Database;

namespace MidgardMessenger
{
	public static class UtilsAndConstants
	{
		private static Java.IO.File _imagesDir;
		public static Action<int> downloadProgressChanged;
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

		public static async Task DownloadRemoteImageFile (Uri uri, string fileName)
		{
			string fileNameWithPath = ImagesDir.AbsolutePath + "/" + fileName;
			CreateDirectoryForPictures ();
			using (var client = new WebClient ()) {
				try {
					client.DownloadFileAsync(uri, fileNameWithPath);
					client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
						if(downloadProgressChanged != null)
							downloadProgressChanged(e.ProgressPercentage);
					};
				} catch (Exception e) {
					System.Console.WriteLine(e);
				}
			}
		}
		private static void CreateDirectoryForPictures ()
		{
			_imagesDir = new Java.IO.File (Android.OS.Environment.GetExternalStoragePublicDirectory (Android.OS.Environment.DirectoryPictures), "MidgardMessengerPhotographs");
			if (!_imagesDir.Exists ()) {
				_imagesDir.Mkdirs ();
		    }	
		}

		public static bool isImage(string path){
			List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
			if (ImageExtensions.Contains(System.IO.Path.GetExtension(path).ToUpperInvariant()))
	        {
	            return true;
	        }
	        return false;
		}
		public static bool isVideo(string path){
			List<string> ImageExtensions = new List<string> { ".MP4"};
			if (ImageExtensions.Contains(System.IO.Path.GetExtension(path).ToUpperInvariant()))
	        {
	            return true;
	        }
	        return false;
		}
		public static bool isAudio(string path){
			List<string> ImageExtensions = new List<string> { ".WAV", ".MP3", ".OGG" };
			if (ImageExtensions.Contains(System.IO.Path.GetExtension(path).ToUpperInvariant()))
	        {
	            return true;
	        }
	        return false;
		}
	}
}

