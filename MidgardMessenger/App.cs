using System;
using Android.App;
using Android.Runtime;
using Parse;

namespace MidgardMessenger
{
	[Application(Name="cl.mjolnir.midgardmessenger.App")]
	public class App : Application
	{
		public App (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate ()
		{
			base.OnCreate ();

			// Initialize the parse client with your Application ID and .NET Key found on
			// your Parse dashboard
			ParseClient.Initialize("sq3Jmu8tZ60I8SIT2rR6dWIV3GJ8qM2i18BranLx",
				"23e3kFxr90XyOhOfPIZ3zvnCqRBei1Z5DIr7vsDT");
			ParsePush.ParsePushNotificationReceived += (object sender, ParsePushNotificationEventArgs e) => {
				Console.WriteLine("PARSE PUSH");
			};
		}
	}
}