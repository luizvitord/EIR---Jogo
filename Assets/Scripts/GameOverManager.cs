using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameOverManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textoPrompt;
    public float velocidadeFade = 1f;

    private bool playerMorreu = false;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0; // Começa invisível
        }
    }

    public void RevelarTela()
    {
        if (canvasGroup == null || textoPrompt == null) return;

        playerMorreu = true;
        StartCoroutine(FadeInRoutine());
        StartCoroutine(PiscarTextoRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * velocidadeFade;
            yield return null;
        }
    }

    IEnumerator PiscarTextoRoutine()
    {
        while (playerMorreu)
        {
            textoPrompt.alpha = 0.3f + Mathf.PingPong(Time.time * 2f, 0.7f);
            yield return null;
        }
    }

    void Update()
    {
        if (playerMorreu && Keyboard.current != null)
        {
            // TENTAR NOVAMENTE (Aperta R)
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f; // Garante que o jogo não volte pausado
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            // VOLTAR AO MENU / ABRIR MODAL (Aperta ESC)
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // Apenas avisa no console por enquanto. 
                // Futuramente, você coloca o código de ativar o painel do Modal aqui!
                Debug.Log("Apertou ESC no Game Over! Aqui vai abrir o Modal de confirmação no futuro.");
            }
        }
    }
}