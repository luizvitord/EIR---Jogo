using UnityEngine;
using System.Collections;

public class BossBattleManager : MonoBehaviour
{
    public static BossBattleManager Instance;

    [Header("Referências do Boss")]
    public GameObject bossObject;
    public EnemyHealth bossHealth;
    public Animator bossAnim;
    public Transform pontoFrenteTrono;
    public float velocidadeCaminhadaBoss = 2.5f;
    public RuntimeAnimatorController animatorMonstro;

    [Header("Efeitos da Transformação (NOVO)")]
    public AudioClip somGritoMonstro; // Arraste o rugido do monstro aqui
    public float intensidadeTremor = 0.4f; // Força da tremida da tela
    public float duracaoTremor = 2.0f; // Tempo que a tela fica tremendo

    [Header("Final do Boss")]
    public RuntimeAnimatorController animatorHumano;
    public DialogueData falasMorteBoss;

    [Header("Diálogos Iniciais")]
    public DialogueData falasDoRei;
    public DialogueData falasDoMonstro;

    [Header("Referências do Jogador")]
    public Transform playerTransform;

    [Header("UI / Interface")]
    public BossHealthBar bossHealthBar;
    public GameObject hudPrincipal;
    public DialogueSystem sistemaDialogo;
    public GameObject painelDialogo;

    [Header("Áudio / Trilhas")]
    public AudioSource bgmSource;
    public AudioClip musicaFaseHumana;
    public AudioClip musicaFaseMonstro;

    [Header("Configurações da Câmera")]
    public CameraFollow cameraFollow;
    public float zoomFocoSize = 2.5f;
    public float zoomNormal = 5.0f;

    public Color corBarraHumano = new Color(0.8f, 0.1f, 0.1f, 1f);
    public Color corBarraMonstro = new Color(0.5f, 0f, 0.6f, 1f);

    private void Awake()
    {
        Instance = this;
    }

    public void IniciarIntroBoss()
    {
        StartCoroutine(SequenciaIntroBoss());
    }

