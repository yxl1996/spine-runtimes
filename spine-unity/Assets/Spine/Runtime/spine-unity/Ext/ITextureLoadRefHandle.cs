using UnityEngine;

namespace Spine.Unity
{
    public interface ITextureLoadCustomHandle
    {
        void TryInit();
        void AddLoadedTextureRef(Texture placeholderTexture);
        void RemoveLoadedTextureRef(Texture targetTexture);
        void AddLoadedTextureRef(int materialIndex);
        void RemoveLoadedTextureRef(int materialIndex);
        void TryUnloadUnusedTextures(bool ignoreRequestFrame);
    }
}
