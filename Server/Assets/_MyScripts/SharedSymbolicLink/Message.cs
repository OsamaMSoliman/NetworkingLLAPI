using System;

[Serializable]
public abstract class Message
{
	public OperationCode op = OperationCode.None;

	public enum OperationCode : byte
	{
		None,
		CreateAccountRequest,
		CreateAccountResponse,
		LoginRequest,
		LoginResponse
	}
}

[Serializable]
public class MsgRequest_CreateAccount : Message
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string Email { get; set; }
	public MsgRequest_CreateAccount( string username , string password , string email )
	{
		op = OperationCode.CreateAccountRequest;
		this.Username = username;
		this.Password = password;
		this.Email = email;
	}
}

[Serializable]
public class MsgResponse_CreateAccount : Message
{
	public byte Status { get; set; }
	public string Discremenator { get; set; }
	public MsgResponse_CreateAccount( byte status , string discremenator )
	{
		op = OperationCode.CreateAccountResponse;
		this.Status = status;
		this.Discremenator = discremenator;
	}
}

[Serializable]
public class MsgRequest_Login : Message
{
	public string EmailOrUsername { get; set; }
	public string Password { get; set; }

	public MsgRequest_Login( string EmailOrUsername , string password )
	{
		op = OperationCode.LoginRequest;
		this.EmailOrUsername = EmailOrUsername;
		this.Password = password;
	}
}

[Serializable]
public class MsgResponse_Login : Message
{
	public byte Status { get; set; }
	public string Discremenator { get; set; }
	public MsgResponse_Login( byte status , string discremenator )
	{
		op = OperationCode.LoginResponse;
		this.Status = status;
		this.Discremenator = discremenator;
	}
}