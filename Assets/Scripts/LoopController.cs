﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]

public class FriendType {
	public string name;
	public AudioClip[] clips;
	public int count;
}

public class LoopController : MonoBehaviour {
	public AudioSource environmentSound; //TODO
	public FriendType[] friendTypes;
	private List<List<AudioSource>> _audioSources = new List<List<AudioSource>> ();

	void Start() {
		foreach (FriendType friend in friendTypes) {
			var friendList = new List<AudioSource> ();
			foreach (AudioClip clip in friend.clips) {
				AudioSource audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.clip = clip;
				audioSource.volume = 0.0f;
				audioSource.loop = true;
				audioSource.Play();
				friendList.Add (audioSource);
			}
			_audioSources.Add (friendList);
		}
		EventManager.OnJoinBand += JoinBand;
		EventManager.OnLeaveBand += LeaveBand;
	}

	public void JoinBand(string name) {
		FriendType friend = friendTypes[FindFriend (name)];
		AddTrack (friend);
		Debug.Log (friend.name + " #" + friend.count + " joined the band.");
	}

	public void LeaveBand(string name) {
		FriendType friend = friendTypes[FindFriend (name)];
		Debug.Log (friend.name + " #" + friend.count + " left the band.");
		RemoveTrack (friend);
	}

	/** PRIVATE METHODS **/

	private void AddTrack(FriendType friend) {
		friend.count++;
		if (friend.count > friend.clips.Length) {
			return;
		}
		AudioSource track = _audioSources[FindFriend(friend.name)][friend.count - 1];
		StartCoroutine (FadeIn (track));
	}

	private IEnumerator FadeIn (AudioSource source) {
		while (source.volume < 1.0f) {
			source.volume += 0.1f;
			yield return new WaitForSeconds (0.25f);
		}
	}

	private IEnumerator FadeOut (AudioSource source) {
		source.volume = 0.0f;
		yield return new WaitForSeconds(5);
	}

	private int FindFriend(string name) {
		int count = 0;
		foreach (FriendType friend in friendTypes) {
			if (friend.name == name) {
				return count;
			}
			count++;
		}
		Debug.Log ("uh oh! you don't have any friends named " + name);
		return -1;
	}

	private void RemoveTrack(FriendType friend) {
		friend.count = (friend.count == 0) ? 0 : friend.count - 1; //Never go below zero
		if (friend.count >= friend.clips.Length) {
			return;
		}
		AudioSource track = _audioSources[FindFriend(friend.name)][friend.count]; //Not - 1 because friend.count is already decremented
		StartCoroutine (FadeOut (track));
	}
}