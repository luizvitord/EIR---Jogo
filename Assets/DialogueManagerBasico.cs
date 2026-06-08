using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Necessário para usar Coroutines (comandos de tempo)

[System.Serializable]
public struct LinhaDoDialogo
{
    public string nome;
    [TextArea(2, 5)] public string texto;
}

public class DialogueManagerBasico : MonoBehaviour
{
    public static DialogueManagerBasico Instance;

    [Header("Componentes Visuais")]
    public GameObject painelDialogo;
    public TextMeshProUGUI txtNome;
    public TextMeshProUGUI txtFala;

    [Header("Configuração do Tempo")]
    [Tooltip("Tempo em segundos que cada frase fica visível na tela")]
    public float tempoPorFala = 4.0f;

    [Header("Roteiro das Cenas")]
    public LinhaDoDialogo[] falasCena1Universidade;
    public LinhaDoDialogo[] falasCena2Laboratorio;

    private LinhaDoDialogo[] falasAtuais;
    private int indiceAtual = 0;
    private bool carregandoCena2 = false;
    private Coroutine rotinaDoDialogo; // Guarda a rotina atual para controle

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        painelDialogo.SetActive(false);

        // Dispara automaticamente a CENA 1 assim que o jogo inicia
        IniciarSequencia(falasCena1Universidade, false);
    }

    public void IniciarSequencia(LinhaDoDialogo[] novasFalas, bool ehCena2)
    {
        falasAtuais = novasFalas;
        indiceAtual = 0;
        carregandoCena2 = ehCena2;

        painelDialogo.SetActive(true);
        BloquearJogador(true);

        // Se já houver um diálogo rodando por segurança, nós o paramos antes de começar o novo
        if (rotinaDoDialogo != null)
        {
            StopCoroutine(rotinaDoDialogo);
        }

        // Inicia a contagem de tempo automática
        rotinaDoDialogo = StartCoroutine(FluxoDoDialogoAutomatico());
    }

    // Esta função controla o relógio invisível do diálogo
    IEnumerator FluxoDoDialogoAutomatico()
    {
        while (indiceAtual < falasAtuais.Length)
        {
            // Atualiza os textos na tela
            txtNome.text = falasAtuais[indiceAtual].nome;
            txtFala.text = falasAtuais[indiceAtual].texto;

            // MÁGICA: O jogo continua rodando, mas este script espera os segundos passarem
            yield return new WaitForSeconds(tempoPorFala);

            // Avança o índice para a próxima frase
            indiceAtual++;
        }

        // Se o loop terminou, significa que todas as falas daquela cena acabaram
        FinalizarDialogo();
    }

    void FinalizarDialogo()
    {
        painelDialogo.SetActive(false);
        BloquearJogador(false);

        // Se acabou de ler a Cena 2 (Laboratório), viaja para o mundo 2D
        if (carregandoCena2)
        {
            ViajarParaMundo2D();
        }
    }

    void BloquearJogador(bool bloquear)
    {
        PlayerMovement movScript = FindObjectOfType<PlayerMovement>();
        if (movScript != null)
        {
            movScript.canMove = !bloquear;
        }
    }

    void ViajarParaMundo2D()
    {
        GameObject jogador3D = GameObject.FindGameObjectWithTag("Player");
        if (jogador3D != null) Destroy(jogador3D);

        SceneManager.LoadScene("Primeira_Cidade");
    }
}