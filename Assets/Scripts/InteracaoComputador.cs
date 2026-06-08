using UnityEngine;
using UnityEngine.SceneManagement;

public class InteracaoComputador : MonoBehaviour
{
    private Transform jogador;
    public float distanciaInteracao = 3.0f; // Distância mínima para interagir

    void Start()
    {
        // Encontra o jogador automaticamente pela Tag
        GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
        if (objJogador != null)
        {
            jogador = objJogador.transform;
        }
    }

    void Update()
    {
        // Se o jogador existir, verifica a distância entre ele e o computador
        if (jogador != null)
        {
            float distancia = Vector3.Distance(transform.position, jogador.position);

            // Se estiver perto o suficiente e apertar Espaço ou E
            if (distancia <= distanciaInteracao && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            {
                AcessarComputador();
            }
        }
    }

    public void AcessarComputador()
    {
        if (DialogueManagerBasico.Instance != null)
        {
            DialogueManagerBasico.Instance.IniciarSequencia(DialogueManagerBasico.Instance.falasCena2Laboratorio, true);
        }
        else
        {
            Debug.LogWarning("DialogueManagerBasico não encontrado! Pulando o diálogo.");
            ViajarParaMundo2D();
        }
    }

    private void ViajarParaMundo2D()
    {
        GameObject jogador3D = GameObject.FindGameObjectWithTag("Player");
        if (jogador3D != null) Destroy(jogador3D);

        SceneManager.LoadScene("Primeira_Cidade");
    }
}