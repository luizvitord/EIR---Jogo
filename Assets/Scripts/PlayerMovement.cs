using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Necessário para a Corotina de sumir o fantasma

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    // ---> NOVO: Precisamos saber qual é a imagem atual do personagem
    private SpriteRenderer spriteRenderer;

    public bool canMove = true;

    [Header("Controladores de Animação")]
    [SerializeField] private RuntimeAnimatorController baseUnarmedController;
    [SerializeField] private AnimatorOverrideController woodenSwordController;
    [SerializeField] private AnimatorOverrideController ironSwordController;
    [SerializeField] private AnimatorOverrideController ironSwordAuraController;

    [Header("Sons de Passo")]
    public AudioClip somPassoGrama;
    public float intervaloPassos = 0.35f;
    private float timerPasso = 0f;
    private AudioSource audioSource;

    [Header("Sons de Ataque")]
    public AudioClip somAtaqueMadeira;
    public AudioClip somAtaqueFerro;

    // ---> NOVO: Configurações do Rastro da Aura <---
    [Header("Efeito Fantasma (Aura)")]
    public float tempoEntreFantasmas = 0.08f; // Quão rápido os clones surgem
    public float tempoDeVidaFantasma = 0.4f;  // Quanto tempo eles demoram pra sumir
    public Color corDoFantasma = new Color(1f, 0.84f, 0f, 0.6f); // Dourado com leve transparência
    private float timerFantasma = 0f;

    public enum WeaponType { Unarmed, WoodenSword, IronSword }

    [Header("Estado Atual")]
    public WeaponType currentWeapon = WeaponType.Unarmed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Pega a referência do renderizador de imagem do seu personagem
        spriteRenderer = GetComponent<SpriteRenderer>();

        AtualizarControllerDaArma();
    }

    void OnValidate()
    {
        if (animator == null) animator = GetComponent<Animator>();
        AtualizarControllerDaArma();
    }

    void Update()
    {
        if (canMove && moveInput.magnitude > 0.1f)
        {
            // --- LÓGICA DE PASSOS ---
            timerPasso -= Time.deltaTime;

            if (timerPasso <= 0f)
            {
                if (audioSource != null && somPassoGrama != null)
                {
                    audioSource.PlayOneShot(somPassoGrama, 0.3f);
                }
                timerPasso = intervaloPassos;
            }

            // ---> NOVO: LÓGICA DO RASTRO DA AURA <---
            if (GetComponent<PlayerStats>().isAuraActive)
            {
                timerFantasma -= Time.deltaTime;
                if (timerFantasma <= 0f)
                {
                    GerarFantasma();
                    timerFantasma = tempoEntreFantasmas;
                }
            }
        }
        else
        {
            timerPasso = 0f;
            // Zera o timer do fantasma para que, ao voltar a andar, crie um na hora
            timerFantasma = 0f;
        }
    }

    void FixedUpdate()
    {
        Debug.Log("O movimento foi bloqueado porque canMove está FALSO!");
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        Debug.Log("O Input System enviou o comando! Valor do teclado: " + context.ReadValue<Vector2>());
        if (!canMove)
        {
            moveInput = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }

        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (moveInput.x != 0 || moveInput.y != 0)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        if (context.started)
        {
            if (currentWeapon != WeaponType.Unarmed)
            {
                if (GetComponent<PlayerCombat>().PodeAtacar())
                {
                    animator.SetTrigger("Attack");

                    if (audioSource != null)
                    {
                        if (currentWeapon == WeaponType.WoodenSword && somAtaqueMadeira != null)
                        {
                            audioSource.PlayOneShot(somAtaqueMadeira, 0.7f);
                        }
                        else if (currentWeapon == WeaponType.IronSword && somAtaqueFerro != null)
                        {
                            audioSource.PlayOneShot(somAtaqueFerro, 0.9f);
                        }
                    }

                    GetComponent<PlayerCombat>().PerformMeleeAttack();
                }
            }
        }
    }

    public void AtualizarControllerDaArma()
    {
        if (animator == null) return;

        switch (currentWeapon)
        {
            case WeaponType.WoodenSword:
                if (woodenSwordController != null) animator.runtimeAnimatorController = woodenSwordController;
                break;

            case WeaponType.IronSword:
                if (GetComponent<PlayerStats>().isAuraActive && ironSwordAuraController != null)
                {
                    animator.runtimeAnimatorController = ironSwordAuraController;
                }
                else if (ironSwordController != null)
                {
                    animator.runtimeAnimatorController = ironSwordController;
                }
                break;

            case WeaponType.Unarmed:
                if (baseUnarmedController != null) animator.runtimeAnimatorController = baseUnarmedController;
                break;
        }
    }

    public void EquipWoodenSword()
    {
        currentWeapon = WeaponType.WoodenSword;
        AtualizarControllerDaArma();
    }

    public void EquipIronSword()
    {
        currentWeapon = WeaponType.IronSword;
        AtualizarControllerDaArma();
    }

    public void Desarmar()
    {
        currentWeapon = WeaponType.Unarmed;
        AtualizarControllerDaArma();
    }

    // ==========================================
    // SISTEMA DE CRIAÇÃO E FADE DO FANTASMA
    // ==========================================
    private void GerarFantasma()
    {
        GameObject fantasma = new GameObject("FantasmaAura");

        fantasma.transform.position = transform.position;
        fantasma.transform.localScale = transform.localScale;

        SpriteRenderer sr = fantasma.AddComponent<SpriteRenderer>();
        sr.sprite = spriteRenderer.sprite;
        sr.color = corDoFantasma;

        // ---> CORREÇÃO AQUI <---
        // Força o fantasma a aparecer usando o mesmo Sorting Layer do jogador
        sr.sortingLayerID = spriteRenderer.sortingLayerID;

        // Em vez de colocar -1 (que pode afundar no mapa), colocamos o fantasma
        // exatamente na MESMA camada do jogador. Como ele tem transparência,
        // o efeito visual continua perfeito e não entra debaixo da grama!
        sr.sortingOrder = spriteRenderer.sortingOrder;

        StartCoroutine(FadeFantasma(sr));
    }

    private IEnumerator FadeFantasma(SpriteRenderer sr)
    {
        float tempo = 0f;
        Color corInicial = sr.color;

        while (tempo < tempoDeVidaFantasma)
        {
            tempo += Time.deltaTime;
            // Vai reduzindo o Alpha de 0.6 para 0 suavemente
            float alphaAtual = Mathf.Lerp(corInicial.a, 0f, tempo / tempoDeVidaFantasma);

            sr.color = new Color(corInicial.r, corInicial.g, corInicial.b, alphaAtual);
            yield return null;
        }

        // Quando ficar 100% invisível, destrói o objeto fantasma
        Destroy(sr.gameObject);
    }
}