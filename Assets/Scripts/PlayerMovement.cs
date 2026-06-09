using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Necessário para a Corotina de sumir o fantasma

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    private SpriteRenderer spriteRenderer;

    public bool canMove = true;

    // ---> NOVO: Variável de Trava Cinematográfica
    [Header("Configurações de Cena")]
    public bool isCinematicScene = false;

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

    [Header("Efeito Fantasma (Aura)")]
    public float tempoEntreFantasmas = 0.08f;
    public float tempoDeVidaFantasma = 0.4f;
    public Color corDoFantasma = new Color(1f, 0.84f, 0f, 0.6f);
    private float timerFantasma = 0f;

    public enum WeaponType { Unarmed, WoodenSword, IronSword }

    [Header("Estado Atual")]
    public WeaponType currentWeapon = WeaponType.Unarmed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

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

            // --- LÓGICA DO RASTRO DA AURA ---
            if (GetComponent<PlayerStats>() != null && GetComponent<PlayerStats>().isAuraActive)
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
            timerFantasma = 0f;
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            Debug.Log("O movimento foi bloqueado porque canMove está FALSO!");
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
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

        // Se for cena cinematográfica, trava o ataque na raiz
        if (isCinematicScene) return;

        if (context.started)
        {
            if (currentWeapon != WeaponType.Unarmed)
            {
                PlayerCombat combatStats = GetComponent<PlayerCombat>();
                if (combatStats != null && combatStats.PodeAtacar())
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

                    combatStats.PerformMeleeAttack();
                }
            }
        }
    }

    public void AtualizarControllerDaArma()
    {
        if (animator == null) return;

        // ---> NOVO: Trava de Segurança
        // Se for uma cena de história (Cena 5), não tente forçar uma arma na mão do jogador!
        if (isCinematicScene) return;

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

        sr.sortingLayerID = spriteRenderer.sortingLayerID;
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
            float alphaAtual = Mathf.Lerp(corInicial.a, 0f, tempo / tempoDeVidaFantasma);

            sr.color = new Color(corInicial.r, corInicial.g, corInicial.b, alphaAtual);
            yield return null;
        }

        Destroy(sr.gameObject);
    }
}