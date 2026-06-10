using UnityEngine;

public class GestorPuntuacion : MonoBehaviour
{
    public static GestorPuntuacion Instancia { get; private set; }

    public static event System.Action<int> OnPuntuacionActualizada;

    private int puntuacion = 0;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        EnemyController.OnEnemigoDerrotado += AgregarPuntos;
    }

    void OnDisable()
    {
        EnemyController.OnEnemigoDerrotado -= AgregarPuntos;
    }

    public void AgregarPuntos(int cantidad)
    {
        puntuacion += cantidad;
        OnPuntuacionActualizada?.Invoke(puntuacion);
    }

    public int ObtenerPuntuacion()
    {
        return puntuacion;
    }
}
