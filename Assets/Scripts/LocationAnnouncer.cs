using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocationAnnouncer : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    public float tempoFadeIn = 1.5f;
    public float tempoExibicao = 2.0f;
    public float tempoFadeOut = 1.5f;

    [Header("Configurações de Início")]
    public bool mostrarAoIniciar = true; // Se marcado, roda o fade assim que a cena abre

    private CanvasGroup canvasGroup;
    private Image imageComponent;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        imageComponent = GetComponent<Image>(); // Pega a imagem para podermos trocá-la via código

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
    }

    void Start()
    {
        // Se estiver marcado para mostrar ao iniciar, solta o fade com a imagem padrão
        if (mostrarAoIniciar)
        {
            TriggerFade();
        }
    }

    // Função para rodar o efeito na imagem atual
    public void TriggerFade()
    {
        if (canvasGroup == null) return;

        StopAllCoroutines(); // Para o fade atual caso o Slime ou NPC chamem no meio de outro
        gameObject.SetActive(true);
        StartCoroutine(SequenciaDeFade());
    }

    // ---> FUNÇÃO NOVA: O NPC VAI CHAMAR ESSA AQUI! <---
    public void TriggerFadeComNovaImagem(Sprite novaImagem)
    {
        if (imageComponent != null && novaImagem != null)
        {
            imageComponent.sprite = novaImagem; // Troca o PNG antigo pelo novo
        }

        TriggerFade(); // Dá o play no efeito de aparecer e sumir
    }

    IEnumerator SequenciaDeFade()
    {
        // 1. FADE IN
        float timer = 0;
        while (timer < tempoFadeIn)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = timer / tempoFadeIn;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // 2. ESPERA
        yield return new WaitForSeconds(tempoExibicao);

        // 3. FADE OUT
        timer = 0;
        while (timer < tempoFadeOut)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = 1 - (timer / tempoFadeOut);
            yield return null;
        }
        canvasGroup.alpha = 0;

        gameObject.SetActive(false);
    }
}