using Balla.Core;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla
{
    /// <summary>
    /// Client-side object that just allows the Explosion Manager to move an explosion to the target point and trigger the effect.
    /// <br></br>
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        public VisualEffect[] effects;
        public void PlayEffects()
        {
            foreach (var item in effects)
            {
                item.Play();
            }
        }
    }
}
