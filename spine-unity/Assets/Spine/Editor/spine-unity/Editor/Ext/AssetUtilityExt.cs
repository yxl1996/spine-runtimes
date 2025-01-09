using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{
    public class AssetUtilityExt
    {
        public static Material GetDefaultMaterial(string assetFolderPath)
        {
            Material templateMaterial = SpineEditorUtilities.Preferences.GetTemplateMaterial(assetFolderPath);
            Material material = null;

            if (templateMaterial != null)
            {
#if UNITY_2022_3_OR_NEWER
                material = new Material(templateMaterial.shader)
                {
                    parent = templateMaterial
                };
                material.RevertAllPropertyOverrides();
#else
                material = new Material(templateMaterial);
#endif
            }

            if (material == null)
            {
                Shader shader = AssetUtility.GetDefaultShader();
                if (shader != null)
                    material = new Material(shader);
            }

            return material;
        }

        public static void TrySetMainTexture(Material material, Texture texture)
        {
            var mainTex = material.mainTexture;
            if (mainTex != null)
            {
                if (mainTex.name.Replace("_placeholder", "").Equals(texture.name))
                {
                    return;
                }
            }

            material.mainTexture = texture;
        }

        public static void TrySetLoader(AtlasAssetBase atlasAsset,string folderPath)
        {
            if (atlasAsset.OnDemandTextureLoader != null)
                return;

            string[] loaderAssets = AssetDatabase.FindAssets("t:OnDemandTextureLoader", new[] {folderPath});
            foreach (string loaderAsset in loaderAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(loaderAsset);
                OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
                if (loader != null)
                {
                    atlasAsset.OnDemandTextureLoader = loader;
                    Debug.Log($"设置atlasAsset的TextureLoader:{loaderAsset}");
                    break;
                }
            }
        } 
    }

    public partial class SpinePreferences
    {
        public List<MaterialDir> MaterialShaderConfigs;

        public Material GetTemplateMaterial(string path)
        {
            foreach (var config in MaterialShaderConfigs)
            {
                foreach (string folderPath in config.FolderList)
                {
                    if (path.StartsWith(folderPath))
                    {
                        return config.TemplateMaterial;
                    }
                }
            }

            return null;
        }

#if UNITY_2022_3_OR_NEWER
        public void ApplyAllTemplateMaterials()
        {
            foreach (MaterialDir config in MaterialShaderConfigs)
            {
                foreach (string folder in config.FolderList)
                {
                    string[] guids = AssetDatabase.FindAssets("t:material", new[] {folder});
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                        mat.parent = config.TemplateMaterial;
                        var mainTex = mat.mainTexture;
                        mat.RevertAllPropertyOverrides();
                        mat.SetTexture("_MainTex", mainTex);
                    }
                }
            }
        }
#endif
    }

    [System.Serializable]
    public class MaterialDir
    {
        public List<string> FolderList;
        public Material TemplateMaterial;
    }
}