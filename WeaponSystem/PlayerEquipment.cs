using Balla.Core;
using Balla.Gameplay.Player;
using Balla.UI;
using System;
using UnityEngine;

namespace Balla.Equipment
{
    public class PlayerEquipment : EquipmentHolder
    {
        public BaseWeapon CurrentWeapon => weapons[weaponIndex];
        public int weaponIndex;
        public BaseWeapon[] weapons;
        public PlayerController pc;

        public BaseUseable ability;
        public RecoilData recoilData;
        Vector3 addPos, addRot;
        public Transform camRecoilTarget;
        public Vector3 camPosScale, camRotScale;
        Vector3 camPos, camRot, camPosTarg, camRotTarg;
        float camIntensity;
        public Action<BaseWeapon, BaseWeapon> weaponSwitched;
        public bool isUnarmed;


        //Aim stuff
        [SerializeField] protected float aimAmount;
        /// <summary>
        /// a 0-1 value that tells the player how zoomed in they are when aiming
        /// </summary>
        public float Aim => aimAmount;


        public override (float crouch, float move, float aim, bool air) SpreadInfluence
        {
            get
            {
                //If we have no weapon, return 0, 0
                if (CurrentWeapon == null || pc == null)
                    return base.SpreadInfluence;
                return (pc.currentCrouch, Input.Move.magnitude, Aim, !pc.isGrounded || pc.moveState == MovementState.Slide);
            }
        }

        protected override void Initialise()
        {
            base.Initialise();
            Input.SubscribeToActionPerform(Input.actions.Player.Next, NextWeapon);
            Input.SubscribeToActionPerform(Input.actions.Player.Previous, PreviousWeapon);
            Input.SubscribeToActionPerform(Input.actions.Player.Reload, ReloadWeapon);
            weapons ??= new BaseWeapon[4];
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    if (i == 0)
                    {
                        weapons[i].OnSelect(null);
                        PlayerUI.Instance.WeaponSwitched(CurrentWeapon);
                    }
                    else
                        weapons[i].OnDeselect(null);
                }
            }
            recoilReturnTime = 1;

