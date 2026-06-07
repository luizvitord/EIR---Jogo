using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DurganInteract : MonoBehaviour
{
    [Header("Configuração de Diálogos")]
    public DialogueData dialogoAntesDaElfa; // O fora do forasteiro
    public DialogueData myDialogue;         // O diálogo principal da espada
    public DialogueData dialogoDepoisDaEspada; // Fala repetitiva pro resto do jogo

    [Header("Configuração da Recompensa")]
    public GameObject popUpEspadaMadeira;
    public float tempoDoPopUp = 3f;
    public float tempoDeFade = 0.5f;

    private DialogueSystem dialogueSystem;
    private PlayerMovement playerMovement;
    private bool isPlayerNearby = false;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        // Só interage se o jogador estiver perto e apertar E
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Verifica se o diálogo já não está aberto para não bugar
            if (dialogueSystem == null) return;

            if (GameManager.conversouComDurgan)
            {
                // ESTADO 3: Já tem a espada
                if (dialogoDepoisDaEspada != null)
                {
                    dialogueSystem.StartDialogue(dialogoDepoisDaEspada);
                }
            }
            else if (GameManager.conversouComElfa)
            {
                // ESTADO 2: Cumpriu os requisitos, vai ganhar a espada!
                if (myDialogue != null)
                {
                    dialogueSystem.onDialogueComplete = AoTerminarDialogo;
                    dialogueSystem.StartDialogue(myDialogue);
                }
            }
            else
            {
                // ESTADO 1: Tomando um fora porque não falou com a elfa ainda
                if (dialogoAntesDaElfa != null)
                {
                    dialogueSystem.StartDialogue(dialogoAntesDaElfa);
                }
            }
        }
    }

    void AoTerminarDialogo()
    {
        GameManager.conversouComDurgan = true;
        StartCoroutine(RotinaDarEspada());
    }

    private IEnumerator RotinaDarEspada()
    {
        playerMovement.canMove = false;

        if (popUpEspadaMadeira != null)
        {
            popUpEspadaMadeira.SetActive(true);

            CanvasGroup canvasGroup = popUpEspadaMadeira.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = popUpEspadaMadeira.AddComponent<CanvasGroup>();

            float tempoGasto = 0f;
            while (tempoGasto < tempoDeFade)
            {
                tempoGasto += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, tempoGasto / tempoDeFade);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(tempoDoPopUp);

            tempoGasto = 0f;
            while (tempoGasto < tempoDeFade)
            {
                tempoGasto += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, tempoGasto / tempoDeFade);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            popUpEspadaMadeira.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(tempoDoPopUp);
        }

        playerMovement.EquipWoodenSword();
        playerMovement.canMove = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = false;
    }
}