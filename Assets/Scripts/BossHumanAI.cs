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
    public float forcaInvestida = 15f; // Agora isso atua como a velocidade MÁXIMA dele correndo!
    public float tempoParaCarregarInvestida = 8f;

    [Header("Dano")]
    public int danoAtaqueNormal = 3;
    public int danoInvestida = 5;
    public Transform pontoDeAtaque;
    public float distanciaHitbox = 0.8f;
    public float raioHitbox = 0.4f;
    public LayerMask layerPlayer;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth healthScript;
    private SpriteRenderer sr;

    // Máquina de Estados Interna
    private bool estaAtacando = false;
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
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.4f);

        estaAtacando = false;
        StartCoroutine(IniciarCooldown());
    }

    // =========================================================
    // A NOVA INVESTIDA (O TERROR DO JOGADOR)
    // =========================================================
    private IEnumerator AtaqueInvestida()
    {
        estaAtacando = true;
        PararMovimento();
        timerInvestida = tempoParaCarregarInvestida;

        // 1. O AVISO (Brilha vermelho encarando o Kuro)
        sr.color = new Color(1f, 0.4f, 0.4f);
        OlharParaOPlayer();
        yield return new WaitForSeconds(tempoPausaInvestida);
        sr.color = Color.white;

        // 2. GRAVA O DESTINO EXATO DE ONDE O JOGADOR ESTAVA
        Vector2 destinoDoDash = player.position;
        Vector2 direcaoDoDash = (destinoDoDash - (Vector2)transform.position).normalized;

        anim.SetFloat("LastInputX", direcaoDoDash.x);
        anim.SetFloat("LastInputY", direcaoDoDash.y);

        float tempoSeguranca = 2f; // Trava de segurança pra ele não ficar preso numa parede pra sempre
        float intervaloEntreGolpes = 0.2f; // A cada 0.2s ele dá uma espadada no ar!
        float timerGolpe = 0f;

        // 3. A CORRIDA (Ele não usa mais AddForce, ele corre fisicamente até o ponto!)
        while (Vector2.Distance(transform.position, destinoDoDash) > 0.5f && tempoSeguranca > 0)
        {
            tempoSeguranca -= Time.deltaTime;
            timerGolpe -= Time.deltaTime;

            // Move fisicamente na velocidade do Dash
            rb.linearVelocity = direcaoDoDash * forcaInvestida;
            AtualizarPosicaoDoAtaque(); // A hitbox viaja na frente dele

            // 4. MULTI-HIT: Solta a animação e o dano várias vezes no trajeto!
            if (timerGolpe <= 0f)
            {
                anim.SetTrigger("RunAttack"); // Dispara a animação visualmente de novo
                AplicarDanoMultiploNoDash();  // Checa a colisão e tira vida
                timerGolpe = intervaloEntreGolpes; // Reseta o timer pro próximo golpe
            }

            yield return null; // Passa pro próximo milissegundo do jogo
        }

        // 5. CHEGOU NO DESTINO (Freia e descansa)
        PararMovimento();
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

    // Usado pelo Animation Event do Ataque Normal
    public void CausarDanoNormal()
    {
        if (!jaDeuDanoNesseAtaque) AplicarDanoNoPlayer(danoAtaqueNormal);
    }

    // Mantido para não quebrar o seu Animation Event (se ainda estiver configurado)
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

    // =========================================================
    // FUNÇÃO ESPECIAL SEM TRAVA PARA O DASH MULTI-HIT
    // =========================================================
    private void AplicarDanoMultiploNoDash()
    {
        if (pontoDeAtaque == null) return;

        // Procura você na ponta da espada dele enquanto ele corre
        Collider2D hit = Physics2D.OverlapCircle(pontoDeAtaque.position, raioHitbox, layerPlayer);
        if (hit != null)
        {
            PlayerStats playerHealth = hit.GetComponent<PlayerStats>();
            if (playerHealth != null)
            {
                // Dá o dano direto, ignorando a trava "jaDeuDanoNesseAtaque"
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