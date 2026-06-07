using UnityEngine;
using UnityEngine.UI;

public class AuraCooldownUI : MonoBehaviour
{
    [Header("Imagens da UI")]
    public Image iconePrincipal; // A foto colorida da Aura
    public Image imagemEscura;   // O relógio preto transparente por cima

    [Header("Conexão")]
    public PlayerStats stats;

    private bool faseDuracao = false;
    private bool faseCooldown = false;

    private float timerAtual = 0f;

    void Start()
    {
        iconePrincipal.fillAmount = 1f; // Começa cheia e visível
        imagemEscura.fillAmount = 0f;   // Começa sem o escuro
    }

    void Update()
    {
        // 1. GATILHO INICIAL: O jogador apertou E e ativou a Aura
        if (stats.isAuraActive && !faseDuracao && !faseCooldown)
        {
            faseDuracao = true;
            timerAtual = stats.auraDuration; // Começa com 4 segundos
            imagemEscura.fillAmount = 0f;
        }

        // 2. FASE 1 (DURAÇÃO): A imagem colorida vai sumindo
        if (faseDuracao)
        {
            timerAtual -= Time.deltaTime;
            iconePrincipal.fillAmount = timerAtual / stats.auraDuration; // Vai de 1 para 0

            // Quando os 4 segundos acabam...
            if (timerAtual <= 0)
            {
                faseDuracao = false;
                iconePrincipal.fillAmount = 1f; // A imagem colorida pula de volta pra tela!

                // Inicia a Fase 2!
                faseCooldown = true;
                timerAtual = stats.auraCooldown; // Começa a contar os 10 segundos
            }
        }

        // 3. FASE 2 (COOLDOWN): A imagem escura vai girando e sumindo
        if (faseCooldown)
        {
            timerAtual -= Time.deltaTime;
            imagemEscura.fillAmount = timerAtual / stats.auraCooldown; // Vai de 1 para 0

            // Quando os 10 segundos acabam...
            if (timerAtual <= 0)
            {
                faseCooldown = false;
                imagemEscura.fillAmount = 0f; // Libera a tela pra usar a habilidade de novo
            }
        }
    }
}