
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
using System.Threading.Tasks;
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
			ChatRoom chatroom = null;
			string chatroomWebId = Intent.GetStringExtra("chatroomWebId");
			if(chatroomWebId != null)
				chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom(chatroomWebId);

			searchContactsBtn.Click += async (sender, e) =>  {
				contactsAdapter = new ContactsAdapter (this);
				var contactsListView = FindViewById<ListView> (Resource.Id.contactsListView);
				contactsListView.Adapter = contactsAdapter;	
				if(chatroom == null){
					GetParseUsers();
				}
				else{
					GetParseUsersNotInChatRoom(chatroom);
				}
				contactsListView.ItemClick += async (object sender2, AdapterView.ItemClickEventArgs e2) => {
					User curritem = contactsAdapter.GetUserAt(e2.Position);
					ParseChatRoomDatabase pcrd = new ParseChatRoomDatabase();
					ChatRoom newchatroom;
					if(chatroom == null){
						newchatroom = new ChatRoom();
						await pcrd.SaveChatRoomAsync(newchatroom);
						DatabaseAccessors.ChatRoomDatabaseAccessor.SaveChatRoom(newchatroom);
					}
					else {
						newchatroom = chatroom;
					}

					List<User> chatroomUsers = new List<User>();
					chatroomUsers.Add(curritem);
					if(chatroom==null && curritem.webID != DatabaseAccessors.CurrentUser().webID){
						chatroomUsers.Add(DatabaseAccessors.CurrentUser());
					}
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
					if(chatroom==null){
						var intent = new Intent(this, typeof(ChatRoomActivity));
						intent.PutExtra("chatroom", newchatroom.webID);
						StartActivity(intent);
					}
					var push = new ParsePush();
					push.Channels = new List<string> {UtilsAndConstants.PUSH_PREFIX + curritem.webID};
					push.Alert = "Your men might be requesting help!";
					await push.SendAsync();

					Intent myIntent = new Intent(this, typeof(ContactsActivity));
					SetResult(Result.Ok, myIntent);
					Finish();
				};
			};

<<<<<<< HEAD

=======
>>>>>>> 80b5fa3d56e0d5b59fbc37b348a94b8d004e62a5


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

		protected async void GetParseUsersNotInChatRoom (ChatRoom chatroom)
		{
			var usersInChatRoom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoomUsers (chatroom.webID);

			GetParseUsers ();

			var allUsers = DatabaseAccessors.UserDatabaseAccessor.GetUsers ();
			List<User> result = new List<User> ();
			foreach (var user in allUsers) {
				bool isContained = false;
				foreach (var userInChatRoom in usersInChatRoom) {
					if(userInChatRoom.userID == user.webID)
						isContained = true;
				}
				if(isContained == false)
					result.Add(user);
			}
			contactsAdapter.SetContactList (result);

		}
	}
}

