using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))] // Garante que a Unity saiba que esse objeto tem um Canvas Group
public class BossHealthBar : MonoBehaviour
{
    [Header("UI Elementos")]
    public CanvasGroup painelCanvasGroup;
    public Image barraPreenchimento;
    public TextMeshProUGUI textoNomeBoss;
    public TextMeshProUGUI textoNumerosVida;

    [Header("Configurações")]
    public string nomeDoBoss = "Aetherion"; // Pode deixar em branco se for setar por outro script

    private EnemyHealth bossHealthScript;

    void Start()
    {
        // Se você esquecer de arrastar no Inspector, o script acha o Canvas Group sozinho
        if (painelCanvasGroup == null) painelCanvasGroup = GetComponent<CanvasGroup>();

        // Garante que a barra comece invisível (Alpha 0), mas o objeto continua ATIVO!
        if (painelCanvasGroup != null) painelCanvasGroup.alpha = 0f;
    }

    public void AtivarBossBar(EnemyHealth scriptDeVida)
    {
        bossHealthScript = scriptDeVida;
        if (textoNomeBoss != null) textoNomeBoss.text = nomeDoBoss;

        // Muda a transparência para 1 (Visível)
        if (painelCanvasGroup != null) painelCanvasGroup.alpha = 1f;

        AtualizarBarra();
    }

    public void DesativarBossBar()
    {
        // Muda a transparência para 0 (Invisível)
        if (painelCanvasGroup != null) painelCanvasGroup.alpha = 0f;
    }

    void Update()
    {
        // Como o objeto não desliga mais, checamos se o Alpha > 0 para saber se a barra está visível na tela
        if (bossHealthScript != null && painelCanvasGroup != null && painelCanvasGroup.alpha > 0f)
        {
            AtualizarBarra();
        }
    }

    private void AtualizarBarra()
    {
        // A matemática da barra: Vida Atual dividida pela Vida Máxima (gera um número de 0.0 a 1.0)
        float porcentagemVida = (float)bossHealthScript.currentHealth / bossHealthScript.maxHealth;
        barraPreenchimento.fillAmount = porcentagemVida;

        // Atualiza o texto centralizado (Ex: "10 / 15")
        if (textoNumerosVida != null)
        {
            textoNumerosVida.text = bossHealthScript.currentHealth + " / " + bossHealthScript.maxHealth;
        }
    }

    // ==========================================
    // FUNÇÕES DE CUSTOMIZAÇÃO
    // ==========================================

    public void MudarCorDaBarra(Color novaCor)
    {
        // AQUI ESTAVA O PROBLEMA! Faltava aplicar a cor na imagem.
        if (barraPreenchimento != null)
        {
            barraPreenchimento.color = novaCor;
        }
    }

    public void MudarNomeDoBoss(string novoNome)
    {
        nomeDoBoss = novoNome;
        if (textoNomeBoss != null)
        {
            textoNomeBoss.text = nomeDoBoss;
        }
    }

    public void RedimensionarBarra(float novaLargura)
    {
        if (barraPreenchimento != null)
        {
            // Pega o transform da barra e altera apenas a largura
            RectTransform rt = barraPreenchimento.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(novaLargura, rt.sizeDelta.y);
            }
        }
    }
}