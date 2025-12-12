/*--------------------------------------------------------------------------------*
  File Name: CombatSystem.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using Ilumisoft.HealthSystem;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.UIElements;
using GestureNX;

public class CombatSystem : MonoBehaviour
{
    public EnemyCombat enemy;
    public PlayerCombat player;

    public Animator enemyAnimator;
    public AnimatorOverrideController deadController;
    public AnimatorOverrideController damageController;
    public Vector3 dodgeOffset1 = new Vector3(-20f, 0f, 0f); // left
    public Vector3 dodgeOffset2 = new Vector3(20f, 0f, 0f);  // right
    public float dodgeDuration = 0.25f;   // time to dodge and time to return
    public bool isDodging = false;
    private bool fightEnded = false;

    [SerializeField]
    private DrawBoardController drawBoardController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DamageEnemy(int amount)
    {
        if (fightEnded) return;

        // Enemy successfully dodged
        if (!isDodging && Random.value < enemy.dodgeProbability)
        {
            // Pick random offset
            Vector3 offset = (Random.value < 0.5f) ? dodgeOffset1 : dodgeOffset2;
            StartCoroutine(DodgeRoutine(offset, enemy.transform));
            return;
        }

        // Calculate damage
        float multiplier = Mathf.Clamp(Gaussian(1f, 0.1f), 0.8f, 1.2f);
        float damage = amount * multiplier;

        enemy.health.ApplyDamage(damage);

        enemyAnimator.runtimeAnimatorController = damageController;
        enemyAnimator.Play("anim", 0, 0);

        if (enemy.health.CurrentHealth <= 0.0f && !fightEnded)
        {
            fightEnded = true;

            // Enemy is dead
            enemyAnimator.runtimeAnimatorController = deadController;
            enemyAnimator.Play("anim", 0, 0);
            drawBoardController.OnFightFinish();

            Invoke("ReturnToLevel", 4f);

            GameManager gameManager = FindFirstObjectByType<GameManager>();
            ++gameManager.currentLevel;
        }
    }

    public void DamagePlayer(int amount)
    {
        if (fightEnded) return;

        // No damage when dodging
        if (player.isDodged)
        {
            // Kill player's dodge
            player.isDodged = false;

            Vector3 offset;
            if (player.dodgeDirection == PlayerCombat.Direction.LEFT) { offset = dodgeOffset1; }
            else                                                      { offset = dodgeOffset2; }
            StartCoroutine(DodgeRoutine(offset, player.transform));

            return;
        }

        float multiplier = Mathf.Clamp(Gaussian(1f, 0.1f), 0.8f, 1.2f);
        float damage = amount * multiplier;

        // Check if player is guarded
        if (player.isGuarded)
        {
            player.health.ApplyDamage((int)(damage * player.guardDamageDampener));
        }
        else
        {
            player.health.ApplyDamage((int)damage);
        }

        // Check if player is dead
        if (player.health.CurrentHealth <= 0.0f && !fightEnded)
        {
            fightEnded = true;
            drawBoardController.OnFightFinish();

            // Player is dead
            LevelManager.instance.LoadScene("LoseScene", "CrossFade");
        }
    }

    private float Gaussian(float mean = 0f, float stdDev = 1f)
    {
        float u1 = 1.0f - Random.value;
        float u2 = 1.0f - Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }

    private void ReturnToLevel()
    {
        LevelManager.instance.LoadScene("GameScene", "CrossFade");
        AudioSystem.Instance.PlayMusic(0);
    }

    private IEnumerator DodgeRoutine(Vector3 offset, Transform transform)
    {
        isDodging = true;

        Vector3 startPos = transform.position;
        Vector3 dodgePos = startPos + offset;

        // Move to dodge position
        float t = 0f;
        while (t < dodgeDuration)
        {
            t += Time.deltaTime;
            float lerp = t / dodgeDuration;
            transform.position = Vector3.Lerp(startPos, dodgePos, lerp);
            yield return null;
        }

        // Move back to start
        t = 0f;
        while (t < dodgeDuration)
        {
            t += Time.deltaTime;
            float lerp = t / dodgeDuration;
            transform.position = Vector3.Lerp(dodgePos, startPos, lerp);
            yield return null;
        }

        isDodging = false;
    }
}
