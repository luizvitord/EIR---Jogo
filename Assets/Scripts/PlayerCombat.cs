using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Configurações do Ataque")]
    public float attackOffset = 0.6f;
    public float hitRadius = 0.5f;
    public LayerMask enemyLayers;

    [Header("Regras de Tempo")]
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    private PlayerStats stats;
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        VerificarHudAura();
        ChecarInputAura();
    }

    // ---> NOVO: Centralizado no Combate
    void VerificarHudAura()
    {
        if (stats == null || stats.hudAura == null) return;

        if (playerMovement.currentWeapon == PlayerMovement.WeaponType.IronSword)
        {
            stats.hudAura.SetActive(true);
        }
        else
        {
            stats.hudAura.SetActive(false);
        }
    }

    // ---> NOVO: Gatilho no E
    void ChecarInputAura()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && playerMovement.currentWeapon == PlayerMovement.WeaponType.IronSword)
        {
            stats.AtivarAura();
        }
    }

    public bool PodeAtacar()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            return true;
        }
        return false;
    }

    public void PerformMeleeAttack()
    {
        float lastX = anim.GetFloat("LastInputX");
        float lastY = anim.GetFloat("LastInputY");

        if (lastX == 0 && lastY == 0) lastY = -1;

        Vector2 attackDirection = new Vector2(lastX, lastY).normalized;
        Vector2 hitPoint = rb.position + (attackDirection * attackOffset);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(hitPoint, hitRadius, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("A espada cortou o: " + enemy.name);

            int finalDamage = stats.GetCurrentDamage();
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(finalDamage, attackDirection);

                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.TriggerShake(0.15f, 0.15f);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            float lastX = GetComponent<Animator>().GetFloat("LastInputX");
            float lastY = GetComponent<Animator>().GetFloat("LastInputY");
            if (lastX == 0 && lastY == 0) lastY = -1;

            Vector2 attackDirection = new Vector2(lastX, lastY).normalized;
            Vector2 hitPoint = GetComponent<Rigidbody2D>().position + (attackDirection * attackOffset);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitPoint, hitRadius);
        }
    }
}