using System;
using TMPro;
using UnityEngine;

public class LobbyPanel : MonoBehaviour {
	[SerializeField] TMP_Text infoText;
	[Header("Create Account")]
	[SerializeField] TMP_InputField usernameCreateInputField;
	[SerializeField] TMP_InputField passwordCreateInputField;
	[SerializeField] TMP_InputField emailCreateInputField;
	[Header("Login")]
	[SerializeField] TMP_InputField emailOrUsernameLoginInputField;
	[SerializeField] TMP_InputField passwordLoginInputField;

	private static string SENDING_REQUEST = "Sending Request...";
	private CanvasGroup cg;

	private Action<ResponseMsg_CreateAccount> ocar;
	private Action<ResponseMsg_Login> olr;
	private Action timeout;

	private void Start() {
		cg = GetComponent<CanvasGroup>();

		ocar = msg => OnCreateAccountResponse(msg.Status == MessageEnums.Status.OK, msg.Status.ToString());
		olr = msg => OnLogInResponse(msg.Status == MessageEnums.Status.OK, msg.Status.ToString());
		timeout = delegate { cg.interactable = true; infoText.text = "Timeout!"; };

		Client.Self.CreateAccountResponseReceived += ocar;
		Client.Self.LogInResponseReceived += olr;
		Client.Self.TimeOut += timeout;
	}

	private void OnDestroy() {
		Client.Self.CreateAccountResponseReceived -= ocar;
		Client.Self.LogInResponseReceived -= olr;
		Client.Self.TimeOut -= timeout;
	}

	public void OnClickCreateAccount() {
		infoText.text = Client.Self.RequestCreateAccount(usernameCreateInputField.text, passwordCreateInputField.text, emailCreateInputField.text) ?? SENDING_REQUEST;
		cg.interactable = infoText.text != SENDING_REQUEST;
	}
	public void OnClickLogin() {
		infoText.text = Client.Self.RequestLogIn(emailOrUsernameLoginInputField.text, passwordLoginInputField.text) ?? SENDING_REQUEST;
		cg.interactable = infoText.text != SENDING_REQUEST;
	}

	private void OnCreateAccountResponse(bool success, string msg) {
		cg.interactable = true;
		infoText.text = (success ? "Success !" : "Failed !") + " " + msg;
	}

	private void OnLogInResponse(bool success, string msg) {
		cg.interactable = !success;
		infoText.text = (success ? "Success !" : "Failed !") + " " + msg;
	}
}
