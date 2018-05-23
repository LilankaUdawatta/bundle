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
using System.Collections.Generic;
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
    public string url = "Load";
    private string url_loaded = "Unload";
    private bool loded;
    private WWW www;
    //private List<GameObject> _instance;
    private GameObject[] _instance;
    private GameObject clone;
    
    //load CloudRecoBehaviour <type> 
    //must include using vuforia
    public CloudRecoBehaviour CloudRecognitionGameObject;
    // load SimpleCloudHandler1 script from CloudRecoBehaviour attached
    private SimpleCloudHandler1 CloudHandlerScript;

    //Added

    protected virtual void Start()
    {
        // (re-)initialize as empty list
        //WWW www = new WWW();
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


        //download assetbundles
        if (String.Equals(url_loaded,url))
        {
            //StartCoroutine (LoadAsse());
            LoadAsse(); 
        }

        else
        {
            StartCoroutine (DownloadAsse());   
        }   
    }

//Added
    
    IEnumerator DownloadAsse()
    {   
        i = 0;
        // Frees the memory from the web stream
       // www.Dispose();
        // Store Previous loaded url
        //url_loaded = url;
        _instance = new GameObject[20];

        //Load "SimpleCloudHandler1" from "CloudRecoBehaviour"
        CloudHandlerScript = CloudRecognitionGameObject.GetComponent<SimpleCloudHandler1>();
        // access "ReadMeta()" function from "SimpleCloudHandler1" script
        url = CloudHandlerScript.ReadMeta();
        Debug.Log("Downloading from yes yes worked:"+ url);
        // Update url_loaded to prevent downloading assets again for the reco
        url_loaded = url;
        //download AssetBundle
        www = new WWW(url);
        // using (www = new WWW(url)) -- Or use this
        using (www)
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
                //Instantiate(asseBundle.LoadAsset(assetNames[i]));
                    //GameObject clone = (GameObject)Instantiate(asseBundle.LoadAsset(assetNames[i]));
                //_instance[i] = clone;
                //_instance.Add(clone);
                GameObject clone = new GameObject();
                clone = Instantiate(asseBundle.LoadAsset(assetNames[i])) as GameObject;
                _instance[i] = clone;
                //clone.transform.parent = mTrackableBehaviour.transform;

                //Destory(clone);
                //_instance.Add((GameObject)Instantiate(asseBundle.LoadAsset(assetNames[i])));    
                i++;
            }   
        } 
    }

   /* IEnumerator LoadAsse()
    {
        //load all assets
            i = 0;
            //_instance = new List<GameObject>();
            _instance = new GameObject[20];
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
                    //Instantiate(asseBundle.LoadAsset(assetNames[i]));
                    GameObject clone = new GameObject();
                    clone = (GameObject)Instantiate(asseBundle.LoadAsset(assetNames[i]));
                    _instance[i] = clone;
                    //_instance.Add(clone);
                    //_instance.Add((GameObject)Instantiate(asseBundle.LoadAsset(assetNames[i])));    
                    i++;
                }
                
    }*/

    private void LoadAsse()
    {
        EnableObjects();
    }


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

         //asseBundle.Unload(false);

       /* i=0;
        foreach (UnityEngine.Object eachAsset in _instance)
        {
            Destroy(_instance[i]);
            i++;
        }*/
        //disable from play.. on refound, enable 
        // on new, destory and proceed
        
       //DisableObjects();
    }

    private void EnableObjects()
    {
        i = 0;
        foreach (UnityEngine.Object eachAsset in _assets)
        {
           _instance[i].SetActive(true);
            i++;
        }
    }

    private void DisableObjects()
    {
        i = 0;
        foreach (UnityEngine.Object eachAsset in _assets)
        {
            _instance[i].SetActive(false);
            i++;
        }
    }

    #endregion // PROTECTED_METHODS
}
