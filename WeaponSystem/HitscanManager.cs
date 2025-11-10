using Balla.Core;
using UnityEngine;

namespace Balla.Gameplay
{
    /// <summary>
    /// Hitscan weapons will make a call to this class to request to fire. at the end of the physics update, all of these shots will be fired simultaneously.<br></br>
    /// This way, we can:
    /// * Batch data when sending
    /// * Multithread raycasts 
    /// * Eliminate situations where both players shoot at the same time, but only one actually logs their hit.
    /// </summary>
    public class HitscanManager : BallaScript
    {
        public static HitscanManager Instance { get; private set; }

        public LayerMask hitscanMask;
        public int hitsPerRay = 16;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                return;
            }
        }
    }
}
