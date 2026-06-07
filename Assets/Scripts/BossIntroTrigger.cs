using UnityEngine;

public class BossIntroTrigger : MonoBehaviour
{
    private bool introDisparada = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !introDisparada)
        {
            introDisparada = true;

            // Avisa o Manager para começar o filme de introdução
            if (BossBattleManager.Instance != null)
            {
                BossBattleManager.Instance.IniciarIntroBoss();
            }

            // Desativa o colisor para a cena nunca mais repetir
            GetComponent<Collider2D>().enabled = false;
        }
    }
}