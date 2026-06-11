using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GerenciadorEscolha : MonoBehaviour
{
    public Button botaoCura;
    public GameObject painelBotao; // Arraste o seu "Canvas_Botão" ou o botão aqui
    public GameObject fundoEscuro; // Arraste o objeto "Fundo_Escurecido" aqui


    private void Awake() // Usamos Awake para garantir que a ligação ocorra antes do Start
    {
        if (botaoCura != null)
        {
            // Isso substitui o "On Click" do Inspector
            botaoCura.onClick.AddListener(SelecionarItem);
        }
    }

    private void Start()
    {
        // Garante que tudo comece invisível
        painelBotao.SetActive(false);
        fundoEscuro.SetActive(false);
    }

    private void Update()
    {
        // Garante que o Enter (tanto o normal quanto o do teclado numérico) funcione
        // apenas se o painel de escolha estiver visível na tela
        if (painelBotao.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SelecionarItem();
            }
        }
    }

    public void AtivarSelecao()
    {
        fundoEscuro.SetActive(true);
        painelBotao.SetActive(true);
        // Opcional: Se quiser congelar o tempo do jogo enquanto escolhe
        Time.timeScale = 0f;

        // O SEGREDO DO FOCO: Dizemos ao EventSystem para selecionar o botão
        if (botaoCura != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Limpa qualquer seleção fantasma
            EventSystem.current.SetSelectedGameObject(botaoCura.gameObject); // Foca no botão da cura
        }
    }

    public void SelecionarItem()
    {
        Time.timeScale = 1f; // Volta o tempo
        fundoEscuro.SetActive(false);
        painelBotao.SetActive(false);

        // Aqui você chama a próxima parte do diálogo da Ciborgue
        FindObjectOfType<CutsceneFinal>().IniciarTransferencia();
    }
}