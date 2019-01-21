using TMPro;
using UnityEngine;

public class LobbyPanel : MonoBehaviour
{
	[SerializeField] TMP_Text welcomeText;
	[SerializeField] TMP_Text authText;
	[Header("Create Account")]
	[SerializeField] TMP_InputField usernameCreateInputField;
	[SerializeField] TMP_InputField passwordCreateInputField;
	[SerializeField] TMP_InputField emailCreateInputField;
	[Header("Login")]
	[SerializeField] TMP_InputField emailOrUsernameLoginInputField;
	[SerializeField] TMP_InputField passwordLoginInputField;

	private CanvasGroup cg;
	private void Start()
	{
		cg = GetComponent<CanvasGroup>();
		Client.Self.CreateAccountResponseReceived += msg => OnCreateAccountResponse((msg.Status & 0) == 0 , msg.Discremenator);
		Client.Self.LogInResponseReceived += msg => OnLogInResponse((msg.Status & 0) == 0);
	}


	public void OnClickCreateAccount()
	{
		cg.interactable = false;
		Client.Self.CreateAccountRequest(usernameCreateInputField.text , passwordCreateInputField.text , emailCreateInputField.text);
	}
	public void OnClickLogin()
	{
		cg.interactable = false;
		Client.Self.LogInRequest(emailOrUsernameLoginInputField.text , passwordLoginInputField.text);
	}
	private void OnCreateAccountResponse( bool success , string name )
	{
		cg.interactable = !success;
		welcomeText.text = name;
	}
	private void OnLogInResponse( bool success )
	{
		cg.interactable = !success;
		authText.text = success ? "Success !" : "Failed !";
	}
}
