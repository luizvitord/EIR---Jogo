using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Atributos")]
    public int maxHealth = 15;
    public int currentHealth;
    public int expReward = 50;

    [Header("Efeitos Visuais")]
    public GameObject damagePopupPrefab;

    [Header("Sons de Vida")]
    public AudioClip somDano;
    public AudioClip somMorte;
    public AudioClip musicaVitoriaBoss;
    private AudioSource audioSource;

    [Header("Efeito de Dano (Flash)")]
    public Material materialFlashBranco;
    private Material materialOriginal;
    private SpriteRenderer spriteRenderer;

    private Animator anim;
    private Collider2D col;
    private Rigidbody2D rb;
    private SlimeAI aiScript;
    private OrcAI orcAI;
    private BossHumanAI bossHumanAI;

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        aiScript = GetComponent<SlimeAI>();
        orcAI = GetComponent<OrcAI>();
        bossHumanAI = GetComponent<BossHumanAI>();

        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            materialOriginal = spriteRenderer.material;
        }
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " tomou " + damage + " de dano! Vida restante: " + currentHealth);

        StartCoroutine(EfeitoFlashBranco());

        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            PlayerStats playerStats = Object.FindFirstObjectByType<PlayerStats>();

            bool hitComAura = false;
            if (playerStats != null && playerStats.isAuraActive)
            {
                hitComAura = true;
            }

            popup.GetComponent<DamagePopup>().Setup(damage, hitComAura);
        }

        if (currentHealth > 0)
        {
            bool temHyperArmor = false;
            if (orcAI != null && orcAI.estaAtacando) temHyperArmor = true;
            if (bossHumanAI != null && bossHumanAI.estaAtacando) temHyperArmor = true;

            if (!temHyperArmor)
            {
                if (anim != null) anim.SetTrigger("Hurt");
                if (orcAI != null) orcAI.SofrerImpacto(0.4f);
                if (aiScript != null) aiScript.ApplyKnockback(attackDirection);
            }

            if (audioSource != null && somDano != null)
            {
                audioSource.PlayOneShot(somDano, 0.6f);
            }
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " foi derrotado!");

        if (orcAI != null)
        {
            BossHealthBar bossBar = Object.FindFirstObjectByType<BossHealthBar>();
            if (bossBar != null) bossBar.DesativarBossBar();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
            orcAI.enabled = false;
            if (col != null) col.enabled = false;

            if (spriteRenderer != null) spriteRenderer.sortingOrder = -10;

            PlayerMovement playerMov = Object.FindFirstObjectByType<PlayerMovement>();
            if (playerMov != null) playerMov.EquipIronSword();

            PopUpConquista popUp = Object.FindFirstObjectByType<PopUpConquista>();
            if (popUp != null) popUp.MostrarPopUp();

            OrcSpawnTrigger gatilhoSpawn = Object.FindFirstObjectByType<OrcSpawnTrigger>();
            if (gatilhoSpawn != null && gatilhoSpawn.audioSourceGeral != null)
            {
                gatilhoSpawn.audioSourceGeral.Stop();
            }

            if (audioSource != null && musicaVitoriaBoss != null)
            {
                audioSource.clip = musicaVitoriaBoss;
                audioSource.loop = false;
                audioSource.volume = 0.4f;
                audioSource.Play();
            }

            DarRecompensasDaMorte();
        }
        else if (bossHumanAI != null)
        {
            BossHealthBar bossBar = Object.FindFirstObjectByType<BossHealthBar>();
            if (bossBar != null) bossBar.DesativarBossBar();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            bossHumanAI.enabled = false;
            if (col != null) col.enabled = false;

            if (bossHumanAI.isMonstro)
            {
                // =======================================================
                // CHAMA A CUTSCENE FINAL DO MANAGER EM VEZ DE DESTRUIR
                // =======================================================
                if (BossBattleManager.Instance != null)
                {
                    BossBattleManager.Instance.IniciarMorteFinalBoss(this);
                }
            }
            else
            {
                if (spriteRenderer != null) spriteRenderer.sortingOrder = -10;

                if (BossBattleManager.Instance != null)
                {
                    BossBattleManager.Instance.IniciarTransicaoMonstro();
                }
            }
        }
        else
        {
            if (aiScript != null) aiScript.enabled = false;
            Destroy(gameObject, 0.8f);

            DarRecompensasDaMorte();
        }

        if (anim != null) anim.SetTrigger("Death");

        if (somMorte != null)
        {
            Vector3 posicaoSom = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
            AudioSource.PlayClipAtPoint(somMorte, posicaoSom, 0.6f);
        }
    }

    // AVISO: Agora essa função é PUBLIC para o Manager poder chamar no fim do diálogo!
    public void DarRecompensasDaMorte()
    {
        PlayerStats playerStats = Object.FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.GainExp(expReward);
            playerStats.VerificarCuraAuraVital();
        }

        WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.RegistrarMorteSlime();
    }

    private IEnumerator EfeitoFlashBranco()
    {
        if (spriteRenderer != null && materialFlashBranco != null)
        {
            spriteRenderer.material = materialFlashBranco;
            yield return new WaitForSeconds(0.08f);
            spriteRenderer.material = materialOriginal;
        }
    }
}