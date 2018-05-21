using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.Networking;

public class asdfgh : MonoBehaviour {

	// string nameOfAssetBundle = "2";
	// string nameOfObjectToLoad = "Cube";

	//Variable "assetNames" to load all asset names in assetbundles
	static string[] assetNames;
	//Variable to Load all assets in assetbundles to "_assets"
	static UnityEngine.Object[] _assets;
	//Number of assets in AssetBundle
	int i=0;

	// // Use this for initialization
	// void Start () {
	
	// 	StartCoroutine (load (soundAsset));
	// }


	public string url = "https://firebasestorage.googleapis.com/v0/b/arapp-ba619.appspot.com/o/Editor%2F2_Game%2F2?alt=media&token=38c7179e-6bff-4dc2-aded-89001c842dbd";
    IEnumerator Start()
    {
		//download AssetBundle
        using (WWW www = new WWW(url))
        {
			//wait for download
            yield return www;

			Debug.Log ("Loaded ");
        	if (www.error != null)
            throw new Exception ("WWW download had an error: " + www.error);

			AssetBundle asseBundle = www.assetBundle;

			//load all assets
			_assets = www.assetBundle.LoadAllAssets();
			//load all asset names
			assetNames = www.assetBundle.GetAllAssetNames();
			
			foreach (UnityEngine.Object eachAsset in _assets)
			{
				// request prefab to load
				AssetBundleRequest asset = asseBundle.LoadAssetAsync<GameObject>(assetNames[i]);
				yield return asset;
				//load prefab as gameobject
				GameObject loadedAsset = asset.asset as GameObject;
				//Instanciate Prefab
				Instantiate(asseBundle.LoadAsset(assetNames[i]));
				i++;
			}
        }
    }

	// Update is called once per frame
	void Update () {
		
	}

}