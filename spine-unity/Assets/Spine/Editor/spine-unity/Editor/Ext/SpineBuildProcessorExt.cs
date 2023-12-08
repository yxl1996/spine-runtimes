using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{
    public class SpineBuildProcessorExt
    {
        static readonly List<string> textureLoadersToRestore = new List<string>();

        internal static void PreprocessOnDemandTextureLoaders()
        {
            BuildUtilities.IsInSkeletonAssetBuildPreProcessing = true;
            try
            {
                AssetDatabase.StartAssetEditing();
                textureLoadersToRestore.Clear();
                string[] loaderAssets = AssetDatabase.FindAssets("t:OnDemandTextureLoader");
                foreach (string loaderAsset in loaderAssets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(loaderAsset);
                    OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
                    bool isLoaderUsed = loader.atlasAsset && loader.atlasAsset.OnDemandTextureLoader == loader &&
                                        loader.atlasAsset.TextureLoadingMode == AtlasAssetBase.LoadingMode.OnDemand;
                    if (isLoaderUsed)
                    {
                        IEnumerable<Material> modifiedMaterials;
                        textureLoadersToRestore.Add(assetPath);
                        loader.AssignPlaceholderTextures(out modifiedMaterials);

#if HAS_SAVE_ASSET_IF_DIRTY
						foreach (Material material in modifiedMaterials) {
							AssetDatabase.SaveAssetIfDirty(material);
						}
#endif
                    }
                }

                EditorUtility.UnloadUnusedAssetsImmediate();
                AssetDatabase.StopAssetEditing();
#if !HAS_SAVE_ASSET_IF_DIRTY
                if (textureLoadersToRestore.Count > 0)
                    AssetDatabase.SaveAssets();
#endif
            }
            finally
            {
                BuildUtilities.IsInSkeletonAssetBuildPreProcessing = false;
            }
        }

        internal static void PostprocessOnDemandTextureLoaders()
        {
            BuildUtilities.IsInSkeletonAssetBuildPostProcessing = true;
            try
            {
                foreach (string assetPath in textureLoadersToRestore)
                {
                    OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
                    IEnumerable<Material> modifiedMaterials;
                    loader.AssignTargetTextures(out modifiedMaterials);
#if HAS_SAVE_ASSET_IF_DIRTY
					foreach (Material material in modifiedMaterials) {
						AssetDatabase.SaveAssetIfDirty(material);
					}
#endif
                }
#if !HAS_SAVE_ASSET_IF_DIRTY
                if (textureLoadersToRestore.Count > 0)
                    AssetDatabase.SaveAssets();
#endif
                textureLoadersToRestore.Clear();
            }
            finally
            {
                BuildUtilities.IsInSkeletonAssetBuildPostProcessing = false;
            }
        }
    }
}