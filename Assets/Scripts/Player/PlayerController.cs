using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    [SerializeField] private float velocidad = 5f;

    [Header("Configuración de disparo")]
    [SerializeField] private float tiempoRecarga = 1f;
    private bool puedeDisparar = true;

    [Header("Vida del jugador")]
    [SerializeField] private int vidaMaxima = 3;
    private int vidaActual;

    [Header("Eventos")]
    public UnityEvent OnDisparoRealizado;
    public static event System.Action<int> OnVidaCambiada;
    public static event System.Action OnJugadorMuerto;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        MoverJugador();
        VerificarDisparo();
    }

    private void MoverJugador()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector2 direccion = new Vector2(horizontal, vertical);
        transform.Translate(direccion * velocidad * Time.deltaTime);
    }

    private void VerificarDisparo()
    {
        if (Input.GetKeyDown(KeyCode.Space) && puedeDisparar)
        {
            OnDisparoRealizado?.Invoke();
            StartCoroutine(CooldownDisparo());
        }
    }

    public void RecibirDano(int cantidad)
    {
        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
        OnVidaCambiada?.Invoke(vidaActual);

        if (vidaActual <= 0)
        {
            StartCoroutine(SecuenciaMuerte());
        }
        else
        {
            StartCoroutine(EfectoParpadeo(4, 0.1f));
        }
    }

    private System.Collections.IEnumerator CooldownDisparo()
    {
        puedeDisparar = false;
        yield return new WaitForSeconds(tiempoRecarga);
        puedeDisparar = true;
    }

    private System.Collections.IEnumerator EfectoParpadeo(int veces, float intervalo)
    {
        for (int i = 0; i < veces; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(intervalo);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(intervalo);
        }
    }

    private System.Collections.IEnumerator SecuenciaMuerte()
    {
        puedeDisparar = false;
        yield return new WaitForSeconds(0.5f);
        OnJugadorMuerto?.Invoke();
        gameObject.SetActive(false);
    }
}
