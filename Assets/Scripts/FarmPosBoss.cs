using System.Collections;
using UnityEngine;

public class FarmPosBoss : MonoBehaviour
{
    [Header("Configurações do Farm")]
    public GameObject slimePrefab;
    public float tempoEntreSpawns = 5f; // Nasce um a cada 5 segundos

    [Header("Locais de Nascimento")]
    public Transform[] pontosDeSpawn; // Uma lista de lugares onde eles podem nascer

    private bool farmAtivo = false;

    public void IniciarFarm()
    {
        // Garante que a rotina só inicie uma vez
        if (!farmAtivo)
        {
            farmAtivo = true;
            StartCoroutine(RotinaDeSpawns());
            Debug.Log("Fase de Farm iniciada! Slimes surgindo...");
        }
    }

    private IEnumerator RotinaDeSpawns()
    {
        // O loop "while(true)" com o farmAtivo faz com que isso se repita infinitamente
        // até o jogador decidir sair da fase (mudar de cena)
        while (farmAtivo)
        {
            // Espera o intervalo de tempo antes de nascer o próximo
            yield return new WaitForSeconds(tempoEntreSpawns);

            // Sorteia um dos pontos da sua lista
            if (pontosDeSpawn.Length > 0 && slimePrefab != null)
            {
                int indexAleatorio = Random.Range(0, pontosDeSpawn.Length);
                Transform pontoEscolhido = pontosDeSpawn[indexAleatorio];

                // Spawna o Slime no local sorteado!
                Instantiate(slimePrefab, pontoEscolhido.position, Quaternion.identity);
            }
        }
    }
}