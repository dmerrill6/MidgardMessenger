
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
namespace MidgardMessenger
{
	public class ContactsAdapter : BaseAdapter
	{
		List<User> _contactList;
		bool preventReload = false;
		Activity _activity;

		public ContactsAdapter (Activity activity)
		{
			_activity = activity;
			FillContacts ();
		}

		public override Java.Lang.Object GetItem (int position)
		{
			throw new NotImplementedException ();
		}

		void FillContacts ()
		{
			_contactList = DatabaseAccessors.UserDatabaseAccessor.GetUsers ().ToList ();
		}
		public void SetContactList (List<User> contactList)
		{
			_contactList = contactList;
			preventReload = true;
			base.NotifyDataSetChanged();
		}

		public override void NotifyDataSetChanged ()
		{
			if(preventReload == false)
				FillContacts();
			base.NotifyDataSetChanged ();
		}



		public override int Count {
			get { return _contactList.Count; }
		}

		public User GetUserAt(int position)
		{
			return _contactList [position];
		}

		public override long GetItemId (int position) {
			return _contactList [position].ID;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView ?? _activity.LayoutInflater.Inflate (
				Resource.Layout.ContactListItem, parent, false);
			var contactName = view.FindViewById<TextView> (Resource.Id.ContactName);
			var contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			contactName.Text = _contactList [position].name;


			contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			contactImage.SetImageResource (Resource.Drawable.ContactImage);
		

			return view;
		}

	
	}
}