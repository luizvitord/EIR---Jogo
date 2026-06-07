using UnityEngine;
using UnityEngine.InputSystem;

public enum STATE
{
    DISABLED,
    WAITING,
    TYPING,
    CLOSING
}

public class DialogueSystem : MonoBehaviour
{
    public DialogueData dialogueData;

    int currentText = 0;
    bool finished = false;

    TypeTextAnimation typeText;
    DialogueUI dialogueUI;

    STATE state;

    public System.Action onDialogueComplete;

    [Header("Integração Visual")]
    public GameObject painelHUD; // O painel com o rosto e a vida do jogador

    void Awake()
    {
        typeText = FindObjectOfType<TypeTextAnimation>();
        dialogueUI = FindObjectOfType<DialogueUI>();

        typeText.TypeFinished = OnTypeFinishe;

        // MÁGICA: Acha o Painel_Jogador sozinho procurando pelo script HUDManager!
        if (painelHUD == null)
        {
            HUDManager hud = FindObjectOfType<HUDManager>();
            if (hud != null)
            {
                painelHUD = hud.gameObject;
            }
        }
    }

    void Start()
    {
        state = STATE.DISABLED;
    }

    void Update()
    {
        if (state == STATE.DISABLED || state == STATE.CLOSING) return;

        switch (state)
        {
            case STATE.WAITING:
                Waiting();
                break;
            case STATE.TYPING:
                Typing();
                break;
        }
    }

    public void StartDialogue(DialogueData data)
    {
        Debug.Log("🚨 ALGUÉM INICIOU UM DIÁLOGO INVISÍVEL! O jogador foi congelado.");
        if (state != STATE.DISABLED) return;

        dialogueData = data;
        currentText = 0;
        finished = false;

        // 1. ESCONDE O HUD PARA DAR AR CINEMATOGRÁFICO
        if (painelHUD != null)
        {
            painelHUD.SetActive(false);
        }

        // 2. CONGELA O JOGADOR 
        PlayerMovement scriptDoJogador = FindObjectOfType<PlayerMovement>();
        if (scriptDoJogador != null)
        {
            scriptDoJogador.canMove = false;
        }

        Next();
    }

    public void Next()
    {
        if (currentText == 0)
        {
            dialogueUI.Enable();
        }

        Dialogue currentDialogue = dialogueData.talkScript[currentText];

        dialogueUI.SetName(currentDialogue.name);
        dialogueUI.SetPortrait(currentDialogue.portrait);

        typeText.fullText = currentDialogue.text;

        currentText++;

        if (currentText == dialogueData.talkScript.Count) finished = true;

        typeText.StartTyping();
        state = STATE.TYPING;
    }

    void OnTypeFinishe()
    {
        state = STATE.WAITING;
    }

    void Waiting()
    {
        if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
        {
            if (!finished)
            {
                Next();
            }
            else
            {
                dialogueUI.Disable();

                state = STATE.CLOSING;
                Invoke("ResetState", 0.1f);

                currentText = 0;
                finished = false;

                // 1. MOSTRA O HUD DE VOLTA AO TERMINAR
                if (painelHUD != null)
                {
                    painelHUD.SetActive(true);
                }

                // 2. DESCONGELA O JOGADOR
                PlayerMovement scriptDoJogador = FindObjectOfType<PlayerMovement>();
                if (scriptDoJogador != null)
                {
                    scriptDoJogador.canMove = true;
                }

                if (onDialogueComplete != null)
                {
                    onDialogueComplete.Invoke();
                    onDialogueComplete = null;
                }
            }
        }
    }

    void Typing()
    {
        if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
        {
            typeText.Skip();
            state = STATE.WAITING;
        }
    }

    void ResetState()
    {
        state = STATE.DISABLED;
    }
}