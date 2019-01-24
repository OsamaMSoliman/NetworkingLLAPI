using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUBCanvas : MonoBehaviour {
	[SerializeField] private GameObject FollowerPrefab;
	[SerializeField] private TextMeshProUGUI userInfo;
	[SerializeField] private TMP_InputField input;
	[SerializeField] private Transform followListContent;

	private Action<ResponseMsg_FollowAddRemove> frr;

	private void Start() {
		this.userInfo.text = Client.Self.Info.Username + "#" + Client.Self.Info.Discriminator;
		Client.Self.RequestListOfFollows();
		frr = msg => OnFollowResponse(msg);
		Client.Self.FollowResponseReceived += frr;
	}

	private void OnDestroy() => Client.Self.FollowResponseReceived -= frr;


	private void OnFollowResponse(ResponseMsg_FollowAddRemove msg) {
		Debug.Log("OnFollowResponse: " + msg.Status);
		if (msg.Follow != null) {
			GameObject go = Instantiate(FollowerPrefab, followListContent);
			//the name
			go.GetComponentInChildren<TextMeshProUGUI>().SetText(string.Format("{0}#{1}", msg.Follow.Username, msg.Follow.Discriminator));
			//the online status
			go.GetComponentInChildren<Image>().color = msg.Follow.Status == MessageEnums.Status.LoggedIn ? Color.green : Color.grey;
			//the delete button 
			go.GetComponentInChildren<Button>().onClick.AddListener(delegate {
				Debug.Log("Unfollow: " + msg.Follow.Username + "#" + msg.Follow.Discriminator);
				Client.Self.RequestAddRemoveFollow(true, msg.Follow.Username + "#" + msg.Follow.Discriminator);
				Destroy(go);
			});
		}
	}

	public void OnClickAddFollow() {
		if (!Utilities.IsUsernameAndDiscriminator(input.text) && !Utilities.IsEmail(input.text)) {
			Debug.Log("Error");
			//TODO: show error
			return;
		}
		Client.Self.RequestAddRemoveFollow(false, input.text);
	}
}
