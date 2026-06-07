using System.Collections;
using UnityEngine;

public class SlimeAI : MonoBehaviour
{
    [Header("Configurações de Patrulha")]
    public Transform centroDaArea;
    public float raioDaArea = 4f;
    public float velocidadeAndar = 2f;
    public float tempoDeEspera = 2f;

    [Header("Configurações de Caça")]
    public Transform player;
    public float raioDeVisao = 5f;
    public float velocidadeCorrer = 3.5f;

    [Header("Gamefeel - Knockback")]
    public float forçaKnockback = 5f;
    public float tempoAtordoado = 0.2f;
    private bool estaAtordoado = false;

    [Header("Combate do Slime")]
    public float raioDeAtaque = 0.8f;
    public int danoAoPlayer = 2;
    public float tempoEntreAtaques = 1.5f;
    private float timerAtaque = 0f;

    [Header("Ataque em Área (Impacto)")]
    public float tempoParaCair = 0.4f;
    public float tempoTotalDaAnimacao = 0.8f;
    public float raioDoImpacto = 1.2f;

    // ---> NOVO: ÁUDIO <---
    [Header("Sons do Slime")]
    public float distanciaMaxSom = 8f;
    public AudioClip somAndar;
    public AudioClip somCorrer;
    public AudioClip somAtaque;
    public float intervaloAndar = 0.5f;   // Passos lentos (Patrulha)
    public float intervaloCorrer = 0.25f; // Passos rápidos (Caçando)
    private float timerPasso = 0f;
    private AudioSource audioSource;

    private bool estaAtacando = false;
    public float distanciaDeAtaque = 0.8f;

    private Vector2 targetPos;
    private Vector2 posicaoCentral;
    private bool isWaiting = false;
    private bool estaCacando = false;

