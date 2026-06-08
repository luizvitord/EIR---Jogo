using UnityEngine;
using System.Collections;

public class BossHumanAI : MonoBehaviour
{
    // ==========================================
    // CONFIGURAÇÕES DA FORMA HUMANA
    // ==========================================
    [Header("--- FORMA HUMANA ---")]
    public float velocidadeHumana = 3.0f;
    public float velocidadeRecuperacaoHumana = 3.0f; // Humano não cansa, então fica igual à normal
    public float distanciaAtaqueHumana = 1.5f;
    public float tempoEntreAtaquesHumana = 1.5f;
    public float tempoPausaInvestidaHumana = 1.2f;
    public float forcaInvestidaHumana = 12f;
    public float tempoParaCarregarInvestidaHumana = 8f;
    public int danoAtaqueHumanoNormal = 3;
    public int danoInvestidaHumano = 5;
    public float distanciaHitboxHumana = 1.0f;
    public float raioHitboxHumana = 0.3f;

    // ==========================================
    // CONFIGURAÇÕES DA FORMA MONSTRO
    // ==========================================
    [Header("--- FORMA MONSTRO ---")]
    public float velocidadeMonstro = 4.5f;
    public float velocidadeRecuperacaoMonstro = 1.5f; // Monstro anda devagar para recuperar fôlego
    public float distanciaAtaqueMonstro = 1.8f;
    public float tempoEntreAtaquesMonstro = 2.0f;
    public float tempoPausaInvestidaMonstro = 3.0f;
    public float forcaInvestidaMonstro = 18f;
    public float tempoParaCarregarInvestidaMonstro = 6f;
    public int danoAtaqueMonstroNormal = 6;
    public int danoInvestidaMonstro = 10;
    public float distanciaHitboxMonstro = 1.5f;
    public float raioHitboxMonstro = 0.6f;

    // ==========================================
    // PROPRIEDADES DINÂMICAS (O Cérebro)
    // Elas leem "isMonstro" e entregam o valor certo automaticamente!
    // ==========================================
    private float VelocidadePadrao => isMonstro ? velocidadeMonstro : velocidadeHumana;
    private float VelocidadeRecuperacao => isMonstro ? velocidadeRecuperacaoMonstro : velocidadeRecuperacaoHumana;
    private float DistanciaAtaque => isMonstro ? distanciaAtaqueMonstro : distanciaAtaqueHumana;
    private float TempoEntreAtaques => isMonstro ? tempoEntreAtaquesMonstro : tempoEntreAtaquesHumana;
    private float TempoPausaInvestida => isMonstro ? tempoPausaInvestidaMonstro : tempoPausaInvestidaHumana;
    private float ForcaInvestida => isMonstro ? forcaInvestidaMonstro : forcaInvestidaHumana;
    private float TempoParaCarregarInvestida => isMonstro ? tempoParaCarregarInvestidaMonstro : tempoParaCarregarInvestidaHumana;
    private int DanoAtaqueNormal => isMonstro ? danoAtaqueMonstroNormal : danoAtaqueHumanoNormal;
    private int DanoInvestida => isMonstro ? danoInvestidaMonstro : danoInvestidaHumano;
    private float DistanciaHitbox => isMonstro ? distanciaHitboxMonstro : distanciaHitboxHumana;
    private float RaioHitbox => isMonstro ? raioHitboxMonstro : raioHitboxHumana;


    [Header("Configurações Compartilhadas")]
    public Transform pontoDeAtaque;
    public LayerMask layerPlayer;

    [Header("Áudio / Efeitos Sonoros")]
    public AudioSource sfxSource;
    public bool isMonstro = false;
    private bool jaTransformou = false; // Trava para atualizar status na hora da cutscene

    [Header("Volumes (Ajuste na Unity)")]
    [Range(0f, 1f)] public float volumePassoNormal = 0.1f;
    [Range(0f, 1f)] public float volumePassoDash = 0.9f;
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
    private float velocidadeAtual;
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

