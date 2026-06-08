using UnityEngine;
using UnityEngine.Rendering; // ---> NOVO: Biblioteca da URP!

public class AuraPostProcessing : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Volume volumeAura; // ---> NOVO: Tipo correto da URP!
    private PlayerStats playerStats;

    [Header("Configurações")]
    [SerializeField] private float velocidadeTransicao = 5f;

    private float pesoAlvo = 0f;

    void Start()
    {
        // Pega o componente Volume novo que você acabou de adicionar
        if (volumeAura == null) volumeAura = GetComponent<Volume>();
        if (volumeAura != null) volumeAura.weight = 0f;

        playerStats = Object.FindFirstObjectByType<PlayerStats>();
    }

    void Update()
    {
        if (playerStats == null || volumeAura == null) return;

        pesoAlvo = playerStats.isAuraActive ? 1f : 0f;

        // Faz a transição do Weight de 0 a 1 suavemente (A mágica continua a mesma!)
        volumeAura.weight = Mathf.MoveTowards(volumeAura.weight, pesoAlvo, velocidadeTransicao * Time.unscaledDeltaTime);
    }
}