using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
	[SerializeField] PhotonView playerPV;
	[SerializeField] TMP_Text text;


	void Start()
	{
		if(playerPV.IsMine)
		{
			gameObject.SetActive(false);
		}

		text.text = playerPV.Owner.NickName;
	}

}
