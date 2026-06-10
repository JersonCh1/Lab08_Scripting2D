# INSTRUCCIONES_CLAUDE_CODE.md — Lab08 Unity (automatización por CLI)

Este archivo es autocontenido. Claude Code debe leerlo y ejecutar la sección
**## TAREA** en orden. Todo el código de los scripts está embebido más abajo:
Claude Code debe CREAR esos archivos en disco con el contenido exacto indicado.

---

## ANTES de correr Claude Code (manual, una sola vez, ~4 min)

Esto NO lo hace Claude Code porque requiere el GUI de Unity Hub:

1. **Unity Hub → New project → plantilla `2D Core` → nombre exacto `Lab08_Scripting2D`.**
2. Abre el proyecto una vez. En el menú: **Window > TextMeshPro > Import TMP Essential Resources** (si no, los textos salen vacíos). Espera a que importe.
3. **Cierra Unity por completo** (el batchmode necesita que esté cerrado).
4. Anota la ruta de tu `Unity.exe`:
   Unity Hub → pestaña *Installs* → engranaje de tu versión → *Show in Explorer*.
   Ejemplo típico: `C:\Program Files\Unity\Hub\Editor\6000.0.32f1\Editor\Unity.exe`
5. (Opcional, recomendado) Si NO tienes `gh` CLI instalado, crea en github.com un repo
   **vacío, público**, llamado `Lab08_Scripting2D` y copia su URL `.git`.

---

## PROMPT que el usuario pega en Claude Code

Abrir Claude Code **dentro de la carpeta del proyecto** `Lab08_Scripting2D` y pegar:

> Lee `INSTRUCCIONES_CLAUDE_CODE.md` y ejecuta TODA la sección TAREA en orden.
> Mi Unity.exe está en: `C:\...\Unity.exe`.
> Para git: usa `gh` si está disponible; si no, mi repo es `https://github.com/MIUSUARIO/Lab08_Scripting2D.git`.
> Detente después del primer push y avísame para que yo tome las capturas de Play mode.

---

## TAREA (para Claude Code — ejecutar en orden)

1. Crear carpetas (si no existen):
   `Assets/Scripts/Player`, `Assets/Scripts/Enemy`, `Assets/Scripts/Managers`,
   `Assets/Scripts/UI`, `Assets/Editor`, `Assets/Prefabs`, `Assets/Scenes`.

2. Crear los 7 archivos de la sección **## ARCHIVOS** con su contenido EXACTO
   (6 scripts `.cs` + el `.gitignore`).

3. Verificar que Unity esté cerrado. Ejecutar el build por batchmode (PowerShell),
   reemplazando la ruta por la del usuario y usando la carpeta actual como projectPath:

   ```powershell
   & "C:\...\Unity.exe" -batchmode -projectPath "." -executeMethod ConstructorLab08.ConstruirBatch -quit -logFile -
   ```

   En el log debe aparecer: `Lab08 BATCH: escena construida y guardada`.
   Si hay errores de compilación, mostrármelos y corregir antes de seguir.

4. Inicializar git y hacer el PRIMER commit:
   ```bash
   git init
   git add .
   git commit -m "Inicio del proyecto Lab08_Scripting2D"
   ```

5. Conectar el remoto y push:
   - Si hay `gh`:
     ```bash
     gh repo create Lab08_Scripting2D --public --source=. --remote=origin --push
     ```
   - Si no:
     ```bash
     git remote add origin https://github.com/MIUSUARIO/Lab08_Scripting2D.git
     git branch -M main
     git push -u origin main
     ```

6. **DETENERSE AQUÍ** y avisar al usuario. (Él abrirá Unity, dará Play y tomará las
   6 capturas: GitHub x2, Inspector x1, Play mode x3.)

7. Solo cuando el usuario confirme que ya verificó el Play mode, hacer el commit final:
   ```bash
   git add .
   git commit -m "Fase 4 completa: integracion de eventos, corutinas y comunicacion entre objetos"
   git push
   ```

---

## ARCHIVOS

### Archivo: `Assets/Editor/ConstructorLab08.cs`

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

// Va en una carpeta llamada "Editor".
public class ConstructorLab08
{
    [MenuItem("Lab08/Construir Escena Completa")]
    public static void ConstruirDesdeMenu()
    {
        ConstruirEscena();
        Debug.Log("Lab08: escena construida. Guarda con Ctrl+S.");
    }

