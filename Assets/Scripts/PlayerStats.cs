using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Nível e Progressão")]
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    [Header("Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Dano das Armas")]
    public int unarmedDamage = 1;
    public int woodenSwordDamage = 4;
    public int ironSwordDamage = 9;

    [Header("Habilidades")]
    public bool isAuraActive = false;
    public float auraDuration = 4f;
    public GameObject hudAura;

    [Header("Perks do Level Up")]
    public int bonusDamage = 0;
    public int auraVitalChance = 0;

    [Header("Sons de Vida")]
    public AudioClip somDano;
    public AudioClip somMorte;
    private AudioSource audioSource;

    [Header("Interface (UI)")]
    public LevelUpManager levelUpManager;

    [Header("Efeitos Visuais")]
    public GameObject efeitoFumaca; // Arraste o objeto FX_FumacaAura aqui
    public float atrasoDaTransformacao = 0.3f; // Tempo para a fumaça cobrir o boneco
    public float tempoParaSumirFumaca = 0.8f; // Tempo da animação da fumaça acabar

    [Header("Sons da Aura")]
    public AudioClip somAuraEntrada;
    public AudioClip somAuraSaida;

    public float auraCooldown = 30f; // O tempo total que a habilidade demora para voltar
    private float tempoParaProximaAura = 0f; // O relógio interno que trava o spam

    // Referências a outros scripts do Player
    private PlayerMovement playerMovement;
    private Animator anim;
    private PlayerCombat playerCombat; // ---> ADICIONADO PARA SALVAR A AGILIDADE

    void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>(); // ---> ADICIONADO
        audioSource = GetComponent<AudioSource>();

        // ---> MÁGICA AQUI: Puxa o save do disco na hora que o Kuro nasce!
        CarregarProgresso();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player tomou " + damage + " de dano! Vida atual: " + currentHealth);

        if (currentHealth > 0)
        {
            if (anim != null) anim.SetTrigger("Hurt");

            if (audioSource != null && somDano != null)
            {
                audioSource.PlayOneShot(somDano, 0.8f);
            }
        }
        else
        {
            Die();
        }
    }

    // A lógica real da Aura de Dano
    public void AtivarAura()
    {
        // Só ativa se a aura já não estiver rodando E se o tempo TOTAL de recarga já passou
        if (!isAuraActive && Time.time >= tempoParaProximaAura)
        {
            // O tempo bloqueado agora é: 4s (Duração) + 10s (Cooldown) = 14s totais!
            tempoParaProximaAura = Time.time + auraDuration + auraCooldown;
            StartCoroutine(RotinaAura());
        }
        else
        {
            Debug.Log("Aura em uso ou recarregando... Aguarde!");
        }
    }

    private IEnumerator RotinaAura()
    {
        // --- 1. ENTRADA DA AURA ---
        if (efeitoFumaca != null)
        {
            efeitoFumaca.SetActive(true);

            Animator animFumaca = efeitoFumaca.GetComponent<Animator>();
            if (animFumaca != null) animFumaca.Play("AnimFumaca");

            StartCoroutine(DesligarFumaca(0.8f));
        }

        // Toca o som de explosão/entrada da Aura
        if (audioSource != null && somAuraEntrada != null)
        {
            audioSource.PlayOneShot(somAuraEntrada, 0.4f); // 0.8f é o volume, pode ajustar!
        }

        // 2. A Pausa Dramática: Espera a nuvem cobrir (0.3s)
        yield return new WaitForSeconds(atrasoDaTransformacao);

        // 3. A TRANSFORMAÇÃO
        isAuraActive = true;
        if (playerMovement != null) playerMovement.AtualizarControllerDaArma();

        // 4. O tempo do poder rodando (4s)
        yield return new WaitForSeconds(auraDuration);

        // --- 2. SAÍDA DA AURA ---
        if (efeitoFumaca != null)
        {
            efeitoFumaca.SetActive(true);

            Animator animFumaca = efeitoFumaca.GetComponent<Animator>();
            if (animFumaca != null) animFumaca.Play("AnimFumacaSaindo");

            StartCoroutine(DesligarFumaca(0.8f));
        }

        // Toca o som da Aura se dissipando
        if (audioSource != null && somAuraSaida != null)
        {
            audioSource.PlayOneShot(somAuraSaida, 0.8f);
        }

        isAuraActive = false;
        if (playerMovement != null) playerMovement.AtualizarControllerDaArma();

        Debug.Log("Aura Finalizada.");
    }

    private IEnumerator DesligarFumaca(float tempo)
    {
        yield return new WaitForSeconds(tempo);

        // Só desativa o objeto se não houver um novo pedido de entrada da fumaça rodando
        if (efeitoFumaca != null) efeitoFumaca.SetActive(false);
    }

    public int GetCurrentDamage(bool ignorarAura = false)
    {
        int finalDamage = 0;
        bool hasIronSword = false;

        if (playerMovement != null)
        {
            if (playerMovement.currentWeapon == PlayerMovement.WeaponType.WoodenSword)
            {
                finalDamage = woodenSwordDamage;
            }
            else if (playerMovement.currentWeapon == PlayerMovement.WeaponType.IronSword)
            {
                finalDamage = ironSwordDamage;
                hasIronSword = true;
            }
            else
            {
                finalDamage = unarmedDamage;
            }
        }

        finalDamage += bonusDamage;

        // Regra da Aura: Se estiver ativa, dobra o dano final!
        if (isAuraActive && hasIronSword && !ignorarAura)
        {
            finalDamage *= 2;
        }

        return finalDamage;
    }

    public void VerificarCuraAuraVital()
    {
        if (auraVitalChance <= 0) return;

        int sorteio = Random.Range(1, 101);

        if (sorteio <= auraVitalChance)
        {
            currentHealth = maxHealth;
            Debug.Log("AURA VITAL ATIVADA! Vida restaurada para 100%!");
        }
    }

    public void GainExp(int expAmount)
    {
        currentExp += expAmount;
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel += 50;

        Debug.Log("LEVEL UP! Nível atual: " + level);

        if (levelUpManager != null)
        {
            levelUpManager.SimularLevelUp();
        }
    }

    private void Die()
    {
        Debug.Log("Player Morreu!");

        if (anim != null) anim.SetTrigger("Die");

        // O grito de morte normal do personagem
        if (somMorte != null)
        {
            Vector3 posicaoDoSom = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
            AudioSource.PlayClipAtPoint(somMorte, posicaoDoSom, 1f);
        }

        if (playerMovement != null) playerMovement.canMove = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Camera.main.GetComponent<CameraFollow>().IniciarZoomMorte(2.5f, 3.0f);

        // ESCONDER A INTERFACE (HUD)
        GameObject hud = GameObject.Find("UI_HUD");
        if (hud != null)
        {
            hud.SetActive(false); // Desliga a barra de vida, nível, etc.
        }

        // CALA ABSOLUTAMENTE TUDO NO JOGO
        GameOverManager goManager = Object.FindFirstObjectByType<GameOverManager>();
        AudioSource musicaGameOver = null;

        if (goManager != null)
        {
            musicaGameOver = goManager.GetComponent<AudioSource>();
        }

        // Passa o rodo buscando todos os AudioSources que existem na cena atual
        AudioSource[] todosOsSons = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource som in todosOsSons)
        {
            // Se o som estiver tocando E NÃO for o alto-falante do Game Over, manda parar na hora!
            if (som.isPlaying && som != musicaGameOver)
            {
                som.Stop();
            }
        }

        // Agora sim, revela a tela e toca a música de Game Over com 100% de prioridade
        if (goManager != null)
        {
            goManager.RevelarTela();

            if (musicaGameOver != null)
            {
                musicaGameOver.Play();
            }
        }
    }

    public void ResetarCooldownAura()
    {
        tempoParaProximaAura = 0f;
        isAuraActive = false;

        // Força o controlador de animação a voltar para o estado normal da espada de ferro
        if (playerMovement != null)
        {
            playerMovement.AtualizarControllerDaArma();
        }

        Debug.Log("Gasto narrativo limpo! Aura totalmente pronta para o jogador usar.");
    }

    // ==========================================
    // SISTEMA DE SAVE / LOAD ENTRE CENAS
    // ==========================================
    public void SalvarProgresso()
    {
        PlayerPrefs.SetInt("LevelKuro", level);
        PlayerPrefs.SetInt("ExpAtual", currentExp);
        PlayerPrefs.SetInt("ExpProxLevel", expToNextLevel);
        PlayerPrefs.SetInt("VidaMaxima", maxHealth);
        PlayerPrefs.SetInt("VidaAtual", currentHealth);

        // Salva os Perks
        PlayerPrefs.SetInt("BonusDano", bonusDamage);
        PlayerPrefs.SetInt("AuraVital", auraVitalChance);

        if (playerMovement != null)
        {
            PlayerPrefs.SetInt("ArmaEquipada", (int)playerMovement.currentWeapon);
        }

        // Salva a "Agilidade" (Cooldown de Ataque) do script de Combate
        if (playerCombat != null)
        {
            PlayerPrefs.SetFloat("AgilidadeAtaque", playerCombat.attackCooldown);
        }

        PlayerPrefs.Save();
        Debug.Log("Progresso e Perks salvos para a próxima cena!");
    }

    public void CarregarProgresso()
    {
        if (PlayerPrefs.HasKey("LevelKuro"))
        {
            level = PlayerPrefs.GetInt("LevelKuro");
            currentExp = PlayerPrefs.GetInt("ExpAtual");
            expToNextLevel = PlayerPrefs.GetInt("ExpProxLevel");
            maxHealth = PlayerPrefs.GetInt("VidaMaxima");
            currentHealth = PlayerPrefs.GetInt("VidaAtual");

            // Carrega os Perks
            bonusDamage = PlayerPrefs.GetInt("BonusDano");
            auraVitalChance = PlayerPrefs.GetInt("AuraVital");

            if (playerMovement != null)
            {
                playerMovement.currentWeapon = (PlayerMovement.WeaponType)PlayerPrefs.GetInt("ArmaEquipada");
                playerMovement.AtualizarControllerDaArma();
            }

            // Carrega a "Agilidade" de volta para o script de Combate
            // (O 0.5f ali no final é o valor padrão de segurança, caso não tenha save antigo)
            if (playerCombat != null)
            {
                playerCombat.attackCooldown = PlayerPrefs.GetFloat("AgilidadeAtaque", 0.5f);
            }

            Debug.Log("Progresso do Kuro Carregado! Nível atual: " + level);
        }
    }

    // Ferramenta de Teste: Aperte F12 no Editor para "limpar o cartão de memória"
#if UNITY_EDITOR
    void Update()
    {
        // ---> CORREÇÃO: Usando o Novo Input System para a tecla F12
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.f12Key.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("DELETADO: Todos os saves apagados! Voltou ao Nível 1.");
        }
    }
#endif
}