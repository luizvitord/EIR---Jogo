using UnityEngine;
using UnityEngine.SceneManagement; // Obrigatório para gerenciamento de cenas

public class InteracaoComputador : MonoBehaviour
{
    // Variável interna para saber se o jogador está na área
    private bool estaPertoDoComputador = false;

    // Detecta quando algo entra na área do computador
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem entrou tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            estaPertoDoComputador = true;
            Debug.Log("Jogador perto do computador. Aperte ESPAÇO para interagir.");
        }
    }

    // Detecta quando o jogador se afasta do computador
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            estaPertoDoComputador = false;
            Debug.Log("Jogador se afastou do computador.");
        }
    }

    // O Update roda a cada frame do jogo
    private void Update()
    {
        // Se o jogador estiver perto E apertar a tecla Espaço
        if (estaPertoDoComputador && Input.GetKeyDown(KeyCode.Space))
        {
            AcessarComputador();
        }
    }

    private void AcessarComputador()
    {
        Debug.Log("Carregando o mundo 2D...");
        
        // Carrega a cena usando o nome exato (sem o ".unity" no final)
        SceneManager.LoadScene("Primeira_Cidade");
    }
}