    private IEnumerator SequenciaIntroBoss()
    {
        if (playerTransform != null)
        {
            PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
            if (movScript != null) movScript.canMove = false;

            MonoBehaviour combatScript = playerTransform.GetComponent("PlayerCombat") as MonoBehaviour;
            if (combatScript != null) combatScript.enabled = false;

            Rigidbody2D rbKuro = playerTransform.GetComponent<Rigidbody2D>();
            if (rbKuro != null) rbKuro.linearVelocity = Vector2.zero;

            Animator animKuro = playerTransform.GetComponent<Animator>();
            if (animKuro != null) animKuro.SetBool("isWalking", false);
        }

        if (hudPrincipal != null) hudPrincipal.SetActive(false);
        if (bossHealthBar != null) bossHealthBar.DesativarBossBar();

        if (cameraFollow != null)
        {
            cameraFollow.target = bossObject.transform;
            cameraFollow.MudarZoomSmooth(zoomFocoSize, 1.5f);
        }

        yield return new WaitForSeconds(1.5f);

        if (bossObject != null && pontoFrenteTrono != null)
        {
            Vector2 destino = pontoFrenteTrono.position;
            Vector2 direcao = (destino - (Vector2)bossObject.transform.position).normalized;

            if (bossAnim != null)
            {
                bossAnim.SetBool("isWalking", true);
                bossAnim.SetFloat("InputX", direcao.x);
                bossAnim.SetFloat("InputY", direcao.y);
            }

            while (Vector2.Distance(bossObject.transform.position, destino) > 0.05f)
            {
                bossObject.transform.position = Vector2.MoveTowards(
                    bossObject.transform.position, destino, velocidadeCaminhadaBoss * Time.deltaTime);
                yield return null;
            }

            if (bossAnim != null)
            {
                bossAnim.SetBool("isWalking", false);
            }
        }

        yield return new WaitForSeconds(0.5f);

        FazerBossOlharParaPlayer();

        if (sistemaDialogo != null && falasDoRei != null)
        {
            if (painelDialogo != null) painelDialogo.SetActive(true);

            bool dialogoConcluido = false;
            sistemaDialogo.onDialogueComplete = () => { dialogoConcluido = true; };

            sistemaDialogo.StartDialogue(falasDoRei);

            while (!dialogoConcluido) yield return null;

            sistemaDialogo.onDialogueComplete = null;
            if (painelDialogo != null) painelDialogo.SetActive(false);

            if (playerTransform != null)
            {
                PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
                if (movScript != null) movScript.canMove = false;
            }
        }

        if (cameraFollow != null)
        {
            cameraFollow.target = playerTransform;
            cameraFollow.MudarZoomSmooth(zoomNormal, 1.5f);
        }

        yield return new WaitForSeconds(1.5f);

        if (hudPrincipal != null) hudPrincipal.SetActive(true);

        if (playerTransform != null)
        {
            PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
            if (movScript != null) movScript.canMove = true;

            MonoBehaviour combatScript = playerTransform.GetComponent("PlayerCombat") as MonoBehaviour;
            if (combatScript != null) combatScript.enabled = true;
        }

        BossHumanAI bossAI = bossObject.GetComponent<BossHumanAI>();
        if (bossAI != null) bossAI.enabled = true;

        if (bossHealthBar != null && bossHealth != null)
        {
            bossHealthBar.MudarCorDaBarra(corBarraHumano);
            bossHealthBar.AtivarBossBar(bossHealth);
        }

        if (bgmSource != null && musicaFaseHumana != null)
        {
            bgmSource.clip = musicaFaseHumana;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void IniciarTransicaoMonstro()
    {
        StartCoroutine(SequenciaCutsceneMonstro());
    }

    private IEnumerator SequenciaCutsceneMonstro()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (bossHealthBar != null) bossHealthBar.DesativarBossBar();

        yield return new WaitForSeconds(5f);

        if (playerTransform != null)
        {
            PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
            if (movScript != null) movScript.canMove = false;

            MonoBehaviour combatScript = playerTransform.GetComponent("PlayerCombat") as MonoBehaviour;
            if (combatScript != null) combatScript.enabled = false;

            Rigidbody2D rbKuro = playerTransform.GetComponent<Rigidbody2D>();
            if (rbKuro != null) rbKuro.linearVelocity = Vector2.zero;

            Animator animKuro = playerTransform.GetComponent<Animator>();
            if (animKuro != null) animKuro.SetBool("isWalking", false);
        }

        if (hudPrincipal != null) hudPrincipal.SetActive(false);

        if (cameraFollow != null)
        {
            cameraFollow.target = bossObject.transform;
            cameraFollow.MudarZoomSmooth(zoomFocoSize, 2.0f);
        }

        yield return new WaitForSeconds(3.5f);

        if (bossAnim != null && animatorMonstro != null)
        {
            bossAnim.runtimeAnimatorController = animatorMonstro;
            bossAnim.SetTrigger("Spawn");

            if (bossHealth != null)
            {
                bossHealth.maxHealth = 400;
                bossHealth.currentHealth = 400;

                if (bossHealthBar != null)
                {
                    bossHealthBar.RedimensionarBarra(800f); // Ajuste este valor se ficar muito grande
                }
            }

            BossHumanAI iaDoBoss = bossObject.GetComponent<BossHumanAI>();
            if (iaDoBoss != null)
            {
                iaDoBoss.isMonstro = true;
                iaDoBoss.estaAtacando = false;
            }
        }

        if (bossHealth != null)
        {
            bossHealth.currentHealth = bossHealth.maxHealth;

            Collider2D bossCollider = bossObject.GetComponent<Collider2D>();
            if (bossCollider != null) bossCollider.enabled = true;

            Rigidbody2D rbBoss = bossObject.GetComponent<Rigidbody2D>();
            if (rbBoss != null) rbBoss.bodyType = RigidbodyType2D.Dynamic;

            SpriteRenderer srBoss = bossObject.GetComponent<SpriteRenderer>();
            if (srBoss != null) srBoss.sortingOrder = 0;
        }

        yield return new WaitForSeconds(2.5f);

        FazerBossOlharParaPlayer();

        if (sistemaDialogo != null && falasDoMonstro != null)
        {
            if (painelDialogo != null) painelDialogo.SetActive(true);

            bool dialogoConcluido = false;
            sistemaDialogo.onDialogueComplete = () => { dialogoConcluido = true; };

            sistemaDialogo.StartDialogue(falasDoMonstro);

            while (!dialogoConcluido) yield return null;

            sistemaDialogo.onDialogueComplete = null;
            if (painelDialogo != null) painelDialogo.SetActive(false);

            if (playerTransform != null)
            {
                PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
                if (movScript != null) movScript.canMove = false;
            }
        }

        // =======================================================
        // O GRAN FINALE: GRITO E TREMOR DE TELA
        // =======================================================
        if (bgmSource != null && somGritoMonstro != null)
        {
            // Toca o grito no máximo (1f)
            bgmSource.PlayOneShot(somGritoMonstro, 1f);
        }

        // Dá inicio à tremida épica na câmera
        yield return StartCoroutine(TremerTela(duracaoTremor, intensidadeTremor));
        // =======================================================


        if (cameraFollow != null)
        {
            cameraFollow.target = playerTransform;
            cameraFollow.MudarZoomSmooth(zoomNormal, 1.5f);
        }

        yield return new WaitForSeconds(1.5f);

        if (playerTransform != null)
        {
            PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
            if (movScript != null) movScript.canMove = true;

            MonoBehaviour combatScript = playerTransform.GetComponent("PlayerCombat") as MonoBehaviour;
            if (combatScript != null) combatScript.enabled = true;
        }

        if (hudPrincipal != null) hudPrincipal.SetActive(true);

        BossHumanAI bossAI = bossObject.GetComponent<BossHumanAI>();
        if (bossAI != null) bossAI.enabled = true;

        if (bossHealthBar != null && bossHealth != null)
        {
            bossHealthBar.MudarCorDaBarra(corBarraMonstro);
            bossHealthBar.AtivarBossBar(bossHealth);
        }

        if (bgmSource != null && musicaFaseMonstro != null)
        {
            bgmSource.clip = musicaFaseMonstro;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void IniciarMorteFinalBoss(EnemyHealth scriptMorte)
    {
        StartCoroutine(SequenciaMorteFinal(scriptMorte));
    }

    private IEnumerator SequenciaMorteFinal(EnemyHealth scriptMorte)
    {
        if (bgmSource != null) bgmSource.Stop();

        if (playerTransform != null)
        {
            PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
            if (movScript != null) movScript.canMove = false;

            MonoBehaviour combatScript = playerTransform.GetComponent("PlayerCombat") as MonoBehaviour;
            if (combatScript != null) combatScript.enabled = false;

            Rigidbody2D rbKuro = playerTransform.GetComponent<Rigidbody2D>();
            if (rbKuro != null) rbKuro.linearVelocity = Vector2.zero;

            Animator animKuro = playerTransform.GetComponent<Animator>();
            if (animKuro != null) animKuro.SetBool("isWalking", false);
        }

        yield return new WaitForSeconds(1.2f);

        if (bossAnim != null && animatorHumano != null)
        {
            bossAnim.runtimeAnimatorController = animatorHumano;
            bossAnim.SetFloat("LastInputX", 0);
            bossAnim.SetFloat("LastInputY", -1);
            bossAnim.SetTrigger("Death");
        }

        SpriteRenderer srBoss = bossObject.GetComponent<SpriteRenderer>();
        if (srBoss != null) srBoss.sortingOrder = 10;

        if (bgmSource != null && scriptMorte.musicaVitoriaBoss != null)
        {
            bgmSource.clip = scriptMorte.musicaVitoriaBoss;
            bgmSource.loop = false;
            bgmSource.volume = 0.4f;
            bgmSource.Play();
        }

        if (cameraFollow != null)
        {
            cameraFollow.target = bossObject.transform;
            cameraFollow.MudarZoomSmooth(zoomFocoSize, 1.5f);
        }

        yield return new WaitForSeconds(2.0f);

        if (sistemaDialogo != null && falasMorteBoss != null)
        {
            if (painelDialogo != null) painelDialogo.SetActive(true);

            bool dialogoConcluido = false;
            sistemaDialogo.onDialogueComplete = () => { dialogoConcluido = true; };

            sistemaDialogo.StartDialogue(falasMorteBoss);

            while (!dialogoConcluido) yield return null;

            sistemaDialogo.onDialogueComplete = null;
            if (painelDialogo != null) painelDialogo.SetActive(false);
        }

        PopUpConquista popUp = Object.FindFirstObjectByType<PopUpConquista>();
        if (popUp != null)
        {
            popUp.MostrarPopUpCura();

            yield return new WaitForSeconds(0.5f + 2.5f + 0.5f);
        }

        if (cameraFollow != null)
        {
            cameraFollow.target = playerTransform;
            cameraFollow.MudarZoomSmooth(zoomNormal, 1.5f);
        }

        if (srBoss != null) srBoss.sortingOrder = -10;

        scriptMorte.DarRecompensasDaMorte();

        yield return new WaitForSeconds(1.0f);

        if (playerTransform != null)
        {
            PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
            if (movScript != null) movScript.canMove = true;

            MonoBehaviour combatScript = playerTransform.GetComponent("PlayerCombat") as MonoBehaviour;
            if (combatScript != null) combatScript.enabled = true;
        }
    }

    private void FazerBossOlharParaPlayer()
    {
        if (bossAnim != null && playerTransform != null && bossObject != null)
        {
            Vector2 direcaoParaPlayer = ((Vector2)playerTransform.position - (Vector2)bossObject.transform.position).normalized;

            float inputX = 0;
            float inputY = 0;

            if (Mathf.Abs(direcaoParaPlayer.x) > Mathf.Abs(direcaoParaPlayer.y))
            {
                inputX = direcaoParaPlayer.x > 0 ? 1 : -1;
            }
            else
            {
                inputY = direcaoParaPlayer.y > 0 ? 1 : -1;
            }

            bossAnim.SetFloat("LastInputX", inputX);
            bossAnim.SetFloat("LastInputY", inputY);
        }
    }

    // =========================================================
    // COROUTINE PARA TREMER A TELA (SHAKE EFFECT)
    // =========================================================
    private IEnumerator TremerTela(float duracao, float magnitude)
    {
        if (cameraFollow == null) yield break;

        // Guarda o offset original da sua câmera (Ex: X 0, Y 0, Z -10)
        // NOTA: Se essa palavra "offset" der erro vermelho no seu Unity dizendo que não existe, 
        // apenas mude o "o" minúsculo para "O" maiúsculo (Offset).
        Vector3 offsetOriginal = cameraFollow.offset;
        float tempoPassado = 0f;

        while (tempoPassado < duracao)
        {
            // Sorteia valores aleatórios bem curtinhos
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Aplica no deslocamento da câmera (para ela tremer sem o CameraFollow estragar)
            cameraFollow.offset = new Vector3(offsetOriginal.x + x, offsetOriginal.y + y, offsetOriginal.z);

            tempoPassado += Time.deltaTime;
            yield return null;
        }

        // Restaura perfeitamente o offset pra câmera não ficar torta pro resto do jogo!
        cameraFollow.offset = offsetOriginal;
    }
}