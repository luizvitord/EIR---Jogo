using UnityEngine;
using System.Collections;
using UnityEngine.Video;

public class CutsceneFinal : MonoBehaviour
{

    [Header("Sistema de Créditos")]
    public GameObject painelCreditos;     // Arraste o Painel_Creditos aqui
    public DialogueData dadosCreditos;

    [Header("Referências da Cena")]
    public AudioSource bgmSource;         // Arraste o seu BGM_Manager aqui
    public AudioClip somGlitch;

    [Header("Referências dos Personagens")]
    public GameObject npcCiborgue;        // Arraste a Ciborgue aqui
    public Transform pontoSpawnCiborgue; // Um Empty GameObject posicionado logo à frente de onde o player estará

    [Header("Configurações do Empurrão")]
    public float forcaEmpurrao = 8f;
    public float duracaoTransicao = 0.4f;

    [Header("Configurações de Câmera")]
    public float zoomFocoCinematico = 2.0f; // Bem perto para ver os detalhes
    public float tempoDoZoom = 0.2f;        // Muito rápido para ser impactante

    [Header("Sistema de Diálogo")]
    public DialogueSystem sistemaDialogo;   // Seu script de diálogo
    public DialogueData dialogoFinal;        // As falas dessa cena
    public GameObject painelDialogo;
    public DialogueData dialogoTransferencia;

    public GerenciadorEscolha gerenciadorEscolha; // Arraste seu objeto aqui no Inspector
    public VideoPlayer videoCutscene;

    public GameObject uiHudObject;
    public PlayerMovement playerMovement;

