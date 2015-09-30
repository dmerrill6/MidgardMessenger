
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
using Parse;

namespace MidgardMessenger
{
	[Activity (Label = "ContactsActivity")]			
	public class ContactsActivity : Activity
	{
		protected ContactsAdapter contactsAdapter;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Contacts);
			Button searchContactsBtn = FindViewById<Button> (Resource.Id.search_contacts);
			searchContactsBtn.Click += async (sender, e) =>  {
				GetParseUsers();
				contactsAdapter = new ContactsAdapter (this);
				var contactsListView = FindViewById<ListView> (Resource.Id.contactsListView);
				contactsListView.Adapter = contactsAdapter;	
				contactsListView.ItemClick += async (object sender2, AdapterView.ItemClickEventArgs e2) => {
					User curritem = contactsAdapter.GetUserAt(e2.Position);
					ParseChatRoomDatabase pcrd = new ParseChatRoomDatabase();
					ChatRoom newchatroom = new ChatRoom();
					await pcrd.SaveChatRoomAsync(newchatroom);
					DatabaseAccessors.ChatRoomDatabaseAccessor.SaveChatRoom(newchatroom);
					List<User> chatroomUsers = new List<User>();
					chatroomUsers.Add(curritem);
					chatroomUsers.Add(DatabaseAccessors.CurrentUser());
					DatabaseAccessors.ChatRoomDatabaseAccessor.SaveChatRoomUsers(chatroomUsers, newchatroom);
					var crus = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoomUsers(newchatroom.webID);
					foreach(ChatRoomUser cru in crus){
						await pcrd.SaveChatRoomUsersAsync(cru);
						var push = new ParsePush();
						push.Channels = new List<string> {cru.userID};
						push.Alert = "Your men might be requesting help!";
						await push.SendAsync();
					}
					ChatsActivity.NotifyChatRoomsUpdate();

					var intent = new Intent(this, typeof(ChatRoomActivity));
					intent.PutExtra("chatroom", newchatroom.webID);
					StartActivity(intent);

					this.Finish();
				};
			};



		}

		protected async void GetParseUsers(){
			var query  = ParseObject.GetQuery ("UserInformation");
			IEnumerable<ParseObject> results = await query.FindAsync();
			foreach (var user in results) {
				string userId = user ["userId"].ToString ();
				string fullName = user["fullName"].ToString();
				DatabaseAccessors.UserDatabaseAccessor.SaveUser (fullName, userId);

			}
			contactsAdapter.NotifyDataSetChanged ();
		}
	}
}

