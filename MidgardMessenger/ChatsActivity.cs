using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using Android.Database;
using Parse;
using System.Threading.Tasks;
using PerpetualEngine.Storage;
using Facebook;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MidgardMessenger
{
	[Activity (Label = "Midgard Messenger", MainLauncher = true, Icon = "@drawable/icon")]
	public class ChatsActivity : Activity
	{
		protected bool isLoggedIn;
		protected ParseUser user;
		protected static ChatRoomsAdapter chatroomsAdapter;
		public static void NotifyChatRoomsUpdate(){
			if(chatroomsAdapter != null)
				chatroomsAdapter.NotifyDataSetChanged();
		}


		protected void CreateChatRooms(){
			chatroomsAdapter = new ChatRoomsAdapter (this);
			var chatRoomsListView = FindViewById<ListView> (Resource.Id.chatroomsListView);
			chatRoomsListView.Adapter = chatroomsAdapter;
			chatRoomsListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				ChatRoom currChatRoom = chatroomsAdapter.GetChatRoomAt(e.Position);
				var intent = new Intent(this, typeof(ChatRoomActivity));
				intent.PutExtra("chatroom", currChatRoom.webID);
				StartActivity(intent);
			};
			chatRoomsListView.ItemLongClick += async (object sender, AdapterView.ItemLongClickEventArgs e) => {
				AlertDialog.Builder alert = new AlertDialog.Builder(this);
				alert.SetTitle("Do you want to delete this chatroom?");
				alert.SetPositiveButton("Yes", (senderAlert, args) => {
					ChatRoom currChatRoom = chatroomsAdapter.GetChatRoomAt(e.Position);
					ParsePush.UnsubscribeAsync (currChatRoom.webID);
					ChatRoomUser cru = DatabaseAccessors.ChatRoomDatabaseAccessor.DeleteChatRoomUser(DatabaseAccessors.CurrentUser().webID, currChatRoom.webID);
					DatabaseAccessors.ChatRoomDatabaseAccessor.DeleteChatRoom(currChatRoom.webID);
					ParseChatRoomDatabase pcrd = new ParseChatRoomDatabase();
					pcrd.DeleteChatRoomUserAsync(cru);
					Console.WriteLine("ERASED");
					chatroomsAdapter.NotifyDataSetChanged();

				});
				alert.SetNegativeButton("No", (senderAlert, args) => {
				});	
				alert.Show();
			};
		}

		protected async Task SynchronizeWithParse(){
			Console.WriteLine ("Synching");
			Console.WriteLine ("sy " + DatabaseAccessors.CurrentUser ().webID);
			ParseChatRoomDatabase parseDB = new ParseChatRoomDatabase ();
			await parseDB.GetAndSyncChatRoomsAsync ();
			Console.WriteLine ("synched");
			RunOnUiThread (() => chatroomsAdapter.NotifyDataSetChanged ());

		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Try to load access token
			SimpleStorage.EditGroup = (string groupName) => {
				return new DroidSimpleStorage(groupName, this);
			};
			var storage = SimpleStorage.EditGroup(LoginActivity.logInDataGroup);
			var accessToken = storage.Get("access_token");
			var userId = storage.Get("user_id");
			if (ParseUser.CurrentUser != null) {
				Task getUserInfoTask = new Task (async () => {
					var query  = ParseObject.GetQuery ("UserInformation").WhereEqualTo ("userId", ParseUser.CurrentUser.ObjectId);
					IEnumerable<ParseObject> userInfo = await query.FindAsync();

					if (userInfo.ToList().Count == 0) {
						var fb = new FacebookClient ();
						fb.AccessToken = accessToken;
						await fb.GetTaskAsync ("me").ContinueWith (t => {
							if(!t.IsFaulted){
								var result = (IDictionary<string, object>) t.Result;
								string profileName = (string) result["name"];
								//ParseUser.CurrentUser["name"] = profileName;
								ParseObject newUserInfo = new ParseObject("UserInformation");
								newUserInfo["userId"] = ParseUser.CurrentUser.ObjectId;
								newUserInfo["fullName"] = profileName;
								newUserInfo.SaveAsync();
							}
						});
					}
				});

				getUserInfoTask.Start ();
				

				Task getUpdatedInfo = new Task (async () => {
					await SynchronizeWithParse ();
				});

				getUpdatedInfo.Start ();

				Task subscribeToUserChannel = new Task (async () => {
					Console.WriteLine("Test + " +ParseUser.CurrentUser.ObjectId);
					string channel = ParseUser.CurrentUser.ObjectId;
					var installation = ParseInstallation.CurrentInstallation;
					if(installation.Channels == null)
						installation.Channels = new List<string>();
					if(installation.Channels.Contains(channel) == false)
						installation.Channels.Add(channel);
					await installation.SaveAsync();

				});
				//subscribeToUserChannel.Start ();
			} else {
				var loginIntent = new Intent(this, typeof(LoginActivity));
				StartActivity(loginIntent);
			}

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			var noContactsTV = FindViewById<TextView> (Resource.Id.no_contacts_added);
			CreateChatRooms ();
			if (chatroomsAdapter.GetCount () > 0)
				noContactsTV.Visibility = ViewStates.Gone;

			var toolbar = FindViewById<Toolbar> (Resource.Id.toolbar);
			//Toolbar will now take on default Action Bar characteristics
			SetActionBar (toolbar);

			ActionBar.Title = "Midgard Messenger";
			// Get our button from the layout resource,
			// and attach an event to it


			Button button = FindViewById<Button> (Resource.Id.add_contact);

			button.Click += delegate {
				var intent = new Intent(this, typeof(ContactsActivity));
				StartActivity(intent);
			};
			ParsePush.ParsePushNotificationReceived += async (sender, args) => {
				Console.WriteLine("TEST PUSH PARSE");
				await SynchronizeWithParse();
			};
		}
		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.home, menu);
			return base.OnCreateOptionsMenu (menu);
		}
		public override bool OnOptionsItemSelected (IMenuItem item)
		{	
			Toast.MakeText(this, "Top ActionBar pressed: " + item.TitleFormatted, ToastLength.Short).Show();
			return base.OnOptionsItemSelected (item);
		}
	}


}


