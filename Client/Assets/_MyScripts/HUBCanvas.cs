using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUBCanvas : MonoBehaviour {
	[SerializeField] private GameObject FollowerPrefab;
	[SerializeField] private TextMeshProUGUI userInfo;
	[SerializeField] private TMP_InputField input;
	[SerializeField] private Transform followListContent;

	private struct Follow {
		public PublicInfo info;
		public GameObject goRef;
	}

	private Dictionary<string, Follow> followInfoDic;

	private void Start() {
		followInfoDic = new Dictionary<string, Follow>();
		this.userInfo.text = Client.Self.Info.Username + "#" + Client.Self.Info.Discriminator;
		Client.Self.RequestListOfFollows();
		Client.Self.FollowListResponseReceived += OnFollowListResponse;
		Client.Self.FollowResponseReceived += OnFollowResponse;
		Client.Self.FollowUpdateResponseReceived += FollowUpdate;
	}

	private void OnDestroy() => Client.Self.FollowResponseReceived -= OnFollowResponse;

	private void OnFollowResponse(ResponseMsg_FollowAddRemove msg) => AddToFollowList(msg.Follow);
	private void OnFollowListResponse(ResponseMsg_FollowList msg) {
		foreach (var follow in msg.Follows) {
			AddToFollowList(follow);
		}
		Client.Self.FollowListResponseReceived -= OnFollowListResponse;
	}

	private void FollowUpdate(ResponseMsg_FollowUpdate msg) {
		//TODO: update the UI
	}


	private void AddToFollowList(PublicInfo follow) {
		if (follow != null) {
			GameObject go = Instantiate(FollowerPrefab, followListContent);
			//the name
			go.GetComponentInChildren<TextMeshProUGUI>().SetText(string.Format("{0}#{1}", follow.Username, follow.Discriminator));
			//the online status
			go.GetComponentInChildren<Image>().color = follow.Status == MessageEnums.AccountStatus.Online ? Color.green : Color.grey;
			//the delete button 
			go.GetComponentInChildren<Button>().onClick.AddListener(delegate {
				Debug.Log("Unfollow: " + follow.Username + "#" + follow.Discriminator);
				Client.Self.RequestAddRemoveFollow(true, follow.Username + "#" + follow.Discriminator, Utilities.StringTpye.UsernameAndDiscriminator);
				//TODO: run and check it's working fine
				print(follow.ToString());
				followInfoDic.Remove(follow.Email);
				Destroy(go);
			});
			followInfoDic[follow.Email] = new Follow { info = follow, goRef = go };
		}
	}

	public void OnClickAddFollow() {
		switch (Utilities.TestString(input.text)) {
			case Utilities.StringTpye.Email:
				//TODO: show error
				if (existsAlready(input.text))
					Debug.Log("Error Already exists!");
				else if (CheckAddingSelf(input.text, i => i.Email))
					Debug.Log("Error Trying to add self!");
				else
					Client.Self.RequestAddRemoveFollow(false, input.text, Utilities.StringTpye.Email);
				return;
			case Utilities.StringTpye.UsernameAndDiscriminator:
				//TODO: show error
				if (existsAlready(input.text, i => i.Username + '#' + i.Discriminator))
					Debug.Log("Error Already exists!");
				else if (CheckAddingSelf(input.text, i => i.Username + '#' + i.Discriminator))
					Debug.Log("Error Trying to add self!");
				else
					Client.Self.RequestAddRemoveFollow(false, input.text, Utilities.StringTpye.UsernameAndDiscriminator);
				return;
			default:
			case Utilities.StringTpye.Username:
			case Utilities.StringTpye.Undefined:
				Debug.Log("Error wrong format!");
				//TODO: show error
				return;
		}
	}

	private bool existsAlready(string input, Func<PublicInfo, string> func) {
		foreach (var info in followInfoDic.Values)
			if (input == func(info.info))
				return true;
		return false;
	}

	private bool existsAlready(string email) => followInfoDic.ContainsKey(email);

	private bool CheckAddingSelf(string input, Func<PublicInfo, string> func) => input == func(Client.Self.Info);

}
