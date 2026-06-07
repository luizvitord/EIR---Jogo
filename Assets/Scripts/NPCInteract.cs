using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteract : MonoBehaviour
{

    [Header("Configuração de Diálogos")]
    public DialogueData myDialogue;
    public DialogueData dialogoDepois; // NOVO: A fala curta que repete depois

    DialogueSystem dialogueSystem;
    bool isPlayerNearby = false;

    [Header("Integração com o Mapa")]
    public LocationAnnouncer mapAnnouncer;
    public Sprite novaImagemMapa;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {

            // Verificação de segurança
            if (dialogueSystem == null) return;

            // Se o estado do jogo diz que já conversamos com ela...
            if (GameManager.conversouComElfa)
            {
                // ESTADO 2: Fala a frase padrão repetitiva (ex: "Fale com o Durgan!")
                if (dialogoDepois != null)
                {
                    dialogueSystem.StartDialogue(dialogoDepois);
                }
            }
            else
            {
                // ESTADO 1: Diálogo longo da missão principal
                if (myDialogue != null)
                {
                    // Liga a função de concluir a missão apenas neste diálogo principal
                    dialogueSystem.onDialogueComplete = AoTerminarDialogo;
                    dialogueSystem.StartDialogue(myDialogue);
                }
            }
        }
    }

    void AoTerminarDialogo()
    {
        // Ativa a flag global dizendo que a conversa com a elfa terminou
        GameManager.conversouComElfa = true;
        Debug.Log("Conversa com a Elfa concluída! Durgan liberado.");

        // Aciona o script do mapa
        if (mapAnnouncer != null && novaImagemMapa != null)
        {
            mapAnnouncer.TriggerFadeComNovaImagem(novaImagemMapa);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}