    // Pensado para -executeMethod en batchmode (crea y guarda la escena).
    public static void ConstruirBatch()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        ConstruirEscena();
        EditorSceneManager.SaveScene(escena, "Assets/Scenes/EscenaPrincipal.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab08 BATCH: escena construida y guardada en Assets/Scenes/EscenaPrincipal.unity");
    }

    public static void ConstruirEscena()
    {
        Sprite cuadrado = CrearSpriteCuadrado();

        // ----- PLAYER -----
        GameObject player = new GameObject("Player");
        player.transform.position = Vector3.zero;
        player.tag = "Player";
        var srP = player.AddComponent<SpriteRenderer>();
        srP.sprite = cuadrado;
        srP.color = new Color(0f, 120f / 255f, 215f / 255f, 1f);
        player.AddComponent<BoxCollider2D>();
        var pc = player.AddComponent<PlayerController>();
        player.AddComponent<SistemaAtaque>();

        // ----- ENEMY -----
        GameObject enemy = new GameObject("Enemy");
        enemy.transform.position = new Vector3(3f, 0f, 0f);
        var srE = enemy.AddComponent<SpriteRenderer>();
        srE.sprite = cuadrado;
        srE.color = new Color(215f / 255f, 30f / 255f, 30f / 255f, 1f);
        var boxE = enemy.AddComponent<BoxCollider2D>();
        boxE.isTrigger = true;
        enemy.AddComponent<EnemyController>();

        // ----- GESTOR PUNTUACION -----
        GameObject gestorGO = new GameObject("GestorPuntuacion");
        var gestor = gestorGO.AddComponent<GestorPuntuacion>();

        // ----- CANVAS + EVENTSYSTEM -----
        GameObject canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ----- TEXTOS UI -----
        var textoPunt = CrearTexto("TextoPuntuacion", canvasGO.transform, "Puntuación: 0", new Vector2(20, -20));
        var textoVida = CrearTexto("TextoVida", canvasGO.transform, "Vida: 3", new Vector2(20, -75));

        // ----- PANEL GAME OVER -----
        GameObject panel = new GameObject("PanelGameOver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var rtPanel = panel.GetComponent<RectTransform>();
        rtPanel.anchorMin = Vector2.zero; rtPanel.anchorMax = Vector2.one;
        rtPanel.offsetMin = Vector2.zero; rtPanel.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 200f / 255f);

        GameObject textoGO = new GameObject("TextoGameOver", typeof(RectTransform));
        textoGO.transform.SetParent(panel.transform, false);
        var tmpGameOver = textoGO.AddComponent<TextMeshProUGUI>();
        tmpGameOver.text = "GAME OVER";
        tmpGameOver.fontSize = 48;
        tmpGameOver.alignment = TextAlignmentOptions.Center;
        var rtTexto = textoGO.GetComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero; rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero; rtTexto.offsetMax = Vector2.zero;
        panel.SetActive(false);

        // ----- UICONTROLLER + asignación de campos -----
        GameObject uiGO = new GameObject("UIController");
        var ui = uiGO.AddComponent<UIController>();
        var so = new SerializedObject(ui);
        so.FindProperty("textoPuntuacion").objectReferenceValue = textoPunt;
        so.FindProperty("textoVida").objectReferenceValue = textoVida;
        so.FindProperty("panelGameOver").objectReferenceValue = panel;
        so.ApplyModifiedProperties();

        // ----- CABLEAR UNITYEVENT: OnDisparoRealizado -> AgregarPuntos(10) -----
        if (pc.OnDisparoRealizado == null)
            pc.OnDisparoRealizado = new UnityEvent();
        UnityEventTools.AddIntPersistentListener(pc.OnDisparoRealizado, gestor.AgregarPuntos, 10);

        // ----- PREFAB DEL ENEMIGO + 3 COPIAS -----
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string ruta = "Assets/Prefabs/Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(enemy, ruta, InteractionMode.UserAction);

        Vector3[] posiciones =
        {
            new Vector3(-3, 2, 0),
            new Vector3(3, -2, 0),
            new Vector3(-3, -2, 0)
        };
        foreach (var p in posiciones)
        {
            GameObject copia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            copia.transform.position = p;
        }
    }

    private static Sprite CrearSpriteCuadrado()
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] px = new Color[64 * 64];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
    }

    private static TextMeshProUGUI CrearTexto(string nombre, Transform padre, string contenido, Vector2 pos)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = contenido;
        tmp.fontSize = 24;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(250, 50);
        return tmp;
    }
}
#endif
```

### Archivo: `Assets/Scripts/Player/PlayerController.cs`

```csharp
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
```

### Archivo: `Assets/Scripts/Player/SistemaAtaque.cs`

```csharp
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
```

### Archivo: `Assets/Scripts/Enemy/EnemyController.cs`

```csharp
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
```

### Archivo: `Assets/Scripts/Managers/GestorPuntuacion.cs`

```csharp
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
```

### Archivo: `Assets/Scripts/UI/UIController.cs`

```csharp
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
```

### Archivo: `.gitignore` (raíz del proyecto)

```gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
[Mm]emoryCaptures/
.vs/
.idea/
*.csproj
*.sln
*.user
*.unityproj
*.booproj
Assets/AssetStoreTools*
```

---

## Notas para Claude Code

- El método de batch es `ConstructorLab08.ConstruirBatch` (sin namespace).
- Unity debe estar cerrado antes del batchmode o dará error de lock.
- El método `RecibirDano` va SIN ñ (así lo invoca EnemyController).
- Si el log muestra errores de compilación, NO continuar con git: corregir primero.
- No tocar `Library/` ni forzar su commit; el `.gitignore` ya lo excluye.
