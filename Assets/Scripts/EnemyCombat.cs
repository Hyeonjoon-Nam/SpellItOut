/*--------------------------------------------------------------------------------*
  File Name: EnemyCombat.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using Ilumisoft.HealthSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class EnemyCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public int damage = 10;      // How much damage the enemy does
    public int healAmount = 20;  // How much HP the enemy heals

    [Header("Action Frequencies")]
    public float attackFrequency = 3.0f;        // How often (in seconds) enemy attacks
    public float healFrequency = 4.0f;          // How often (in seconds) enemy heals
    public float randomFrequencyOffset = 0.6f;  // [-Offset, Offset] that is added to the timers
    public float dodgeProbability = 0.2f;       // [0-1] Chance that enemy dodges player attack

    private float attackTimer = 0.0f;
    private float healTimer = 0.0f;

    [Header("VFX")]
    public ParticleSystem healPs;
    public ParticleSystem meleeAttackPs;

    [Header("Animations")]
    public AnimatorOverrideController slashController;
    public AnimatorOverrideController healController;
    public RuntimeAnimatorController idleController;

    private CombatSystem combatSystem;
    private Animator animator;
    [HideInInspector] public Health health;

    private GameManager gameManager;

    [Header("SFX")]
    [SerializeField] AudioClip[] swordSfxs;
    [SerializeField] AudioClip healSfx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatSystem = FindFirstObjectByType<CombatSystem>();
        gameManager = FindFirstObjectByType<GameManager>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        healPs.Stop();
        meleeAttackPs.Stop();

        // Difficulty based on current level
        if (gameManager)
        {
            health.MaxHealth = 60 + (15 * gameManager.currentLevel);
            damage = 10 + (int)(1.2f * gameManager.currentLevel);
            attackFrequency = 3.2f - (0.07f * gameManager.currentLevel);
            healAmount = 10 + (2 * gameManager.currentLevel);
            healFrequency = 8 - (0.15f * gameManager.currentLevel);
            dodgeProbability = 0.15f + (0.02f * gameManager.currentLevel);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (health.CurrentHealth <= 0) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackFrequency + Random.Range(-randomFrequencyOffset, randomFrequencyOffset))
        {
            // Attack
            combatSystem.DamagePlayer(damage);
            attackTimer = 0.0f;

            // VFX
            meleeAttackPs.Play();
            Invoke("StopMeleeAttackParticles", 1f);

            // Animation
            animator.runtimeAnimatorController = slashController;
            animator.Play("anim", 0, 0);

            // SFX
            int swordSfxIndex = Random.Range(0, swordSfxs.Length - 1);
            AudioSystem.Instance.PlaySFXAtPosition(swordSfxs[swordSfxIndex], transform.position);
        }

        healTimer += Time.deltaTime;
        if (healTimer >= healFrequency + Random.Range(-randomFrequencyOffset, randomFrequencyOffset))
        {
            // Heal
            health.AddHealth(healAmount);
            healTimer = 0.0f;

            // VFX
            healPs.Play();
            Invoke("StopHealParticles", 2f);

            // Animation
            animator.runtimeAnimatorController = healController;
            animator.Play("anim", 0, 0);

            // SFX
            AudioSystem.Instance.PlaySFXAtPosition(healSfx, transform.position);
        }
    }

    void StopHealParticles()
    {
        healPs.Stop();
    }

    void StopMeleeAttackParticles()
    {
        meleeAttackPs.Stop();
    }
}
