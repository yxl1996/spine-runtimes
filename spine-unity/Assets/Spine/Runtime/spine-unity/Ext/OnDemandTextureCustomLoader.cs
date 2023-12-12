#define SPINE_OPTIONAL_ON_DEMAND_LOADING

using System.Collections.Generic;
using UnityEngine;

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
namespace Spine.Unity
{
    public abstract class OnDemandTextureCustomLoader : ScriptableObject
    {
        public AtlasAssetBase atlasAsset;

        /// <param name="originalTextureName">Original texture name without extension.</param>
        /// <returns>The placeholder texture's name for a given original target texture name.</returns>
        public abstract string GetPlaceholderTextureName(string originalTextureName);

        /// <summary>
        /// Assigns previously setup placeholder textures at each Material of the associated AtlasAssetBase.</summary>
        /// <returns>True on success, false if the placeholder texture could not be assigned at any of the
        /// AtlasAssetBase's materials.</returns>
        public abstract bool AssignPlaceholderTextures(out IEnumerable<Material> modifiedMaterials);

        /// <summary>
        /// Returns whether any placeholder textures are assigned at the Material of the associated AtlasAssetBase.
        /// </summary>
        /// <param name="placeholderMaterials">A newly created list of materials which has a placeholder texture assigned.</param>
        /// <returns>True, if any placeholder texture is assigned at a Material of the associated AtlasAssetBase.</returns>
        public abstract bool HasPlaceholderTexturesAssigned(out List<Material> placeholderMaterials);

        /// <summary>
        /// Returns whether any main texture is null at a Material of the associated AtlasAssetBase.
        /// </summary>
        /// <param name="nullTextureMaterials">A newly created list of materials which has a null main texture assigned.</param>
        /// <returns>True, if any null main texture is assigned at a Material of the associated AtlasAssetBase.</returns>
        public virtual bool HasNullMainTexturesAssigned(out List<Material> nullTextureMaterials)
        {
            nullTextureMaterials = null;
            if (!atlasAsset) return false;

            bool anyNullTexture = false;
            foreach (Material material in atlasAsset.Materials)
            {
                if (material.mainTexture == null)
                {
                    anyNullTexture = true;
                    if (nullTextureMaterials == null) nullTextureMaterials = new List<Material>();
                    nullTextureMaterials.Add(material);
                }
            }

            return anyNullTexture;
        }

        /// <summary>
        /// Assigns previously setup target textures at each Material where placeholder textures are setup.</summary>
        /// <returns>True on success, false if the target texture could not be assigned at any of the
        /// AtlasAssetBase's materials.</returns>
        public abstract bool AssignTargetTextures(out IEnumerable<Material> modifiedMaterials);

        public abstract void BeginCustomTextureLoading();
        public abstract void EndCustomTextureLoading();
        public abstract bool HasPlaceholderAssigned(Material material);
        public abstract void RequestLoadMaterialTextures(Material material, ref Material overrideMaterial);

        public abstract void RequestLoadTexture(Texture placeholderTexture, ref Texture replacementTexture,
            System.Action<Texture> onTextureLoaded = null);

        public abstract void Clear(bool clearAtlasAsset = false);

        #region Event delegates

        public delegate void TextureLoadDelegate(OnDemandTextureCustomLoader loader, Material material,
            int textureIndex);

        protected event TextureLoadDelegate onTextureRequested;
        protected event TextureLoadDelegate onTextureLoaded;
        protected event TextureLoadDelegate onTextureUnloaded;

        public event TextureLoadDelegate TextureRequested
        {
            add { onTextureRequested += value; }
            remove { onTextureRequested -= value; }
        }

        public event TextureLoadDelegate TextureLoaded
        {
            add { onTextureLoaded += value; }
            remove { onTextureLoaded -= value; }
        }

        public event TextureLoadDelegate TextureUnloaded
        {
            add { onTextureUnloaded += value; }
            remove { onTextureUnloaded -= value; }
        }

        protected void OnTextureRequested(Material material, int textureIndex)
        {
            if (onTextureRequested != null)
                onTextureRequested(this, material, textureIndex);
        }

        protected void OnTextureLoaded(Material material, int textureIndex)
        {
            if (onTextureLoaded != null)
                onTextureLoaded(this, material, textureIndex);
        }

        protected void OnTextureUnloaded(Material material, int textureIndex)
        {
            if (onTextureUnloaded != null)
                onTextureUnloaded(this, material, textureIndex);
        }

        #endregion
    }
}
#endif