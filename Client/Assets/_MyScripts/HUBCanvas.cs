using TMPro;
using UnityEngine;

public class HUBCanvas : MonoBehaviour {
	[SerializeField] private GameObject FollowerPrefab;
	[SerializeField] private TextMeshProUGUI userInfo;
	[SerializeField] private TMP_InputField input;

	private void Start() {
		this.userInfo.text = Client.Self.Info.Username + "#" + Client.Self.Info.Discriminator;
		Client.Self.RequestListOfFollows();
	}

	public void OnClickAddFollow() {
		if (!Utilities.IsUsernameAndDiscriminator(input.text) && !Utilities.IsEmail(input.text)) {
			Debug.Log("Error");
			//TODO: show error
			return;
		}
		Client.Self.RequestAddRemoveFollow(false, input.text);
	}

	public void OnClickRemoveFollow(string username, string discriminator) => Client.Self.RequestAddRemoveFollow(false, username + "#" + discriminator);

}
