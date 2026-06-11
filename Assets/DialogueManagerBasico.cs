using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public float tempoPorFala = 4.0f;

    [Header("Roteiro das Cenas")]
    public LinhaDoDialogo[] falasCena1Universidade;
    public LinhaDoDialogo[] falasCena2LabAproximacao;
    public LinhaDoDialogo[] falasCena2LabInteracao;

    private LinhaDoDialogo[] falasAtuais;
    private int indiceAtual = 0;
    private bool carregandoCena2 = false;
    private Coroutine rotinaDoDialogo;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        painelDialogo.SetActive(false);
        // O terceiro "true" avisa que a faculdade pode passar e fechar sozinha
        IniciarSequencia(falasCena1Universidade, false, true);
    }

    // Adicionamos o "fecharAutomatico" na regra
    public void IniciarSequencia(LinhaDoDialogo[] novasFalas, bool ehCena2, bool fecharAutomatico = true)
    {
        falasAtuais = novasFalas;
        indiceAtual = 0;
        carregandoCena2 = ehCena2;

        painelDialogo.SetActive(true);
        BloquearJogador(true);

        if (rotinaDoDialogo != null)
        {
            StopCoroutine(rotinaDoDialogo);
        }

        rotinaDoDialogo = StartCoroutine(FluxoDoDialogoAutomatico(fecharAutomatico));
    }

    IEnumerator FluxoDoDialogoAutomatico(bool fecharAutomatico)
    {
        while (indiceAtual < falasAtuais.Length)
        {
            txtNome.text = falasAtuais[indiceAtual].nome;
            txtFala.text = falasAtuais[indiceAtual].texto;

            // MÁGICA DO CONGELAMENTO: Se for a última frase e não for para fechar, paramos o relógio aqui!
            if (indiceAtual == falasAtuais.Length - 1 && !fecharAutomatico)
            {
                yield break; // Encerra a rotina de tempo silenciosamente, deixando o texto na tela para sempre
            }

            yield return new WaitForSeconds(tempoPorFala);
            indiceAtual++;
        }

        FinalizarDialogo();
    }

    void FinalizarDialogo()
    {
        painelDialogo.SetActive(false);
        BloquearJogador(false);

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
            // Opcional: Se quiser que ele ande na sala COM o aviso na tela, 
            // basta comentar a linha abaixo. 
            movScript.canMove = !bloquear;
        }
    }

    void ViajarParaMundo2D()
    {
        SceneManager.LoadScene("Primeira_Cidade");
    }
}