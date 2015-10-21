
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
	public class ChatRoomsAdapter : BaseAdapter
	{
		public List<ChatRoom> _chatroomLists;
		Activity _activity;

		public ChatRoomsAdapter (Activity activity)
		{
			_activity = activity;
			FillContacts ();
			Console.WriteLine ("ttestes" + _chatroomLists.Count);
		}

		public override Java.Lang.Object GetItem (int position)
		{
			throw new NotImplementedException ();
		}

		public int GetCount(){
			return _chatroomLists.Count ();
		}

		void FillContacts ()
		{
			_chatroomLists = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRooms ().ToList ();

			_chatroomLists.Sort(CompareChatRooms);
			_chatroomLists.Reverse();
		}

		private int CompareChatRooms (ChatRoom a, ChatRoom b)
		{
			var a_chatItems = DatabaseAccessors.ChatDatabaseAccessor.GetChats (a.webID);
			var b_chatItems = DatabaseAccessors.ChatDatabaseAccessor.GetChats (b.webID);
			var a_comparer = a.createdAt;
			var b_comparer = b.createdAt;
			if(a_chatItems.Count() > 0)
				a_comparer = a_chatItems.Last().createdAt;
			if(b_chatItems.Count() > 0)
				b_comparer = b_chatItems.Last().createdAt;
			return a_comparer.CompareTo(b_comparer);
		}

		public override void NotifyDataSetChanged ()
		{
			FillContacts ();
			base.NotifyDataSetChanged ();
		}

		public override int Count {
			get { return _chatroomLists.Count; }
		}

		public ChatRoom GetChatRoomAt(int position)
		{
			return _chatroomLists [position];
		}

		public override long GetItemId (int position) {
			return 0;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView ?? _activity.LayoutInflater.Inflate (
				Resource.Layout.ChatRoomListItem, parent, false);
			var contactName = view.FindViewById<TextView> (Resource.Id.ContactName);
			var contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			List<User> users = DatabaseAccessors.ChatRoomDatabaseAccessor.GetUsers (_chatroomLists [position].webID).ToList();
			string chatroomName = "Untitled";
			ChatRoom currChatRoom = _chatroomLists[position];
			if(currChatRoom.chatRoomName != null)
				chatroomName = currChatRoom.chatRoomName;
			else if (users.Count > 0) {
				chatroomName = users.ElementAt (0).name;
				if (users.Count > 1 && chatroomName == DatabaseAccessors.CurrentUser ().name)
					chatroomName = users.ElementAt (1).name;
			}
			contactName.Text = chatroomName;

			var unreadMessages = view.FindViewById<TextView>(Resource.Id.unreadChats);
			var unreadMsgsAmount = DatabaseAccessors.ChatDatabaseAccessor.GetUnread(currChatRoom);
			unreadMessages.Text = unreadMsgsAmount.ToString();
			if(unreadMsgsAmount > 0)
				unreadMessages.Visibility = ViewStates.Visible;
			else
				unreadMessages.Visibility = ViewStates.Gone;

			contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			if(currChatRoom.chatRoomName != null)
				contactImage.SetImageResource (Resource.Drawable.group_chat_icon);
			else
				contactImage.SetImageResource (Resource.Drawable.ContactImage);


			return view;
		}


	}
}