using UnityEngine;
using System.Collections;

public class BossHumanAI : MonoBehaviour
{
    [Header("Atributos de Movimento")]
    public float velocidade = 3.5f;
    public float distanciaAtaque = 1.5f;

    [Header("Tempos (Cooldowns)")]
    public float tempoEntreAtaques = 1.5f;
    public float tempoPausaInvestida = 1.2f;
    public float forcaInvestida = 15f;
    public float tempoParaCarregarInvestida = 8f;

    [Header("Dano")]
    public int danoAtaqueNormal = 3;
    public int danoInvestida = 5;
    public Transform pontoDeAtaque;
    public float distanciaHitbox = 0.8f;
    public float raioHitbox = 0.4f;
    public LayerMask layerPlayer;

    [Header("Áudio / Efeitos Sonoros")]
    public AudioSource sfxSource; // Arraste o AudioSource do Boss aqui
    public bool isMonstro = false; // O Manager vai mudar isso pra true na fase 2!

    [Header("Volumes (Ajuste na Unity)")]
    [Range(0f, 1f)] public float volumePassoNormal = 0.3f;
    [Range(0f, 1f)] public float volumePassoDash = 0.15f;
    [Range(0f, 1f)] public float volumeAtaque = 0.8f;
    [Range(0f, 1f)] public float volumeGritos = 1.0f;

    [Space(10)]
    public AudioClip somPassoHumano;
    public AudioClip somAtaqueHumano;
    public AudioClip somPreparaDashHumano;
    public AudioClip somDashHumano;

    [Space(10)]
    public AudioClip somPassoMonstro;
    public AudioClip somAtaqueMonstro;
    public AudioClip somPreparaDashMonstro;
    public AudioClip somDashMonstro;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth healthScript;
    private SpriteRenderer sr;

