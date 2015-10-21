
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
	[Activity (Label = "CreateGroupChatActivity")]			
	public class CreateGroupChatActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.CreateGroupChat);
			Button nextBtn = FindViewById<Button>(Resource.Id.create_chat_room_btn);
			EditText nameET = FindViewById<EditText>(Resource.Id.chatroom_name_tb);
			string additionalText = Intent.GetStringExtra("extraMessage");
			if(additionalText != null)
				FindViewById<TextView>(Resource.Id.additional_text_create_group_chat).Text = additionalText;

			nameET.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => {
				if (nameET.Text.Length > 0)
					nextBtn.Enabled = true;
				else
					nextBtn.Enabled = false;
			};

			nextBtn.Click += async (sender, e) =>  {
				ChatRoom newchatroom = new ChatRoom();
				string webId = Intent.GetStringExtra("chatroomWebId");
				if(webId != null){
					newchatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom(webId);
					newchatroom.chatRoomName = nameET.Text;
					ParseChatRoomDatabase pcrd = new ParseChatRoomDatabase();
					await pcrd.SaveChatRoomAsync(newchatroom);
					DatabaseAccessors.ChatRoomDatabaseAccessor.UpdateChatRoom(newchatroom);
					ChatsActivity.NotifyChatRoomsUpdate();
					Intent myIntent = new Intent(this, typeof(CreateGroupChatActivity));

					SetResult(Result.Ok, myIntent);
					Finish();
				}
				else{
					newchatroom.chatRoomName = nameET.Text;
					
					ParseChatRoomDatabase pcrd = new ParseChatRoomDatabase();
					
					await pcrd.SaveChatRoomAsync(newchatroom);
					DatabaseAccessors.ChatRoomDatabaseAccessor.SaveChatRoom(newchatroom);
					List<User> chatroomUsers = new List<User>();
					
					chatroomUsers.Add(DatabaseAccessors.CurrentUser());
					DatabaseAccessors.ChatRoomDatabaseAccessor.SaveChatRoomUsers(chatroomUsers, newchatroom);
					var crus = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoomUsers(newchatroom.webID);
					foreach(ChatRoomUser cru in crus)
						await pcrd.SaveChatRoomUsersAsync(cru);
					ChatsActivity.NotifyChatRoomsUpdate();
					
					var intent = new Intent(this, typeof(ChatRoomActivity));
					intent.PutExtra("chatroom", newchatroom.webID);
					StartActivity(intent);
					
					this.Finish();
				}
			};

		}


	}
}

