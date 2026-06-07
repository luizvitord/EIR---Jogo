using UnityEngine;
using System.Collections;

public class JuiceLevelUpUI : MonoBehaviour
{
    [Header("Arraste os objetos da sua Hierarchy aqui")]
    public RectTransform textoTitulo;      // O seu Texto_LevelUp_Titulo
    public RectTransform textoProgresso;   // O seu Texto_LevelUp_Progresso
    public GameObject areaDasCartas;       // A sua Area_Das_Cartas

    // O OnEnable roda AUTOMATICAMENTE no exato milissegundo que o painel é ativado na tela
    void OnEnable()
    {
        StartCoroutine(AnimarEntrada());
    }

    private IEnumerator AnimarEntrada()
    {
        // 1. Esconde as cartas na mesma hora
        if (areaDasCartas != null) areaDasCartas.SetActive(false);

        // 2. Esmaga o tamanho dos textos para zero
        if (textoTitulo != null) textoTitulo.localScale = Vector3.zero;
        if (textoProgresso != null) textoProgresso.localScale = Vector3.zero;

        // 3. Faz o texto estourar (Efeito Elástico)
        float tempoAnimacao = 0.4f;
        float tempoDecorrido = 0f;

        while (tempoDecorrido < tempoAnimacao)
        {
            // Usamos unscaledDeltaTime para a UI não congelar com o pause do jogo!
            tempoDecorrido += Time.unscaledDeltaTime;
            float progresso = tempoDecorrido / tempoAnimacao;

            // A matemática do soco: vai de 0 até 1.2 (passa do tamanho) e volta pra 1
            float escala = Mathf.Lerp(0f, 1.2f, progresso);
            if (progresso > 0.7f)
            {
                escala = Mathf.Lerp(1.2f, 1f, (progresso - 0.7f) / 0.3f);
            }

            if (textoTitulo != null) textoTitulo.localScale = new Vector3(escala, escala, escala);
            if (textoProgresso != null) textoProgresso.localScale = new Vector3(escala, escala, escala);

            yield return null;
        }

        // Garante que o texto fique 100% no tamanho certo no final da animação
        if (textoTitulo != null) textoTitulo.localScale = Vector3.one;
        if (textoProgresso != null) textoProgresso.localScale = Vector3.one;

        // 4. A pausa dramática: Deixa o jogador ler o nível e comemorar
        yield return new WaitForSecondsRealtime(0.7f);

        // 5. Revela as cartas para ele escolher!
        if (areaDasCartas != null) areaDasCartas.SetActive(true);
    }
}