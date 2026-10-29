#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;

public static class VisualExtensions
{
    public static void CreateItemIcon(this GameObject gameObject)
    {
        var texture = AssetPreview.GetAssetPreview(gameObject);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), Vector2.zero, 100);
        var i = 0;
        while(AssetPreview.IsLoadingAssetPreviews())
        {
            i++;
        }
        Debug.Log("" + i);

        var colorValue = 321;
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.width; y++)
            {
                var pixel = texture.GetPixel(x, y);
                if((int)(pixel.r * 1000) == colorValue && (int)(pixel.g * 1000) == colorValue && (int)(pixel.b * 1000) == colorValue)
                {
                    var newColor = pixel;
                    newColor.a = 0;
                    texture.SetPixel(x, y, newColor);
                }

            }

        }
        texture.Apply();
        //var dirPath = "Assets/Resources/Sprites/Items/";
        //var filePath = $"{dirPath}{item.ItemName}.asset";
        //if (!Directory.Exists(dirPath))
        //{
        //    Directory.CreateDirectory(dirPath);
        //}
        //AssetDatabase.CreateAsset(sprite, filePath);
        //var loadedTexture = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
        //item.Icon = loadedTexture;
        //Debug.Log("Loaded Texture Successfully: " + loadedTexture != null);
        var bytes = texture.EncodeToPNG();
        var dirPath = $"{Application.dataPath}/Player/Tools/";
        var filePath = $"{dirPath}{gameObject.name}.png";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Complete, wrote: {filePath}");

    }

    public static void SaveMesh(this SkinnedMeshRenderer renderer, string path = "Assets/Visuals/GeneratedMeshes/")
    {
        if (!path.StartsWith("Assets/"))
        {
            Debug.LogError("Mesh save path must start with 'Assets/'.");
            return;
        }

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        AssetDatabase.CreateAsset(renderer.sharedMesh, path + renderer.name + ".asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Mesh saved to: {path}");
    }
}

#endif