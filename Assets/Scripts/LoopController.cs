﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class FriendType {
	public EFriendType type;
	public AudioClip[] clips;
	public int count;
}

public class LoopController : MonoBehaviour {
//	public AudioMixerGroup audioGroup;
	public FriendType[] friendTypes;

    public float fadingPrecision = 0.05f;
    public float fadingDuration = 3f;


    private List<List<AudioSource>> _audioSources = new List<List<AudioSource>> ();

	void Start() {
		foreach (FriendType friend in friendTypes) {
			var friendList = new List<AudioSource> ();
			foreach (AudioClip clip in friend.clips) {
				AudioSource audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.clip = clip;
				audioSource.volume = 0.0f;
				audioSource.loop = true;
//				audioSource.outputAudioMixerGroup = audioGroup;
				audioSource.Play();
				friendList.Add (audioSource);
			}
			_audioSources.Add (friendList);
		}
		EventManager.OnJoinBand += JoinBand;
		EventManager.OnLeaveBand += LeaveBand;
	}

	public void JoinBand(Friend friend) {
		var friendIndex = FindFriend (friend.friendType);
		if (friendIndex == -1) {
			return;
		}
		FriendType friendType = friendTypes[friendIndex];
		AddTrack (friendType);
		Debug.Log (friendType.type + " #" + friendType.count + " joined the band.");
	}

	public void LeaveBand(Friend friend) {
		var friendIndex = FindFriend (friend.friendType);
		if (friendIndex == -1) {
			return;
		}
		FriendType friendType = friendTypes[friendIndex];
		Debug.Log (friendType.type + " #" + friendType.count + " left the band.");
		RemoveTrack (friendType);
	}

	/** PRIVATE METHODS **/

	private void AddTrack(FriendType friend) {
		friend.count++;
		if (friend.count > friend.clips.Length) {
			return;
		}
		AudioSource track = _audioSources[FindFriend(friend.type)][friend.count - 1];
		StartCoroutine (FadeIn (track));
	}

	private IEnumerator FadeIn (AudioSource source) {
        var volume = source.volume;
		while (volume < 1.0f) {
			volume += fadingPrecision;
            source.volume = Mathf.Clamp01(volume);
			yield return new WaitForSeconds (fadingDuration * fadingPrecision);
		}
	}

	private IEnumerator FadeOut (AudioSource source) {
		//FIXME: Could cause problems if called while FadeIn is running on same object
		//Maybe add time limit?
		var volume = source.volume;
		while (volume > 0.0f) {
			volume -= fadingPrecision;
			source.volume = Mathf.Clamp01(volume);
			yield return new WaitForSeconds (fadingDuration * fadingPrecision);
		}
	}

	private int FindFriend(EFriendType type) {
		int count = 0;
		foreach (FriendType friend in friendTypes) {
			if (friend.type == type) {
				return count;
			}
			count++;
		}
//		Debug.Log ("uh oh! you don't have any friends named " + name);
		return -1;
	}

	private void RemoveTrack(FriendType friend) {
		friend.count = (friend.count == 0) ? 0 : friend.count - 1; //Never go below zero
		if (friend.count >= friend.clips.Length) {
			return;
		}
		AudioSource track = _audioSources[FindFriend(friend.type)][friend.count]; //Not - 1 because friend.count is already decremented
		StartCoroutine (FadeOut (track));
	}
}
