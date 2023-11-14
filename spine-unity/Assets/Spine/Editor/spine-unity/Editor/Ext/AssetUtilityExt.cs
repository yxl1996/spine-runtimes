using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Editor
{
    public class AssetUtilityExt
    {
        public static Shader GetDefaultShader(string assetFolderPath)
        {
            Shader shader = SpineEditorUtilities.Preferences.GetShader(assetFolderPath);

            if (shader == null)
                shader = AssetUtility.GetDefaultShader();

            return shader;
        }
    }

    public partial class SpinePreferences
    {
        public List<ShaderDir> MaterialShaderConfigs;

        public Shader GetShader(string path)
        {
            foreach (ShaderDir config in MaterialShaderConfigs)
            {
                foreach (string folderPath in config.FolderList)
                {
                    if (path.StartsWith(folderPath))
                    {
                        return config.Shader;
                    }
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public class ShaderDir
    {
        public List<string> FolderList;
        public Shader Shader;
    }
}