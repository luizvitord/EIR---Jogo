using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuVideoController : MonoBehaviour
{
    [Header("Referências Visuais")]
    public VideoPlayer videoPlayer;
    public CanvasGroup grupoBotoes; // Painel que vai agrupar os 4 botões

    [Header("Botões do Menu")]
    public Button btnNewGame;
    public Button btnContinue;
    public Button btnCredits;
    public Button btnExit;

    // 1. MODIFIQUE O START
    private void Start()
    {
        if (grupoBotoes != null)
        {
            grupoBotoes.alpha = 0f;
            grupoBotoes.interactable = false;
            grupoBotoes.blocksRaycasts = false;
        }

        if (btnNewGame != null) btnNewGame.onClick.AddListener(NovoJogo);
        if (btnContinue != null) btnContinue.onClick.AddListener(Continuar);
        if (btnCredits != null) btnCredits.onClick.AddListener(Creditos);
        if (btnExit != null) btnExit.onClick.AddListener(Sair);

        StartCoroutine(GatilhoTempoBotoes(1f));
    }

    private IEnumerator GatilhoTempoBotoes(float tempoEspera)
    {
        // Espera o tempo determinado (ex: 1 segundos de introdução do vídeo)
        yield return new WaitForSeconds(tempoEspera);

        // Dispara o efeito de aparecer os botões
        StartCoroutine(FadeInBotoes());
    }

    private IEnumerator FadeInBotoes()
    {
        float duration = 0.6f;
        float elapsed = 0f;

        // Libera o clique nos botões
        grupoBotoes.interactable = true;
        grupoBotoes.blocksRaycasts = true;

        // Faz o fade in suave
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            grupoBotoes.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        grupoBotoes.alpha = 1f;
    }

    // --- LÓGICA DE CADA BOTÃO ---
    // NO SEU SCRIPT DO MENU PRINCIPAL:
    public void NovoJogo()
    {
        // 1. Limpa TODOS os saves antigos para garantir que ele comece pelado e nível 1.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Saves deletados! Iniciando Novo Jogo limpo.");

        // 2. Carrega a cena inicial
        SceneManager.LoadScene("SampleScene");
    }

    public void Continuar()
    {
        // Verificamos se existe alguma cena guardada no registro do computador
        if (PlayerPrefs.HasKey("CenaSalva"))
        {
            string cenaParaCarregar = PlayerPrefs.GetString("CenaSalva");
            Debug.Log("Save encontrado! Carregando a cena: " + cenaParaCarregar);

            // Carrega direto a cidade onde o jogador parou
            SceneManager.LoadScene(cenaParaCarregar);
        }
        else
        {
            Debug.LogWarning("Nenhum save de progresso foi encontrado! Redirecionando para Novo Jogo...");

            // Fallback de segurança: Se o jogador clicar em continuar sem ter um save, começa do zero
            NovoJogo();
        }
    }

    public void Creditos() => Debug.Log("Créditos clicado!");

    public void Sair()
    {
        Debug.Log("Fechando o jogo...");
        Application.Quit();
    }
}