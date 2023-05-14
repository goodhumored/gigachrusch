using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FPS.Scripts.Game.Shared
{
    public class MeleeWeaponController : WeaponController
    {
        public float Range;
        public float meleeDamage;
        public AudioClip hitSfx;

        protected override void HandleUsage()
        {
            var ray = new Ray()
            {
                origin = WeaponRoot.transform.position,
                direction = transform.forward
            };  
            if (Physics.Raycast(ray, out var raycastHit, Range, LayerMask.GetMask("Default")))
            {
                var damageable = raycastHit.transform.GetComponent<Damageable>();
                if (damageable)
                {
                    meleeDamage *= 1 + CurrentCharge;
                    damageable.InflictDamage(meleeDamage, false, gameObject);
                }
                m_UsageAudioSource.PlayOneShot(hitSfx);
            }
            base.HandleUsage();
        }
    } 
}