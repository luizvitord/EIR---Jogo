using UnityEngine;
using UnityEngine.Rendering; // <--- NOVO: Biblioteca correta da URP!

public class LowHealthAudio : MonoBehaviour
{
    [Header("Referências")]
    public PlayerStats playerStats;
    public AudioClip somCoracao;
    public Volume volumeBaixaVida; // <--- NOVO: Tipo correto do componente da URP!

    [Header("Configurações")]
    [Range(0f, 1f)]
    public float porcentagemBaixaVida = 0.25f;
    public float velocidadeTransicaoAudio = 5000f;
    public float velocidadeTransicaoVisual = 4f; // Quão rápido a tela distorce

    private AudioSource heartbeatSource;
    private AudioLowPassFilter lowPassFilter;

    private float frequenciaNormal = 22000f;
    private float frequenciaAbafada = 1200f;

    void Start()
    {
        if (playerStats == null) playerStats = Object.FindFirstObjectByType<PlayerStats>();

        heartbeatSource = gameObject.AddComponent<AudioSource>();
        heartbeatSource.clip = somCoracao;
        heartbeatSource.loop = true;
        heartbeatSource.volume = 1.2f;
        heartbeatSource.bypassEffects = true;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            lowPassFilter = mainCam.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = mainCam.gameObject.AddComponent<AudioLowPassFilter>();
            }
            lowPassFilter.cutoffFrequency = frequenciaNormal;
        }

        // Garante que o efeito visual comece 100% invisível
        if (volumeBaixaVida != null) volumeBaixaVida.weight = 0f;
    }

    void Update()
    {
        if (playerStats == null || lowPassFilter == null) return;

        float porcentagemVidaAtual = (float)playerStats.currentHealth / playerStats.maxHealth;
        bool vidaBaixa = porcentagemVidaAtual <= porcentagemBaixaVida && playerStats.currentHealth > 0;

        if (vidaBaixa)
        {
            // --- 1. IMPACTO DE ÁUDIO ---
            lowPassFilter.cutoffFrequency = Mathf.MoveTowards(lowPassFilter.cutoffFrequency, frequenciaAbafada, velocidadeTransicaoAudio * Time.deltaTime);

            if (!heartbeatSource.isPlaying) heartbeatSource.Play();

            // --- 2. IMPACTO VISUAL (NOVO) ---
            if (volumeBaixaVida != null)
            {
                // Desliza o peso do efeito visual em direção a 1 (100% ativo)
                volumeBaixaVida.weight = Mathf.MoveTowards(volumeBaixaVida.weight, 1f, velocidadeTransicaoVisual * Time.deltaTime);
            }
        }
        else
        {
            // --- REVERTER ÁUDIO ---
            lowPassFilter.cutoffFrequency = Mathf.MoveTowards(lowPassFilter.cutoffFrequency, frequenciaNormal, (velocidadeTransicaoAudio * 2f) * Time.deltaTime);

            if (heartbeatSource.isPlaying && lowPassFilter.cutoffFrequency > 20000f)
            {
                heartbeatSource.Stop();
            }

            // --- REVERTER VISUAL (NOVO) ---
            if (volumeBaixaVida != null)
            {
                // Desliza o peso de volta para 0 (invisível) caso se cure ou morra
                volumeBaixaVida.weight = Mathf.MoveTowards(volumeBaixaVida.weight, 0f, (velocidadeTransicaoVisual * 2f) * Time.deltaTime);
            }
        }
    }
}