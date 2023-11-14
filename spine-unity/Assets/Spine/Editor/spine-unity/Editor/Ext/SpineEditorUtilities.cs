using System.Collections.Generic;

namespace Spine.Unity.Editor
{
    public class SpineEditorUtilitiesExt
    {
        private static List<string> assetsCache = new List<string>();

        public static string[] DoAssetsFilter(string[] imported)
        {
            assetsCache.Clear();
            foreach (string asset in imported)
            {
                if (asset.Contains("Assets/AppearanceAssets"))
                    assetsCache.Add(asset);
            }

            return assetsCache.ToArray();
        }
    }
}