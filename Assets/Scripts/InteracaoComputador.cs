using UnityEngine;

public class InteracaoComputador : MonoBehaviour
{
    private Transform jogador;

    [Header("Configuração de Interação")]
    [Tooltip("Distância para conseguir apertar E e tocar na tela")]
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

        // FASE 2: Chegou colado no PC e apertou E -> Susto e Teletransporte
        float distancia = Vector3.Distance(transform.position, jogador.position);

        if (distancia <= distanciaInteracao && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            // Só deixa interagir se ele já passou pelo pensamento da porta
            if (jaDisparouAproximacao && !jaDisparouInteracao)
            {
                jaDisparouInteracao = true; // Trava de segurança
                AcessarComputador();
            }
        }
    }

    // FASE 1: O jogador pisou DENTRO do Box Collider da sala
    private void OnTriggerEnter(Collider outro)
    {
        Debug.Log("Entrou na sala");
        // Verifica se quem pisou foi o Player e se o diálogo já não rodou
        if (outro.CompareTag("Player") && !jaDisparouAproximacao)
        {
            Debug.Log("Entrou no IF");
            jaDisparouAproximacao = true; // Trava para não disparar de novo

            if (DialogueManagerBasico.Instance != null)
            {
                DialogueManagerBasico.Instance.IniciarSequencia(DialogueManagerBasico.Instance.falasCena2LabAproximacao, false);
            }
        }
    }

    public void AcessarComputador()
    {
        if (DialogueManagerBasico.Instance != null)
        {
            DialogueManagerBasico.Instance.IniciarSequencia(DialogueManagerBasico.Instance.falasCena2LabInteracao, true);
        }
    }
}