
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
		List<ChatRoom> _chatroomLists;
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

		void FillContacts ()
		{
			_chatroomLists = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRooms ().ToList ();
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
				Resource.Layout.ContactListItem, parent, false);
			var contactName = view.FindViewById<TextView> (Resource.Id.ContactName);
			var contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			List<User> users = DatabaseAccessors.ChatRoomDatabaseAccessor.GetUsers (_chatroomLists [position].webID).ToList();
			string chatroomName = "Untitled";
			if (users.Count > 0) {
				chatroomName = users.ElementAt (0).name;
				if (users.Count > 1 && chatroomName == DatabaseAccessors.CurrentUser ().name)
					chatroomName = users.ElementAt (1).name;
			}
			contactName.Text = chatroomName;


			contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			contactImage.SetImageResource (Resource.Drawable.ContactImage);


			return view;
		}


	}
}