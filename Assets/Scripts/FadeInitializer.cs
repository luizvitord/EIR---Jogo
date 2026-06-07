using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Essencial para mexer na Imagem

public class FadeInitializer : MonoBehaviour
{
    [Header("Configurações do FadeInImage")]
    public Image fadeInImage; // Arraste a imagem preta desta cena aqui
    public float fadeInDuration = 1f; // Tempo que leva para clarear

    private void Start()
    {
        // O Start roda assim que a cena é carregada
        if (fadeInImage != null)
        {
            // Garante que ela comece preta (Alpha 1)
            Color color = fadeInImage.color;
            color.a = 0f;
            fadeInImage.color = color;

            // Inicia o Fade In
            StartCoroutine(StartSceneFadeIn());
        }
    }

    private IEnumerator StartSceneFadeIn()
    {
        // Pega a cor atual (preto sólido)
        Color color = fadeInImage.color;
        float elapsedTime = 0f;

        // Loop que clareia a tela aos poucos
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            // Diminui o Alpha de 1 até 0 baseado no tempo
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
            fadeInImage.color = color;
            yield return null; // Espera o próximo frame
        }

        // Garante que a tela fique 100% transparente no final
        color.a = 0f;
        fadeInImage.color = color;

        // Opcional: Desativar a imagem para garantir que ela não gaste processamento
        fadeInImage.gameObject.SetActive(false);
    }
}