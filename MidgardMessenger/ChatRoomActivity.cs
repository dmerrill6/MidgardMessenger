
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
	[Activity (Label = "ChatRoomActivity")]			
	public class ChatRoomActivity : Activity
	{
		protected ChatRoom chatroom;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom (Intent.GetStringExtra ("chatroom"));
			SetContentView (Resource.Layout.ChatRoom);
		}
	}
}

