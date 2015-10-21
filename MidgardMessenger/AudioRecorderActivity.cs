
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
using Android.Media;

namespace MidgardMessenger
{
	[Activity (Label = "AudioRecorderActivity")]			
	public class AudioRecorderActivity : Activity
	{
		MediaRecorder _recorder;
		MediaPlayer _player;
		Button _start;
		Button _stop;
		Button _sendAudio;
		Button _restart;
		Chronometer _chronometer;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AudioRecorder);
			_start = FindViewById<Button> (Resource.Id.start_audio);
			_stop = FindViewById<Button> (Resource.Id.stop_audio);
			_sendAudio = FindViewById<Button>(Resource.Id.send_audio);

			_chronometer = FindViewById<Chronometer>(Resource.Id.audio_chronometer);
			string path = UtilsAndConstants.AudioDir + "/" + "MidgardAudio" + UtilsAndConstants.RandomString(20) + ".3gpp";

			_start.Click += delegate {
				_stop.Enabled = !_stop.Enabled;
				_start.Enabled = !_start.Enabled;

				_recorder.SetAudioSource (AudioSource.Mic);
				_recorder.SetOutputFormat (OutputFormat.ThreeGpp);
				_recorder.SetAudioEncoder (AudioEncoder.AmrNb);
				_recorder.SetOutputFile (path);
				_recorder.Prepare ();
	   	        _recorder.Start ();
	   	        _chronometer.Base = SystemClock.ElapsedRealtime();
	   	        _chronometer.Start();

			} ;

			_stop.Click += delegate {
				_stop.Enabled = !_stop.Enabled;

				_recorder.Stop ();
				_recorder.Reset ();

				_player.SetDataSource (path);
				_sendAudio.Enabled = true;
				_player.Prepare ();
				_player.Start ();
				_chronometer.Stop();

			} ;
			_sendAudio.Click += delegate {
				Intent myIntent = new Intent(this, typeof(AudioRecorderActivity));
				myIntent.PutExtra("path", path);
				SetResult(Result.Ok, myIntent);
				Finish();
			};
		}
		protected override void OnResume ()
		{	
			base.OnResume ();

			_recorder = new MediaRecorder ();
			_player = new MediaPlayer ();

			_player.Completion += (sender, e) => {
				_player.Reset ();
				_start.Enabled = !_start.Enabled;
			} ;

		}

		protected override void OnPause ()
		{
			base.OnPause ();

			_player.Release ();
			_recorder.Release ();
			_player.Dispose ();
			_recorder.Dispose ();
			_player = null;
			_recorder = null;
		}
	}
}

