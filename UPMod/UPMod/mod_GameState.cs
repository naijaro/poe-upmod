using System.Collections.Generic;
using Patchwork.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace UPMod
{
    [ModifiesType]
    public class mod_GameState : GameState
    {
        [ModifiesMember("UpdateIsInCombatAndGameOver")]
        public void mod_UpdateIsInCombatAndGameOver()
        {
            if (GameState.ForceCombatMode)
            {
                global::Console.AddMessage("ForceCombatMode is TRUE", Color.blue);
                GameState.s_isInCombat = true;
                Stealth.GlobalSetInStealthMode(false);
                return;
            }
            if (GameState.s_firstLoad)
            {
                return;
            }
            if (GameState.s_inRestMode)
            {
                if (GameState.OnResting != null)
                {
                    GameState.OnResting(null, EventArgs.Empty);
                }
                GameState.s_inRestMode = false;
                return;
            }
            if (GameState.Paused)
            {
                return;
            }
            if (GameState.s_playerCharacter == null)
            {
                return;
            }
            if (GameState.s_outOfCombatTimer > 0f)
            {
                GameState.s_outOfCombatTimer -= Time.deltaTime;
            }
            if (GameState.s_isInCombat && GameState.s_combatPauseTimer > 0f)
            {
                GameState.s_combatPauseTimer -= Time.deltaTime;
            }
            if (GameState.s_isInCombat)
            {
                GameState.s_inCombatTimer += Time.deltaTime;
            }
            if (GameState.s_noSaveInCombatCountdown > 0)
            {
                GameState.s_noSaveInCombatCountdown--;
            }
            GameObject target = null;
            bool flag = GameState.s_isInCombat;
            bool flag2 = false;
            if (Faction.ActiveFactionComponents.Count == 0)
            {
                return;
            }
            AIController activeCombatant = GameState.GetActiveCombatant(false);
            if (activeCombatant)
            {
                target = activeCombatant.CurrentTarget;
                GameState.s_isInCombat = true;
                flag2 = true;
                GameState.s_outOfCombatTimer = CharacterStats.StaminaRechargeDelay;
            }
            // PATCH START:
            // RELATED BUGS: #7
            else if (s_playerCharacter != null)
            {
                // going invisible (e.g. Shadowing Beyond, Withdraw) should not reset combat for solo players;
                // combat should end only if there are no enemies in range

                CharacterStats playerStats = s_playerCharacter.gameObject.GetComponent<CharacterStats>();
                AIController aIController = GameUtilities.FindActiveAIController(s_playerCharacter.gameObject);
                bool prolongCombat = false;             

                if (aIController && playerStats && playerStats.IsInvisible)
                {
                    float range = Math.Max(aIController.PerceptionDistance, 12);
                    List<GameObject> enemies = new List<GameObject>();
                    GameUtilities.GetEnemiesInRange(s_playerCharacter.gameObject, aIController, range, enemies, false);
                    prolongCombat = enemies.Count > 0;                    
                }

                if (prolongCombat)
                {
                    flag2 = true;
                    GameState.s_isInCombat = true;                    
                    GameState.s_outOfCombatTimer = CharacterStats.StaminaRechargeDelay;
                }
            }
            // PATCH END

            GameState.s_isInCombat = (GameState.s_outOfCombatTimer > 0f);

            if (GameState.s_isInCombat && GameState.s_combatPauseTimer <= 0f)
            {
                GameState.AutoPause(AutoPauseOptions.PauseEvent.CombatTimer, null, null, null);
                GameState.s_combatPauseTimer = GameState.Option.AutoPause.CombatRoundTime;
            }
            if (GameState.s_isInCombat != flag || (flag2 && GameState.s_isInTrapTriggeredCombat))
            {
                GameState.s_inCombatTimer = 0f;
                if (GameState.s_isInCombat)
                {
                    GameState.s_isInTrapTriggeredCombat = false;
                    if (!GameState.s_isInTrapTriggeredCombat)
                    {
                        TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.COMBAT_START);
                    }
                    if (GameState.OnCombatStart != null)
                    {
                        GameState.OnCombatStart(null, EventArgs.Empty);
                    }
                    if (!GameState.s_isInTrapTriggeredCombat)
                    {
                        GameState.AutoPause(AutoPauseOptions.PauseEvent.CombatStart, target, null, null);
                    }
                }
                else
                {
                    GameState.s_combatPauseTimer = 0f;
                    GameState.s_noSaveInCombatCountdown = 10;
                    if (GameState.OnCombatEnd != null)
                    {
                        GameState.OnCombatEnd(null, EventArgs.Empty);
                    }
                    GameState.s_isInTrapTriggeredCombat = false;
                }
            }
            if (!GameState.s_gameOver && GameState.PartyDead)
            {
                GameState.s_gameOver = true;
                GameState.s_deadTimer = 2f;
            }
            if (GameState.s_gameOver && GameState.s_deadTimer > 0f)
            {
                GameState.s_deadTimer -= Time.deltaTime;
                if (GameState.s_deadTimer <= 0f)
                {
                    GameState.s_deadTimer = 0f;
                    UIDeathManager.Instance.ShowWindow();
                }
            }
        }

    }
}