
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using PerpetualEngine.Storage;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Xamarin.Auth;
using Parse;
using Facebook;

namespace MidgardMessenger
{
	[Activity (Label = "Midgard Messenger")]			
	public class LoginActivity : Activity
	{
		
		private const string AppId = "908126919279823";
		private const string ExtendedPermissions = "";
		public const string logInDataGroup = "login_data_group";
		FacebookClient fb;
		bool isLoggedIn;
		string lastMessageId;

		void LoginToGoogle (bool allowCancel)
		{
			var auth = new OAuth2Authenticator (
				clientId: "864887531566-q0hdj7jm6caqib1qk37qbh7aa0aec2tt.apps.googleusercontent.com",
				scope: "https://www.googleapis.com/auth/plus.login https://www.googleapis.com/auth/userinfo.email",
				authorizeUrl: new Uri ("https://accounts.google.com/o/oauth2/auth"),
				redirectUrl: new Uri ("http://midgard-messenger.herokuapp.com"));

			auth.AllowCancel = allowCancel;


			// If authorization succeeds or is canceled, .Completed will be fired.
			auth.Completed += (s, ee) => {
				if (!ee.IsAuthenticated) {
					var builder = new AlertDialog.Builder (this);
					builder.SetMessage ("Not Authenticated");
					builder.SetPositiveButton ("Ok", (o, e) => { });
					builder.Create().Show();
					return;
				}


				// Now that we're logged in, make a OAuth2 request to get the user's info.
				var request = new OAuth2Request ("GET", new Uri ("https://www.googleapis.com/oauth2/v1/userinfo" ), null, ee.Account);
				request.GetResponseAsync().ContinueWith (t => {
					var builder = new AlertDialog.Builder (this);
					if (t.IsFaulted) {
						builder.SetTitle ("Error");
						builder.SetMessage (t.Exception.Flatten().InnerException.ToString());
					} else if (t.IsCanceled)
						builder.SetTitle ("Task Canceled");
					else {
						var obj = Newtonsoft.Json.Linq.JObject.Parse(t.Result.GetResponseText());

						builder.SetTitle ("Logged in");
						builder.SetMessage ("Name: " + obj);

						var chatsIntent = new Intent(this, typeof(ChatsActivity));
						StartActivity(chatsIntent);

//						var serverRequest = HttpWebRequest.Create("http://midgard-messenger.herokuapp.com/api/users");
//						serverRequest.ContentType = "application/json";
//						serverRequest.Method = "POST";
//						serverRequest.ContentType = "text/json";
//
//						using (var streamWriter = new StreamWriter(serverRequest.GetRequestStream()))
//						{
//							string json = t.Result.GetResponseText();
//
//							streamWriter.Write(json);
//							streamWriter.Flush();
//							streamWriter.Close();
//						}
//
//						var httpResponse = (HttpWebResponse)serverRequest.GetResponse();
//						string result = "error";
//						using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
//						{
//							result = streamReader.ReadToEnd();
//						}
//
//						builder.SetMessage(result);
					}

					builder.SetPositiveButton ("Ok", (o, e) => { });
					//builder.Create().Show();
				}, UIScheduler);
			};

			var intent = auth.GetUI (this);
			StartActivity (intent);
		}

		private static readonly TaskScheduler UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SimpleStorage.EditGroup = (string groupName) => {
				return new DroidSimpleStorage(groupName, this);
			};

			SetContentView (Resource.Layout.Login);
			Button button = FindViewById<Button> (Resource.Id.sign_in_button);

			button.Click += delegate {LoginToGoogle(true);};
					
			Button facebooklogin = FindViewById<Button>(Resource.Id.sign_in_facebook_button);

			facebooklogin.Click += async (sender, e) => {
				var webAuth = new Intent (this, typeof(FBWebAuthActivity));
				webAuth.PutExtra("AppId", AppId);
				webAuth.PutExtra("ExtendedPermissions", ExtendedPermissions);
				StartActivityForResult(webAuth, 0);

			};


//			var facebookNoCancel = FindViewById<Button> (Resource.Id.FacebookButtonNoCancel);
//			facebookNoCancel.Click += delegate { LoginToFacebook(false);};
		}
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (resultCode) {
			case Result.Ok:

				string accessToken = data.GetStringExtra ("AccessToken");
				string userId = data.GetStringExtra ("UserId");
				string error = data.GetStringExtra ("Exception");

				// Save access token
				var storage = SimpleStorage.EditGroup(logInDataGroup);
				storage.Put("access_token", accessToken);
				storage.Put("user_id", userId);

				fb = new FacebookClient (accessToken);


				fb.GetTaskAsync ("me").ContinueWith( t => {
					
					if (!t.IsFaulted) {
						var result = (IDictionary<string, object>)t.Result;
						Console.WriteLine("entered " + t.Result);

						string profileData = "Name: " + (string)result["name"] + "\n";


						Task registerUser = new Task (async () => {
							Console.WriteLine("logging in");
							ParseUser user = await ParseFacebookUtils.LogInAsync (userId, accessToken, DateTime.Now.AddYears (1));
							isLoggedIn = true;
							Finish();
						});

						registerUser.Start ();
					}
				});

				break;
			case Result.Canceled:
				Alert ("Failed to Log In", "User Cancelled", false, (res) => {} );
				break;
			default:
				break;
			}
		}
		public void Alert (string title, string message, bool CancelButton , Action<Result> callback)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetTitle(title);
			builder.SetMessage(message);

			builder.SetPositiveButton("Ok", (sender, e) => {
				callback(Result.Ok);
			});

			if (CancelButton) {
				builder.SetNegativeButton("Cancel", (sender, e) => {
					callback(Result.Canceled);
				});
			}

			builder.Show();
		}
	}
}

