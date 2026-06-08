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

    [Tooltip("Falas automáticas ao chegar PERTO do computador")]
    public LinhaDoDialogo[] falasCena2LabAproximacao; // "...O que é isso?"

    [Tooltip("Falas ao INTERAGIR apertando E no computador")]
    public LinhaDoDialogo[] falasCena2LabInteracao;   // "Espera— o que está acontecendo?!"

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
        IniciarSequencia(falasCena1Universidade, false);
    }

    public void IniciarSequencia(LinhaDoDialogo[] novasFalas, bool ehCena2)
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

        rotinaDoDialogo = StartCoroutine(FluxoDoDialogoAutomatico());
    }

    IEnumerator FluxoDoDialogoAutomatico()
    {
        while (indiceAtual < falasAtuais.Length)
        {
            txtNome.text = falasAtuais[indiceAtual].nome;
            txtFala.text = falasAtuais[indiceAtual].texto;

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