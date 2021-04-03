using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class XOAssetBundleBuilder : EditorWindow
{

    private Sprite CrossSymbol;
    private Sprite CircleSymbol;
    private Sprite BackgroundGraphic;
    private string BundleName;

    [MenuItem("Window/XO Asset Bundle Builder")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(XOAssetBundleBuilder));
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        BundleName = EditorGUILayout.TextField("Asset Bundle Name", BundleName);

        EditorGUILayout.Space();

        CrossSymbol = (Sprite)EditorGUILayout.ObjectField("X Symbol", CrossSymbol, typeof(Sprite), false);
        CircleSymbol = (Sprite)EditorGUILayout.ObjectField("O Symbol", CircleSymbol, typeof(Sprite), false);
        BackgroundGraphic = (Sprite)EditorGUILayout.ObjectField("Background", BackgroundGraphic, typeof(Sprite), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Asset Bundle"))
        {
            BuildAssetBundle();
        }
    }

    private void BuildAssetBundle()
    {
        string assetBundleDirectory = Application.streamingAssetsPath;
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        string[] bundleAssets = new string[3];
        string[] addressableNames = new string[3];
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        addressableNames[0] = "Cross";
        bundleAssets[0] = AssetDatabase.GetAssetPath(CrossSymbol);
        addressableNames[1] = "Circle";
        bundleAssets[1] = AssetDatabase.GetAssetPath(CircleSymbol);
        addressableNames[2] = "Background";
        bundleAssets[2] = AssetDatabase.GetAssetPath(BackgroundGraphic);
        buildMap[0].assetBundleName = BundleName;
        buildMap[0].assetNames = bundleAssets;
        buildMap[0].addressableNames = addressableNames;

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}
