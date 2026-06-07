using UnityEngine;
using UnityEngine.UI;
using TMPro; // ---> NOVO: Necessário para controlar textos do TextMeshPro

public class HUDManager : MonoBehaviour
{
    [Header("Conexão com o Player")]
    public PlayerStats playerStats;
    private PlayerMovement playerMovement;

    [Header("Barras da UI")]
    public Image barraVidaFill;
    public Image barraXPFill;

    // ---> NOVO: TEXTO DO NÍVEL <---
    [Header("Textos do HUD")]
    public TextMeshProUGUI textoLevel;

    [Header("Imagens da Arma (Moldura)")]
    public Image iconeArma;
    public Image iconeMouse;

    [Header("Artes das Espadas")]
    public Sprite spriteEspadaMadeira;
    public Sprite spriteEspadaFerro;

    void Start()
    {
        if (playerStats != null)
        {
            playerMovement = playerStats.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (playerStats == null) return;

        // 1. Atualiza a Barra de Vida
        float porcentagemVida = (float)playerStats.currentHealth / playerStats.maxHealth;
        barraVidaFill.fillAmount = porcentagemVida;

        // 2. Atualiza a Barra de XP
        float porcentagemXP = (float)playerStats.currentExp / playerStats.expToNextLevel;
        barraXPFill.fillAmount = porcentagemXP;

        // 3. ---> NOVO: Atualiza o texto do Level em tempo real <---
        if (textoLevel != null)
        {
            textoLevel.text = "LVL. " + playerStats.level;
        }

        // 4. Verifica e atualiza a UI da Arma
        AtualizarIconeArma();
    }

    private void AtualizarIconeArma()
    {
        if (playerMovement == null || iconeArma == null || iconeMouse == null) return;

        if (playerMovement.currentWeapon == PlayerMovement.WeaponType.WoodenSword)
        {
            iconeArma.enabled = true;
            iconeMouse.enabled = true;
            iconeArma.sprite = spriteEspadaMadeira;
        }
        else if (playerMovement.currentWeapon == PlayerMovement.WeaponType.IronSword)
        {
            iconeArma.enabled = true;
            iconeMouse.enabled = true;
            iconeArma.sprite = spriteEspadaFerro;
        }
        else
        {
            iconeArma.enabled = false;
            iconeMouse.enabled = false;
        }
    }
}