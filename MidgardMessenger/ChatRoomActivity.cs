
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Media;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Parse;
using Android.Graphics;
using Android.Provider;
using Uri = Android.Net.Uri;
using Android.Content.PM;
using Java.IO;
using Android.Database;

namespace MidgardMessenger
{
	[Activity (Label = "ChatRoomActivity")]			
	public class ChatRoomActivity : Activity
	{	
		public static File _file;
	    public static Bitmap bitmap;
	    public static Action<int, string> chatImageUploadProgressUpdated; 
		private const int TAKE_PICTURE_RC = 0;
		private const int SEND_AUDIO_RC = 1;
	    private const int SEND_IMAGE_RC = 2;
		private const int SEND_VIDEO_RC = 3;
		private const int ADD_CHATROOM_NAME_RC = 4;
		private const int ADD_USER_TO_CONV_RC = 5;
		private MediaPlayer _player;
	    
		protected ChatRoom chatroom;
		public ChatRoom ChatRoomAccessor{
			get{
				return chatroom;
			}
		}
		protected ChatAdapter chatsAdapter;

		protected async Task SynchronizeWithParse(){
			ParseChatItemDatabase parseDB = new ParseChatItemDatabase ();
			await parseDB.GetAndSyncChatItemsAsync (chatroom.webID);
			await ParsePush.SubscribeAsync (UtilsAndConstants.PUSH_PREFIX + chatroom.webID);
			RunOnUiThread (() => chatsAdapter.NotifyDataSetChanged ());

			RunOnUiThread( () => FindViewById<ListView>(Resource.Id.chatsListView).SmoothScrollToPosition(chatsAdapter.GetCount() - 1));
			//ChatsActivity.NotifyChatRoomsUpdate();

		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom (Intent.GetStringExtra ("chatroom"));
			DatabaseAccessors.ChatDatabaseAccessor.MarkAsRead(chatroom);
			var crus = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoomUsers(chatroom.webID);
			SetContentView (Resource.Layout.ChatRoom);
			var toolbar = FindViewById<Toolbar> (Resource.Id.chatroom_toolbar);
			//Toolbar will now take on default Action Bar characteristics
			SetActionBar (toolbar);
			toolbar.Click += (object sender, EventArgs e) => {
				Intent intent = new Intent(this, typeof(ChatRoomInfoActivity));
				intent.PutExtra("chatroomWebId", chatroom.webID);
				StartActivityForResult(intent, ADD_CHATROOM_NAME_RC);
			};
			SetChatRoomName();
			chatsAdapter = new ChatAdapter (this);
			var chatsListView = FindViewById<ListView> (Resource.Id.chatsListView);
			chatsListView.Adapter = chatsAdapter;
			chatsListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				ChatItem chatItem = chatsAdapter.GetChatAt(e.Position);
				string path = chatItem.pathToFile + "/" + chatItem.fileName;
				if(UtilsAndConstants.isVideo(path)){
					var intent = new Intent(this, typeof(VideoPlayerActivity));
					intent.PutExtra("chatItem", chatItem.ID);
					StartActivity(intent);
				} else if (UtilsAndConstants.isAudio(path)){
					_player.SetDataSource(path);
					_player.Prepare ();
					_player.Start();

				}

			};

			var sendButton = FindViewById<Button> (Resource.Id.send_message_button);
			var textBoxEdit = FindViewById<EditText> (Resource.Id.chatInputTextBox);
			textBoxEdit.Click += (sender, e) => {
				RunOnUiThread( () => FindViewById<ListView>(Resource.Id.chatsListView).SmoothScrollToPosition(chatsAdapter.GetCount() - 1));
			};
			sendButton.Click += async (sender, e) => {
				EditText textEdit = FindViewById<EditText>(Resource.Id.chatInputTextBox);
				string messageContent = textEdit.Text.ToString();
				RunOnUiThread( () => textEdit.Text = "");
				ParseChatItemDatabase parseItemDB = new ParseChatItemDatabase();
				ChatItem chat = new ChatItem();
				chat.chatroomID = chatroom.webID;
				chat.senderID = DatabaseAccessors.CurrentUser().webID;
				chat.content = messageContent;

				await parseItemDB.SaveChatItemAsync(chat);
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chat);
				RunOnUiThread(() => chatsAdapter.NotifyDataSetChanged());
				RunOnUiThread( () => FindViewById<ListView>(Resource.Id.chatsListView).SmoothScrollToPosition(chatsAdapter.GetCount() - 1));
				DatabaseAccessors.ChatDatabaseAccessor.MarkAsRead(chatroom);


