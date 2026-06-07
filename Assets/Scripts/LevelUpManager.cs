using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    [Header("Telas")]
    public GameObject painelLevelUp;
    public GameObject primeiroBotaoSelecionado;
    public TextMeshProUGUI textoLevelProgresso;

    [Header("Áudio da Interface")]
    public AudioSource audioSourceUI; // Alto-falante da UI
    public AudioClip somLevelUpAparece;
    public AudioClip somHoverBotao;
    public AudioClip somClickBotao;

    [Header("Conexão com o Player")]
    public PlayerStats playerStats;
    public PlayerCombat playerCombat;
    public PlayerMovement playerMovement;

    [Header("Contadores Visuais (Textos)")]
    public TextMeshProUGUI textoContadorForca;
    public TextMeshProUGUI textoContadorVida;
    public TextMeshProUGUI textoContadorCooldown;
    public TextMeshProUGUI textoContadorAura;

    private int nivelAtualDoJogador = 1;
    private bool podeEscolher = false;

    private int ganhoDeForca = 2;
    private int ganhoDeVida = 12;
    private float reducaoDeCooldown = 0.05f;
    private int ganhoDeAura = 5;
    private int niveisPendentes = 0;

    // Criamos uma estrutura para guardar a progressão de cada nível ganho
    private class Progressao { public int de; public int para; }
    private Queue<Progressao> filaDeNiveis = new Queue<Progressao>();

    void Start()
    {
        painelLevelUp.SetActive(false);
    }

    void Update()
    {
        // Atalho de teste (pode remover depois se quiser)
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        {
            SimularLevelUp(1, 2);
        }

        // ---> NOVO: TRAVA DE SEGURANÇA DO MOUSE/TECLADO <---
        // Se a tela de Level Up estiver aberta...
        if (painelLevelUp.activeInHierarchy && EventSystem.current != null)
        {
            // ...e o jogador clicar fora fazendo o sistema perder a seleção...
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                // ...força o sistema a selecionar o primeiro botão novamente na mesma hora!
                EventSystem.current.SetSelectedGameObject(primeiroBotaoSelecionado);
            }
        }
    }

    public void SimularLevelUp(int antigo, int novo)
    {
        // Enfileira este nível ganho
        filaDeNiveis.Enqueue(new Progressao { de = antigo, para = novo });

        // Se a tela não estiver ativa, inicia o processamento
        if (!painelLevelUp.activeInHierarchy)
        {
            ProcessarProximoNivel();
        }
    }

    private void ProcessarProximoNivel()
    {
        if (filaDeNiveis.Count > 0)
        {
            Progressao p = filaDeNiveis.Peek(); // Olha o próximo da fila
            textoLevelProgresso.text = "Lvl " + p.de + "  <color=#FFD700>→</color>  Lvl " + p.para;
            AtivarTelaLevelUp();
        }
    }

    private IEnumerator AtrasoParaAbrirTela()
    {
        yield return new WaitForSeconds(0.3f);
        AtivarTelaLevelUp();
    }

    public void AtivarTelaLevelUp()
    {
        AtualizarTextosDePrevisao();

        painelLevelUp.SetActive(true);
        Time.timeScale = 0f;

        if (audioSourceUI != null && somLevelUpAparece != null)
            audioSourceUI.PlayOneShot(somLevelUpAparece);

        if (playerMovement != null) playerMovement.canMove = false;

        EventSystem.current.SetSelectedGameObject(primeiroBotaoSelecionado);

        podeEscolher = false;
        StartCoroutine(LiberarCliqueAposDelay());
    }

    private void AtualizarTextosDePrevisao()
    {
        if (playerStats != null)
        {
            int danoAtual = playerStats.GetCurrentDamage(true);
            textoContadorForca.text = danoAtual + " <color=#FFD700>→</color> <color=#00FF00>" + (danoAtual + ganhoDeForca) + "</color>";

            int vidaAtual = playerStats.maxHealth;
            textoContadorVida.text = vidaAtual + " <color=#FFD700>→</color> <color=#00FF00>" + (vidaAtual + ganhoDeVida) + "</color>";

            int auraAtual = playerStats.auraVitalChance;
            textoContadorAura.text = auraAtual + "% <color=#FFD700>→</color> <color=#00FF00>" + (auraAtual + ganhoDeAura) + "%</color>";
        }

        if (playerCombat != null)
        {
            float cdAtual = playerCombat.attackCooldown;
            textoContadorCooldown.text = cdAtual.ToString("F2") + "s <color=#FFD700>→</color> <color=#00FF00>" + (cdAtual - reducaoDeCooldown).ToString("F2") + "s</color>";
        }
    }

    IEnumerator LiberarCliqueAposDelay()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        podeEscolher = true;
    }

    // ---> NOVO: Função para o Hover que será chamada pelos botões
    public void TocarSomHover()
    {
        // Só toca se a trava já liberou, evitando tocar no instante que a tela abre!
        if (audioSourceUI != null && somHoverBotao != null && podeEscolher)
        {
            audioSourceUI.PlayOneShot(somHoverBotao);
        }
    }

    private void FinalizarEscolha()
    {
        if (audioSourceUI != null && somClickBotao != null)
            audioSourceUI.PlayOneShot(somClickBotao);

        filaDeNiveis.Dequeue(); // Remove o nível que já foi escolhido

        if (filaDeNiveis.Count > 0)
        {
            // Se ainda tem níveis na fila, processa o próximo
            ProcessarProximoNivel();
        }
        else
        {
            // Fila vazia, fecha a tela e volta o tempo
            painelLevelUp.SetActive(false);
            Time.timeScale = 1f;
            if (playerMovement != null) playerMovement.canMove = true;
        }
    }

    // --- APLICAÇÃO DOS STATUS ---

    public void EscolherForca()
    {
        if (!podeEscolher) return;
        playerStats.bonusDamage += ganhoDeForca;
        FinalizarEscolha();
    }

    public void EscolherVida()
    {
        if (!podeEscolher) return;
        playerStats.maxHealth += ganhoDeVida;
        playerStats.currentHealth += ganhoDeVida;
        FinalizarEscolha();
    }

    public void EscolherCooldown()
    {
        if (!podeEscolher) return;
        playerCombat.attackCooldown -= reducaoDeCooldown;
        if (playerCombat.attackCooldown < 0.1f) playerCombat.attackCooldown = 0.1f;
        FinalizarEscolha();
    }

    public void EscolherAuraVital()
    {
        if (!podeEscolher) return;
        playerStats.auraVitalChance += ganhoDeAura;
        FinalizarEscolha();
    }

    public void FocarBotaoNoMouse(GameObject botao)
    {
        // Só muda o foco se a tela já estiver liberada para interação
        if (podeEscolher && EventSystem.current.currentSelectedGameObject != botao)
        {
            EventSystem.current.SetSelectedGameObject(botao);
        }
    }
}