        timerInvestida = TempoParaCarregarInvestida;
        velocidadeAtual = VelocidadePadrao;
    }

    void Update()
    {
        if (isMonstro && !jaTransformou)
        {
            jaTransformou = true;
            velocidadeAtual = VelocidadePadrao;
            timerInvestida = TempoParaCarregarInvestida;
        }

        if (player == null || estaAtacando || healthScript.currentHealth <= 0)
        {
            return;
        }

        timerInvestida -= Time.deltaTime;
        float distanciaDoPlayer = Vector2.Distance(transform.position, player.position);

        if (timerInvestida <= 0f && distanciaDoPlayer > DistanciaAtaque)
        {
            StartCoroutine(AtaqueInvestida());
        }
        else if (distanciaDoPlayer <= DistanciaAtaque && !emCooldown)
        {
            StartCoroutine(AtaqueNormal());
        }
        else
        {
            if (distanciaDoPlayer > DistanciaAtaque * 0.9f)
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
        rb.linearVelocity = direcao * velocidadeAtual;

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

        TocarSomAtaque();

        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.4f);

        estaAtacando = false;
        StartCoroutine(IniciarCooldown());
    }

    private IEnumerator AtaqueInvestida()
    {
        estaAtacando = true;
        PararMovimento();
        timerInvestida = TempoParaCarregarInvestida;

        if (!isMonstro && sr != null) sr.color = new Color(1f, 0.4f, 0.4f);
        OlharParaOPlayer();
        TocarSomPreparaDash();

        if (isMonstro)
        {
            anim.SetTrigger("RunAttack");
            anim.SetBool("isWalking", true);
        }

        if (BossBattleManager.Instance != null && BossBattleManager.Instance.cameraFollow != null)
        {
            BossBattleManager.Instance.cameraFollow.MudarZoomSmooth(4.0f, TempoPausaInvestida);
        }

        yield return new WaitForSeconds(TempoPausaInvestida);

        if (!isMonstro && sr != null) sr.color = Color.white;

        if (BossBattleManager.Instance != null && BossBattleManager.Instance.cameraFollow != null)
        {
            BossBattleManager.Instance.cameraFollow.MudarZoomSmooth(5.0f, 0.2f);
        }

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

        while (Vector2.Distance(transform.position, destinoDoDash) > 0.5f && tempoSeguranca > 0)
        {
            tempoSeguranca -= Time.deltaTime;
            timerGolpe -= Time.deltaTime;
            timerPassoDash -= Time.deltaTime;

            rb.linearVelocity = direcaoDoDash * ForcaInvestida;
            AtualizarPosicaoDoAtaque();

            if (!isMonstro && timerPassoDash <= 0f)
            {
                TocarPassoRapidoDash();
                timerPassoDash = intervaloEntrePassosDash;
            }

            if (timerGolpe <= 0f)
            {
                if (!isMonstro) anim.SetTrigger("RunAttack");

                AplicarDanoMultiploNoDash();
                timerGolpe = intervaloEntreGolpes;
            }

            yield return null;
        }

        PararMovimento();
        anim.speed = 1f;

        yield return new WaitForSeconds(0.6f);

        estaAtacando = false;
        StartCoroutine(IniciarCooldown());
    }

    private IEnumerator IniciarCooldown()
    {
        emCooldown = true;

        velocidadeAtual = VelocidadeRecuperacao;

        yield return new WaitForSeconds(TempoEntreAtaques);

        velocidadeAtual = VelocidadePadrao;

        emCooldown = false;
    }

    private void AtualizarPosicaoDoAtaque()
    {
        if (pontoDeAtaque == null) return;

        float x = anim.GetFloat("LastInputX");
        float y = anim.GetFloat("LastInputY");
        Vector2 direcao = new Vector2(x, y).normalized;

        // CORRIGIDO PARA A LETRA MAIÚSCULA
        pontoDeAtaque.localPosition = direcao * DistanciaHitbox;
    }

    // =========================================================
    // GERENCIADOR DE ÁUDIO 
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
        if (!jaDeuDanoNesseAtaque) AplicarDanoNoPlayer(DanoAtaqueNormal);
    }

    public void CausarDanoInvestida()
    {
        if (!jaDeuDanoNesseAtaque) AplicarDanoNoPlayer(DanoInvestida);
    }

    private void AplicarDanoNoPlayer(int valorDano)
    {
        if (pontoDeAtaque == null) return;

        // CORRIGIDO PARA A LETRA MAIÚSCULA
        Collider2D hit = Physics2D.OverlapCircle(pontoDeAtaque.position, RaioHitbox, layerPlayer);
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

        // CORRIGIDO PARA A LETRA MAIÚSCULA
        Collider2D hit = Physics2D.OverlapCircle(pontoDeAtaque.position, RaioHitbox, layerPlayer);
        if (hit != null)
        {
            PlayerStats playerHealth = hit.GetComponent<PlayerStats>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(DanoInvestida);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoDeAtaque != null)
        {
            Gizmos.color = Color.red;
            // CORRIGIDO PARA A LETRA MAIÚSCULA
            Gizmos.DrawWireSphere(pontoDeAtaque.position, RaioHitbox);
        }
    }
}