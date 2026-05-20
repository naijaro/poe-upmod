using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

namespace UPMod
{
    [ModifiesType]
    public class mod_Health : Health
    {
        [ModifiesMember("ApplyDamageDirectly")]
        public void mod_ApplyDamageDirectly(float amount, DamagePacket.DamageType damageType, GameObject source, StatusEffect sourceEffect)
        {
            if (this.Dead || this.Unconscious || !this.CanBeTargeted)
            {
                return;
            }
            Health health = null;
            if (amount < 0f)
            {
                amount = -amount;
                Debug.LogWarning("Script Error: DoDamage(float) was passed a negative value. Damage should be positive.", base.gameObject);
            }
            if (source != null)
            {
                health = source.GetComponent<Health>();
                CharacterStats component = source.GetComponent<CharacterStats>();
                if (component != null)
                {
                    // ORIGINAL:
                    // amount *= component.StatDamageHealMultiplier;
                    // amount *= component.GetDifficultyDamageMultiplier(this);

                    // PATCH START:
                    // RELATED BUGS: #2, #3
                    float dmgCoef = component.StatDamageHealMultiplier;

                    if (sourceEffect != null && sourceEffect.IsDOT)
                    {
                        CharacterStats attackersStats = source.GetComponent<CharacterStats>();
                        CharacterStats attackedCharacterStats = this.gameObject.GetComponent<CharacterStats>();

                        float elementalDamageCoef = System.Math.Max(1, attackersStats.GetBonusDamagePerType(damageType));
                        float raceSlayerDamageCoef = System.Math.Max(1, attackersStats.GetBonusDamagePerRaceMultiplier(attackedCharacterStats.CharacterRace));

                        float spiritSlayerDamageCoef = System.Math.Max(1, attackersStats.GetBonusDamagePerRaceMultiplier(CharacterStats.Race.Spirit));
                        float vesselSlayerDamageCoef = System.Math.Max(1, attackersStats.GetBonusDamagePerRaceMultiplier(CharacterStats.Race.Vessel));

                        // non raw dots should be affected by elemental talents
                        if (damageType != DamagePacket.DamageType.Raw)
                        {
                            dmgCoef += elementalDamageCoef - 1;
                        }

                        // non fixed dots should be affected by racial slayer talents
                        string originName = sourceEffect.Origin?.name ?? string.Empty;
                        bool isWounding =
                            sourceEffect.AbilityType.ToString().Equals("WeaponOrShield", System.StringComparison.OrdinalIgnoreCase);
                        bool isWoundingShot =
                            originName.IndexOf("woundingshot", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            originName.IndexOf("wounding_shot", System.StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isEnduringFlames =
                            originName.IndexOf("flamesofdevotion", System.StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isRecallAgony =
                            originName.IndexOf("recallagony", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            originName.IndexOf("recall_agony", System.StringComparison.OrdinalIgnoreCase) >= 0; ;
                        bool isFixedDoT = isWounding || isWoundingShot || isEnduringFlames;

                        // there are several 'fixed' dots whose damage is a percentage of weapon damage, and thus should not be affected (because these talents have already increased hit damage):
                        // wounding (from tidefall, drawn-in-spring and persistence) - is of [WeaponOrShield] type
                        // wounding shot - is of [Ability] type
                        // enduring flames - is of [Talent] type

                        if (!isFixedDoT && !isRecallAgony)
                        {
                            dmgCoef += raceSlayerDamageCoef - 1;
                        }
                    }

                    amount *= dmgCoef;
                    amount *= component.GetDifficultyDamageMultiplier(this);
                    // PATCH END

                }
            }
            if (this.m_stats != null)
            {
                amount = this.m_stats.AdjustDamageByDTDR(amount, damageType, null, source, 0.25f);
            }

            float num = 100f;
            if (this.m_stats)
            {
                num = this.m_stats.Stamina;
            }
            if (this.m_currentStamina >= num)
            {
                this.m_canRegenTimer = CharacterStats.StaminaRechargeDelay;
            }
            float currentStamina = this.m_currentStamina;
            float currentHealth = this.m_currentHealth;
            if (this.TakesDamage && !Health.NoDamage)
            {
                if (num > 0f)
                {
                    this.m_currentStamina -= amount;
                    if (!this.m_isAnimalCompanion)
                    {
                        this.m_currentHealth -= amount;
                    }
                }
                else if (!this.m_isAnimalCompanion)
                {
                    this.m_currentHealth -= amount;
                }
                this.TryPlayInjuredSound(currentStamina, currentHealth);
            }
            if (this.Unconscious && !this.Dead)
            {
                this.AddInjury(damageType);
                this.HandleUnconscious(source);
            }
            if (this.ShowDead)
            {
                if (this.CanDie)
                {
                    this.m_currentHealth = 0f;
                    this.HandleDeath(source);
                    if (health != null && health.OnKill != null)
                    {
                        GameEventArgs gameEventArgs = new GameEventArgs();
                        gameEventArgs.Type = GameEventType.Killed;
                        gameEventArgs.GameObjectData = new GameObject[1];
                        gameEventArgs.GameObjectData[0] = base.gameObject;
                        health.OnKill(source, gameEventArgs);
                    }
                }
                else
                {
                    this.m_currentHealth = 1f;
                }
            }
            GameEventArgs gameEventArgs2 = new GameEventArgs();
            gameEventArgs2.Type = GameEventType.Damaged;
            gameEventArgs2.FloatData = new float[2];
            gameEventArgs2.FloatData[0] = amount;
            gameEventArgs2.FloatData[1] = currentStamina;
            gameEventArgs2.GameObjectData = new GameObject[1];
            gameEventArgs2.GameObjectData[0] = source;
            gameEventArgs2.GenericData = new object[2];
            gameEventArgs2.GenericData[0] = new DamageInfo(base.gameObject, amount, null)
            {
                DamageType = damageType
            };
            gameEventArgs2.GenericData[1] = sourceEffect;
            if (this.OnDamaged != null)
            {
                this.OnDamaged(base.gameObject, gameEventArgs2);
            }
            if (health != null && health.OnDamageDealt != null)
            {
                health.OnDamageDealt(base.gameObject, gameEventArgs2);
            }
            UIHealthstringManager.Instance.ShowNumber(amount, base.gameObject);
            if (sourceEffect == null || sourceEffect.Params.IsHostile)
            {
                ScriptEvent component2 = base.GetComponent<ScriptEvent>();
                if (component2)
                {
                    component2.ExecuteScript(ScriptEvent.ScriptEvents.OnAttacked);
                    if (this.m_attackedByPlayer)
                    {
                        component2.ExecuteScript(ScriptEvent.ScriptEvents.OnAttackedByParty);
                    }
                }
            }

            // PATCH START:
            // RELATED BUGS: #1
            // TASK: DoT damage should be registered in personal stat tracker (personal records) as well
            if (PartyHelper.IsPartyMember(source))
            {
                CharacterStats attStats = source.GetComponent<CharacterStats>();
                PartyMemberAI memberAI = source.GetComponent<PartyMemberAI>();

                if (memberAI != null)
                {
                    DamageInfo dmgInfo = new DamageInfo(base.gameObject, amount, null);
                    dmgInfo.DamageType = damageType;
                    dmgInfo.FinalAdjustedDamage = amount;

                    memberAI.StatsOnHit(attStats.gameObject, new CombatEventArgs(dmgInfo, source, attStats.gameObject));
                }
            }
            // PATCH END
        }
    }
}