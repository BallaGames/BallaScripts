using Balla.Core;
using UnityEngine;

namespace Balla.Equipment
{
    public class PlayerEquipment : EquipmentHolder
    {
        public BaseWeapon CurrentWeapon => weapons[weaponIndex];
        public int weaponIndex;
        public BaseWeapon[] weapons;
        public RecoilData recoilData;
        Vector3 addPos, addRot;
        protected override void Initialise()
        {
            base.Initialise();
            Input.SubscribeToActionPerform(Input.actions.Player.Next, NextWeapon);
            Input.SubscribeToActionPerform(Input.actions.Player.Previous, PreviousWeapon);
            weapons ??= new BaseWeapon[4];
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    if (i == 0)
                        weapons[i].OnSelect(null);
                    else
                        weapons[i].OnDeselect(null);
                }
            }
            recoilReturnTime = 1;
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
            if(CurrentWeapon is RangedWeapon rw)
            {
                recoilData = rw.recoilData;
            }
        }
        protected override void OnFrame()
        {
            CalculateRecoil();
        }
        protected override void Timestep()
        {
            base.Timestep();
            if(CurrentWeapon != null)
            {
                CurrentWeapon.SetAttackInput(Input.Attack);
                CurrentWeapon.SetAltAttackInput(Input.AltAttack);
            }
        }
        public override void ReceiveRecoil()
        {
            recoilIntensity += recoilData.intensityClimb;
            linRecoilTarg += MathUtils.Vec3Random(recoilData.linearForceMin, recoilData.linearForceMax) * recoilData.linearIntensity.Evaluate(recoilIntensity);
            angRecoilTarg += MathUtils.Vec3Random(recoilData.angularForceMin, recoilData.angularForceMax) * recoilData.angularIntensity.Evaluate(recoilIntensity);
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
                    recoilIntensity = Mathf.Clamp01(recoilIntensity - (recoilData.intensityDecay * (1 + (recoilIntensity * recoilData.intensityDecayMult))));
                }
                else
                {
                    recoilWaitTime += Time.smoothDeltaTime;
                }
                linearRecoilCurr = Vector3.Lerp(linearRecoilCurr, linRecoilTarg, recoilData.linearSharp * Time.smoothDeltaTime);
                if (recoilData.addPosition)
                {
                    addPos = new Vector3()
                    {
                        x = recoilData.addXPos.Evaluate(recoilReturnTime),
                        y = recoilData.addYPos.Evaluate(recoilReturnTime),
                        z = recoilData.addZPos.Evaluate(recoilReturnTime),
                    };
                    addPos = Vector3.zero;
                }
                angularRecoilCurr = Vector3.Lerp(angularRecoilCurr, angRecoilTarg, recoilData.angularSharp * Time.smoothDeltaTime);
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
                    recoilTransform.SetLocalPositionAndRotation(linearRecoilCurr + CurrentWeapon.idleOffset + addPos, Quaternion.Euler(angularRecoilCurr + addRot) * CurrentWeapon.idleRotation);
            }

        }
    }
}