    public bool estaAtacando = false;
    private bool emCooldown = false;
    private float timerInvestida;
    private bool jaDeuDanoNesseAtaque = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>();
        sr = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        timerInvestida = tempoParaCarregarInvestida;
    }

    void Update()
    {
        if (player == null || estaAtacando || healthScript.currentHealth <= 0)
        {
            return;
        }

        timerInvestida -= Time.deltaTime;
        float distanciaDoPlayer = Vector2.Distance(transform.position, player.position);

        if (timerInvestida <= 0f && distanciaDoPlayer > distanciaAtaque)
        {
            StartCoroutine(AtaqueInvestida());
        }
        else if (distanciaDoPlayer <= distanciaAtaque && !emCooldown)
        {
            StartCoroutine(AtaqueNormal());
        }
        else
        {
            if (distanciaDoPlayer > distanciaAtaque * 0.9f)
            {
                PerseguirPlayer();
            }
            else
            {
                PararMovimento();
                OlharParaOPlayer();
            }
        }
    }

    private void PerseguirPlayer()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        rb.linearVelocity = direcao * velocidade;

        anim.SetBool("isWalking", true);
        anim.SetFloat("InputX", direcao.x);
        anim.SetFloat("InputY", direcao.y);
        anim.SetFloat("LastInputX", direcao.x);
        anim.SetFloat("LastInputY", direcao.y);
    }

    private void PararMovimento()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isWalking", false);
    }

    private void OlharParaOPlayer()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        anim.SetFloat("LastInputX", direcao.x);
        anim.SetFloat("LastInputY", direcao.y);
    }

    private IEnumerator AtaqueNormal()
    {
        estaAtacando = true;
        jaDeuDanoNesseAtaque = false;
        PararMovimento();

        OlharParaOPlayer();
        AtualizarPosicaoDoAtaque();

        yield return new WaitForSeconds(0.2f);

        // TOCA O SOM DO GOLPE
        TocarSomAtaque();

        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.4f);

        estaAtacando = false;
        StartCoroutine(IniciarCooldown());
    }

    // =========================================================
    // A NOVA INVESTIDA (ANIME DASH / FURACÃO DE ESPADAS)
    // =========================================================
    private IEnumerator AtaqueInvestida()
    {
        estaAtacando = true;
        PararMovimento();
        timerInvestida = tempoParaCarregarInvestida;

        // --- NOVO: ZOOM CINEMATOGRÁFICO DE PREPARAÇÃO ---
        // Dá um zoom sutil (de 5 para 4) durante o tempo de preparação
        if (BossBattleManager.Instance != null && BossBattleManager.Instance.cameraFollow != null)
        {
            BossBattleManager.Instance.cameraFollow.MudarZoomSmooth(4.0f, tempoPausaInvestida);
        }

        OlharParaOPlayer();
        TocarSomPreparaDash();

        yield return new WaitForSeconds(tempoPausaInvestida);

        // --- NOVO: VOLTA O ZOOM AO NORMAL RÁPIDO AO DAR O DASH ---
        if (BossBattleManager.Instance != null && BossBattleManager.Instance.cameraFollow != null)
        {
            BossBattleManager.Instance.cameraFollow.MudarZoomSmooth(5.0f, 0.2f);
        }

        // 2. GRAVA O DESTINO EXATO DE ONDE O JOGADOR ESTAVA
        Vector2 destinoDoDash = player.position;
        Vector2 direcaoDoDash = (destinoDoDash - (Vector2)transform.position).normalized;

        anim.SetFloat("LastInputX", direcaoDoDash.x);
        anim.SetFloat("LastInputY", direcaoDoDash.y);

        float tempoSeguranca = 2f;
        float intervaloEntreGolpes = 0.15f;
        float timerGolpe = 0f;

        float intervaloEntrePassosDash = 0.08f;
        float timerPassoDash = 0f;

        anim.speed = 3f;

        if (isMonstro) TocarSomDash();

        // 3. A CORRIDA INSONE
        while (Vector2.Distance(transform.position, destinoDoDash) > 0.5f && tempoSeguranca > 0)
        {
            tempoSeguranca -= Time.deltaTime;
            timerGolpe -= Time.deltaTime;
            timerPassoDash -= Time.deltaTime;

            rb.linearVelocity = direcaoDoDash * forcaInvestida;
            AtualizarPosicaoDoAtaque();

            if (!isMonstro && timerPassoDash <= 0f)
            {
                TocarPassoRapidoDash();
                timerPassoDash = intervaloEntrePassosDash;
            }

            if (timerGolpe <= 0f)
            {
                anim.SetTrigger("RunAttack");
                AplicarDanoMultiploNoDash();
                timerGolpe = intervaloEntreGolpes;
            }

            yield return null;
        }

        // 4. CHEGOU NO DESTINO
        PararMovimento();
        anim.speed = 1f;

        yield return new WaitForSeconds(0.6f);

        estaAtacando = false;
        StartCoroutine(IniciarCooldown());
    }

    private IEnumerator IniciarCooldown()
    {
        emCooldown = true;
        yield return new WaitForSeconds(tempoEntreAtaques);
        emCooldown = false;
    }

    private void AtualizarPosicaoDoAtaque()
    {
        if (pontoDeAtaque == null) return;

        float x = anim.GetFloat("LastInputX");
        float y = anim.GetFloat("LastInputY");
        Vector2 direcao = new Vector2(x, y).normalized;

        pontoDeAtaque.localPosition = direcao * distanciaHitbox;
    }

    // =========================================================
    // GERENCIADOR DE ÁUDIO (Escolhe automaticamente se é humano ou monstro)
    // =========================================================
    public void TocarPasso()
    {
        if (sfxSource == null) return;
        AudioClip clip = isMonstro ? somPassoMonstro : somPassoHumano;
        if (clip != null) sfxSource.PlayOneShot(clip, volumePassoNormal);
    }

    public void TocarPassoRapidoDash()
    {
        if (sfxSource == null) return;
        AudioClip clip = isMonstro ? somPassoMonstro : somPassoHumano;
        if (clip != null) sfxSource.PlayOneShot(clip, volumePassoDash);
    }

    private void TocarSomAtaque()
    {
        if (sfxSource == null) return;
        AudioClip clip = isMonstro ? somAtaqueMonstro : somAtaqueHumano;
        if (clip != null) sfxSource.PlayOneShot(clip, volumeAtaque);
    }

    private void TocarSomPreparaDash()
    {
        if (sfxSource == null) return;
        AudioClip clip = isMonstro ? somPreparaDashMonstro : somPreparaDashHumano;
        if (clip != null) sfxSource.PlayOneShot(clip, volumeGritos);
    }

    private void TocarSomDash()
    {
        if (sfxSource == null) return;
        AudioClip clip = isMonstro ? somDashMonstro : somDashHumano;
        if (clip != null) sfxSource.PlayOneShot(clip, volumeGritos);
    }

    // ==========================================
    // MÉTODOS PARA OS ANIMATION EVENTS
    // ==========================================
    public void CausarDanoNormal()
    {
        if (!jaDeuDanoNesseAtaque) AplicarDanoNoPlayer(danoAtaqueNormal);
    }

    public void CausarDanoInvestida()
    {
        if (!jaDeuDanoNesseAtaque) AplicarDanoNoPlayer(danoInvestida);
    }

    private void AplicarDanoNoPlayer(int valorDano)
    {
        if (pontoDeAtaque == null) return;

        Collider2D hit = Physics2D.OverlapCircle(pontoDeAtaque.position, raioHitbox, layerPlayer);
        if (hit != null)
        {
            PlayerStats playerHealth = hit.GetComponent<PlayerStats>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(valorDano);
                jaDeuDanoNesseAtaque = true;
            }
        }
    }

    private void AplicarDanoMultiploNoDash()
    {
        if (pontoDeAtaque == null) return;

        Collider2D hit = Physics2D.OverlapCircle(pontoDeAtaque.position, raioHitbox, layerPlayer);
        if (hit != null)
        {
            PlayerStats playerHealth = hit.GetComponent<PlayerStats>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(danoInvestida);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoDeAtaque != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pontoDeAtaque.position, raioHitbox);
        }
    }
}