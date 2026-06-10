using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private TextMeshProUGUI textoVida;
    [SerializeField] private GameObject panelGameOver;

    void OnEnable()
    {
        GestorPuntuacion.OnPuntuacionActualizada += ActualizarPuntuacion;
        PlayerController.OnVidaCambiada += ActualizarVida;
        PlayerController.OnJugadorMuerto += MostrarGameOver;
    }

    void OnDisable()
    {
        GestorPuntuacion.OnPuntuacionActualizada -= ActualizarPuntuacion;
        PlayerController.OnVidaCambiada -= ActualizarVida;
        PlayerController.OnJugadorMuerto -= MostrarGameOver;
    }

    private void ActualizarPuntuacion(int nuevaPuntuacion)
    {
        textoPuntuacion.text = $"Puntuación: {nuevaPuntuacion}";
    }

    private void ActualizarVida(int vidaActual)
    {
        textoVida.text = $"Vida: {vidaActual}";
    }

    private void MostrarGameOver()
    {
        panelGameOver.SetActive(true);
    }
}
