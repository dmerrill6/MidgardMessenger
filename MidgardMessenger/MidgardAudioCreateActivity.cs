
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
	[Activity (Label = "MidgardAudioCreateActivity")]			
	public class MidgardAudioCreateActivity : Activity
	{
		Button _selectPhotoBtn;
		Button _selectAudioBtn;
		Button _sendBtn;
		TextView _audioPathTV;
		ImageView _imagePreview;
		string audioPath;
		string imagePath;

		private const int SELECT_IMAGE_RC = 0;
		private const int SELECT_AUDIO_RC = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.MidgardAudioCreate);

			_selectPhotoBtn = FindViewById<Button>(Resource.Id.midgard_audio_select_photo);
			_selectAudioBtn = FindViewById<Button>(Resource.Id.midgard_audio_create_audio);
			_sendBtn = FindViewById<Button>(Resource.Id.send_midgard_audio);
			_audioPathTV = FindViewById<TextView>(Resource.Id.migard_audio_path_preview);
			_imagePreview = FindViewById<ImageView>(Resource.Id.migard_audio_image_preview);

			_selectPhotoBtn.Click += delegate {
				var imageIntent = new Intent ();
				imageIntent.SetType ("image/*");
				imageIntent.SetAction (Intent.ActionGetContent);

				StartActivityForResult (
					Intent.CreateChooser (imageIntent, "Select photo"), SELECT_IMAGE_RC);
			};

			_selectAudioBtn.Click += delegate {
				Intent audioIntent = new Intent (this, typeof(AudioRecorderActivity));
				StartActivityForResult (audioIntent, SELECT_AUDIO_RC);
			};
			_sendBtn.Click += delegate {
				Intent myIntent = new Intent(this, typeof(MidgardAudioCreateActivity));
				myIntent.PutExtra("imagePath", imagePath);
				myIntent.PutExtra("audioPath", audioPath);
				SetResult(Result.Ok, myIntent);
				Finish();
			};
			// Create your application here
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok) {
				switch (requestCode) {
					case SELECT_IMAGE_RC:
						imagePath = ChatRoomActivity.GetFileForUriAsync(this, data.Data, true).Result.Item1;
						var bitmapToDisplay = BitmapHelpers.LoadAndResizeBitmap (imagePath, 300, 300);
						_imagePreview.SetImageBitmap (bitmapToDisplay);
						
						break;
					case SELECT_AUDIO_RC:
						audioPath = data.GetStringExtra("path");
						_audioPathTV.Text = audioPath;
						break;
					
				}
			}
			CheckCompletition();
		}

		private void CheckCompletition(){
			if(imagePath != null && audioPath != null)
				_sendBtn.Enabled = true;
			else
				_sendBtn.Enabled = false;
		}
	}
}

