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
using Android.Graphics;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Media;

namespace MidgardMessenger
{
	public class ChatAdapter : BaseAdapter
	{
		List<ChatItem> _chatsList;
		Dictionary<ChatItem, Bitmap> _chatToImageDict = new Dictionary<ChatItem, Bitmap>();
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
			ViewHolder holder;
			ChatItem currChat = GetChatAt (position);
			if (convertView == null) {
				convertView = _activity.LayoutInflater.Inflate (
					Resource.Layout.ChatBubble, parent, false);
				holder = new ViewHolder ();
				holder.usernameView = (TextView)convertView.FindViewById (Resource.Id.chat_bubble_userName); 
				holder.contentView = (TextView)convertView.FindViewById (Resource.Id.chat_bubble_message); 
				holder.imageView = (ImageView)convertView.FindViewById (Resource.Id.chatBubbleImage);
				convertView.Tag = holder;
			} else {
				holder = (ViewHolder)convertView.Tag;
			}

			User chatUser = DatabaseAccessors.UserDatabaseAccessor.GetUser (currChat.senderID);
			RelativeLayout.LayoutParams layParams = (RelativeLayout.LayoutParams)holder.contentView.LayoutParameters; 

			if (chatUser.ID != DatabaseAccessors.CurrentUser ().ID) {
				holder.usernameView.Visibility = ViewStates.Visible;
				holder.usernameView.Text = (chatUser.name);
				holder.contentView.SetBackgroundColor (Color.LightGray);
				layParams.AddRule (LayoutRules.AlignParentLeft);
				layParams.RemoveRule (LayoutRules.AlignParentRight);


			} else {
				holder.usernameView.Visibility = ViewStates.Gone;
				layParams.AddRule (LayoutRules.AlignParentRight);
				layParams.RemoveRule (LayoutRules.AlignParentLeft);
				holder.contentView.SetBackgroundColor (Color.Rgb (205, 239, 250));
				layParams.TopMargin = 25;

			}
			holder.contentView.LayoutParameters = layParams;

			holder.contentView.Text = (currChat.content);


			if (holder.imageView != null) {
				if (currChat.fileName != null && currChat.pathToFile != null) {
					var path = currChat.pathToFile + "/" + currChat.fileName;
					if (UtilsAndConstants.isImage(path)) {
						holder.imageView.SetImageResource (Resource.Drawable.loading_image);

					
						if (System.IO.File.Exists (path)) {
							
							var imageFile = new Java.IO.File (path);
							Bitmap bitmapToDisplay;
							if (_chatToImageDict.ContainsKey (currChat))
								bitmapToDisplay = _chatToImageDict [currChat];
							else {
								bitmapToDisplay = BitmapHelpers.LoadAndResizeBitmap (imageFile.AbsolutePath, 400, 400);
								_chatToImageDict.Add (currChat, bitmapToDisplay);
							}
							holder.imageView.SetImageBitmap (bitmapToDisplay);
							holder.imageView.Visibility = ViewStates.Visible;
							holder.imageView.SetPadding (0, 0, 0, 2);
						
						} else {
							holder.imageView.SetImageResource (Resource.Drawable.image_broken);
						
						}
					} else if (UtilsAndConstants.isVideo (path)) {
						Bitmap bitmapToDisplay;
						if (_chatToImageDict.ContainsKey (currChat))
								bitmapToDisplay = _chatToImageDict [currChat];
						else {
							bitmapToDisplay = bitmapToDisplay = ThumbnailUtils.CreateVideoThumbnail(path, ThumbnailKind.MiniKind);
							_chatToImageDict.Add (currChat, bitmapToDisplay);
						}
						holder.imageView.SetImageBitmap (bitmapToDisplay);
						holder.imageView.Visibility = ViewStates.Visible;
						holder.imageView.SetPadding (0, 0, 0, 2);
					} else if (UtilsAndConstants.isAudio (path)) {
					}

				} else if( currChat.fileName == null || currChat.pathToFile == null) {
					holder.imageView.SetImageBitmap(null);
					holder.imageView.Visibility = ViewStates.Gone;
					holder.imageView.SetPadding(0,0,0,0);
				}

			}





			return convertView;
		}


		private class ViewHolder : Java.Lang.Object{
			public TextView usernameView;
			 public TextView contentView;
			 public ImageView imageView;
		}	
	}
}

