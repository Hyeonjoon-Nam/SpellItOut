/*--------------------------------------------------------------------------------*
  File Name: PlayerCombat.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using Ilumisoft.HealthSystem;

[RequireComponent(typeof(Health))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float guardDamageDampener = 0.5f;  // Multiplies with oncoming damage when guarding
    public int healAmount = 20;               // How much a healing spell heals HP
    public int magicAttackDamage = 5;         // How much damage a magic attack does
    public int meleeAttackDamage = 1;         // How much damage a melee attack does

    // States
    [HideInInspector] public bool isGuarded = false;  // Whether player is guarding or not
    [HideInInspector] public bool isDodged = false;   // Whether player is dodging or not

    // Cooldowns between abilities
    [Header("Ability Cooldowns")]
    public float magicAttackCooldown = 3.0f;  // How long between magic attacks
    public float meleeAttackCooldown = 4.0f;  // How long between melee attacks
    public float healCooldown = 5.0f;         // How long between heal spells
    public float guardCooldown = 6.0f;        // How long between guard spells
    public float dodgeCooldown = 7.0f;        // How long between dodges

    [SerializeField] private GestureNX.BattleCooldownBar meleeAttackCooldownBar;
    [SerializeField] private GestureNX.BattleCooldownBar magicAttackCooldownBar;
    [SerializeField] private GestureNX.BattleCooldownBar healCooldownBar;
    [SerializeField] private GestureNX.BattleCooldownBar guardCooldownBar;
    [SerializeField] private GestureNX.BattleCooldownBar dodgeCooldownBar;

    [Header("State Lengths")]
    public float dodgeTimeLength = 1.0f;  // How long a dodge lasts
    public float guardTimeLength = 5.0f;  // How long a guard spell lasts

    // Cooldown Timers
    private float meleeAttackTimer = 0.0f;
    private float magicAttackTimer = 0.0f;
    private float healCooldownTimer = 0.0f;
    private float guardCooldownTimer = 0.0f;
    private float dodgeCooldownTimer = 0.0f;

    // Other timers
    private float guardTimer = 0.0f;
    private float dodgeTimer = 0.0f;

    [Header("VFX")]
    public ParticleSystem healPs;
    public ParticleSystem guardPs;
    public ParticleSystem magicAttackPs;
    public ParticleSystem meleeAttackPs;

    [HideInInspector]
    public enum Direction
    {
        RIGHT,
        LEFT
    }
    [HideInInspector] public Direction dodgeDirection = Direction.LEFT;

    //-----------------------------------------------------------------------------------------------------------------

    private CombatSystem combatSystem;
    [HideInInspector] public Health health;

    [Header("SFX")]
    [SerializeField] AudioClip[] swordSfxs;
    [SerializeField] AudioClip guardSfx;
    [SerializeField] AudioClip magicSfx;
    [SerializeField] AudioClip healSfx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatSystem = FindFirstObjectByType<CombatSystem>();
        health = GetComponent<Health>();

        meleeAttackPs.Stop();
        magicAttackPs.Stop();
        healPs.Stop();
        guardPs.Stop();

        ResetMeleeAttackCooldown();
        ResetMagicAttackCooldown();
        ResetHealCooldown();
        ResetGuardCooldown();
        ResetDodgeCooldown();
    }

    // Update is called once per frame
    void Update()
    {
        // Tick timers
        magicAttackTimer -= Time.deltaTime;
        meleeAttackTimer -= Time.deltaTime;
        healCooldownTimer -= Time.deltaTime;

        // Update dodge state
        if (dodgeTimer <= 0.0f)
        {
            dodgeCooldownTimer -= Time.deltaTime;
            isDodged = false;
        }
        else
        {
            dodgeTimer -= Time.deltaTime;
        }

        // Update guard state
        if (guardTimer <= 0.0f)
        {
            guardCooldownTimer -= Time.deltaTime;
            isGuarded = false;
            StopGuardParticles();
        }
        else
        {
            guardTimer -= Time.deltaTime;
        }
    }

    public void MagicAttackButtonPress() { MagicAttack(); }
    public bool MagicAttack()
    {
        if (magicAttackTimer <= 0.0f)
        {
            combatSystem.DamageEnemy(magicAttackDamage);
            ResetMagicAttackCooldown();

            magicAttackPs.Play();
            Invoke("StopMagicAttackParticles", 1f);

            AudioSystem.Instance.PlaySFXAtPosition(magicSfx, transform.position);

            return true;
        }

        return false;
    }

    private void ResetMagicAttackCooldown()
    {
        meleeAttackTimer = magicAttackCooldown;
        magicAttackCooldownBar.TimeLimit = magicAttackCooldown;
    }

    public void MeleeAttackButtonPress() { MeleeAttack(); }
    public bool MeleeAttack()
    {
        if (meleeAttackTimer <= 0.0f)
        {
            combatSystem.DamageEnemy(meleeAttackDamage);
            ResetMeleeAttackCooldown();

            meleeAttackPs.Play();
            Invoke("StopMeleeAttackParticles", 1f);

            int swordSfxIndex = Random.Range(0, swordSfxs.Length - 1);
            AudioSystem.Instance.PlaySFXAtPosition(swordSfxs[swordSfxIndex], transform.position);

            return true;
        }

        return false;
    }

    private void ResetMeleeAttackCooldown()
    {
        meleeAttackTimer = meleeAttackCooldown;
        meleeAttackCooldownBar.TimeLimit = meleeAttackCooldown;
    }

    public void HealButtonPress() { Heal(); }
    public bool Heal()
    {
        if (healCooldownTimer <= 0.0f)
        {
            health.AddHealth(healAmount);
            ResetHealCooldown();

            healPs.Play();
            Invoke("StopHealParticles", 2f);

            AudioSystem.Instance.PlaySFXAtPosition(healSfx, transform.position);

            return true;
        }

        return false;
    }

    private void ResetHealCooldown()
    {
        healCooldownTimer = healCooldown;
        healCooldownBar.TimeLimit = healCooldown;
    }

    public void DodgeLeftButtonPress() { DodgeLeft(); }
    public bool DodgeLeft()
    {
        if (dodgeCooldownTimer <= 0.0f)
        {
            isDodged = true;
            
            ResetDodgeCooldown();

            dodgeTimer = dodgeTimeLength;
            dodgeDirection = Direction.LEFT;
            
            return true;
        }

        return false;
    }

    public void DodgeRightButtonPress() { DodgeRight(); }
    public bool DodgeRight()
    {
        if (dodgeCooldownTimer <= 0.0f)
        {
            isDodged = true;

            ResetDodgeCooldown();

            dodgeTimer = dodgeTimeLength;
            dodgeDirection = Direction.RIGHT;
            
            return true;
        }

        return false;
    }

    private void ResetDodgeCooldown()
    {
        dodgeCooldownTimer = dodgeCooldown;
        dodgeCooldownBar.TimeLimit = dodgeCooldown;
    }

    public void GuardButtonPress() { Guard(); }
    public bool Guard()
    {
        if (guardCooldownTimer <= 0.0f)
        {
            isGuarded = true;
            
            ResetGuardCooldown();

            guardTimer = guardTimeLength;
            guardPs.Play();

            AudioSystem.Instance.PlaySFXAtPosition(guardSfx, transform.position);

            return true;
        }

        return false;
    }

    private void ResetGuardCooldown()
    {
        guardCooldownTimer = guardCooldown;
        guardCooldownBar.TimeLimit = guardCooldown;
    }

    void StopHealParticles()
    {
        healPs.Stop();
    }

    void StopGuardParticles()
    {
        guardPs.Stop();
    }

    void StopMagicAttackParticles()
    {
        magicAttackPs.Stop();
    }

    void StopMeleeAttackParticles()
    {
        meleeAttackPs.Stop();
    }
}