    private Animator anim;
    private Rigidbody2D rb;
    private PlayerStats playerStats;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); // ---> NOVO: Puxa o alto-falante

        posicaoCentral = centroDaArea != null ? (Vector2)centroDaArea.position : (Vector2)transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        ProcurarNovoDestino();
    }

    void Update()
    {
        ChecarProximidadeDoPlayer();

        if (timerAtaque > 0)
        {
            timerAtaque -= Time.deltaTime;
        }

        // ---> NOVO: Lógica de passos do Slime (ÁUDIO COM DISTÂNCIA) <---
        if (rb.linearVelocity.magnitude > 0.1f && !estaAtacando && !estaAtordoado && !isWaiting)
        {
            timerPasso -= Time.deltaTime;

            if (timerPasso <= 0f)
            {
                // 1. Calcula a distância entre o Slime e o Player
                float distancia = Vector2.Distance(transform.position, player.position);

                // 2. Transforma isso num multiplicador (se estiver colado é 1, se estiver no limite é 0)
                float volumePorDistancia = 1f - (distancia / distanciaMaxSom);

                // Trava matemática para o volume nunca ficar negativo caso ele esteja MUITO longe
                volumePorDistancia = Mathf.Clamp01(volumePorDistancia);

                // 3. Só toca o som se o volume for maior que zero (ou seja, se estiver perto o suficiente)
                if (volumePorDistancia > 0f && audioSource != null)
                {
                    AudioClip somAtual = estaCacando ? somCorrer : somAndar;

                    // Pega o volume original que nós configuramos e multiplica pela distância
                    float volumeBase = estaCacando ? 0.5f : 0.3f;
                    float volumeFinal = volumeBase * volumePorDistancia;

                    if (somAtual != null) audioSource.PlayOneShot(somAtual, volumeFinal);
                }

                timerPasso = estaCacando ? intervaloCorrer : intervaloAndar;
            }
        }
        else
        {
            timerPasso = 0f;
        }
    }

    void FixedUpdate()
    {
        if (estaAtordoado || estaAtacando)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isWaiting && !estaCacando)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoverSlime();
    }

    void ChecarProximidadeDoPlayer()
    {
        if (player == null) return;

        if (playerStats != null && playerStats.currentHealth <= 0)
        {
            if (estaCacando)
            {
                estaCacando = false;
                ProcurarNovoDestino(); // Volta a passear pela floresta
            }
            return; // Aborta a função de ataque na hora!
        }

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= raioDeVisao)
        {
            if (!estaCacando)
            {
                estaCacando = true;
                isWaiting = false;
                StopAllCoroutines();
            }

            // ---> CORREÇÃO: Mudei para "&&" (Não está atordoado E não está atacando)
            if (!estaAtordoado && !estaAtacando)
            {
                if (distancia <= raioDeAtaque)
                {
                    rb.linearVelocity = Vector2.zero;

                    if (anim != null) anim.SetBool("isRunning", false);

                    if (timerAtaque <= 0f)
                    {
                        AtacarOPlayer();
                        timerAtaque = tempoEntreAtaques;
                    }
                }
                else
                {
                    targetPos = player.position;
                }
            }
        }
        else if (estaCacando)
        {
            estaCacando = false;
            ProcurarNovoDestino();
        }
    }

    void MoverSlime()
    {
        Vector2 direcao = (targetPos - (Vector2)transform.position).normalized;
        float velAtual = estaCacando ? velocidadeCorrer : velocidadeAndar;

        rb.linearVelocity = direcao * velAtual;

        if (anim != null)
        {
            anim.SetBool("isWalking", !estaCacando);
            anim.SetBool("isRunning", estaCacando);

            if (direcao.x != 0 || direcao.y != 0)
            {
                anim.SetFloat("InputX", direcao.x);
                anim.SetFloat("InputY", direcao.y);
                anim.SetFloat("LastInputX", direcao.x);
                anim.SetFloat("LastInputY", direcao.y);
            }
        }

        if (!estaCacando && Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(WaitRoutine());
        }
    }

    void ProcurarNovoDestino()
    {
        Vector2 deslocamentoAleatorio = Random.insideUnitCircle * raioDaArea;
        targetPos = posicaoCentral + deslocamentoAleatorio;
    }

    public void ApplyKnockback(Vector2 direçãoAtaque)
    {
        if (!enabled) return;
        StopAllCoroutines();
        estaAtacando = false;
        isWaiting = false;
        StartCoroutine(KnockbackRoutine(direçãoAtaque));
    }

    private IEnumerator KnockbackRoutine(Vector2 direçãoAtaque)
    {
        estaAtordoado = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direçãoAtaque * forçaKnockback, ForceMode2D.Impulse);
        yield return new WaitForSeconds(tempoAtordoado);
        rb.linearVelocity = Vector2.zero;
        estaAtordoado = false;
    }

    IEnumerator WaitRoutine()
    {
        isWaiting = true;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
        }

        yield return new WaitForSeconds(tempoDeEspera);

        ProcurarNovoDestino();
        isWaiting = false;
    }

    private void AtacarOPlayer()
    {
        estaAtacando = true;

        if (anim != null) anim.SetTrigger("Attack");

        // ---> NOVO: ÁUDIO - Toca o som do ataque do Slime
        if (audioSource != null && somAtaque != null)
        {
            audioSource.PlayOneShot(somAtaque, 0.8f);
        }

        StartCoroutine(DanoNoChaoRoutine());
    }

    private IEnumerator DanoNoChaoRoutine()
    {
        yield return new WaitForSeconds(tempoParaCair);

        Collider2D[] objetosAtingidos = Physics2D.OverlapCircleAll(transform.position, raioDoImpacto);

        foreach (Collider2D obj in objetosAtingidos)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerStats stats = obj.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(danoAoPlayer);
                }
            }
        } // ---> CORREÇÃO: Fechei a chave do foreach aqui!

        // A parte de destravar o Slime precisa ficar FORA do foreach, para ele não tentar destravar 5 vezes
        float tempoRestante = tempoTotalDaAnimacao - tempoParaCair;
        if (tempoRestante > 0)
        {
            yield return new WaitForSeconds(tempoRestante);
        }

        estaAtacando = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 centro = centroDaArea != null ? centroDaArea.position : transform.position;
        Gizmos.DrawWireSphere(centro, raioDaArea);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioDeVisao);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioDeAtaque);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioDoImpacto);
    }
}