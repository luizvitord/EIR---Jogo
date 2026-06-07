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
    public AudioClip musicaVitoriaBoss; // ---> NOVO: Música épica de vitória
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

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        aiScript = GetComponent<SlimeAI>();
        orcAI = GetComponent<OrcAI>();

        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            materialOriginal = spriteRenderer.material; // Salva o material padrão
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

            // 1. Procura o seu script de status onde a Habilidade está guardada
            PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();

            // 2. Olha diretamente para a variável que ativa o dobro de dano
            bool hitComAura = false;
            if (playerStats != null && playerStats.isAuraActive)
            {
                hitComAura = true;
            }

            // 3. O PopUp agora vai tremer e ficar dourado apenas nos 4 segundos que a Aura durar!
            popup.GetComponent<DamagePopup>().Setup(damage, hitComAura);
        }

        if (currentHealth > 0)
        {
            // Verifica se o inimigo é um Orc e se ele está no meio de um ataque (Hyper Armor)
            bool temHyperArmor = (orcAI != null && orcAI.estaAtacando);

            if (!temHyperArmor)
            {
                // Só toca a animação de dor e trava o inimigo se ele NÃO tiver Hyper Armor
                if (anim != null) anim.SetTrigger("Hurt");
                if (orcAI != null) orcAI.SofrerImpacto(0.4f); // Trava o Orc de andar
                if (aiScript != null) aiScript.ApplyKnockback(attackDirection); // Empurra o Slime
            }

            // O som de dano toca sempre, mesmo com Hyper Armor!
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
        if (orcAI != null)
        {
            BossHealthBar bossBar = Object.FindFirstObjectByType<BossHealthBar>();
            if (bossBar != null) bossBar.DesativarBossBar();

            // COMPORTAMENTO DO ORC (Vira decoração)
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
            if (orcAI != null) orcAI.enabled = false;
            if (col != null) col.enabled = false;

            SpriteRenderer spriteOrc = GetComponent<SpriteRenderer>();
            if (spriteOrc != null)
            {
                spriteOrc.sortingOrder = -10; // Fica no fundo
            }

            // RECOMPENSA (Espada e PopUp)
            PlayerMovement playerMov = FindFirstObjectByType<PlayerMovement>();
            if (playerMov != null) playerMov.EquipIronSword();

            PopUpConquista popUp = FindFirstObjectByType<PopUpConquista>();
            if (popUp != null) popUp.MostrarPopUp();

            // ---> A CORREÇÃO DA MÚSICA <---

            // 1. Procura o script do Gatilho e para a música de Boss
            OrcSpawnTrigger gatilhoSpawn = FindFirstObjectByType<OrcSpawnTrigger>();
            if (gatilhoSpawn != null && gatilhoSpawn.audioSourceGeral != null)
            {
                gatilhoSpawn.audioSourceGeral.Stop(); // Calou a música da batalha!
            }

            // 2. Toca a música da vitória no próprio corpo do Orc
            if (audioSource != null && musicaVitoriaBoss != null)
            {
                audioSource.clip = musicaVitoriaBoss;
                audioSource.loop = false;
                audioSource.volume = 0.4f;
                audioSource.Play();
            }
        }
        else
        {
            // COMPORTAMENTO DO SLIME
            if (aiScript != null) aiScript.enabled = false;
            Destroy(gameObject, 0.8f);
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
            // Fica totalmente branco
            spriteRenderer.material = materialFlashBranco;

            // Espera uma fração de segundo (o tempo exato de 4 a 5 frames a 60fps)
            yield return new WaitForSeconds(0.08f);

            // Devolve o material com as cores normais do pixel art
            spriteRenderer.material = materialOriginal;
        }
    }
}