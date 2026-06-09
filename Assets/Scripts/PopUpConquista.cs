using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class PopUpConquista : MonoBehaviour
{
    [Header("Referências Gerais")]
    public GameObject painelLevelUp;

    // ---> CORREÇÃO: Agora vamos esconder só o painel de vida/status, e não o Canvas inteiro!
    public GameObject painelJogador;

    [Header("Configurações de Tempo dos PopUps")]
    public float tempoFade = 0.5f;
    public float tempoVisivel = 2.5f;

    [Header("PopUp de Cura (Novo)")]
    public CanvasGroup canvasGroupCura;
    public AudioClip somPopUpCura;

    [Header("PopUp da Aura (Novo)")]
    public CanvasGroup canvasGroupAura;
    public AudioClip somPopUpAura;

    [Header("UI Permanente")]
    public GameObject slotCuraUI; // Arraste o objeto que você acabou de criar aqui

    [Header("Cutscene da Aura (Câmera)")]
    public float zoomDaCutscene = 2.5f;
    public float zoomNormal = 4.5f;
    public float tempoDeZoom = 1.5f;

    private CanvasGroup canvasGroupEspada;
    private AudioSource audioSource;

    void Awake()
    {
        canvasGroupEspada = GetComponent<CanvasGroup>();
        canvasGroupEspada.alpha = 0f;

        audioSource = GetComponent<AudioSource>();

        if (canvasGroupAura != null)
        {
            canvasGroupAura.alpha = 0f;
        }
    }

    public void MostrarPopUp()
    {
        StartCoroutine(RotinaFade());
    }

    private IEnumerator RotinaFade()
    {
        // 1. Espera o Level Up fechar
        if (painelLevelUp != null)
        {
            while (painelLevelUp.activeInHierarchy)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);

        // 2. FADE IN DA ESPADA
        while (canvasGroupEspada.alpha < 1f)
        {
            canvasGroupEspada.alpha += Time.deltaTime / tempoFade;
            yield return null;
        }
        canvasGroupEspada.alpha = 1f;

        // 3. PAUSA DA ESPADA
        yield return new WaitForSeconds(tempoVisivel);

        // 4. FADE OUT DA ESPADA
        while (canvasGroupEspada.alpha > 0f)
        {
            canvasGroupEspada.alpha -= Time.deltaTime / tempoFade;
            yield return null;
        }
        canvasGroupEspada.alpha = 0f;

        // ---> INICIA A CUTSCENE DA AURA <---
        StartCoroutine(CutsceneAura());
    }

    private IEnumerator CutsceneAura()
    {
        PlayerMovement playerMov = FindFirstObjectByType<PlayerMovement>();
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();

        if (playerMov == null || playerStats == null || camFollow == null) yield break;

        // 1. ESCONDE APENAS O PAINEL DE STATUS E TRAVA O JOGADOR
        if (painelJogador == null) painelJogador = GameObject.Find("Painel_Jogador"); // Busca de segurança
        if (painelJogador != null) painelJogador.SetActive(false);

        playerMov.canMove = false;
        Animator playerAnim = playerMov.GetComponent<Animator>();
        if (playerAnim != null) playerAnim.SetBool("isWalking", false);

        // 2. PUXA A CÂMERA (Zoom In Dramático)
        camFollow.MudarZoomSmooth(zoomDaCutscene, tempoDeZoom);
        yield return new WaitForSeconds(tempoDeZoom);

        // 3. EXPLODE A AURA! (Isso aqui ativa o cooldown nos bastidores)
        playerStats.AtivarAura();
        yield return new WaitForSeconds(3.5f);

        // 4. VOLTA A CÂMERA (Zoom Out)
        camFollow.MudarZoomSmooth(zoomNormal, tempoDeZoom);
        yield return new WaitForSeconds(tempoDeZoom);

        // 5. APARECE A CONQUISTA DA AURA + SOM
        if (canvasGroupAura != null)
        {
            if (audioSource != null && somPopUpAura != null)
            {
                audioSource.PlayOneShot(somPopUpAura, 0.8f);
            }

            while (canvasGroupAura.alpha < 1f)
            {
                canvasGroupAura.alpha += Time.deltaTime / tempoFade;
                yield return null;
            }
            canvasGroupAura.alpha = 1f;

            yield return new WaitForSeconds(tempoVisivel);

            while (canvasGroupAura.alpha > 0f)
            {
                canvasGroupAura.alpha -= Time.deltaTime / tempoFade;
                yield return null;
            }
            canvasGroupAura.alpha = 0f;
        }

        // 6. DEVOLVE OS STATUS E LIBERA O JOGADOR PARA JOGAR
        if (painelJogador != null) painelJogador.SetActive(true);

        // ---> A MÁGICA ACONTECE AQUI! <---
        // Limpa o tempo gasto pela animação do Passo 3 e zera a habilidade!
        if (playerStats != null)
        {
            playerStats.ResetarCooldownAura();
        }

        playerMov.canMove = true;

        FarmPosBoss sistemaDeFarm = Object.FindFirstObjectByType<FarmPosBoss>();
        if (sistemaDeFarm != null)
        {
            sistemaDeFarm.IniciarFarm();
        }
    }

    // --- NOVO MÉTODO ---
    public void MostrarPopUpCura()
    {
        StartCoroutine(RotinaPopUpCura());
    }

    private IEnumerator RotinaPopUpCura()
    {
        if (canvasGroupCura != null)
        {
            if (audioSource != null && somPopUpCura != null)
            {
                audioSource.PlayOneShot(somPopUpCura, 0.8f);
            }

            // FADE IN
            while (canvasGroupCura.alpha < 1f)
            {
                canvasGroupCura.alpha += Time.deltaTime / tempoFade;
                yield return null;
            }
            canvasGroupCura.alpha = 1f;

            // PAUSA
            yield return new WaitForSeconds(tempoVisivel);

            // FADE OUT
            while (canvasGroupCura.alpha > 0f)
            {
                canvasGroupCura.alpha -= Time.deltaTime / tempoFade;
                yield return null;
            }
            canvasGroupCura.alpha = 0f;
        }

        // 4. ATIVA O QUADRINHO PERMANENTE NA HUD
        if (slotCuraUI != null)
        {
            slotCuraUI.SetActive(true);
            PlayerStats stats = Object.FindFirstObjectByType<PlayerStats>();
            if (stats != null) stats.temCura = true;
        }
    }
}