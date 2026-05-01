using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Balla
{
    public static class VFXUtilities
    {
        public static void Play(this VisualEffect[] visualEffects)
        {
            for (int i = 0; i < visualEffects.Length; i++)
            {
                if (visualEffects[i] != null)
                    visualEffects[i].Play();
            }
        }
    }
}
