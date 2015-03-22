using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class FindScriptRef : EditorWindow
{
    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/Find Script Reference")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(FindScriptRef));
    }
    

    void OnGUI()
    {
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
        if (isFinding)
        {
            GUILayout.Label("finding...");
            return;
        }
        
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label(name);
		bool click = GUILayout.Button("Find");
		GUILayout.EndHorizontal();

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
    List<string> findResult;
	void Find(System.Type type){
		var tp = typeof(GameObject);
		var guids = AssetDatabase.FindAssets("t:GameObject");
		isFinding = true;
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
		}

        string sceneName = EditorApplication.currentScene;
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
                    findResult.Add(scene + ":" + obj.name);
                }
                
            }
        }

        EditorApplication.OpenScene(sceneName);
        isFinding = false;
		Debug.Log ("finish");
	}
}