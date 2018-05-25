using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Vuforia;
using UnityEngine.UI;

/// <summary>
/// This MonoBehaviour implements the Cloud Reco Event handling for this sample.
/// It registers itself at the CloudRecoBehaviour and is notified of new search results.
/// </summary>
public class SimpleCloudHandler : MonoBehaviour, ICloudRecoEventHandler
{
	public ImageTargetBehaviour ImageTargetTemplate;
	private CloudRecoBehaviour mCloudRecoBehaviour;
	private bool mIsScanning = false;
	public string mTargetMetadata = "";
	public string loadedData;
	private string jsonString;
	public string editorUrl;
	// Use this for initialization
	void Start () {
		// register this event handler at the cloud reco behaviour
		mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();

		if (mCloudRecoBehaviour)
		{
			mCloudRecoBehaviour.RegisterEventHandler(this);
		}
	}

	public void OnInitialized() {
		Debug.Log ("Cloud Reco initialized");
	}
	public void OnInitError(TargetFinder.InitState initError) {
		Debug.Log ("Cloud Reco init error " + initError.ToString());
	}
	public void OnUpdateError(TargetFinder.UpdateState updateError) {
		Debug.Log ("Cloud Reco update error " + updateError.ToString());
	}

	public void OnStateChanged(bool scanning) {
		mIsScanning = scanning;
		if (scanning)
		{
			// clear all known trackables
			var tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
			tracker.TargetFinder.ClearTrackables(false);
		}
	}

	// Here we handle a cloud target recognition event
	public void  OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult) {

		if (ImageTargetTemplate) {
			// enable the new result with the same ImageTargetBehaviour:
			ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
			ImageTargetBehaviour imageTargetBehaviour =
				(ImageTargetBehaviour)tracker.TargetFinder.EnableTracking(
					targetSearchResult, ImageTargetTemplate.gameObject);
		}
		
		// do something with the target metadata
		mTargetMetadata = targetSearchResult.MetaData;
		/*string a = ReadMeta();
		Debug.Log(a);*/
		//Debug.Log ("Meta Data:" + mTargetMetadata);
		// stop the target finder (i.e. stop scanning the cloud)
		//ReadMeta();
		mCloudRecoBehaviour.CloudRecoEnabled = false;
	}

	public string ReadMeta()
	{
		string returnMeta = mTargetMetadata;
		//Debug.Log ("Meta Data:" + returnMeta);
		MetaDataFields loadedData = JsonUtility.FromJson<MetaDataFields>(returnMeta);
		//Debug.Log(loadedData.Editor);
		// Debug.Log(loadedData.Android);
		// Debug.Log(loadedData.iOS);
		// PLatform Specific runtime
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			editorUrl = loadedData.Editor;
			return loadedData.Editor;
		}

		else if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return loadedData.Android;
		}

		else //(Application.platform == RuntimePlatform.Android)
		{		
			return loadedData.iOS;
		}

		//return returnMeta;
		
	}

	[System.Serializable]
	public class MetaDataFields
	{
		public string Editor;
		public string Android;
		public string iOS;
	}
}
