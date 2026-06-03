using Balla.Core;
using Balla.Gameplay.Player;
using Balla.UI;
using System;
using System.Transactions;
using UnityEngine;

namespace Balla.Equipment
{
    public class PlayerEquipment : EquipmentHolder
    {
        public BaseWeapon CurrentWeapon => weapons.Length == 0 ? null : weapons[weaponIndex];
        public int weaponIndex;
        public BaseWeapon[] weapons;
        public PlayerController pc;


        protected Vector3 motionAddPos, motionAddPosTarget, motionAddRot, motionAddRotTarget;
        protected float lookMotionX, lookMotionY;
        protected float swayAngleX, swayAngleY;
        protected Vector3 swayTargetPos, swayTargetRot, swayFinalPos, swayFinalRot;

        public SwayConfig swayConfig;

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
        public Action OnReceivedFire;

        public Action OnFireStart, OnFireEnd, OnReloadStart, OnReloadEnd;


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
            if (CurrentWeapon != null && CurrentWeapon.UseAmmo && CurrentWeapon.AmmoRatio != 1 && !CurrentWeapon.IsReloading)
            {
                Debug.Log("Started reload");
                CurrentWeapon.StartReload();
                OnReloadStart?.Invoke();
            }
        }

        void OnWeaponFireStart()
        {
            OnFireStart?.Invoke();
        }
        void OnWeaponFireEnd()
        {
            OnFireEnd?.Invoke();
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
            if(CurrentWeapon != null)
            {
                if(oldWeapon != null)
                {
                    oldWeapon.OnDeselect(CurrentWeapon);
                    oldWeapon.OnFireStart -= OnFireStart;
                    oldWeapon.OnFireEnd -= OnFireEnd;
                    oldWeapon.OnReloadEnd -= OnReloadEnd;
                    oldWeapon.OnReloadStart -= OnReloadStart;
                    CurrentWeapon.OnSelect(oldWeapon);
                    oldWeapon.OnUnequip();
                }
                CurrentWeapon.OnEquip();
                CurrentWeapon.OnFireStart += OnFireStart;
                CurrentWeapon.OnFireEnd += OnFireEnd;
                CurrentWeapon.OnReloadStart += OnReloadStart;
                CurrentWeapon.OnReloadEnd += OnReloadEnd;
                
                PlayerUI.Instance.WeaponSwitched(CurrentWeapon);
                
                if(CurrentWeapon is RangedWeapon rw)
                {
                    recoilData = rw.recoilData;
                }
                weaponSwitched?.Invoke(oldWeapon, CurrentWeapon);
            }
            else
            {
                //If we have no valid weapon to switch to, then we will attempt to revert the weapon switch
                weaponIndex += next ? -1 : 1;
                weaponIndex %= weapons.Length;
                if(weaponIndex < 0)
                {
                    weaponIndex += weapons.Length;
                }
            }
        }
        protected override void OnFrame()
        {
            CalculateWeaponMotion();
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
            recoilIntensity += recoilData.intensityClimb;
            camIntensity += recoilData.camRecoilAdd;
            linRecoilTarg += MathUtils.Vec3Random(recoilData.linearForceMin, recoilData.linearForceMax).ScaleComponent(Vector3.Lerp(Vector3.one, recoilData.aimRecoilPosMult, Aim))
                * recoilData.linearIntensity.Evaluate(recoilIntensity);
            angRecoilTarg += MathUtils.Vec3Random(recoilData.angularForceMin, recoilData.angularForceMax).ScaleComponent(Vector3.Lerp(Vector3.one, recoilData.aimRecoilRotMult, Aim)) 
                * recoilData.angularIntensity.Evaluate(recoilIntensity);
            camPosTarg += MathUtils.Vec3Random(recoilData.minCamPosAdd, recoilData.maxCamPosAdd).ScaleComponent(Vector3.Lerp(Vector3.one, recoilData.aimRecoilPosMultCam, Aim))
                * recoilData.camPosIntensity.Evaluate(camIntensity);
            camRotTarg += MathUtils.Vec3Random(recoilData.minCamRotAdd, recoilData.maxCamRotAdd).ScaleComponent(Vector3.Lerp(Vector3.one, recoilData.aimRecoilRotMultCam, Aim)) 
                * recoilData.camRotIntensity.Evaluate(camIntensity);

            linRecoilMax = linRecoilTarg;
            angRecoilMax = angRecoilTarg;

            recoilWaitTime = 0;
            recoilReturnTime = 0;

            OnReceivedFire?.Invoke();

        }
        protected override void CalculateWeaponMotion()
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

