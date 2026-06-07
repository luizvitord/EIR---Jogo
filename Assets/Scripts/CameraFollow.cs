using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo da Camera")]
    public Transform target;

    [Header("Configuracoes")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Screen Shake (Tremor)")]
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // ---> NOVO: A TRAVA CONTRA O TERREMOTO NO PAUSE <---
        // Se o tempo do jogo estiver parado (Menu de Level Up aberto),
        // abortamos a atualização da câmera instantaneamente!
        if (Time.timeScale == 0f) return;

        if (target == null) return;

        // 1. Calcula onde a câmera DEVE estar para seguir o player
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 2. Se o tremor estiver ativado, adicionamos um "caos" na posição final
        if (shakeDuration > 0)
        {
            // Gera uma posição aleatória num círculo e multiplica pela força (magnitude)
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude;

            // Soma esse tremor à posição suave da câmera
            smoothedPosition.x += shakeOffset.x;
            smoothedPosition.y += shakeOffset.y;

            // Reduz o tempo do tremor até chegar a zero
            shakeDuration -= Time.deltaTime;
        }

        // 3. Aplica a posição final na câmera
        transform.position = smoothedPosition;
    }

    public void TriggerShake(float duration = 0.15f, float magnitude = 0.15f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    public void IniciarZoomMorte(float zoomAlvo, float duracao)
    {
        if (cam == null) cam = GetComponent<Camera>();

        StopAllCoroutines();
        StartCoroutine(RoutineZoom(zoomAlvo, duracao));
    }

    // ---> NOVO: Função para dar zoom na Cutscene de forma limpa
    public void MudarZoomSmooth(float zoomAlvo, float duracao)
    {
        if (cam == null) cam = GetComponent<Camera>();
        StopAllCoroutines(); // Para o zoom anterior para não dar conflito
        StartCoroutine(RoutineZoom(zoomAlvo, duracao));
    }

    private IEnumerator RoutineZoom(float alvo, float tempo)
    {
        float inicial = cam.orthographicSize;
        float cronometro = 0;

        while (cronometro < tempo)
        {
            // Nota: Como a morte não pausa o Time.timeScale, o deltaTime aqui funciona perfeitamente!
            cronometro += Time.deltaTime;
            cam.orthographicSize = Mathf.Lerp(inicial, alvo, cronometro / tempo);
            yield return null;
        }

        cam.orthographicSize = alvo;
    }
}