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
    private BossHumanAI bossHumanAI; // <--- NOVA REFERÊNCIA PRO AETHERION

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        aiScript = GetComponent<SlimeAI>();
        orcAI = GetComponent<OrcAI>();
        bossHumanAI = GetComponent<BossHumanAI>(); // <--- PUXANDO O SCRIPT DELE

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

            PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();

            bool hitComAura = false;
            if (playerStats != null && playerStats.isAuraActive)
            {
                hitComAura = true;
            }

            popup.GetComponent<DamagePopup>().Setup(damage, hitComAura);
        }

        if (currentHealth > 0)
        {
            // O inimigo tem Hyper Armor se for um Orc atacando OU um Boss atacando!
            bool temHyperArmor = false;
            if (orcAI != null && orcAI.estaAtacando) temHyperArmor = true;
            if (bossHumanAI != null && bossHumanAI.estaAtacando) temHyperArmor = true;

            if (!temHyperArmor)
            {
                // Só toca a animação de dor (Hurt) se ele NÃO estiver atacando
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

        // --- LÓGICA ESPECÍFICA POR TIPO DE INIMIGO ---

        // 1. SE FOR O ORC
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

            PlayerMovement playerMov = FindFirstObjectByType<PlayerMovement>();
            if (playerMov != null) playerMov.EquipIronSword();

            PopUpConquista popUp = FindFirstObjectByType<PopUpConquista>();
            if (popUp != null) popUp.MostrarPopUp();

            OrcSpawnTrigger gatilhoSpawn = FindFirstObjectByType<OrcSpawnTrigger>();
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
        }
        // 2. SE FOR O AETHERION
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

            if (spriteRenderer != null) spriteRenderer.sortingOrder = -10;

            // ========================================================
            // A CORREÇÃO ESTÁ AQUI: 
            // Em vez de tocar a música de vitória, ele chama a Cutscene do Monstro!
            // ========================================================
            if (BossBattleManager.Instance != null)
            {
                BossBattleManager.Instance.IniciarTransicaoMonstro();
            }
        }
        // 3. SE NÃO FOR NENHUM DOS DOIS, É UM SLIME (Ou inimigo comum)
        else
        {
            if (aiScript != null) aiScript.enabled = false;
            Destroy(gameObject, 0.8f); // Aqui o lixeiro só passa pra pegar os slimes!
        }

        // --- LÓGICA COMPARTILHADA (XP, Sons e Animação) ---
        if (anim != null) anim.SetTrigger("Death");

        if (somMorte != null)
        {
            Vector3 posicaoSom = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
            AudioSource.PlayClipAtPoint(somMorte, posicaoSom, 0.6f);
        }

        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.GainExp(expReward);
            playerStats.VerificarCuraAuraVital();
        }

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
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