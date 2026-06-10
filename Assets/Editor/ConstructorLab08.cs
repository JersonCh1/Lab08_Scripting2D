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