                //Now we need to do some extra maths for weapon motion. May change in future.

                WeaponMotionCalc();

                addPos = swayFinalPos + motionAddPos;
                addRot = swayFinalRot + motionAddRot;

                    recoilTransform.SetLocalPositionAndRotation
                    (linearRecoilCurr + Vector3.Lerp(CurrentWeapon.idleOffset, CurrentWeapon.aimPosition, CurrentWeapon.aimCurve.Evaluate(aimAmount)) + addPos 
                    + (recoilTransform.parent.localPosition.ScaleComponent(recoilData.aimPositionNegate) * aimAmount), 
                        Quaternion.Euler(angularRecoilCurr + addRot) * 
                        Quaternion.Lerp(CurrentWeapon.idleRotation, 
                        Quaternion.Lerp(Quaternion.identity, Quaternion.Inverse(recoilTransform.parent.localRotation) * recoilData.aimRotation, recoilData.aimRotationNegate), 
                        Aim));
            }
        }
        void WeaponMotionCalc()
        {
            //Get the look motion info
            lookMotionX = pc.lookDelta.x;
            lookMotionY = pc.lookDelta.y;

            if (pc.moveState == MovementState.Walk || pc.moveState == MovementState.None)
            {
                 swayAngleY += Time.deltaTime * swayConfig.swaySpeed.y * Mathf.Lerp(1, swayConfig.swayMoveSpeedMult, Input.Move.sqrMagnitude);
                swayAngleX += Time.deltaTime * swayConfig.swaySpeed.x * Mathf.Lerp(1, swayConfig.swayMoveSpeedMult, Input.Move.sqrMagnitude);
            }
            swayTargetPos = new Vector3(Mathf.Sin(swayAngleX), 0, Mathf.Cos(swayAngleY));
            swayTargetRot = new Vector3(Mathf.Sin(swayAngleY), 0, Mathf.Cos(swayAngleX));

            swayFinalPos = Vector3.Lerp(swayFinalPos, swayTargetPos.ScaleComponent(Vector3.Lerp(swayConfig.swayPosScale, swayConfig.swayMovePosScale, Input.Move.sqrMagnitude)) * Mathf.Lerp(1, swayConfig.aimSwayMult, Aim), (1 / swayConfig.swayDamping) * Time.deltaTime);
            swayFinalRot = Vector3.Lerp(swayFinalRot, swayTargetRot.ScaleComponent(Vector3.Lerp(swayConfig.swayRotScale, swayConfig.swayMoveRotScale, Input.Move.sqrMagnitude)) * Mathf.Lerp(1, swayConfig.aimSwayMult, Aim), (1 / swayConfig.swayDamping) * Time.deltaTime);

            motionAddPosTarget = new Vector3
            {
                x = (Input.Move.x * swayConfig.movementPosScale.x) + (lookMotionX * swayConfig.lookMotionPosScale.x),
                y = Input.Move.y * swayConfig.movementPosScale.y, 
                z = (Input.Move.y * swayConfig.movementPosScale.z) + (lookMotionY * swayConfig.lookMotionPosScale.y),
            };
            motionAddRotTarget = new Vector3
            {
                x = (Input.Move.y * swayConfig.movementRotScale.x) + (lookMotionY * swayConfig.lookMotionRotScale.x),
                y = (Input.Move.x * swayConfig.movementRotScale.y) + (lookMotionX * swayConfig.lookMotionRotScale.y),
                z = (Input.Move.x * swayConfig.movementRotScale.z) + (lookMotionX * swayConfig.lookMotionRotScale.z),
            };

            motionAddPos = Vector3.Lerp(motionAddPos, motionAddPosTarget * Mathf.Lerp(1, swayConfig.aimMotionMult, Aim), (1 / swayConfig.motionPosDamping) * Time.deltaTime);
            motionAddRot = Vector3.Lerp(motionAddRot, motionAddRotTarget * Mathf.Lerp(1, swayConfig.aimMotionMult, Aim), (1 / swayConfig.motionRotDamping) * Time.deltaTime);
        }
    }
}
