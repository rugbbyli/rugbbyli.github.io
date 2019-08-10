using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SpriteExporter : EditorWindow {

    [MenuItem("Assets/Export Sprite", false, 0)]
    static public void OpenSpriteExporter()
    {
        EditorWindow.GetWindow<SpriteExporter>(false, "Export Sprite", true);
    }

    List<Sprite> GetSelectedSprites()
    {
        List<Sprite> textures = new List<Sprite>();

        if (Selection.objects != null && Selection.objects.Length > 0)
        {
            Object[] objects = EditorUtility.CollectDependencies(Selection.objects);

            //textures.AddRange(from obj in objects where obj is Sprite select obj as Sprite);

            foreach (var tex in from obj in objects where obj is Texture2D select obj as Texture2D)
            {
                textures.AddRange(GetTextureSprites(tex));
            }
        }
        return textures;
    }

    void ExportSprites(IEnumerable<Sprite> sprites)
    {
        foreach (var sp in sprites)
        {
            int x = (int)sp.textureRect.x;
            int y = (int)sp.textureRect.y;
            int width = (int)sp.textureRect.width;
            int height = (int)sp.textureRect.height;
            var tex = new Texture2D(width, height, sp.texture.format, false);

            if (MakeTextureReadable(sp.texture))
            {
                Color[] colors = sp.texture.GetPixels(x, y, width, height);

                tex.SetPixels(colors);
                tex.Apply();

                var data = tex.EncodeToPNG();

                var pathbase = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                var path = string.Format("{0}/ExportedSprites/{1}", pathbase, sp.texture.name);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                var file = string.Format("{0}/{1}.png", path, sp.name);
                System.IO.File.WriteAllBytes(file, data);
                //using (var stream = System.IO.File.Create(file))
                //{
                //    stream.Write(data, 0, data.Length);
                //    stream.Flush();
                //}
            }
            else
            {
                Debug.Log("make texture readable false:" + sp.texture.name);
            }
        }
        Debug.Log("export " + sprites.Count() + " sprites.");
    }

    void OnGUI()
    {
        var list = GetSelectedSprites();

        if(GUILayout.Button("OK, Export thess Sprites as image file."))
        {
            ExportSprites(list);
        }

        var rect = GUILayoutUtility.GetLastRect();

        int i = 0, j = 0, w = 50, h = 50, margin = 5;
        foreach(var sp in list)
        {
            int x = (i++) * (w + margin) + margin;
            int y = j * (h + margin) + margin + (int)rect.height;
            if (x + w*2 >= rect.width)
            {
                j++;
                i = 0;
            }
            float rx = x, ry = y, rh = h, rw = w;
            if (sp.rect.width > sp.rect.height)
            {
                rh = w * sp.rect.height / sp.rect.width;
                ry = y + (h - rh) / 2;
            }
            else
            {
                rw = h * sp.rect.width / sp.rect.height;
                rx = x + (w - rw) / 2;
            }
            GUI.DrawTextureWithTexCoords(new Rect(rx,ry,rw,rh), sp.texture, uvRect(sp), true);
        }
    }

    public Rect uvRect(Sprite sp)
    {
        Texture tex = sp.texture;

        if (tex != null)
        {
            Rect rect = sp.textureRect;

            rect.xMin /= tex.width;
            rect.xMax /= tex.width;
            rect.yMin /= tex.height;
            rect.yMax /= tex.height;

            return rect;
        }
        return new Rect(0f, 0f, 1f, 1f);
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    bool MakeTextureReadable(Texture tex)
    {
        string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
        if (string.IsNullOrEmpty(path)) return false;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return false;

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        if (!settings.readable)
        {
            settings.readable = true;

            ti.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        return true;
    }

    IEnumerable<Sprite> GetTextureSprites(Texture2D tex)
    {
        var sprites = new List<Sprite>();
        
        string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
        if (string.IsNullOrEmpty(path)) return sprites;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return sprites;

        foreach (var dat in ti.spritesheet)
        {
            var sp = Sprite.Create(tex, dat.rect, dat.pivot, ti.spritePixelsToUnits);
            sp.name = dat.name;
            sprites.Add(sp);
        }

        return sprites;
    }
}
