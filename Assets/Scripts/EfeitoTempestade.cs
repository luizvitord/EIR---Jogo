using UnityEngine;
using System.Collections;

public class EfeitoTempestade : MonoBehaviour
{
    public Light luzDoRaio;
    public AudioSource somTrovao;
    public AudioSource somChuva;

    public float forcaDoRaio = 100f;
    public float tempoMinimo = 5f;
    public float tempoMaximo = 15f;

    void Start()
    {
        if (somChuva != null)
        {
            somChuva.loop = true;
            somChuva.Play();
        }

        StartCoroutine(GerarRaio());
    }

    IEnumerator GerarRaio()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(tempoMinimo, tempoMaximo));

            if (luzDoRaio != null) luzDoRaio.intensity = forcaDoRaio;
            yield return new WaitForSeconds(0.1f);
            if (luzDoRaio != null) luzDoRaio.intensity = 0f;
            yield return new WaitForSeconds(0.05f);
            if (luzDoRaio != null) luzDoRaio.intensity = (forcaDoRaio * 0.7f);
            yield return new WaitForSeconds(0.1f);
            if (luzDoRaio != null) luzDoRaio.intensity = 0f;

            yield return new WaitForSeconds(0.5f);
            if (somTrovao != null) somTrovao.Play();
        }
    }
}