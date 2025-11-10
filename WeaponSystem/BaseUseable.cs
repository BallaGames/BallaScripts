using Balla.Equipment;
using System;
using UnityEngine;

namespace Balla.Equipment
{
    public abstract class BaseUseable : BaseEquippable
    {
        /// <summary>
        /// First arg is the old weapon, second arg is the new weapon
        /// </summary>
        public Action<BaseUseable, BaseUseable> selectedUseable;

        public Renderer[] renderers;
        public void Show(bool show)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = show;
            }
        }
        

        protected override void Init()
        {
            for (int i = 0;i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].gameObject.layer = 8;
                }
            }
        }

        /// <summary>
        /// Called when switching TO this weapon
        /// </summary>
        /// <param name="previous"></param>
        public virtual void OnSelect(BaseUseable previous)
        {
            selectedUseable?.Invoke(previous, this);
        }
        /// <summary>
        /// Called when switching to something else
        /// </summary>
        /// <param name="next"></param>
        public virtual void OnDeselect(BaseUseable next)
        {
            selectedUseable?.Invoke(this, next);
        }
    }
}
