using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UPMod
{
    [ModifiesType]
    public class mod_PartyMemberAI : PartyMemberAI
    {
        [ModifiesMember("StatsOnHit")]
        public void mod_StatsOnHit(GameObject source, CombatEventArgs args)
        {
            // EDIT:
            // no modifications to this method. 
            // just making it public such that it can be invoked from Health.ApplyDamageDirectly()
            // p.s. technically patchwork contains [ModifiesAccessibility] modifier, but it doesn't seem to work for me
            // global::Console.AddMessage("mod_StatsOnHit accessed", Color.cyan);
            // EDIT END

            this.CheckStatComponent();
            if (this.m_StatTracker)
            {
                this.m_StatTracker.NotifyHit(source, args);
            }
        }
    }
}