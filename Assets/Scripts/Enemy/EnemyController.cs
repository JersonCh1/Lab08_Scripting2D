using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Configuración de patrullaje")]
    [SerializeField] private float velocidadPatrullaje = 2f;
    [SerializeField] private float distanciaPatrullaje = 3f;
    [SerializeField] private float tiempoEsperaEnExtremo = 0.8f;

    [Header("Configuración de daño")]
    [SerializeField] private int danoAlJugador = 1;
    [SerializeField] private int puntosAlMorir = 100;

    public static event System.Action<int> OnEnemigoDerrotado;

    private Vector2 posicionInicial;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        posicionInicial = transform.position;
    }

    void Start()
    {
        StartCoroutine(RutinaPatrullaje());
    }

    private System.Collections.IEnumerator RutinaPatrullaje()
    {
        while (true)
        {
            yield return MoverHacia(posicionInicial + Vector2.right * distanciaPatrullaje);
            yield return new WaitForSeconds(tiempoEsperaEnExtremo);
            yield return MoverHacia(posicionInicial - Vector2.right * distanciaPatrullaje);
            yield return new WaitForSeconds(tiempoEsperaEnExtremo);
        }
    }

    private System.Collections.IEnumerator MoverHacia(Vector2 destino)
    {
        while (Vector2.Distance(transform.position, destino) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                destino,
                velocidadPatrullaje * Time.deltaTime
            );
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Player"))
        {
            PlayerController jugador = otro.GetComponent<PlayerController>();
            if (jugador != null)
            {
                jugador.RecibirDano(danoAlJugador);
            }
        }
    }

    public void Morir()
    {
        OnEnemigoDerrotado?.Invoke(puntosAlMorir);
        Destroy(gameObject);
    }
}
