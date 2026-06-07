using UnityEngine;
using UnityEngine.Rendering.PostProcessing; // ---> NOVO: Biblioteca correta!

public class AuraPostProcessing : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PostProcessVolume volumeAura; // ---> NOVO: Tipo correto!
    private PlayerStats playerStats;

    [Header("Configurações")]
    [SerializeField] private float velocidadeTransicao = 5f;

    private float pesoAlvo = 0f;

    void Start()
    {
        if (volumeAura == null) volumeAura = GetComponent<PostProcessVolume>();
        if (volumeAura != null) volumeAura.weight = 0f;

        playerStats = Object.FindFirstObjectByType<PlayerStats>();
    }

    void Update()
    {
        if (playerStats == null || volumeAura == null) return;

        pesoAlvo = playerStats.isAuraActive ? 1f : 0f;

        // Faz a transição do Weight de 0 a 1 suavemente
        volumeAura.weight = Mathf.MoveTowards(volumeAura.weight, pesoAlvo, velocidadeTransicao * Time.unscaledDeltaTime);
    }
}