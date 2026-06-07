using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    [Header("Configurações da Cena")]
    public string sceneToLoad;

    [Header("Configurações do Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Colisor de Bloqueio")]
    // Arraste o componente BoxCollider2D sólido para cá (o que vai travar o player)
    public Collider2D solidWall;

    [Header("UI Bloqueio")]
    public GameObject blockedUI; // Painel com imagem + texto
    public TMP_Text blockedText;

    [Header("Regras de Acesso Customizadas")]
    // ---> NOVAS CONFIGURAÇÕES PARA REUTILIZAÇÃO <---
    public bool precisaDeEspadaDeFerro = false;   // Marque TRUE apenas no portal da Cidade 2
    public bool mostrarMensagemDeBloqueio = true; // Marque FALSE se quiser que apenas trave fisicamente

    private bool isTransitioning = false;
    private Coroutine blinkCoroutine;
    private Coroutine popupCoroutine;

    private void Start()
    {
        if (blockedUI != null)
        {
            blockedUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Fecha a mensagem ao apertar qualquer tecla
        if (blockedUI != null && blockedUI.activeSelf)
        {
            if (UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame)
            {
                HideBlockedUI();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTransitioning)
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // ---> NOVA LÓGICA DE VALIDAÇÃO DINÂMICA <---
                bool temAcesso = false;

                if (precisaDeEspadaDeFerro)
                {
                    // Para o portal da Cidade 3: Exige EXATAMENTE a Espada de Ferro (Drop do Orc)
                    temAcesso = (player.currentWeapon == PlayerMovement.WeaponType.IronSword);
                }
                else
                {
                    // Para o portal antigo: Só exige não estar desarmado
                    temAcesso = (player.currentWeapon != PlayerMovement.WeaponType.Unarmed);
                }

                if (temAcesso)
                {
                    // SALVAMENTO DO PROGRESSO DA RUN
                    PlayerStats stats = collision.GetComponent<PlayerStats>();
                    if (stats != null)
                    {
                        stats.SalvarProgresso(); // Grava Vida, Nível, Perks, Agilidade e Arma!
                    }

                    // Grava qual é a próxima fase para o botão "Continuar" do Menu Principal
                    PlayerPrefs.SetString("FaseSalva", sceneToLoad);
                    PlayerPrefs.Save();

                    // DESLIGA a parede sólida para ele passar liso!
                    if (solidWall != null)
                    {
                        solidWall.enabled = false;
                    }

                    // Inicia a transição
                    StartCoroutine(FadeAndLoadScene());
                }
                else
                {
                    // Só mostra o PopUp na tela se a caixinha estiver marcada no Inspector
                    if (mostrarMensagemDeBloqueio)
                    {
                        ShowBlockedUI();
                    }

                    Debug.Log("Aviso da Zona: Jogador bloqueado por não cumprir os requisitos de arma do portal.");
                }
            }
        }
    }

    private void ShowBlockedUI()
    {
        if (blockedUI != null)
        {
            blockedUI.SetActive(true);
            blockedUI.transform.localScale = Vector3.one * 0.7f;

            if (popupCoroutine != null) StopCoroutine(popupCoroutine);
            popupCoroutine = StartCoroutine(PopupAnimation());

            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkText());
        }
    }

    private IEnumerator PopupAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.7f;
        Vector3 endScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blockedUI.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }
        blockedUI.transform.localScale = endScale;
    }

    private void HideBlockedUI()
    {
        if (blockedUI != null) blockedUI.SetActive(false);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
    }

    private IEnumerator BlinkText()
    {
        while (true)
        {
            float alpha = Mathf.PingPong(Time.time * 1.5f, 1f);
            Color color = blockedText.color;
            color.a = Mathf.Lerp(0.4f, 1f, alpha);
            blockedText.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        isTransitioning = true;
        Color color = fadeImage.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        SceneManager.LoadScene(sceneToLoad);
    }
}