#if DebugABS
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Debugger.Util
{
    internal static class DebuggerUtils 
    {
        internal static Texture CreateTexture(Color color)
        {
            var mask = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            mask.SetPixel(0, 0, color);
            mask.Apply();
            return mask;
        }
    }
}
#endif