using System;
using System.Collections.Generic;


//NOTE: A lot of C# 4 code had to be downgraded to C# 3, ALWAYS modify from the Server side!


[Serializable]
public static class MessageEnums {
	public enum OperationCode : byte {
		None,
		CreateAccountRequest,
		CreateAccountResponse,
		LoginRequest,
		LoginResponse,
		FollowAddRemoveRequest,
		FollowAddRemoveResponse,
		FollowListRequest,
		FollowListResponse,
		FollowUpdateResponse,
	}


	public enum Status : byte {
		ERROR, // ~ TODO: this is a placeholder, all errors should be identified in the last version ~
		OK,
		InvalidEmail,
		InvalidUsername,
		EmailAlreayExists,
		FollowAdded,
		FollowRemoved,
		AccountDoesntExist,
	}

	public enum AccountStatus : byte {
		Offline,
		Online,
		Playing,
		AFK,
	}
}

[Serializable]
public abstract class Message {

	//public MessageEnums.OperationCode op { get; protected set; } = MessageEnums.OperationCode.None;
	public MessageEnums.OperationCode op { get; protected set; }
	protected Message() { op = MessageEnums.OperationCode.None; }
}

[Serializable]
public class RequestMsg_CreateAccount : Message {
	public string Username { get; private set; }
	public string Password { get; private set; }
	public string Email { get; private set; }
	public RequestMsg_CreateAccount(string username, string password, string email) {
		op = MessageEnums.OperationCode.CreateAccountRequest;
		this.Username = username;
		this.Password = password;
		this.Email = email;
	}
}

[Serializable]
public class ResponseMsg_CreateAccount : Message {
	public MessageEnums.Status Status { get; private set; }
	public ResponseMsg_CreateAccount(MessageEnums.Status status) {
		op = MessageEnums.OperationCode.CreateAccountResponse;
		this.Status = status;
	}
}

[Serializable]
public class RequestMsg_Login : Message {
	public string EmailOrUsername { get; private set; }
	public string Password { get; private set; }

	public RequestMsg_Login(string EmailOrUsername, string password) {
		op = MessageEnums.OperationCode.LoginRequest;
		this.EmailOrUsername = EmailOrUsername;
		this.Password = password;
	}
}

[Serializable]
public class ResponseMsg_Login : Message {
	public MessageEnums.Status Status { get; private set; }
	public string Username { get; private set; }
	public string Discriminator { get; private set; }
	public string Token { get; private set; }
	public string Email { get; private set; }

	public ResponseMsg_Login(MessageEnums.Status Status, Account account) {
		op = MessageEnums.OperationCode.LoginResponse;
		this.Status = Status;
		if (Status != MessageEnums.Status.OK)
			return;
		this.Username = account.Username;
		this.Discriminator = account.Discriminator;
		this.Token = account.Token;
		this.Email = account.Email;
	}
}

[Serializable]
public class RequestMsg_FollowAddRemove : Message {
	public bool Unfollow { get; private set; } // whether it's add or remove
	public string Token { get; private set; } // who's asking
	public string UsernameDiscriminatorOrEmail { get; private set; }
	public bool IsEmail { get; private set; }

	public RequestMsg_FollowAddRemove(bool unfollow, string token, string UsernameDiscriminatorOrEmail, bool IsEmail) {
		op = MessageEnums.OperationCode.FollowAddRemoveRequest;
		this.Unfollow = unfollow;
		this.Token = token;
		this.UsernameDiscriminatorOrEmail = UsernameDiscriminatorOrEmail;
		this.IsEmail = IsEmail;
	}
}

[Serializable]
public class ResponseMsg_FollowAddRemove : Message {
	public MessageEnums.Status Status { get; private set; } // Not utilized, onAdd status = OK | onRemove no msg sent to begin with
	public PublicInfo Follow { get; private set; }

	public ResponseMsg_FollowAddRemove(MessageEnums.Status status, PublicInfo follow) {
		op = MessageEnums.OperationCode.FollowAddRemoveResponse;
		this.Status = status;
		this.Follow = follow;
	}
}


[Serializable]
public class RequestMsg_FollowList : Message {
	public string Token { get; private set; } // who's asking

	public RequestMsg_FollowList(string token) {
		op = MessageEnums.OperationCode.FollowListRequest;
		this.Token = token;
	}
}

[Serializable]
public class ResponseMsg_FollowList : Message {
	public MessageEnums.Status Status { get; private set; }// redundant as the server won't send the msg if the Follows list was empty
	public List<PublicInfo> Follows { get; private set; }

	public ResponseMsg_FollowList(MessageEnums.Status status, List<PublicInfo> follows) {
		op = MessageEnums.OperationCode.FollowListResponse;
		this.Status = status;
		this.Follows = follows;
	}
}

[Serializable]
public class ResponseMsg_FollowUpdate : Message {
	//public MessageEnums.Status Status { get; private set; } // redundant as the server is the one who process the msg (internally not needed, easier to check for null and not sending)
	public PublicInfo Follow { get; private set; }

	public ResponseMsg_FollowUpdate(PublicInfo follow) {
		op = MessageEnums.OperationCode.FollowUpdateResponse;
		this.Follow = follow;
	}
}