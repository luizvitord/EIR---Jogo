using UnityEngine;

public class InteracaoComputador : MonoBehaviour
{
    private Transform jogador;

    [Header("Configuração de Interação")]
    public float distanciaInteracao = 2.5f;

    private bool jaDisparouAproximacao = false;
    private bool jaDisparouInteracao = false;

    void Start()
    {
        GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
        if (objJogador != null)
        {
            jogador = objJogador.transform;
        }
    }

    void Update()
    {
        if (jogador == null) return;

        float distancia = Vector3.Distance(transform.position, jogador.position);

        if (distancia <= distanciaInteracao && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (jaDisparouAproximacao && !jaDisparouInteracao)
            {
                jaDisparouInteracao = true;
                AcessarComputador();
            }
        }
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player") && !jaDisparouAproximacao)
        {
            jaDisparouAproximacao = true;

            if (DialogueManagerBasico.Instance != null)
            {
                // Aqui passamos o 'false' no final. Ele vai ler as frases e congelar na última!
                DialogueManagerBasico.Instance.IniciarSequencia(DialogueManagerBasico.Instance.falasCena2LabAproximacao, false, false);
            }
        }
    }

    public void AcessarComputador()
    {
        if (DialogueManagerBasico.Instance != null)
        {
            // Aqui passamos 'true' no final. Ele vai ler a frase de susto, esperar o tempo, fechar e viajar.
            DialogueManagerBasico.Instance.IniciarSequencia(DialogueManagerBasico.Instance.falasCena2LabInteracao, true, true);
        }
    }
}