using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    public float velocidade = 5f;
    private Rigidbody rb;
    private Vector3 direcaoMovimento;

    [Header("Câmera e Visão")]
    public Transform cameraTransform;
    public float sensibilidadeMouse = 2f;
    private float rotacaoVertical = 0f;

    [Header("Balanço da Câmera (Head Bobbing)")]
    public float velocidadeBalanco = 10f;
    public float amplitudeBalanco = 0.05f;
    private float posicaoYOriginalCamera;
    private float timerBalanco = 0f;

    [Header("Animação")]
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        posicaoYOriginalCamera = cameraTransform.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        direcaoMovimento = (transform.forward * inputZ + transform.right * inputX).normalized;

        if (animator != null)
        {
            animator.SetFloat("InputX", inputX, 0.10f, Time.deltaTime);
            animator.SetFloat("InputZ", inputZ, 0.10f, Time.deltaTime);
        }
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadeMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadeMouse;

        transform.Rotate(Vector3.up * mouseX);

        rotacaoVertical -= mouseY;
        rotacaoVertical = Mathf.Clamp(rotacaoVertical, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(rotacaoVertical, 0f, 0f);

        EfeitoBalancoCamera(inputX, inputZ);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direcaoMovimento * velocidade * Time.fixedDeltaTime);
    }

    void EfeitoBalancoCamera(float inputX, float inputZ)
    {
        if (inputX != 0 || inputZ != 0) 
        {
            timerBalanco += Time.deltaTime * velocidadeBalanco;
            float novoY = posicaoYOriginalCamera + Mathf.Sin(timerBalanco) * amplitudeBalanco;
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, novoY, cameraTransform.localPosition.z);
        }
        else
        {
            timerBalanco = 0f;
            float transicao = Mathf.Lerp(cameraTransform.localPosition.y, posicaoYOriginalCamera, Time.deltaTime * velocidadeBalanco);
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, transicao, cameraTransform.localPosition.z);
        }
    }
}