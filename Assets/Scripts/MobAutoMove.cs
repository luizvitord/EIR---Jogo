using UnityEngine;

public class MobAutoMove : MonoBehaviour
{
    [Header("Configurações de Área")]
    public Transform centroDaArea;
    public float raioDaArea = 4f;

    [Header("Configurações de Movimento")]
    public float velocidadeNormal = 2f;
    public float velocidadeCorrida = 5f;
    public float waitTime = 2f;

    [Header("Fuga do Jogador")]
    public Transform player;
    public float distanciaAssustar = 2.5f;

    [Header("Áudio e Efeitos")]
    public AudioSource audioSource;
    public AudioClip somAssustado; // Para o Galo (toca uma vez ao fugir)
    public AudioClip somAmbiente;  // Para o Pinto (toca de tempos em tempos)
    private float timerSomAmbiente;

    private Vector2 targetPos;
    private Vector2 posicaoCentral;
    private bool isWaiting = false;
    private bool estaAssustado = false;
    private float velocidadeAtual;

    // Componentes do Unity
    private Animator anim;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Garante que o AudioSource seja pego automaticamente se esquecer de arrastar
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Define o tempo inicial para o primeiro som ambiente tocar (entre 2 e 5 segundos)
        timerSomAmbiente = Random.Range(2f, 5f);

        posicaoCentral = centroDaArea != null ? (Vector2)centroDaArea.position : (Vector2)transform.position;
        velocidadeAtual = velocidadeNormal;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        ProcurarNovoDestino();
    }

    void Update()
    {
        ChecarProximidadeDoPlayer();
        TocarSomAmbiente(); // Checa constantemente se está na hora de fazer o barulhinho
    }

    void FixedUpdate()
    {
        if (isWaiting && !estaAssustado)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoverGaloFisico();
    }

    void ChecarProximidadeDoPlayer()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < distanciaAssustar)
        {
            FugirDoPlayer();
        }
        else if (estaAssustado && Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            estaAssustado = false;
            velocidadeAtual = velocidadeNormal;
            StartCoroutine(WaitRoutine());
        }
    }

    void FugirDoPlayer()
    {
        // MÁGICA 1: Toca o som do Galo APENAS no momento exato em que ele toma o susto
        if (!estaAssustado && audioSource != null && somAssustado != null)
        {
            audioSource.PlayOneShot(somAssustado);
        }

        StopAllCoroutines();
        isWaiting = false;
        estaAssustado = true;
        velocidadeAtual = velocidadeCorrida;

        Vector2 direcaoOposta = ((Vector2)transform.position - (Vector2)player.position).normalized;
        Vector2 pontoFugaIdeal = (Vector2)transform.position + direcaoOposta * distanciaAssustar;

        if (Vector2.Distance(pontoFugaIdeal, posicaoCentral) <= raioDaArea)
        {
            targetPos = pontoFugaIdeal;
            return;
        }

        for (float anguloDesvio = 20f; anguloDesvio <= 90f; anguloDesvio += 20f)
        {
            Vector2 direcaoDireita = Quaternion.Euler(0, 0, anguloDesvio) * direcaoOposta;
            Vector2 pontoDireita = (Vector2)transform.position + direcaoDireita * distanciaAssustar;

            if (Vector2.Distance(pontoDireita, posicaoCentral) <= raioDaArea)
            {
                targetPos = pontoDireita;
                return;
            }

            Vector2 direcaoEsquerda = Quaternion.Euler(0, 0, -anguloDesvio) * direcaoOposta;
            Vector2 pontoEsquerda = (Vector2)transform.position + direcaoEsquerda * distanciaAssustar;

            if (Vector2.Distance(pontoEsquerda, posicaoCentral) <= raioDaArea)
            {
                targetPos = pontoEsquerda;
                return;
            }
        }

        targetPos = posicaoCentral + direcaoOposta.normalized * raioDaArea;
    }

    void MoverGaloFisico()
    {
        Vector2 direcao = (targetPos - (Vector2)transform.position).normalized;

        rb.linearVelocity = direcao * velocidadeAtual;

        if (anim != null)
        {
            anim.SetBool("isWalking", true);
            anim.SetFloat("velocityX", direcao.x);
            anim.SetFloat("velocityY", direcao.y);
        }

        if (Vector2.Distance(transform.position, targetPos) < 0.2f && !estaAssustado)
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

    // MÁGICA 2: Função que roda o som orgânico do Pintinho enquanto ele não está correndo perigo
    void TocarSomAmbiente()
    {
        if (somAmbiente != null && audioSource != null && !estaAssustado)
        {
            timerSomAmbiente -= Time.deltaTime;

            if (timerSomAmbiente <= 0)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Dá uma leve variada no tom para não ficar robótico
                audioSource.PlayOneShot(somAmbiente);

                // Define quando ele vai piar de novo (aleatório entre 3 e 8 segundos)
                timerSomAmbiente = Random.Range(3f, 8f);
            }
        }
    }

    System.Collections.IEnumerator WaitRoutine()
    {
        isWaiting = true;
        if (anim != null) anim.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitTime);

        ProcurarNovoDestino();
        isWaiting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 centro = centroDaArea != null ? centroDaArea.position : transform.position;
        DesenharCirculoGizmo(centro, raioDaArea);

        Gizmos.color = Color.red;
        DesenharCirculoGizmo(transform.position, distanciaAssustar);

        if (Application.isPlaying)
        {
            Gizmos.color = estaAssustado ? Color.magenta : Color.blue;
            Gizmos.DrawSphere(targetPos, 0.2f);
        }
    }

    void DesenharCirculoGizmo(Vector3 centro, float raio)
    {
        int segmentos = 30;
        float angulo = 0f;
        Vector3 pontoInicial = centro + new Vector3(Mathf.Cos(angulo) * raio, Mathf.Sin(angulo) * raio, 0);
        Vector3 proximoPonto = pontoInicial;

        for (int i = 0; i < segmentos; i++)
        {
            angulo += (2f * Mathf.PI) / segmentos;
            Vector3 pontoFinal = centro + new Vector3(Mathf.Cos(angulo) * raio, Mathf.Sin(angulo) * raio, 0);
            Gizmos.DrawLine(proximoPonto, pontoFinal);
            proximoPonto = pontoFinal;
        }
    }
}