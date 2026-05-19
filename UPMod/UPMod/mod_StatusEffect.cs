using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UPMod
{
    [ModifiesType]
    public class mod_StatusEffect : StatusEffect
    {
        [ModifiesMember("ApplyEffect")]
        private bool mod_ApplyEffect(GameObject target, float appliedValue)
        {
            this.m_restored = true;

            if (this.m_target == null)
            {
                this.m_target = target;

                if (this.m_target != null)
                {
                    this.m_targetStats = this.m_target.GetComponent<CharacterStats>();
                }
            }

            if (!this.Params.CanApply(
                (!(this.AbilityOrigin != null))
                    ? this.Owner
                    : this.AbilityOrigin.gameObject,
                target,
                (!this.IsFromAura)
                    ? this.m_target
                    : this.Owner))
            {
                return false;
            }

            bool flag =
                this.Params.Apply == StatusEffect.ApplyType.ApplyOverTime ||
                this.Params.Apply == StatusEffect.ApplyType.ApplyAtEnd ||
                this.Params.IntervalRate != StatusEffectParams.IntervalRateType.None;

            if (this.m_applied && !flag && target == this.m_target)
            {
                return false;
            }

            if (this.IsSuppressed)
            {
                return false;
            }

            if (this.m_triggerCount > 0 && this.Params.TriggerAdjustment != null)
            {
                if (this.IsScaledMultiplier)
                {
                    appliedValue *= (float)System.Math.Pow(
                        (double)this.Params.TriggerAdjustment.ValueAdjustment,
                        (double)this.m_triggerCount);
                }
                else
                {
                    appliedValue +=
                        this.Params.TriggerAdjustment.ValueAdjustment *
                        (float)this.m_triggerCount;
                }
            }

            // PATCH START:
            // RELATED BUGS: #4
            // TASK: do not apply TransferDamageToCaster (like TakeTheHit) to non party members

            if (this.Params.AffectsStat == StatusEffect.ModifiedStat.TransferDamageToCaster &&
                target != null)
            {
                GameObject animalCompanionMaster = GameUtilities.FindMaster(target);

                bool isPartyMember = PartyHelper.IsPartyMember(target);

                bool isPartyAnimalCompanion =
                    animalCompanionMaster != null &&
                    PartyHelper.IsPartyMember(animalCompanionMaster);

                if (!isPartyMember && !isPartyAnimalCompanion)
                {
                    return false;
                }
            }

            // PATCH END

            if (!this.ApplyEffectHelper(target, appliedValue))
            {
                return false;
            }

            this.m_applied = true;

            if (this.m_target == target)
            {
                this.m_effect_is_on_main_target = true;
            }

            return true;
        }

        [ModifiesMember("UpdateHelper")]
        private void mod_UpdateHelper(float seconds)
        {
            if (this.m_target == null)
            {
                return;
            }

            if (this.IsSuspended)
            {
                return;
            }

            // UPDATED: tactical-mode aware duration handling added by game update
            float timeActive = this.m_timeActive;

            this.m_timeActive = GenericAbility.AddDuration(
                this.m_timeActive,
                this.Duration,
                seconds);

            if (this.Duration > 0f &&
                this.m_timeActive - timeActive < seconds)
            {
                seconds = this.m_timeActive - timeActive;
            }

            if (this.m_intervalTimer > 0f)
            {
                float num = seconds;

                CharacterStats characterStats = this.m_targetStats;

                if (this.IsFromAura)
                {
                    characterStats = this.m_ownerStats;
                }

                if (characterStats != null)
                {
                    if (this.IsDOT)
                    {
                        num *= characterStats.DOTTickMult;
                    }

                    if (this.IsNegativeMovementEffect)
                    {
                        num *= characterStats.NegMoveTickMult;
                    }

                    if (this.IsPoisonEffect)
                    {
                        num *= characterStats.PoisonTickMult;
                    }

                    if (this.IsDiseaseEffect)
                    {
                        num *= characterStats.DiseaseTickMult;
                    }
                }

                this.m_intervalTimer -= num;
            }

            if (!this.m_suppressed && (this.m_applied || this.IsAura))
            {
                this.UpdateActiveEffect(seconds);
            }

            // UPDATED: tactical clear logic added by game update
            if (this.Duration <= 0f ||
                this.m_timeActive < this.Duration ||
                this.DurationAfterBreak > 0f ||
                this.m_TacticalClear)
            {
                if (this.m_cachedTeam != null && this.m_targetStats != null)
                {
                    this.m_swapTeamTimer -= seconds;

                    if (this.m_swapTeamTimer <= 0f)
                    {
                        this.m_swapTeamTimer = StatusEffect.SwapTeamTimerMax;

                        AIController aIController =
                            GameUtilities.FindActiveAIController(this.m_target);

                        // ORIGINAL:
                        // if (aIController != null &&
                        //     aIController.CurrentTarget == null &&
                        //     !aIController.IsConfused &&
                        //     !TacticalModeManager.IsInTacticalCombat(aIController))
                        // {
                        //     if (this.AfflictionOrigin != null)
                        //     {
                        //         this.m_targetStats.ClearEffectFromAffliction(this.AfflictionOrigin);
                        //         return;
                        //     }
                        //
                        //     this.ClearEffect(this.m_target);
                        // }

                        // PATCH START:
                        // RELATED BUGS: #5a, #5b
                        // TASK: fix charm and dominate effects ending prematurely

                        if (aIController != null &&
                            aIController.CurrentTarget == null &&
                            !aIController.IsConfused &&
                            !TacticalModeManager.IsInTacticalCombat(aIController))
                        {
                            bool isPartyMember =
                                PartyHelper.IsPartyMember(this.m_target);

                            float range =
                                System.Math.Max(aIController.PerceptionDistance, 15);

                            List<GameObject> enemies =
                                new List<GameObject>();

                            GameUtilities.GetEnemiesInRange(
                                this.m_target,
                                aIController,
                                range,
                                enemies);

                            if (enemies.Count <= 0)
                            {
                                // Last enemy will no longer break charm and dominate
                                // effects immediately. These effects will last at least 5s

                                if (isPartyMember || this.m_timeActive >= 5)
                                {
                                    // clear team swap effect

                                    if (this.AfflictionOrigin != null)
                                    {
                                        this.m_targetStats.ClearEffectFromAffliction(
                                            this.AfflictionOrigin);
                                    }
                                    else
                                    {
                                        this.ClearEffect(this.m_target);
                                    }

                                    if (!isPartyMember)
                                    {
                                        global::Console.AddMessage(
                                            this.m_targetStats.DisplayName.GetText() +
                                            " has broken free!");
                                        // TODO: localize the string
                                    }

                                    return;
                                }
                            }
                        }

                        // PATCH END
                    }
                }

                return;
            }

            this.UpdateFinalTick();

            // UPDATED: tactical combat expiration handling added by game update
            if (!TacticalModeManager.IsInTacticalCombat(this.m_target) ||
                this.Params.AffectsStat == StatusEffect.ModifiedStat.KnockedDown ||
                this.AfflictionOrigin == AfflictionData.Prone)
            {
                this.ClearEffect(this.m_target);
                return;
            }

            this.m_TacticalClear = true;
        }
    }
}