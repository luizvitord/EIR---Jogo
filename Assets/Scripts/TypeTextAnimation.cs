using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypeTextAnimation : MonoBehaviour
{

    public Action TypeFinished;

    [Header("Configurações de Texto")]
    public float typeDelay = 0.05f;
    public TextMeshProUGUI textObject;
    public string fullText;

    [Header("Juice: Som de Voz (Blip)")]
    public AudioSource audioSource;
    public AudioClip blipSound;
    [Range(0.5f, 2f)] public float minPitch = 0.9f;
    [Range(0.5f, 2f)] public float maxPitch = 1.1f;

    // NOVO: Controla a cada quantas letras o som vai tocar
    [Range(1, 5)] public int frequenciaSom = 2;

    Coroutine coroutine;

    void Awake()
    {
        if (textObject == null)
        {
            textObject = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void StartTyping()
    {
        coroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        textObject.text = fullText;
        textObject.maxVisibleCharacters = 0;

        for (int i = 0; i <= textObject.text.Length; i++)
        {
            textObject.maxVisibleCharacters = i;

            if (blipSound != null && i > 0 && i <= textObject.text.Length)
            {
                char c = textObject.text[i - 1];

                if (c != ' ' && c != '\n')
                {
                    // A MÁGICA AQUI: O som só toca se o número da letra for múltiplo da frequência
                    if (i % frequenciaSom == 0)
                    {
                        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
                        audioSource.PlayOneShot(blipSound);
                    }
                }
            }

            yield return new WaitForSeconds(typeDelay);
        }

        TypeFinished?.Invoke();
    }

    public void Skip()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        textObject.maxVisibleCharacters = textObject.text.Length;
        TypeFinished?.Invoke();
    }
}