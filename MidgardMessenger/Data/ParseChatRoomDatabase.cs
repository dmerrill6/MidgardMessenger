﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Parse;

namespace MidgardMessenger
{
	public class ParseChatRoomDatabase
	{
		
		public ParseChatRoomDatabase ()
		{
		}

		ParseObject ToParseObject (ChatRoom chatroom)
		{
			var po = new ParseObject ("ChatRoom");
			if (chatroom.chatRoomName != null) {
				po["chatroomName"] = chatroom.chatRoomName;
			}
			if(chatroom.webID != null)
				po.ObjectId = chatroom.webID;
			return po;
		}

		ParseObject ToParseObject(ChatRoomUser cru)
		{
			var po = new ParseObject ("ChatRoomUser");
			po ["userId"] = cru.userID;
			po ["chatroomId"] = cru.chatRoomID;

			return po;
		}

		static ChatRoom FromParseObject (ParseObject po)
		{
			var t = new ChatRoom ();
			t.webID = po.ObjectId;
			t.createdAt = po.CreatedAt.Value;
			if(po.Keys.Contains("chatroomName"))
				t.chatRoomName= (string)po["chatroomName"];
			return t;
		}
		static ChatRoomUser FromParseObjectChatRoomUser(ParseObject po){
			var t = new ChatRoomUser ();
			t.userID = (string)po ["userId"];
			t.chatRoomID = (string)po ["chatroomId"];

			
			return t;
		}

		public async Task SaveChatRoomAsync(ChatRoom chatroom)
		{
			ParseObject po = ToParseObject (chatroom);
			await po.SaveAsync ();
			chatroom.webID = po.ObjectId;
		}

		public async Task SaveChatRoomUsersAsync(ChatRoomUser cru){
			ParseObject po = ToParseObject (cru);
			await po.SaveAsync ();
		}

		public async Task DeleteChatRoomUserAsync(ChatRoomUser cru){
			var query = ParseObject.GetQuery ("ChatRoomUser").WhereEqualTo ("userId", cru.userID).WhereEqualTo ("chatroomId", cru.chatRoomID);
			IEnumerable<ParseObject> poList = await query.FindAsync ();
			foreach (ParseObject po in poList) {
				await po.DeleteAsync ();
			}
		}

		public async Task GetAndSyncChatRoomsAsync ()
		{
			var query = ParseObject.GetQuery ("ChatRoomUser").WhereEqualTo ("userId", ParseUser.CurrentUser.ObjectId).Select ("chatroomId");
			IEnumerable<ParseObject> chatRoomIds = await query.FindAsync ();
			List<ParseObject> results = new List<ParseObject> ();
			foreach (ParseObject crpo in chatRoomIds) {
				var currCRUQuery = ParseObject.GetQuery ("ChatRoomUser").WhereEqualTo ("chatroomId", crpo ["chatroomId"]);
				foreach (ParseObject cruPO in await currCRUQuery.FindAsync()) {
					results.Add (cruPO);
				}
			}
			List<ChatRoomUser> crusToAdd = new List<ChatRoomUser> ();
			List<String> crToAdd = new List<String> ();
			List<String> usersToAdd = new List<String> ();
			List<string> crToUpdate = new List<string> ();
			foreach (var cruPO in results) {
				ChatRoomUser cru = FromParseObjectChatRoomUser (cruPO);
				if (DatabaseAccessors.ChatRoomDatabaseAccessor.ExistsChatRoomUser (cru.chatRoomID, cru.userID) == false)
					crusToAdd.Add (cru);
				if (DatabaseAccessors.ChatRoomDatabaseAccessor.ExistsChatRoom (cru.chatRoomID) == false && crToAdd.Contains (cru.chatRoomID) == false)
					crToAdd.Add (cru.chatRoomID);
				if (DatabaseAccessors.ChatRoomDatabaseAccessor.ExistsChatRoom (cru.chatRoomID))
					crToUpdate.Add (cru.chatRoomID);
				if (DatabaseAccessors.UserDatabaseAccessor.ExistsUser (cru.userID) == false && usersToAdd.Contains (cru.userID) == false)
					usersToAdd.Add (cru.userID);
			}
			foreach (string userId in usersToAdd) {
				User newUser = new User ();
				var userQuery = ParseObject.GetQuery ("UserInformation").WhereEqualTo ("userId", userId);
				ParseObject userPO = await userQuery.FirstAsync ();
				newUser.name = (string)userPO ["fullName"];
				newUser.webID = userId;
				DatabaseAccessors.UserDatabaseAccessor.SaveUser (newUser);
			}

			foreach (string crid in crToAdd) {
				
				var crQuery = ParseObject.GetQuery ("ChatRoom").WhereEqualTo ("objectId", crid);

				var crPO = await crQuery.FirstAsync ();
				var currCR = FromParseObject (crPO);
				DatabaseAccessors.ChatRoomDatabaseAccessor.SaveChatRoom (currCR);
			}

			foreach (string crid in crToUpdate) {
				var crQuery = ParseObject.GetQuery ("ChatRoom");
				var crPOs = await crQuery.FindAsync ();
				foreach (ParseObject crPO in crPOs) {
					var currCR = FromParseObject (crPO);
					if (DatabaseAccessors.ChatRoomDatabaseAccessor.ExistsChatRoom (currCR.webID)) {
						var target = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom (currCR.webID);
						if (target.chatRoomName != currCR.chatRoomName) {
							DatabaseAccessors.ChatRoomDatabaseAccessor.UpdateChatRoom (currCR);
						}
					}
				}
		
				
			}

			foreach (ChatRoomUser cru in crusToAdd) {
				User currUser = DatabaseAccessors.UserDatabaseAccessor.GetUser (cru.userID);
				List<User> currUserAsList = new List<User> ();
				currUserAsList.Add (currUser);
				ChatRoom chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom (cru.chatRoomID);
				DatabaseAccessors.ChatRoomDatabaseAccessor.SaveChatRoomUsers (currUserAsList, chatroom);
			}


		}
	}
}

