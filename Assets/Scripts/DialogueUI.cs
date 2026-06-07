using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour {

    Image background;
    TextMeshProUGUI nameText;
    TextMeshProUGUI talkText;
    CanvasGroup canvasGroup; 

    // NOVO: Campo para você arrastar o objeto da Foto lá da Unity
    public Image portraitImage; 

    public float speed = 10f;
    bool open = false;

    void Awake() {
        background = transform.GetChild(0).GetComponent<Image>();
        nameText   = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        talkText   = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start() {
        canvasGroup.alpha = 0f;
    }

    void Update() {
        if(open) {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1, speed * Time.deltaTime);
        } else {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, speed * Time.deltaTime);
        }
    }

    public void SetName(string name) {
        nameText.text = name;
    }

    // NOVO: Função que recebe a foto do ScriptableObject e bota na tela
    public void SetPortrait(Sprite sprite) {
        if (portraitImage != null) {
            portraitImage.sprite = sprite;
            
            // Se o NPC não tiver foto, deixa o espaço transparente. Se tiver, fica visível (alpha 1).
            if (sprite == null) {
                portraitImage.color = new Color(1, 1, 1, 0); 
            } else {
                portraitImage.color = new Color(1, 1, 1, 1); 
            }
        }
    }

    public void Enable() {
        open = true;
    }

    public void Disable() {
        open = false;
        nameText.text = "";
        talkText.text = "";
        SetPortrait(null); // Limpa a foto quando fechar a conversa
    }
}