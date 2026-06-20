using UnityEditor;
using UnityEngine;

public static class UrpMaterialConverter
{
    private const string AutoConvertPreferenceKey = "CrossyRoad.AutoConvertImportedMaterialsToUrp";

    [MenuItem("Tools/URP Materials/Convert Selected Materials To URP Lit")]
    public static void ConvertSelectedMaterials()
    {
        var converted = 0;
        var paths = Selection.assetGUIDs.Length > 0
            ? GetSelectedAssetPaths()
            : new[] { "Assets" };

        foreach (var path in paths)
        {
            converted += ConvertMaterialsInPath(path);
        }

        SaveAndShowResult(converted, "selected assets");
    }

    [MenuItem("Tools/URP Materials/Convert All Project Materials To URP Lit")]
    public static void ConvertAllProjectMaterials()
    {
        var converted = ConvertMaterialsInPath("Assets");
        SaveAndShowResult(converted, "project assets");
    }

    [MenuItem("Tools/URP Materials/Auto-Convert Imported Materials")]
    public static void ToggleAutoConvertImportedMaterials()
    {
        EditorPrefs.SetBool(AutoConvertPreferenceKey, !IsAutoConvertEnabled);
    }

    [MenuItem("Tools/URP Materials/Auto-Convert Imported Materials", true)]
    public static bool ToggleAutoConvertImportedMaterialsValidate()
    {
        Menu.SetChecked("Tools/URP Materials/Auto-Convert Imported Materials", IsAutoConvertEnabled);
        return true;
    }

    public static bool IsAutoConvertEnabled => EditorPrefs.GetBool(AutoConvertPreferenceKey, true);

    public static int ConvertImportedMaterials(string[] importedAssetPaths)
    {
        if (!IsAutoConvertEnabled || importedAssetPaths == null || importedAssetPaths.Length == 0)
        {
            return 0;
        }

        var converted = 0;
        foreach (var path in importedAssetPaths)
        {
            if (path.StartsWith("Assets/") && path.EndsWith(".mat"))
            {
                converted += ConvertMaterialAtPath(path);
            }
        }

        if (converted > 0)
        {
            AssetDatabase.SaveAssets();
        }

        return converted;
    }

    private static string[] GetSelectedAssetPaths()
    {
        var paths = new string[Selection.assetGUIDs.Length];
        for (var i = 0; i < Selection.assetGUIDs.Length; i++)
        {
            paths[i] = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
        }

        return paths;
    }

    private static int ConvertMaterialsInPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        if (path.EndsWith(".mat"))
        {
            return ConvertMaterialAtPath(path);
        }

        var materialGuids = AssetDatabase.IsValidFolder(path)
            ? AssetDatabase.FindAssets("t:Material", new[] { path })
            : AssetDatabase.FindAssets("t:Material", new[] { "Assets" });

        var converted = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in materialGuids)
            {
                converted += ConvertMaterialAtPath(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        return converted;
    }

    private static int ConvertMaterialAtPath(string path)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null || !ShouldConvert(material))
        {
            return 0;
        }

        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogWarning("Universal Render Pipeline/Lit shader was not found. Material conversion skipped.");
            return 0;
        }

        var mainTexture = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        var mainTextureScale = material.HasProperty("_MainTex") ? material.GetTextureScale("_MainTex") : Vector2.one;
        var mainTextureOffset = material.HasProperty("_MainTex") ? material.GetTextureOffset("_MainTex") : Vector2.zero;
        var color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
        var standardMode = material.HasProperty("_Mode") ? material.GetFloat("_Mode") : 0f;
        var cutoff = material.HasProperty("_Cutoff") ? material.GetFloat("_Cutoff") : 0.5f;
        var smoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.2f;

        material.shader = urpLit;

        if (mainTexture != null && material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", mainTexture);
            material.SetTextureScale("_BaseMap", mainTextureScale);
            material.SetTextureOffset("_BaseMap", mainTextureOffset);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", standardMode >= 2f ? 1f : 0f);
        }

        if (material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", Mathf.Approximately(standardMode, 1f) ? 1f : 0f);
        }

        if (material.HasProperty("_Cutoff"))
        {
            material.SetFloat("_Cutoff", cutoff);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        EditorUtility.SetDirty(material);
        Debug.Log($"Converted material to URP Lit: {path}", material);
        return 1;
    }

    private static bool ShouldConvert(Material material)
    {
        if (material.shader == null)
        {
            return false;
        }

        var shaderName = material.shader.name;
        if (shaderName.StartsWith("Universal Render Pipeline/"))
        {
            return false;
        }

        return shaderName == "Standard"
            || shaderName.StartsWith("Legacy Shaders/")
            || shaderName.StartsWith("Mobile/")
            || shaderName.StartsWith("Nature/")
            || shaderName.StartsWith("Particles/")
            || shaderName.StartsWith("Sprites/");
    }

    private static void SaveAndShowResult(int converted, string scope)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Material conversion complete",
            $"Converted {converted} {scope} material(s) to Universal Render Pipeline/Lit.",
            "OK");
    }
}

public class UrpMaterialAutoConverter : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!UrpMaterialConverter.IsAutoConvertEnabled || importedAssets == null || importedAssets.Length == 0)
        {
            return;
        }

        var importedAssetPaths = (string[])importedAssets.Clone();
        EditorApplication.delayCall += () =>
        {
            UrpMaterialConverter.ConvertImportedMaterials(importedAssetPaths);
        };
    }
}