				var push = new ParsePush();
				push.Channels = new List<string> {UtilsAndConstants.PUSH_PREFIX + chatroom.webID};
				push.Alert = "Your men might be requesting help!";
				await push.SendAsync();
			};

			Task getUpdatedInfo = new Task (async () => {
				await SynchronizeWithParse ();
			});

			ParsePush.ParsePushNotificationReceived += async (sender, args) => {
				
				await SynchronizeWithParse();
				ChatsActivity.NotifyChatRoomsUpdate();
			};

			UtilsAndConstants.downloadProgressChanged += (int progress) => {
			RunOnUiThread( () => {
				ProgressBar progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarDownload);
				progressBar.Visibility = ViewStates.Visible;
				progressBar.Progress = progress;
				if(progress >= 100){
					progressBar.Visibility = ViewStates.Gone;
					chatsAdapter.NotifyDataSetChanged();
				}
				
			});

				
			};

			getUpdatedInfo.Start ();
		}



		private bool IsThereAnAppToTakePictures ()
		{
		    Intent intent = new Intent (MediaStore.ActionImageCapture);
		    IList<ResolveInfo> availableActivities =
		        PackageManager.QueryIntentActivities (intent, PackageInfoFlags.MatchDefaultOnly);
		    return availableActivities != null && availableActivities.Count > 0;
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.chatroom, menu);

			return base.OnCreateOptionsMenu (menu);
		}
		private void TakeAPicture ()
		{
			System.Console.WriteLine("AAAB");
		    Intent intent = new Intent (MediaStore.ActionImageCapture);
		    ChatRoomActivity._file = new File (UtilsAndConstants.ImagesDir, String.Format("MidgardPhoto_{0}.jpg", Guid.NewGuid()));
		    intent.PutExtra (MediaStore.ExtraOutput, Uri.FromFile (ChatRoomActivity._file));
		    StartActivityForResult (intent, TAKE_PICTURE_RC);
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			// Make it available in the gallery
			if (resultCode == Result.Ok) {
				switch (requestCode) {
					case TAKE_PICTURE_RC:
						CaptureCameraImageAndSave();
						break;
					case SEND_IMAGE_RC:
						SaveSelectedImageFromGallery(data);
						break;
					case SEND_AUDIO_RC:
						SaveRecordedAudio(data);
						break;
					case SEND_VIDEO_RC:
						SaveSelectedVideoFromGallery(data);
						break;
					case ADD_CHATROOM_NAME_RC:
						SetChatRoomName();
						break;
				}
			}
		}
		private void SetChatRoomName ()
		{
			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom(chatroom.webID);
			List<User> users = DatabaseAccessors.ChatRoomDatabaseAccessor.GetUsers(chatroom.webID).ToList();
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
		private void SaveRecordedAudio (Intent data)
		{
			var path = data.GetStringExtra("path");
			string fileName = path.Substring(path.LastIndexOf('/') + 1);
			string fullPath = path.Substring(0,path.LastIndexOf('/'));
			ParseChatItemDatabase pcid = new ParseChatItemDatabase();
            ChatItem chatitem = new ChatItem();
            chatitem.chatroomID = chatroom.webID;
            chatitem.content = DatabaseAccessors.CurrentUser().name + " sent an audio!";
            chatitem.fileName = fileName;
            chatitem.pathToFile = fullPath;
            chatitem.senderID = DatabaseAccessors.CurrentUser().webID;
			DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);
            chatsAdapter.NotifyDataSetChanged();
			Task saveItemAsync = new Task (async () => {

				await pcid.SaveChatItemAsync(chatitem, FindViewById<ProgressBar>(Resource.Id.progressBarUpload), this);
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);

				var push = new ParsePush();
				push.Channels = new List<string> {UtilsAndConstants.PUSH_PREFIX + chatroom.webID};
				push.Alert = "Your men might be requesting help!";
				await push.SendAsync();
        	});
        	saveItemAsync.Start();
		}
		private void SaveSelectedImageFromGallery(Intent data){
			var path = GetFileForUriAsync(this, data.Data, true).Result.Item1;
			string fileName = path.Substring(path.LastIndexOf('/') + 1);
			string fullPath = path.Substring(0,path.LastIndexOf('/'));
			ParseChatItemDatabase pcid = new ParseChatItemDatabase();
            ChatItem chatitem = new ChatItem();
            chatitem.chatroomID = chatroom.webID;
            chatitem.content = DatabaseAccessors.CurrentUser().name + " sent a photograph!";
            chatitem.fileName = fileName;
            chatitem.pathToFile = fullPath;
            chatitem.senderID = DatabaseAccessors.CurrentUser().webID;
			DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);
            chatsAdapter.NotifyDataSetChanged();
			Task saveItemAsync = new Task (async () => {

				await pcid.SaveChatItemAsync(chatitem, FindViewById<ProgressBar>(Resource.Id.progressBarUpload), this);
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);

				var push = new ParsePush();
				push.Channels = new List<string> {UtilsAndConstants.PUSH_PREFIX + chatroom.webID};
				push.Alert = "Your men might be requesting help!";
				await push.SendAsync();
        	});
        	saveItemAsync.Start();
		}

		private void SaveSelectedVideoFromGallery(Intent data){
			var path = GetFileForUriAsync(this, data.Data, false).Result.Item1;
			string fileName = path.Substring(path.LastIndexOf('/') + 1);
			string fullPath = path.Substring(0,path.LastIndexOf('/'));
			ParseChatItemDatabase pcid = new ParseChatItemDatabase();
            ChatItem chatitem = new ChatItem();
            chatitem.chatroomID = chatroom.webID;
            chatitem.content = DatabaseAccessors.CurrentUser().name + " sent a video!";
            chatitem.fileName = fileName;
            chatitem.pathToFile = fullPath;
            chatitem.senderID = DatabaseAccessors.CurrentUser().webID;
			DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);
            chatsAdapter.NotifyDataSetChanged();

			

			Task saveItemAsync = new Task (async () => {
				await pcid.SaveChatItemAsync(chatitem, FindViewById<ProgressBar>(Resource.Id.progressBarUpload), this);
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);
				var push = new ParsePush();
				push.Channels = new List<string> {UtilsAndConstants.PUSH_PREFIX + chatroom.webID};
				push.Alert = "Your men might be requesting help!";
				await push.SendAsync();
        	});
        	saveItemAsync.Start();
		}

		internal static Task<Tuple<string, bool>> GetFileForUriAsync (Context context, Uri uri, bool isPhoto)
		{
			var tcs = new TaskCompletionSource<Tuple<string, bool>>();

			if (uri.Scheme == "file")
				tcs.SetResult (new Tuple<string, bool> (new System.Uri (uri.ToString()).LocalPath, false));
			else if (uri.Scheme == "content")
			{
				Task.Factory.StartNew (() =>
				{
					ICursor cursor = null;
					try
					{
						string contentPath = null;
						try {
							string[] proj = null;
                        	//Android 5.1.1 requires projection
                        	if((int)Build.VERSION.SdkInt >= 22)
                            	proj = new[] { MediaStore.MediaColumns.Data };
							cursor = context.ContentResolver.Query (uri, proj, null, null, null);
						} catch (Exception) {
						}
						if (cursor != null && cursor.MoveToNext()) {
							int column = cursor.GetColumnIndex (MediaStore.MediaColumns.Data);
                            if (column != -1)
                                contentPath = cursor.GetString (column);
						}
						
						bool copied = false;

						// If they don't follow the "rules", try to copy the file locally
						if (contentPath == null || !contentPath.StartsWith ("file"))
						{
							copied = true;
							Uri outputPath = GetOutputMediaFile (context, "temp", null, isPhoto);

							try
							{
								using (System.IO.Stream input = context.ContentResolver.OpenInputStream (uri))
								using (System.IO.Stream output = System.IO.File.Create (outputPath.Path))
									input.CopyTo (output);

								contentPath = outputPath.Path;
							}
							catch (Exception)
							{
								// If there's no data associated with the uri, we don't know
								// how to open this. contentPath will be null which will trigger
								// MediaFileNotFoundException.
							}
						}

						tcs.SetResult (new Tuple<string, bool> (contentPath, copied));
					}
					finally
					{
						if (cursor != null)
						{
							cursor.Close();
							cursor.Dispose();
						}
					}
				}, System.Threading.CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
			}
			else
				tcs.SetResult (new Tuple<string, bool> (null, false));

			return tcs.Task;
		}

		private static Uri GetOutputMediaFile (Context context, string subdir, string name, bool isPhoto)
		{
			subdir = subdir ?? String.Empty;

			if (String.IsNullOrWhiteSpace (name))
			{
				string timestamp = DateTime.Now.ToString ("yyyyMMdd_HHmmss");
				if (isPhoto)
					name = "IMG_" + timestamp + ".jpg";
				else
					name = "VID_" + timestamp + ".mp4";
			}

			string mediaType = (isPhoto) ? Android.OS.Environment.DirectoryPictures : Android.OS.Environment.DirectoryMovies;
			using (Java.IO.File mediaStorageDir = new Java.IO.File (context.GetExternalFilesDir (mediaType), subdir))
			{
				if (!mediaStorageDir.Exists())
				{
					if (!mediaStorageDir.Mkdirs())
						throw new IOException ("Couldn't create directory, have you added the WRITE_EXTERNAL_STORAGE permission?");

					// Ensure this media doesn't show up in gallery apps
					using (Java.IO.File nomedia = new Java.IO.File (mediaStorageDir, ".nomedia"))
						nomedia.CreateNewFile();
				}

				return Uri.FromFile (new Java.IO.File (GetUniquePath (mediaStorageDir.Path, name, isPhoto)));
			}
		}

		private static string GetUniquePath (string folder, string name, bool isPhoto)
		{
			string ext = System.IO.Path.GetExtension (name);
			if (ext == String.Empty)
				ext = ((isPhoto) ? ".jpg" : ".mp4");

			name = System.IO.Path.GetFileNameWithoutExtension (name);

			string nname = name + ext;
			int i = 1;
			while (System.IO.File.Exists (System.IO.Path.Combine (folder, nname)))
				nname = name + "_" + (i++) + ext;

			return System.IO.Path.Combine (folder, nname);
		}



		private void CaptureCameraImageAndSave ()
		{
			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(ChatRoomActivity._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            ParseChatItemDatabase pcid = new ParseChatItemDatabase();
            ChatItem chatitem = new ChatItem();
            chatitem.chatroomID = chatroom.webID;
            chatitem.content = DatabaseAccessors.CurrentUser().name + " sent a photograph!";
            chatitem.fileName = ChatRoomActivity._file.Name;
			System.Console.WriteLine(ChatRoomActivity._file.Name);
            
            chatitem.pathToFile = UtilsAndConstants.ImagesDir.Path;
            chatitem.senderID = DatabaseAccessors.CurrentUser().webID;
            DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);
            chatsAdapter.NotifyDataSetChanged();
			Task saveItemAsync = new Task (async () => {
            	await pcid.SaveChatItemAsync(chatitem, FindViewById<ProgressBar>(Resource.Id.progressBarUpload), this);
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);
				var push = new ParsePush();
				push.Channels = new List<string> {UtilsAndConstants.PUSH_PREFIX + chatroom.webID};
				push.Alert = "Your men might be requesting help!";
				await push.SendAsync();
        	});
        	saveItemAsync.Start();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{	
			switch (item.ItemId) {
			case Resource.Id.send_image_button:
				var imageIntent = new Intent ();
				imageIntent.SetType ("image/*");
				imageIntent.SetAction (Intent.ActionGetContent);

				StartActivityForResult (
					Intent.CreateChooser (imageIntent, "Select photo"), SEND_IMAGE_RC);
				break;
			case Resource.Id.take_a_photograph_button:
				TakeAPicture ();					
				break;
			case Resource.Id.send_video_button:
				Intent intent = new Intent ();
				intent.SetType ("video/*");
				intent.SetAction (Intent.ActionGetContent);
				StartActivityForResult (Intent.CreateChooser (intent, "Send a Video"), SEND_VIDEO_RC);
				break;
			case Resource.Id.send_audio_button:
				Intent audioIntent = new Intent (this, typeof(AudioRecorderActivity));
				StartActivityForResult (audioIntent, SEND_AUDIO_RC);
				break;
			
			}	
		
			return base.OnOptionsItemSelected (item);

		}


		protected void LoadMessages()
		{
			
		}
		protected override void OnResume ()
		{	
			base.OnResume ();
			SetChatRoomName();

			_player = new MediaPlayer ();
			_player.Completion += (sender, e) => {
				_player.Reset ();
			
			} ;

		}

		protected override void OnPause ()
		{
			base.OnPause ();

			_player.Release ();

			_player.Dispose ();

			_player = null;

		}
	}


}

