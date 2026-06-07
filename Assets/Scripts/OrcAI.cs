using System.Collections;
using UnityEngine;

public class OrcAI : MonoBehaviour
{
    [Header("Configurações do Alvo")]
    public Transform player;

    [Header("Movimentação")]
    public float velocidadeCorrer = 3.5f;
    public float velocidadeAndar = 1.5f;
    public float distanciaDeAtaque = 1.2f;

    [Header("Combate")]
    public int danoAoPlayer = 5;
    public float raioDoImpacto = 1.5f;
    public float tempoCansado = 2f;

    [Header("Sons do Orc")]
    public float distanciaMaxSom = 10f;
    public AudioClip somPassoPesado;
    public AudioClip somPassoCansado;
    public AudioClip somAtaque;
    public float intervaloCorrer = 1.2f;
    public float intervaloAndar = 0.8f;
    [Range(0f, 1f)] public float volumePassoCorrer = 0.4f;
    [Range(0f, 1f)] public float volumePassoCansado = 1.0f;
    private float timerPasso = 0f;
    private AudioSource audioSource;

    public bool estaAtacando = false;
    private bool estaSofrendoDano = false;
    private bool estaCansado = false;
    private bool estaMorto = false;

    private Animator anim;
    private Rigidbody2D rb;
    private PlayerStats playerStats;
    private EnemyHealth healthScript;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        healthScript = GetComponent<EnemyHealth>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (healthScript != null && healthScript.currentHealth <= 0)
        {
            estaMorto = true;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (estaMorto || estaAtacando || estaSofrendoDano || player == null) return;

        if (playerStats != null && playerStats.currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
            return;
        }

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= distanciaDeAtaque)
        {
            Atacar();
        }

        if (rb.linearVelocity.magnitude > 0.1f && !estaAtacando)
        {
            timerPasso -= Time.deltaTime;

            if (timerPasso <= 0f)
            {
                float volumePorDistancia = 1f - (distancia / distanciaMaxSom);
                volumePorDistancia = Mathf.Clamp01(volumePorDistancia);

                if (volumePorDistancia > 0f && audioSource != null)
                {
                    AudioClip somAtual = estaCansado ? somPassoCansado : somPassoPesado;
                    float volumeBase = estaCansado ? volumePassoCansado : volumePassoCorrer;
                    float volumeFinal = volumeBase * volumePorDistancia;

                    if (somAtual != null) audioSource.PlayOneShot(somAtual, volumeFinal);
                }

                timerPasso = estaCansado ? intervaloAndar : intervaloCorrer;
            }
        }
        else
        {
            timerPasso = 0f;
        }
    }

    void FixedUpdate()
    {
        if (estaMorto || estaAtacando || estaSofrendoDano || player == null || (playerStats != null && playerStats.currentHealth <= 0))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distancia = Vector2.Distance(transform.position, player.position);
        if (distancia > distanciaDeAtaque)
        {
            MoverAtrasDoPlayer();
        }
    }

    void MoverAtrasDoPlayer()
    {
        Vector2 direcao = (player.position - transform.position).normalized;

        float velAtual = estaCansado ? velocidadeAndar : velocidadeCorrer;
        rb.linearVelocity = direcao * velAtual;

        anim.SetBool("isRunning", !estaCansado);
        anim.SetBool("isWalking", estaCansado);

        if (direcao.x != 0 || direcao.y != 0)
        {
            anim.SetFloat("InputX", direcao.x);
            anim.SetFloat("InputY", direcao.y);
            anim.SetFloat("LastInputX", direcao.x);
            anim.SetFloat("LastInputY", direcao.y);
        }
    }

    void Atacar()
    {
        estaAtacando = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);

        // Dá o gatilho para a animação começar. O resto do trabalho é dos Animation Events!
        anim.SetTrigger("Attack");
    }

    // ---> NOVO: Função chamada no FRAME 0 de cada clipe de ataque para o grito/aviso!
    public void TocarSomAtaque()
    {
        if (audioSource != null && somAtaque != null)
        {
            audioSource.PlayOneShot(somAtaque, 0.3f);
        }
    }

    // Função chamada PELA ANIMAÇÃO no frame exato do golpe (ex: Frame 60)!
    public void CausarDanoDoGolpe()
    {
        // Som removido daqui para não tocar duplicado ou atrasado
        Collider2D[] objetosAtingidos = Physics2D.OverlapCircleAll(transform.position, raioDoImpacto);
        foreach (Collider2D obj in objetosAtingidos)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerStats stats = obj.GetComponent<PlayerStats>();
                if (stats != null) stats.TakeDamage(danoAoPlayer);
            }
        }
    }

    // Função chamada PELA ANIMAÇÃO no último frame para liberar o Orc
    public void FinalizarAtaque()
    {
        estaAtacando = false;
        StartCoroutine(RotinaDeCansaco());
    }

    IEnumerator RotinaDeCansaco()
    {
        estaCansado = true;
        yield return new WaitForSeconds(tempoCansado);
        estaCansado = false;
    }

    public void SofrerImpacto(float tempoParado)
    {
        if (estaAtacando) return;
        StartCoroutine(RotinaImpacto(tempoParado));
    }

    private IEnumerator RotinaImpacto(float tempo)
    {
        estaSofrendoDano = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);

        yield return new WaitForSeconds(tempo);

        estaSofrendoDano = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeAtaque);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioDoImpacto);
    }
}