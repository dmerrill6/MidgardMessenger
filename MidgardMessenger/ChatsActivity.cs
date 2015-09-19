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

namespace MidgardMessenger
{
	[Activity (Label = "Midgard Messenger", MainLauncher = true, Icon = "@drawable/icon")]
	public class ChatsActivity : Activity
	{
		protected bool isLoggedIn;
		protected ParseUser user;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			// Try to load access token
			SimpleStorage.EditGroup = (string groupName) => {
				return new DroidSimpleStorage(groupName, this);
			};
			var storage = SimpleStorage.EditGroup(LoginActivity.logInDataGroup);
			var accessToken = storage.Get("access_token2");
			var userId = storage.Get("user_id");

			if (accessToken != null) {
				Task registerUser = new Task (async () => {
					Console.WriteLine ("logging in");
					user = await ParseFacebookUtils.LogInAsync (userId, accessToken, DateTime.Now.AddYears (1));
					isLoggedIn = true;
				});
				registerUser.Start ();
			} else {
				var loginIntent = new Intent(this, typeof(LoginActivity));
				StartActivity(loginIntent);
			}

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
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