    private bool cutsceneAtivada = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !cutsceneAtivada)
        {
            cutsceneAtivada = true;
            StartCoroutine(RotinaCutsceneRepentina(collision.gameObject));
        }
    }

    private IEnumerator RotinaCutsceneRepentina(GameObject player)
    {

        if (bgmSource != null) bgmSource.Stop();

        // 1. TRAVA O JOGADOR IMEDIATAMENTE
        PlayerMovement movScript = player.GetComponent<PlayerMovement>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Animator playerAnim = player.GetComponent<Animator>();

        if (movScript != null) movScript.canMove = false;
        if (playerAnim != null) playerAnim.SetBool("isWalking", false);

        // 2. APARECE A CIBORGUE DO NADA E DETERMINA O OLHAR DO PLAYER
        if (npcCiborgue != null && pontoSpawnCiborgue != null)
        {
            npcCiborgue.transform.position = pontoSpawnCiborgue.position;
            npcCiborgue.SetActive(true); // ELA SURGE!

            SpriteRenderer srCiborgue = npcCiborgue.GetComponent<SpriteRenderer>();
            if (srCiborgue != null)
            {
                // Espera o efeito de glitch terminar antes de empurrar o player
                yield return StartCoroutine(EfeitoGlitch(srCiborgue, npcCiborgue.transform));
            }
        }

        // Faz o player olhar na direção da ciborgue (ajuste os eixos se ela vier de cima/lados)
        // Vamos assumir que ela surge à direita dele:
        if (playerAnim != null)
        {
            playerAnim.SetFloat("LastInputX", 1f);
            playerAnim.SetFloat("LastInputY", 0f);
        }

        // 3. O EMPURRÃO (Bate e empurra o Kuro de volta)
        if (playerRb != null)
        {
            // Calcula a direção oposta de onde a ciborgue surgiu para empurrar o player
            Vector2 direcaoEmpurrao = (player.transform.position - npcCiborgue.transform.position).normalized;
            playerRb.linearVelocity = direcaoEmpurrao * forcaEmpurrao;

            // Espera um frame curtinho pro empurrão fazer efeito e depois para o corpo do player
            yield return new WaitForSeconds(0.15f);
            playerRb.linearVelocity = Vector2.zero;
        }

        // 4. ZOOM DRAMÁTICO E REPENTINO DA CÂMERA
        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            // Muda o foco para o ponto central entre os dois personagens para pegar ambos na tela
            GameObject pontoFocoMedio = new GameObject("PontoFocoCena");
            pontoFocoMedio.transform.position = (player.transform.position + npcCiborgue.transform.position) / 2f;

            camFollow.target = pontoFocoMedio.transform;
            camFollow.MudarZoomSmooth(zoomFocoCinematico, tempoDoZoom);
        }

        yield return new WaitForSeconds(tempoDoZoom + 1.0f); // Pausa dramática pós-impacto

        // 5. INICIA O DIÁLOGO
        if (sistemaDialogo != null && dialogoFinal != null)
        {
            if (painelDialogo != null) painelDialogo.SetActive(true);

            bool dialogoConcluido = false;
            sistemaDialogo.onDialogueComplete = () => { dialogoConcluido = true; };

            sistemaDialogo.StartDialogue(dialogoFinal);

            while (!dialogoConcluido) yield return null;

            sistemaDialogo.onDialogueComplete = null;

            if (gerenciadorEscolha != null)
            {
                gerenciadorEscolha.AtivarSelecao();
            }
            else
            {
                Debug.LogError("GerenciadorEscolha não foi atribuído no Inspector da CutsceneFinal!");
            }
        }
    }

    // ==========================================
    // EFEITO VISUAL DE GLITCH 
    // ==========================================
    private IEnumerator EfeitoGlitch(SpriteRenderer sr, Transform npcTransform)
    {
        float tempoGlitch = 0.4f; // Duração total do efeito
        float tempoDecorrido = 0f;
        Vector3 posOriginal = npcTransform.position;

        // Toca o som de estática/susto se houver
        if (bgmSource != null && somGlitch != null)
        {
            bgmSource.PlayOneShot(somGlitch, 1f);
        }

        while (tempoDecorrido < tempoGlitch)
        {
            // Treme a ciborgue em posições aleatórias minúsculas
            npcTransform.position = posOriginal + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);

            // Pisca a opacidade e adiciona um tom meio ciano/branco falhando
            sr.color = new Color(Random.Range(0.4f, 1f), Random.Range(0.8f, 1f), 1f, Random.Range(0.2f, 0.8f));

            tempoDecorrido += 0.05f;
            yield return new WaitForSeconds(0.05f); // Frame rate do glitch
        }

        // Estabiliza a Ciborgue de volta ao normal perfeitamente
        npcTransform.position = posOriginal;
        sr.color = Color.white;
    }

    public void IniciarTransferencia()
    {
        StartCoroutine(RotinaTransferencia());
    }

    private IEnumerator RotinaTransferencia()
    {
        // 1. AÇÃO IMEDIATA: ESCONDER HUD E TRAVAR PLAYER
        if (uiHudObject != null) uiHudObject.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = false;

        Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Focar na Ciborgue
        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null && npcCiborgue != null)
        {
            camFollow.target = npcCiborgue.transform;
            camFollow.MudarZoomSmooth(zoomFocoCinematico, 0.5f);
        }

        yield return new WaitForSeconds(0.5f);

        // 3. DIÁLOGO DA TRANSFERÊNCIA
        if (sistemaDialogo != null && dialogoTransferencia != null)
        {
            if (painelDialogo != null) painelDialogo.SetActive(true);
            bool dialogoConcluido = false;
            sistemaDialogo.onDialogueComplete = () => { dialogoConcluido = true; };
            sistemaDialogo.StartDialogue(dialogoTransferencia);
            while (!dialogoConcluido) yield return null;
            sistemaDialogo.onDialogueComplete = null;
        }

        // 4. VÍDEO
        if (videoCutscene != null)
        {
            videoCutscene.gameObject.SetActive(true);
            videoCutscene.Prepare();
            while (!videoCutscene.isPrepared) yield return null;
            videoCutscene.Play();
            while (videoCutscene.isPlaying) yield return null;
            videoCutscene.gameObject.SetActive(false);
        }

        // 5. ENCERRAMENTO DO JOGO
        Debug.Log("Fim do jogo!");

        // Se for no editor, ele para o Play. Se for o build final, ele fecha a aplicação.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}