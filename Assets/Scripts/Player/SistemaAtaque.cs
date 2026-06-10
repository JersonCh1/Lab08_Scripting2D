using UnityEngine;

public class SistemaAtaque : MonoBehaviour
{
    [SerializeField] private float rangoAtaque = 1.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AtacarEnemigoCercano();
        }
    }

    private void AtacarEnemigoCercano()
    {
        Collider2D[] objetosEnRango = Physics2D.OverlapCircleAll(
            transform.position,
            rangoAtaque
        );

        foreach (Collider2D col in objetosEnRango)
        {
            EnemyController enemigo = col.GetComponent<EnemyController>();
            if (enemigo != null)
            {
                enemigo.Morir();
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}
