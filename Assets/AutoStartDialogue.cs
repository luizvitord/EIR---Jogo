using UnityEngine;

public class AutoStartDialogue : MonoBehaviour
{
    public DialogueData dialogoIntro;
    private DialogueSystem sistemaDialogo;

    void Start()
    {
        sistemaDialogo = FindObjectOfType<DialogueSystem>();

        if (sistemaDialogo != null && dialogoIntro != null)
        {
            // Usamos um pequeno atraso de 1 segundo para dar tempo 
            // da tela carregar e o jogador se situar antes do texto subir
            Invoke("TocarIntro", 1.0f);
        }
    }

    void TocarIntro()
    {
        sistemaDialogo.StartDialogue(dialogoIntro);
    }
}