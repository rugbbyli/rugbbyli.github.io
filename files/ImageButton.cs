//using UnityEngine;
//using System.Collections;

////----------------------------------------------
////            NGUI: Next-Gen UI kit
//// Copyright © 2011-2014 Tasharen Entertainment
////----------------------------------------------

//using UnityEngine;

///// <summary>
///// Sample script showing how easy it is to implement a standard button that swaps sprites.
///// </summary>
//[ExecuteInEditMode]
//[AddComponentMenu("UI/ImageButton")]
//public class ImageButton : UI2DSprite
//{
//    //缓存按钮4种状态下的Sprite
//    public Sprite NormalSprite;
//    public Sprite HoverSprite;
//    public Sprite PressedSprite;
//    public Sprite DisabledSprite;

//    /// <summary>
//    /// 获取或设置按钮的可用性。注：按钮必须包含一个Collider才能触发输入事件。
//    /// </summary>
//    public bool isEnabled
//    {
//        get
//        {
//            var col = collider;
//            //return col == null ? (target.collider2D && target.collider2D.enabled) : (col && col.enabled);
//            return col && col.enabled;
//        }
//        set
//        {
//            var col = collider;
//            if (!col) return;

//            if (col.enabled != value)
//            {
//                col.enabled = value;
//                UpdateImage();
//            }
//        }
//    }

//    void OnEnable()
//    {
//        UpdateImage();
//    }

//    void OnValidate()
//    {
//        if (NormalSprite == null) NormalSprite = sprite2D;
//        if (HoverSprite == null) HoverSprite = sprite2D;
//        if (PressedSprite == null) PressedSprite = sprite2D;
//        if (DisabledSprite == null) DisabledSprite = sprite2D;
//    }

//    void UpdateImage()
//    {
//        if (isEnabled) SetSprite(UICamera.IsHighlighted(gameObject) ? HoverSprite : NormalSprite);
//        else SetSprite(DisabledSprite);
//    }

//    void OnHover(bool isOver)
//    {
//        if (isEnabled)
//            SetSprite(isOver ? HoverSprite : NormalSprite);
//    }

//    void OnPress(bool pressed)
//    {
//        if (pressed) SetSprite(PressedSprite);
//        else UpdateImage();
//    }

//    void SetSprite(Sprite sprite)
//    {
//        sprite2D = sprite;
//        //this.SetDirty();
//    }
//}

