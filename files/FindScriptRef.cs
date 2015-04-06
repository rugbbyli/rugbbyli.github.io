using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class FindScriptRef : EditorWindow
{
    [MenuItem("Assets/Find All Reference")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(FindScriptRef));
    }

    void OnGUI()
    {
        if (isFinding)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("finding...");

            GUILayout.HorizontalScrollbar(progress, 1, 0, 100);
            GUILayout.EndVertical();
            return;
        }

        if (Selection.activeObject == null)
        {
            GUILayout.Label("select a script file from Project Window.");
            return;
        }

        var name = Selection.activeObject.name;
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var dict = System.IO.Path.GetDirectoryName(assembly.Location);
        assembly = System.Reflection.Assembly.LoadFile(System.IO.Path.Combine(dict, "Assembly-CSharp.dll"));
        var selectType = assembly.GetType(name);
        if (string.IsNullOrEmpty(name) || selectType == null)
        {
            GUILayout.Label("select a script file from Project Window.");
            return;
        }
        
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label(name);
		bool click = GUILayout.Button("Find");
		GUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (findResult != null && findResult.Count > 0)
        {
            GUILayout.BeginScrollView(Vector2.zero, GUIStyle.none);
            foreach (string path in findResult)
            {
                GUILayout.Label(path);
            }
            GUILayout.EndScrollView();
        }

        if (click)
        {
            Find(selectType);
        }
        GUILayout.EndVertical();
    }

	bool isFinding;
    float progress;
    List<string> findResult;
	void Find(System.Type type){
		var tp = typeof(GameObject);
        var guids = AssetDatabase.FindAssets("t:GameObject");
		isFinding = true;
        progress = 0;
        findResult = new List<string>();
		foreach (var guid in guids)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			var obj = AssetDatabase.LoadAssetAtPath(path, tp) as GameObject;
			if (obj != null)
			{
				var cmp = obj.GetComponent(type);
                if (cmp == null)
                {
                    cmp = obj.GetComponentInChildren(type);
                }
				if (cmp != null)
				{
                    findResult.Add(path);
				}
			}
            progress += 0.5f / guids.Length;
		}

        string curScene = EditorApplication.currentScene;
        EditorApplication.SaveScene();

        string[] scenes = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        foreach (var scene in scenes)
        {
            EditorApplication.OpenScene(scene);

            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                var cmp = obj.GetComponent(type);
                if (cmp == null)
                {
                    cmp = obj.GetComponentInChildren(type);
                }
                if (cmp != null)
                {
                    findResult.Add(scene.Substring(Application.dataPath.Length) + "Assets:" + obj.name);
                }
            }
            progress += 0.5f / scenes.Length;
        }

        EditorApplication.OpenScene(curScene);
        isFinding = false;
		Debug.Log ("finish");
	}
}

class EditorExt
{
    public void Invoke(this EditorApplication app, EditorApplication.CallbackFunction func)
    {
        EditorApplication.delayCall += () =>
        {
            func();
        };
    }
}