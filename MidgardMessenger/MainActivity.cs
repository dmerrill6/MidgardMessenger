﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Auth;


namespace MidgardMessenger
{
	[Activity (Label = "LoginActivity")]			
	public class LoginActivity : Activity
	{
		

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
					builder.Create().Show();
				}, UIScheduler);
			};

			var intent = auth.GetUI (this);
			StartActivity (intent);
		}

		private static readonly TaskScheduler UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Login);
			Button button = FindViewById<Button> (Resource.Id.sign_in_button);
			button.Click += delegate {LoginToGoogle(true);};


//			var facebookNoCancel = FindViewById<Button> (Resource.Id.FacebookButtonNoCancel);
//			facebookNoCancel.Click += delegate { LoginToFacebook(false);};
		}
	}
}

