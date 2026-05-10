using Balla.Core;
using Balla.Equipment;
using Balla.Gameplay.Player;
using System.Globalization;
using UnityEngine;

namespace Balla
{
    [ExecuteAlways]
    public class FirstPersonFOVController : BallaScript
    {
        protected PlayerController controller;
        protected PlayerEquipment equipment;
        [SerializeField, ReadOnly] protected float currFOV;
        [SerializeField, ReadOnly] protected float addFOV;
        /// <summary>
        /// Pulled from the player's settings.
        /// </summary>
        [SerializeField] protected float baseFOV;
        [SerializeField, ReadOnly] protected float targetViewFOV, targetWorldFOV;
        [SerializeField] protected float sprintFOVAdd = 5;
        [SerializeField] protected float slideFOVAdd = 10;
        [SerializeField] protected float aimFOVAdd = -20;
        [SerializeField] protected float fovLerpSpeed = 8;

        float currViewFOV;
        float addViewFOV;

        float fovZoomTime;
        float worldFovVelocity, viewFovVelocity;
        [SerializeField] protected float baseViewmodelFOV = 60;
        [SerializeField] protected float viewmodelAimFOVAdd = -10;

        [SerializeField] protected float scalingFactor = 0.8f;
        float lastScalingFactor;
        int fovKeyID;
        int scalingFactorKeyID;

        private void Start()
        {
            fovKeyID = Shader.PropertyToID("_FOV");
            scalingFactorKeyID = Shader.PropertyToID("_ScalingFactor");
        }

        private void Update()
        {
            if(Input != null)
            {
                switch (controller.moveState)
                {
                    case MovementState.Crouch:
                        break;
                    case MovementState.Sprint:
                        addFOV = sprintFOVAdd;
                        break;
                    case MovementState.Slide:
                        addFOV = slideFOVAdd;
                        break;
                    case MovementState.Air:
                        break;
                    case MovementState.Ladder:
                        addFOV = 0;
                        break;
                    case MovementState.Mantle:
                        addFOV = 0;
                        break;
                    case MovementState.Special:
                        break;
                    default:
                        addFOV = 0;
                        break;
                }

                if (equipment.CurrentWeapon != null)
                {
                    targetViewFOV = equipment.CurrentWeapon.aimViewFOV;
                    targetWorldFOV = equipment.CurrentWeapon.aimWorldFOV;
                }
                else
                {
                    targetWorldFOV = 0;
                    targetViewFOV = 0;
                }

                if (equipment.isUnarmed)
                {
                    addViewFOV = Input.AltAttack ? viewmodelAimFOVAdd : 0;
                }

                currFOV = Mathf.Lerp(currFOV, Mathf.Lerp(baseFOV + addFOV, targetWorldFOV, equipment.Aim), Time.deltaTime * fovLerpSpeed);
                currViewFOV = Mathf.Lerp(currViewFOV, Mathf.Lerp(baseViewmodelFOV + addViewFOV, targetViewFOV, equipment.Aim), Time.deltaTime * fovLerpSpeed);
                controller.cam.fieldOfView = currFOV;
            }
            else
            {
                currViewFOV = baseViewmodelFOV;
                currFOV = baseFOV;
            }
                Shader.SetGlobalFloat(fovKeyID, currViewFOV);
            
            if (lastScalingFactor != scalingFactor)
            {
                Shader.SetGlobalFloat(scalingFactorKeyID, scalingFactor);
                lastScalingFactor = scalingFactor;
            }
        }

        private void OnValidate()
        {
            if (controller == null)
                controller = GetComponent<PlayerController>();
            if (equipment == null)
                equipment = GetComponent<PlayerEquipment>();

            fovZoomTime = 1 / fovLerpSpeed;

        }
    }
}
