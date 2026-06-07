using System.Collections;
using UnityEngine;

public class LuzTerror : MonoBehaviour
{
    private Light luzDaLampada;

    // Configurações para controlar a "loucura" das piscadas
    [Header("Configurações do Flicker (Terror)")]
    [Tooltip("Tempo mínimo que a luz fica acesa ou apagada entre piscadas rápidas.")]
    public float tempoMinimoFlicker = 0.01f; // Curtíssimo (1 centésimo de segundo)

    [Tooltip("Tempo máximo que a luz fica acesa ou apagada entre piscadas rápidas.")]
    public float tempoMaximoFlicker = 0.15f; // Rápido (15 centésimos de segundo)

    [Tooltip("A 'Duração do Surto': Quantos segundos durará a sequência de piscadas rápidas antes de uma pausa?")]
    public float duracaoDoSurto = 1.5f;

    [Header("Configurações da Pausa (Tensão)")]
    [Tooltip("Tempo mínimo que a luz ficará em um estado fixo (geralmente acesa) após um surto.")]
    public float pausaMinimaPosSurto = 1f;

    [Tooltip("Tempo máximo que a luz ficará em um estado fixo após um surto.")]
    public float pausaMaximaPosSurto = 4f;

    void Start()
    {
        // Captura o componente Light (garanta que este script esteja no Point Light)
        luzDaLampada = GetComponent<Light>();

        // Inicia a Coroutine que vai rodar o loop caótico
        StartCoroutine(LogicaLuzTerror());
    }

    IEnumerator LogicaLuzTerror()
    {
        // Loop infinito enquanto o objeto estiver ativo na cena
        while (true)
        {
            // --- FASE 1: O SURTO (Piscadas Caóticas e Rápidas) ---
            float cronometroSurto = 0;

            // Enquanto não passar a duração do surto (ex: 1.5s), fica piscando feito louca
            while (cronometroSurto < duracaoDoSurto)
            {
                // Inverte o estado da luz (se tá ligada, desliga; se tá desligada, liga)
                luzDaLampada.enabled = !luzDaLampada.enabled;

                // Escolhe um tempo aleatório curtíssimo para o próximo estado
                float tempoAleatorio = Random.Range(tempoMinimoFlicker, tempoMaximoFlicker);

                // Soma esse tempo ao cronômetro do surto
                cronometroSurto += tempoAleatorio;

                // Espera esse tempo curtíssimo
                yield return new WaitForSeconds(tempoAleatorio);
            }

            // --- FASE 2: O SUSPENSE (Pausa com Luz Acesa) ---
            // Garante que a luz termine o surto acesa para o jogador conseguir ver (ou crie tensão se ficar apagada)
            luzDaLampada.enabled = true; // Force True para ser acesa, False para terminar apagada.

            // Espera um tempo aleatório maior antes de surtar de novo (para não ficar monótono)
            float tempoDePausa = Random.Range(pausaMinimaPosSurto, pausaMaximaPosSurto);
            yield return new WaitForSeconds(tempoDePausa);
        }
    }
}