            if(pc == null && !TryGetComponent(out pc))
            {
                Debug.LogWarning("Could not fully initialise Player Equipment on this object - no Player Controller was found!");
            }
        }

        private void ReloadWeapon()
        {
            if (CurrentWeapon.UseAmmo && CurrentWeapon.AmmoRatio != 1 && !CurrentWeapon.IsReloading)
            {
                Debug.Log("Started reload");
                CurrentWeapon.StartReload();
            }
        }

        void PreviousWeapon()
        {
            SwitchWeapons(false);
        }
        void NextWeapon()
        {
            SwitchWeapons(true);
        }
        void SwitchWeapons(bool next)
        {
            if(weapons.Length <= 1)
            {
                //we have one or less weapons, so we shouldn't try to switch. back out early.
                return;
            }
            //Set the weapon's inputs to false
            var oldWeapon = CurrentWeapon;
            //Increment/Decrement the weapon index and then mod it to prevent it going out of bounds
            weaponIndex += next ? 1 : -1;
            weaponIndex %= weapons.Length;
            while(weaponIndex < 0)
            {
                weaponIndex += weapons.Length;
            }
            //Then call our select and deselect methods.
            oldWeapon.OnDeselect(CurrentWeapon);
            CurrentWeapon.OnSelect(oldWeapon);
            oldWeapon.OnUnequip();
            CurrentWeapon.OnEquip();
            PlayerUI.Instance.WeaponSwitched(CurrentWeapon);
            if(CurrentWeapon is RangedWeapon rw)
            {
                recoilData = rw.recoilData;
            }
            weaponSwitched?.Invoke(oldWeapon, CurrentWeapon);
        }
        protected override void OnFrame()
        {
            CalculateRecoil();
        }
        protected override void Timestep()
        {
            base.Timestep();
            if (!isUnarmed)
            {
                if (CurrentWeapon != null)
                {
                    CurrentWeapon.SetAttackInput(Input.Attack);
                    CurrentWeapon.SetAltAttackInput(Input.AltAttack);

                    if (CurrentWeapon.hasAim)
                    {
                        aimAmount = Mathf.MoveTowards(aimAmount, Input.AltAttack ? 1 : 0, CurrentWeapon.aimSpeed * Time.fixedDeltaTime);
                        CurrentWeapon.aimAmount = aimAmount;
                    }
                    else
                    {
                        aimAmount = 0;
                    }

                }
            }
        }
        public override void ReceiveRecoil()
        {
            Vector3 posMult = Vector3.Lerp(Vector3.one, recoilData.aimRecoilPosMult, Aim);
            Vector3 rotMult = Vector3.Lerp(Vector3.one, recoilData.aimRecoilRotMult, Aim);

            recoilIntensity += recoilData.intensityClimb;
            camIntensity += recoilData.camRecoilAdd;
            linRecoilTarg += MathUtils.Vec3Random(recoilData.linearForceMin, recoilData.linearForceMax).ScaleComponent(posMult) * recoilData.linearIntensity.Evaluate(recoilIntensity);
            angRecoilTarg += MathUtils.Vec3Random(recoilData.angularForceMin, recoilData.angularForceMax).ScaleComponent(rotMult) * recoilData.angularIntensity.Evaluate(recoilIntensity);
            camPosTarg += MathUtils.Vec3Random(recoilData.minCamPosAdd, recoilData.maxCamPosAdd).ScaleComponent(posMult) * recoilData.camPosIntensity.Evaluate(camIntensity);
            camRotTarg += MathUtils.Vec3Random(recoilData.minCamRotAdd, recoilData.maxCamRotAdd).ScaleComponent(rotMult) * recoilData.camRotIntensity.Evaluate(camIntensity);

            linRecoilMax = linRecoilTarg;
            angRecoilMax = angRecoilTarg;

            recoilWaitTime = 0;
            recoilReturnTime = 0;

        }
        protected override void CalculateRecoil()
        {
            if(recoilData == null)
            {
                if (CurrentWeapon == null)
                {
                    return;
                }
                else if(CurrentWeapon is RangedWeapon rw)
                {
                    recoilData = rw.recoilData;
                }
            }
            if(recoilData != null)
            {
                if(recoilWaitTime >= recoilData.recoilReturnTime && recoilReturnTime <= 1)
                {
                    recoilReturnTime = Mathf.Clamp01(recoilReturnTime + (Time.smoothDeltaTime * recoilData.recoilReturnSpeed));
                    linRecoilTarg = Vector3.LerpUnclamped(linRecoilMax, Vector3.zero, recoilData.linearReturnCurve.Evaluate(recoilReturnTime));
                    angRecoilTarg = Vector3.LerpUnclamped(angRecoilMax, Vector3.zero, recoilData.angularReturnCurve.Evaluate(recoilReturnTime));
                    recoilIntensity = Mathf.Clamp01(recoilIntensity - (recoilData.intensityDecay * (1 + (recoilIntensity * recoilData.intensityDecayMult))) * Time.smoothDeltaTime);

                    camIntensity = Mathf.Clamp01(camIntensity - (recoilData.camRecoilDecay * Time.smoothDeltaTime));
                }
                else
                {
                    recoilWaitTime += Time.smoothDeltaTime;
                }
                linearRecoilCurr = Vector3.Lerp(linearRecoilCurr, linRecoilTarg, recoilData.linearSharp * Time.smoothDeltaTime);
                angularRecoilCurr = Vector3.Lerp(angularRecoilCurr, angRecoilTarg, recoilData.angularSharp * Time.smoothDeltaTime);
                
                if(camRecoilTarget != null)
                {
                    if(camPosTarg != Vector3.zero)
                    {
                        camPosTarg = Vector3.Lerp(camPosTarg, Vector3.zero, recoilData.camPosDecay * Time.smoothDeltaTime);
                    }
                    if(camRotTarg != Vector3.zero)
                    {
                        camRotTarg = Vector3.Lerp(camRotTarg, Vector3.zero, recoilData.camRotDecay * Time.smoothDeltaTime);
                    }
                    camPos = Vector3.Lerp(camPos, camPosTarg, recoilData.camPosSharp * Time.smoothDeltaTime);
                    camRot = Vector3.Lerp(camRot, camRotTarg, recoilData.camRotSharp * Time.smoothDeltaTime);
                    
                    camRecoilTarget.SetLocalPositionAndRotation(camPos.ScaleComponent(camPosScale), Quaternion.Euler(camRot.ScaleComponent(camRotScale)));
                }
                
                if (recoilData.addPosition)
                {
                    addPos = new Vector3()
                    {
                        x = recoilData.addXPos.Evaluate(recoilReturnTime),
                        y = recoilData.addYPos.Evaluate(recoilReturnTime),
                        z = recoilData.addZPos.Evaluate(recoilReturnTime),
                    };
                }
                else
                {
                    addPos = Vector3.zero;
                }
                if (recoilData.addRotation)
                {
                    addRot = new Vector3()
                    {
                        x = recoilData.addXRot.Evaluate(recoilReturnTime),
                        y = recoilData.addYRot.Evaluate(recoilReturnTime),
                        z = recoilData.addZRot.Evaluate(recoilReturnTime),
                    };
                }
                else
                {
                    addRot = Vector3.zero;
                }
                    recoilTransform.SetLocalPositionAndRotation(linearRecoilCurr + Vector3.Lerp(CurrentWeapon.idleOffset, CurrentWeapon.aimPosition, CurrentWeapon.aimCurve.Evaluate(aimAmount)) + addPos, Quaternion.Euler(angularRecoilCurr + addRot) * CurrentWeapon.idleRotation);
            }

        }
    }
}
