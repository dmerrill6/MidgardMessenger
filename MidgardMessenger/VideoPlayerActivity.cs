
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MidgardMessenger
{
	[Activity (Label = "VideoPlayerActivity")]			
	public class VideoPlayerActivity : Activity
	{
		private ChatItem _chatItem;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			_chatItem = DatabaseAccessors.ChatDatabaseAccessor.GetItem( Intent.GetIntExtra ("chatItem", 0) );
			SetContentView(Resource.Layout.VideoPlayer);
			VideoView videoView = FindViewById<VideoView>(Resource.Id.videoPlayerView);
			bool test = videoView.CanPause();
			videoView.SetVideoPath(_chatItem.pathToFile + "/" + _chatItem.fileName);
			videoView.Start();
		}
	}
}

