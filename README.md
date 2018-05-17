# bundle


When building AssetBundle, you should put them in the StreamingAssets folder. Create the StreamingAssets folder in the Assets folder if you have not done this already. Inside that, create another folder called AssetBundle. This is just for the sake of organizing what's in the StreamingAssets folder.

The final path should be Assets/StreamingAssets/AssetBundle.

When loading it, Application.streamingAssetsPath should be used to access the StreamingAssets folder.

To access all the folders use, Application.streamingAssetsPath + "/AssetBundle/" + assetbunlenameWithoutExtension;

It is recommended to use Path.Combine to combine path names so the code below should use that instead.

Your Build script:

public class ExportAssetBundles
{
    [MenuItem("Assets/Build AssetBundle")]
    static void ExportResource()
    {
        string folderName = "AssetBundles";
        string filePath = Path.Combine(Application.streamingAssetsPath, folderName);

        BuildPipeline.BuildAssetBundles(filePath, BuildAssetBundleOptions.None, BuildTarget.iOS);
    }
}
Your load script:

IEnumerator loadAsset(string assetBundleName, string objectNameToLoad)
{
    string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
    filePath = System.IO.Path.Combine(filePath, assetBundleName);

    var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
    yield return assetBundleCreateRequest;

    AssetBundle asseBundle = assetBundleCreateRequest.assetBundle;

    AssetBundleRequest asset = asseBundle.LoadAssetAsync<GameObject>(objectNameToLoad);
    yield return asset;

    GameObject loadedAsset = asset.asset as GameObject;
    //Do something with the loaded loadedAsset  object
}
USAGE:

1.Use the first script (ExportAssetBundles) above to build your AssetBudle by going to Assets --> Build AssetBundle menu.

You should see the built AssetBundles inside the Assets/StreamingAssets/AssetBundles directory.

2. Let's make the following assumptions below:

A. Name of Assetbundle is animals.

B. Name of the Object we want to load from the animals Assetbundle is dog.

enter image description here

The loading is simple as this:

string nameOfAssetBundle = "animals";
string nameOfObjectToLoad = "dog";

StartCoroutine(loadAsset(nameOfAssetBundle, nameOfObjectToLoad));
