#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(ImageButton))]
#else
[CustomEditor(typeof(ImageButton), true)]
#endif
public class ImageButtonEditor : UIWidgetInspector
{
    ImageButton mButton;

    protected override void OnEnable()
    {
        base.OnEnable();
        mButton = target as ImageButton;
    }

    protected override bool ShouldDrawProperties()
    {
        SerializedProperty sp = NGUIEditorTools.DrawProperty("NormalSprite", serializedObject, "mSprite");
        NGUISettings.sprite2D = sp.objectReferenceValue as Sprite;
        mButton.sprite2D = sp.objectReferenceValue as Sprite;
        NGUIEditorTools.DrawProperty("HoverSprite", serializedObject, "HoverSprite");
        NGUIEditorTools.DrawProperty("PressedSprite", serializedObject, "PressedSprite");
        NGUIEditorTools.DrawProperty("DisabledSprite", serializedObject, "DisabledSprite");

        return (sp.objectReferenceValue != null);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (mButton.autoResizeBoxCollider && mButton.collider != null)
        {
            var col = mButton.collider as BoxCollider;
            if (col == null) return;
            col.size = new Vector3(mButton.width, mButton.height, 0);
        }
    }

    //public override void OnPreviewGUI(Rect rect, GUIStyle background)
    //{
    //    if (mButton != null && mButton.sprite2D != null)
    //    {
    //        Texture2D tex = mButton.mainTexture as Texture2D;
    //        if (tex != null) NGUIEditorTools.DrawTexture(tex, rect, mButton.uvRect, mButton.color);

    //        if (mButton.autoResizeBoxCollider && mButton.collider != null)
    //        {
    //            var col = mButton.collider as BoxCollider;
    //            if (col == null) return;
    //            col.size = new Vector3(mButton.width, mButton.height, 0);
    //        }
    //    }
    //}

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
    [MenuItem("UI/", false, 2)]
    static void Nothing() { }

    [MenuItem("UI/Image Button", false, 1)]
    static public void AddImageButton()
    {
        GameObject go = NGUIEditorTools.SelectedRoot(true);
        if (go != null) Selection.activeGameObject = AddImageButton(go).gameObject;
        else Debug.Log("You must select a game object first.");
    }
#endif

    static public ImageButton AddImageButton(GameObject go)
    {
        ImageButton w = NGUITools.AddWidget<ImageButton>(go);
        w.name = "Image Button";
        w.pivot = UIWidget.Pivot.Center;
        w.sprite2D = null;
        w.width = 100;
        w.height = 100;
        var col = w.gameObject.AddComponent<BoxCollider>();
        //col.center = sprite2D.bounds.center;
        col.size = new Vector3(w.width, w.height, 0);
        return w;
    }
}
#endif
