/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using UnityEngine;
using Vuforia;
// added
using System.Collections;
using System.IO;
using System;
using UnityEngine.Networking;
//added

/// <summary>
/// A custom handler that implements the ITrackableEventHandler interface.
/// 
/// Changes made to this file could be overwritten when upgrading the Vuforia version. 
/// When implementing custom event handler behavior, consider inheriting from this class instead.
/// </summary>
public class DefaultTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    #region PROTECTED_MEMBER_VARIABLES

    protected TrackableBehaviour mTrackableBehaviour;

    #endregion // PROTECTED_MEMBER_VARIABLES

    #region 
    
    //Added
    //Variable "assetNames" to load all asset names in assetbundles
	static string[] assetNames;
	//Variable to Load all assets in assetbundles to "_assets"
	static UnityEngine.Object[] _assets;
	//Number of assets in AssetBundle
	int i=0;
    // Variable "asseBundle" to load asset bundle
	private AssetBundle asseBundle;
	// Variable "asset" to load prefab
	private AssetBundleRequest asset;
	//Variable "loadedAsset" to load prefab as gameobject
	private GameObject loadedAsset;	
    // AssetBundle Url
    public string url = "https://firebasestorage.googleapis.com/v0/b/arapp-ba619.appspot.com/o/Editor%2F2_Game%2F2?alt=media&token=38c7179e-6bff-4dc2-aded-89001c842dbd";
    //Added

    protected virtual void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
    }

    protected virtual void OnDestroy()
    {
        if (mTrackableBehaviour)
            mTrackableBehaviour.UnregisterTrackableEventHandler(this);
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    ///     Implementation of the ITrackableEventHandler function called when the
    ///     tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
        TrackableBehaviour.Status previousStatus,
        TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
            OnTrackingFound();
        }
        else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
                 newStatus == TrackableBehaviour.Status.NO_POSE)
        {
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
            OnTrackingLost();
        }
        else
        {
            // For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
            // Vuforia is starting, but tracking has not been lost or found yet
            // Call OnTrackingLost() to hide the augmentations
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS

    #region PROTECTED_METHODS

    protected virtual void OnTrackingFound()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = true;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = true;

        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = true;

        StartCoroutine (DownloadAssetBundles());    
    }

//Added
    IEnumerator DownloadAssetBundles()
    {
		//download AssetBundle
        using (WWW www = new WWW(url))
        {
			//wait for download
            yield return www;

			Debug.Log ("Loaded ");
        	if (www.error != null)
            throw new Exception ("WWW download had an error: " + www.error);

			asseBundle = www.assetBundle;

			//load all assets
			_assets = www.assetBundle.LoadAllAssets();
			//load all asset names
			assetNames = www.assetBundle.GetAllAssetNames();
			
			foreach (UnityEngine.Object eachAsset in _assets)
			{
				// request prefab to load
				asset = asseBundle.LoadAssetAsync<GameObject>(assetNames[i]);
				yield return asset;
				//load prefab as gameobject
				loadedAsset = asset.asset as GameObject;
				//Instanciate Prefab
				Instantiate(asseBundle.LoadAsset(assetNames[i]));
				i++;
			}
        }
    }
    //Added

    protected virtual void OnTrackingLost()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Disable rendering:
        foreach (var component in rendererComponents)
            component.enabled = false;

        // Disable colliders:
        foreach (var component in colliderComponents)
            component.enabled = false;

        // Disable canvas':
        foreach (var component in canvasComponents)
            component.enabled = false;
    }

    #endregion // PROTECTED_METHODS
}
