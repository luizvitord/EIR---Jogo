using UnityEngine;
using System.Collections;

public class OrcSpawnTrigger : MonoBehaviour
{
    [Header("Referências do Mapa")]
    public WaveManager waveManager;
    public CameraFollow cameraFollow;
    public Transform pontoDaFloresta; // Objeto vazio onde a câmera vai focar
    public GameObject orcPrefab;      // Prefab do Orc MiniBoss
    public Transform pontoSpawnOrc;   // Onde o Orc vai surgir fisicamente
    public GameObject painelHUD;

    [Header("Áudios")]
    public AudioSource audioSourceGeral; // Um AudioSource para sons de impacto
    public AudioSource musicaAmbiente;   // O AudioSource que está tocando a música atual da floresta
    public AudioClip somPisadaPesada;
    public AudioClip musicaBoss;

    [Header("Configurações da Cena")]
    public float zoomDaCutscene = 3.5f;
    public float zoomNormal = 5.0f;

    private bool cutsceneDisparada = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se for o Player, se a cutscene não rodou, e se as 10 slimes morreram...
        if (collision.CompareTag("Player") && !cutsceneDisparada)
        {
            if (waveManager != null && waveManager.TodasAsSlimesMorreram())
            {
                cutsceneDisparada = true;
                StartCoroutine(SequenciaCutscene(collision.gameObject));
            }
            else
            {
                Debug.Log("Você sentiu um calafrio... mas a floresta ainda está infestada de Slimes.");
            }
        }
    }

    private IEnumerator SequenciaCutscene(GameObject player)
    {
        // 1. TRAVA O JOGADOR E ESCONDE O HUD
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        Animator playerAnim = player.GetComponent<Animator>();

        if (playerMovement != null) playerMovement.canMove = false;
        if (playerAnim != null) playerAnim.SetBool("isWalking", false);

        if (painelHUD != null) painelHUD.SetActive(false); // Esconde a Vida/XP

        // Parar música ambiente suavemente
        if (musicaAmbiente != null) StartCoroutine(FadeOutMusica(musicaAmbiente, 1.5f));

        yield return new WaitForSeconds(0.5f);

        // 2. MOVE A CÂMERA E DÁ ZOOM
        Transform playerOriginalTarget = cameraFollow.target;
        cameraFollow.target = pontoDaFloresta;
        cameraFollow.MudarZoomSmooth(zoomDaCutscene, 2.0f);

        yield return new WaitForSeconds(2.5f);

        // 3. O ÁUDIO E OS TREMORES (Em paralelo)
        if (audioSourceGeral != null && somPisadaPesada != null)
        {
            audioSourceGeral.PlayOneShot(somPisadaPesada, 1.0f); // Toca o áudio UMA única vez
        }

        // Inicia os tremores em segundo plano, sem travar o código!
        StartCoroutine(RotinaDeTremores());

        // Criamos a variável fora do bloco para poder usá-la no passo 7
        GameObject orcInstance = null;

        // 4. SPAWN DO ORC E CAMINHADA (Sincronizado)
        if (orcPrefab != null && pontoSpawnOrc != null)
        {
            orcInstance = Instantiate(orcPrefab, pontoSpawnOrc.position, Quaternion.identity);

            // ---> CORREÇÃO: Desliga a inteligência de caça temporariamente <---
            OrcAI inteligenciaOrc = orcInstance.GetComponent<OrcAI>();
            if (inteligenciaOrc != null) inteligenciaOrc.enabled = false;

            Animator orcAnim = orcInstance.GetComponent<Animator>();

            Vector2 destino = pontoDaFloresta.position;
            Vector2 direcao = (destino - (Vector2)orcInstance.transform.position).normalized;

            if (orcAnim != null)
            {
                orcAnim.SetBool("isWalking", true);
                orcAnim.SetFloat("InputX", direcao.x);
                orcAnim.SetFloat("InputY", direcao.y);
                orcAnim.SetFloat("LastInputX", direcao.x);
                orcAnim.SetFloat("LastInputY", direcao.y);
            }

            // O tempo que você testou e achou perfeito para sincronizar com o som
            float duracaoDoAudio = 4.3f;

            // Calculamos a velocidade necessária (Velocidade = Distância / Tempo)
            float velocidadeSincronizada = Vector2.Distance(orcInstance.transform.position, destino) / duracaoDoAudio;

            // O Orc vai andar até chegar, e o tempo que ele vai levar será exatamente o do áudio!
            while (Vector2.Distance(orcInstance.transform.position, destino) > 0.1f)
            {
                orcInstance.transform.position = Vector2.MoveTowards(orcInstance.transform.position, destino, velocidadeSincronizada * Time.deltaTime);
                yield return null;
            }

            // Chegou no centro perfeitamente com o fim do som!
            if (orcAnim != null)
            {
                orcAnim.SetBool("isWalking", false);
                orcAnim.SetFloat("LastInputX", 0f);
            }
        }

        // Espera o grito/ataque de introdução dele terminar
        yield return new WaitForSeconds(1.5f);

        // 5. VOLTA A CÂMERA PARA O JOGADOR DE FORMA LENTA E DRAMÁTICA
        cameraFollow.target = playerOriginalTarget;

        // Salva a velocidade normal da câmera e deixa ela mais "preguiçosa" temporariamente
        float velocidadeCameraOriginal = cameraFollow.smoothSpeed;
        cameraFollow.smoothSpeed = 1.5f; // Quanto menor esse número, mais devagar ela desliza

        // Aumentamos o tempo do Zoom de 1.5f para 3.5f (demora mais para afastar)
        float tempoDeVolta = 3.5f;
        cameraFollow.MudarZoomSmooth(zoomNormal, tempoDeVolta);

        // Espera esse novo tempo maior acabar
        yield return new WaitForSeconds(tempoDeVolta);

        // Devolve a velocidade rápida da câmera para a hora do combate!
        cameraFollow.smoothSpeed = velocidadeCameraOriginal;

        // 6. INICIA A TRILHA SONORA DO BOSS E DEVOLVE O HUD
        if (audioSourceGeral != null && musicaBoss != null)
        {
            audioSourceGeral.clip = musicaBoss;
            audioSourceGeral.loop = true;
            audioSourceGeral.volume = 0.5f;
            audioSourceGeral.Play();
        }

        if (painelHUD != null) painelHUD.SetActive(true); // Traz a interface de volta

        // 7. LIBERA O PLAYER PARA INTERAGIR E RELIGA O ORC
        if (playerMovement != null) playerMovement.canMove = true;

        // ---> CORREÇÃO: Ativa o cérebro do Orc para iniciar o combate <---
        if (orcInstance != null)
        {
            OrcAI inteligenciaOrc = orcInstance.GetComponent<OrcAI>();
            if (inteligenciaOrc != null) inteligenciaOrc.enabled = true;

            BossHealthBar bossBar = Object.FindAnyObjectByType<BossHealthBar>(FindObjectsInactive.Include);
        
        if (bossBar != null)
        {
            EnemyHealth vidaDoOrc = orcInstance.GetComponent<EnemyHealth>();
            bossBar.AtivarBossBar(vidaDoOrc);
        }
        }

        Debug.Log("QUE COMECEM OS JOGOS! Defenda-se do Orc!");

        // Desliga apenas a caixa de colisão do gatilho para a cena não repetir, 
        // mas mantém o objeto vivo para a música do Boss continuar tocando!
        GetComponent<Collider2D>().enabled = false;
    }

    // Uma corotina separada só para tremer a tela em paralelo
    private IEnumerator RotinaDeTremores()
    {
        // Pisada 1
        cameraFollow.TriggerShake(0.3f, 0.4f);
        yield return new WaitForSeconds(0.8f);

        // Pisada 2
        cameraFollow.TriggerShake(0.3f, 0.4f);
        yield return new WaitForSeconds(0.9f);

        // Pisada 3
        cameraFollow.TriggerShake(0.3f, 0.4f);
        yield return new WaitForSeconds(0.9f);

        // Pisada 4
        cameraFollow.TriggerShake(0.3f, 0.4f);
        yield return new WaitForSeconds(0.9f);

        // Pisada 5 (A última antes de aparecer)
        cameraFollow.TriggerShake(0.4f, 0.6f);
    }

    // Função utilitária para abaixar o som da floresta devagar
    private IEnumerator FadeOutMusica(AudioSource source, float tempo)
    {
        float volumeInicial = source.volume;
        while (source.volume > 0)
        {
            source.volume -= volumeInicial * Time.deltaTime / tempo;
            yield return null;
        }
        source.Stop();
    }
}