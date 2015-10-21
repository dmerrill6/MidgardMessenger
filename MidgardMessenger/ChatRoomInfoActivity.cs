
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
	[Activity (Label = "ChatRoomInfoActivity")]			
	public class ChatRoomInfoActivity : Activity
	{
		ChatRoom chatroom;
		private const int CHANGE_NAME_RC = 0;
		private const int ADD_USER_TO_CHATROOM_RC = 1;
		private ContactsAdapter contactAdapt;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.ChatRoomInfo);
			var toolbar = FindViewById<Toolbar> (Resource.Id.chatroom_info_toolbar);
			//Toolbar will now take on default Action Bar characteristics
			SetActionBar (toolbar);
			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom(Intent.GetStringExtra("chatroomWebId"));
			SetChatRoomName();
			TextView chatroomCreatedAt = FindViewById<TextView>(Resource.Id.ChatRoomInfoCreatedAt);
			chatroomCreatedAt.Text = chatroom.createdAt.ToLongDateString() + " " + chatroom.createdAt.ToShortTimeString();
			// Create your application here
			Button addUserToConvBtn = FindViewById<Button>(Resource.Id.add_user_to_chatroom_btn);
			Button changeNameBtn = FindViewById<Button>(Resource.Id.change_chatroom_name_btn);
			ListView listView = FindViewById<ListView>(Resource.Id.current_conv_users_list_view);
			var users = DatabaseAccessors.ChatRoomDatabaseAccessor.GetUsers(chatroom.webID).ToList();

			contactAdapt = new ContactsAdapter(this);

			listView.Adapter = contactAdapt;	
			contactAdapt.SetContactList(users);

			addUserToConvBtn.Click += delegate {
				
				if (chatroom.chatRoomName == null) {
					Intent chatroomNameIntent = new Intent (this, typeof(CreateGroupChatActivity));
					chatroomNameIntent.PutExtra("chatroomWebId", chatroom.webID);
					chatroomNameIntent.PutExtra("extraMessage", "Before adding Vikings you must set a name for this conversation");
					StartActivityForResult (chatroomNameIntent, CHANGE_NAME_RC);

				} else {
					Intent addUserIntent = new Intent (this, typeof(ContactsActivity));
					addUserIntent.PutExtra("chatroomWebId", chatroom.webID);

					StartActivityForResult (addUserIntent, ADD_USER_TO_CHATROOM_RC);
				}

			};
			changeNameBtn.Click += delegate {
				Intent chatroomNameIntent = new Intent (this, typeof(CreateGroupChatActivity));
				chatroomNameIntent.PutExtra("chatroomWebId", chatroom.webID);
				StartActivityForResult (chatroomNameIntent, CHANGE_NAME_RC);
	
			};
		}
		private void StartAddUsersIntent ()
		{

		}


		private void SetChatRoomName ()
		{
			List<User> users = DatabaseAccessors.ChatRoomDatabaseAccessor.GetUsers(chatroom.webID).ToList();
			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom(chatroom.webID);
			string chatroomName = "Untitled";
			if(chatroom.chatRoomName != null)
				chatroomName = chatroom.chatRoomName;
			else if (users.Count > 0) {
				chatroomName = users.ElementAt (0).name;
				if (users.Count > 1 && chatroomName == DatabaseAccessors.CurrentUser ().name)
					chatroomName = users.ElementAt (1).name;
			}
			ActionBar.Title = chatroomName;
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			if (resultCode == Result.Ok) {
				switch (requestCode) {
					case CHANGE_NAME_RC:
						SetChatRoomName();
						break;
					case ADD_USER_TO_CHATROOM_RC:
						var users = DatabaseAccessors.ChatRoomDatabaseAccessor.GetUsers(chatroom.webID).ToList();
						contactAdapt.SetContactList(users);
						break;
				}

			}
		}
	}
}

