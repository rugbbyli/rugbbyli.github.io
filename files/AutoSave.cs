using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AutoSave
{
    static System.DateTime _lastSavedTime;
    static int saveDuration = 120;

    static AutoSave()
    {
        EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
        EditorApplication.update += Update;
        
    }

    static void SaveScene()
    {
        _lastSavedTime = System.DateTime.Now;

        if (!string.IsNullOrEmpty(EditorApplication.currentScene) && EditorApplication.SaveScene())
        {
            Debug.Log(string.Format("scene has auto saved (at {0}).", System.DateTime.Now.ToString("HH:mm:ss")));
        }
    }

    static void Update()
    {
        if (System.DateTime.Now.Subtract(_lastSavedTime).TotalSeconds > saveDuration)
        {
            SaveScene();
        }
    }

    static void OnHierarchyWindowChanged()
    {
        SaveScene();
    }
}