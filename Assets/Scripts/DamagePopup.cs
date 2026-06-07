using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer = 0.5f;
    private Color textColor;
    private Vector3 moveVector;
    private Vector3 escalaOriginal;

    [Header("Efeitos da Aura")]
    public Color32 corDourada = new Color32(255, 215, 0, 255);

    [Header("Tamanho do Impacto (Multiplicadores)")]
    [Tooltip("O quão grande ele fica no pico do pulo (Ex: 2 = o dobro do tamanho)")]
    public float multiplicadorImpacto = 2f;

    [Tooltip("O tamanho que ele estabiliza depois (Ex: 1.5 = 50% maior)")]
    public float multiplicadorFinal = 1.5f;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        escalaOriginal = transform.localScale;
    }

    public void Setup(int damageAmount, bool atacouComAura)
    {
        textMesh.SetText(damageAmount.ToString());

        if (atacouComAura)
        {
            textColor = corDourada;
            StartCoroutine(AnimarImpacto());
        }
        else
        {
            textColor = textMesh.color;
        }

        textMesh.color = textColor;

        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(1f, 2f);
        moveVector = new Vector3(randomX, randomY, 0).normalized * 3f;
    }

    void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector.y -= 8f * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator AnimarImpacto()
    {
        Vector3 escalaMaxima = escalaOriginal * multiplicadorImpacto;
        Vector3 escalaFinal = escalaOriginal * multiplicadorFinal;

        transform.localScale = escalaMaxima;

        float tempoAnimacao = 0.15f;
        float tempoDecorrido = 0f;

        while (tempoDecorrido < tempoAnimacao)
        {
            tempoDecorrido += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaMaxima, escalaFinal, tempoDecorrido / tempoAnimacao);
            yield return null;
        }

        transform.localScale = escalaFinal;
    }
}