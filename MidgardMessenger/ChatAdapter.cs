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
using Android.Provider;

namespace MidgardMessenger
{
	public class ChatAdapter : BaseAdapter
	{
		List<ChatItem> _chatsList;
		Activity _activity;
		ChatRoom chatroom;

		public ChatAdapter (Activity activity)
		{
			_activity = activity;
			chatroom = ((ChatRoomActivity)activity).ChatRoomAccessor;
			FillMessages ();

		}

		public int GetCount(){
			return _chatsList.Count ();
		}

		public override Java.Lang.Object GetItem (int position)
		{
			throw new NotImplementedException ();
		}

		void FillMessages ()
		{
			_chatsList = DatabaseAccessors.ChatDatabaseAccessor.GetChats (chatroom.webID).ToList();
		}

		public override void NotifyDataSetChanged ()
		{
			base.NotifyDataSetChanged ();
			FillMessages ();
		}

		public override int Count {
			get { return _chatsList.Count; }
		}

		public ChatItem GetChatAt(int position)
		{
			return _chatsList [position];
		}

		public override long GetItemId (int position) {
			return 0;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView ?? _activity.LayoutInflater.Inflate (
				Resource.Layout.ChatBubble, parent, false);
			var username = view.FindViewById<TextView> (Resource.Id.chat_bubble_userName);
			var message = view.FindViewById<TextView> (Resource.Id.chat_bubble_message);

			ChatItem currChat = GetChatAt (position);

			username.Text = DatabaseAccessors.UserDatabaseAccessor.GetUser (currChat.senderID).name;
			message.Text = currChat.content;

			return view;
		}
	}
}

