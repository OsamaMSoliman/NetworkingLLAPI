using System;

[Serializable]
public static class MessageEnums
{
	public enum OperationCode : byte
	{
		None,
		CreateAccountRequest,
		CreateAccountResponse,
		LoginRequest,
		LoginResponse
	}

	public enum Status : byte
	{
		OK = 1,
		InvalidEmail,
		InvalidUsername,
		EmailAlreayExists,
		LoggedIn,
	}
}

[Serializable]
public abstract class Message { public MessageEnums.OperationCode op = MessageEnums.OperationCode.None; }

[Serializable]
public class RequestMsg_CreateAccount : Message
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string Email { get; set; }
	public RequestMsg_CreateAccount(string username, string password, string email)
	{
		op = MessageEnums.OperationCode.CreateAccountRequest;
		this.Username = username;
		this.Password = password;
		this.Email = email;
	}
}

[Serializable]
public class ResponseMsg_CreateAccount : Message
{
	public MessageEnums.Status Status { get; set; }
	public ResponseMsg_CreateAccount(MessageEnums.Status status)
	{
		op = MessageEnums.OperationCode.CreateAccountResponse;
		this.Status = status;
	}
}

[Serializable]
public class RequestMsg_Login : Message
{
	public string EmailOrUsername { get; set; }
	public string Password { get; set; }

	public RequestMsg_Login(string EmailOrUsername, string password)
	{
		op = MessageEnums.OperationCode.LoginRequest;
		this.EmailOrUsername = EmailOrUsername;
		this.Password = password;
	}
}

[Serializable]
public class ResponseMsg_Login : Message
{
	public MessageEnums.Status Status { get; set; }
	public string Username { get; set; }
	public string Discremenator { get; set; }
	public string Token { get; set; }

	public ResponseMsg_Login(MessageEnums.Status status, string username, string discremenator, string token)
	{
		op = MessageEnums.OperationCode.LoginResponse;
		this.Status = status;
		this.Username = username;
		this.Discremenator = discremenator;
		this.Token = token;
	}
}