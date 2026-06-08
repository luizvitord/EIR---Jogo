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
                bossAnim.SetFloat("LastInputX", direcao.x);
                bossAnim.SetFloat("LastInputY", direcao.y);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // ==========================================
        // DIÁLOGO (Força-bruta local)
        // ==========================================
        if (sistemaDialogo != null && falasDoRei != null)
        {
            // 1. Liga o painel para impedir o erro da Coroutine
            if (painelDialogo != null) painelDialogo.SetActive(true);

            bool dialogoConcluido = false;
            sistemaDialogo.onDialogueComplete = () => { dialogoConcluido = true; };

            sistemaDialogo.StartDialogue(falasDoRei);

            while (!dialogoConcluido)
            {
                yield return null;
            }

            sistemaDialogo.onDialogueComplete = null;

            // 2. O diálogo acabou. Esconde o painel na força
            if (painelDialogo != null) painelDialogo.SetActive(false);

            // 3. O DialogueSystem acabou de soltar o jogador. Vamos TRAVÁ-LO DE NOVO 
            // para ele não andar enquanto a câmera volta.
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

        // 4. Aqui a cutscene finalmente acaba de verdade, então liberamos tudo
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

            // =========================================================
            // AQUI ESTÁ A MÁGICA: AVISA A IA QUE AGORA OS SONS SÃO DE MONSTRO!
            // E LIMPA A MEMÓRIA DE ATAQUES PARA DESTRAVAR O CÓDIGO
            // =========================================================
            BossHumanAI iaDoBoss = bossObject.GetComponent<BossHumanAI>();
            if (iaDoBoss != null)
            {
                iaDoBoss.isMonstro = true;
                iaDoBoss.estaAtacando = false; // <--- CORREÇÃO 1
            }
        }

        if (bossHealth != null)
        {
            bossHealth.currentHealth = bossHealth.maxHealth;

            // 1. Religa o colisor
            Collider2D bossCollider = bossObject.GetComponent<Collider2D>();
            if (bossCollider != null) bossCollider.enabled = true;

            // 2. DESCONGELA A FÍSICA (Tira de Static e volta para Dynamic)
            Rigidbody2D rbBoss = bossObject.GetComponent<Rigidbody2D>();
            if (rbBoss != null) rbBoss.bodyType = RigidbodyType2D.Dynamic;

            // 3. TRAZ O SPRITE PARA A FRENTE NOVAMENTE (Tira do -10)
            SpriteRenderer srBoss = bossObject.GetComponent<SpriteRenderer>();
            if (srBoss != null) srBoss.sortingOrder = 0; // Coloque 0 ou o valor padrão que você usa
        }

        yield return new WaitForSeconds(2.5f);

        // ==========================================
        // DIÁLOGO MONSTRO (Força-bruta local)
        // ==========================================
        if (sistemaDialogo != null && falasDoMonstro != null)
        {
            if (painelDialogo != null) painelDialogo.SetActive(true);

            bool dialogoConcluido = false;
            sistemaDialogo.onDialogueComplete = () => { dialogoConcluido = true; };

            sistemaDialogo.StartDialogue(falasDoMonstro);

            while (!dialogoConcluido)
            {
                yield return null;
            }

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

        if (playerTransform != null)
        {
            PlayerMovement movScript = playerTransform.GetComponent<PlayerMovement>();
            if (movScript != null) movScript.canMove = true;

            MonoBehaviour combatScript = playerTransform.GetComponent("PlayerCombat") as MonoBehaviour;
            if (combatScript != null) combatScript.enabled = true;
        }

        if (hudPrincipal != null) hudPrincipal.SetActive(true);

        // =========================================================
        // CORREÇÃO 2: RELIGA O CÉREBRO DO MONSTRO APÓS A CUTSCENE
        // =========================================================
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
}