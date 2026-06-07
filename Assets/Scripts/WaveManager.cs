using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Configurações de Nascimento")]
    public GameObject slimePrefab;
    public int quantidadeSlimes = 10;
    public float raioNascimento = 5f;

    // ---> NOVO: Controlador de Slimes Vivas
    private int slimesVivas;

    void Start()
    {
        GerarSlimesIniciais();
    }

    void GerarSlimesIniciais()
    {
        slimesVivas = quantidadeSlimes; // Define o total inicial

        for (int i = 0; i < quantidadeSlimes; i++)
        {
            Vector2 posicaoSorteada = (Vector2)transform.position + Random.insideUnitCircle * raioNascimento;
            Instantiate(slimePrefab, posicaoSorteada, Quaternion.identity);
        }
    }

    // ---> NOVO: Função que o script de vida da Slime vai chamar ao morrer
    public void RegistrarMorteSlime()
    {
        slimesVivas--;
        Debug.Log("Slime derrotada! Restam: " + slimesVivas);
    }

    // ---> NOVO: Função que o Gatilho do Orc vai consultar
    public bool TodasAsSlimesMorreram()
    {
        return slimesVivas <= 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, raioNascimento);
    }
}