
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
	[Activity (Label = "ContactsActivity")]			
	public class ContactsActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Contacts);
			Button searchContactsBtn = FindViewById<Button> (Resource.Id.search_contacts);
			searchContactsBtn.Click += delegate {
				var contactsAdapter = new ContactsAdapter (this);
				var contactsListView = FindViewById<ListView> (Resource.Id.contactsListView);
				contactsListView.Adapter = contactsAdapter;	
			};

			Button inviteBtn = FindViewById<Button> (Resource.Id.invite);
			inviteBtn.Click += delegate {

			};


		}
	}
}

