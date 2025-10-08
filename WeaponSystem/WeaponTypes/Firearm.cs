using Unity.Netcode;
using UnityEngine;

namespace Balla.Equipment
{
    public class Firearm : BaseWeapon
    {
        [SerializeField] protected int roundsPerMinute;
        [SerializeField, ReadOnly] protected float timeBetweenRounds;
        float fireTimer;

        protected override void Timestep()
        {
            CycleLogic();
        }
        protected virtual void CycleLogic()
        {
            if(fireTimer <= 0)
            {
                fireTimer %= timeBetweenRounds;
            }
            fireTimer -= Delta * timeBetweenRounds;
        }
        protected virtual void PreFire(Vector3 pos, Vector3 dir)
        {
            FireOnServer_RPC(pos, dir, NetworkManager.LocalTime.Time);
            Debug.Log($"Local:{NetworkManager.LocalTime.Time} Server:{NetworkManager.ServerTime.Time}");
        }
        protected virtual void Fire()
        {

        }
        protected virtual void PostFire()
        {

        }
        protected void FireOnServer_RPC(Vector3 pos, Vector3 dir, double clientTime, RpcParams rpcParams = default)
        {
            
        }
    }
}
