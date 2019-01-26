using System;


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
	}


	//TODO: turn status into a bool Success or Failed
	//TODO: check all the Status msgs in both Server how are they assigned and Client how are they checked
	public enum Status : byte {
		OK = 1,
		InvalidEmail,
		InvalidUsername,
		EmailAlreayExists,
		LoggedIn,
		FollowAdded,
		FollowRemoved,
		FollowListUpdated,
		LoggedOut,
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
	public MessageEnums.Status Status { get; set; }
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
	public MessageEnums.Status Status { get; set; }
	public string Username { get; private set; }
	public string Discriminator { get; private set; }
	public string Token { get; private set; }
	public string Email { get; set; }

	public ResponseMsg_Login(MessageEnums.Status status, string username, string discremenator, string token) {
		op = MessageEnums.OperationCode.LoginResponse;
		this.Status = status;
		this.Username = username;
		this.Discriminator = discremenator;
		this.Token = token;
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
	public MessageEnums.Status Status { get; set; }
	public Info Follow { get; private set; }

	public ResponseMsg_FollowAddRemove(MessageEnums.Status status, Info follow) {
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
	public MessageEnums.Status Status { get; set; }
	public System.Collections.Generic.List<Info> Follows { get; private set; }

	public ResponseMsg_FollowList(MessageEnums.Status status, System.Collections.Generic.List<Info> follow) {
		op = MessageEnums.OperationCode.FollowListResponse;
		this.Status = status;
		this.Follows = follow;
	}
}