using UnityEngine;
using UnityEngine.UI;

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

    public void AtivarSelecao()
    {
        fundoEscuro.SetActive(true);
        painelBotao.SetActive(true);
        // Opcional: Se quiser congelar o tempo do jogo enquanto escolhe
        Time.timeScale = 0f;
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