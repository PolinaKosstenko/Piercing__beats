using UnityEngine;
using UnityEngine.UI;

public class TriggerCongratulations : MonoBehaviour
{
    public float activationRadius = 7f;
    public float deactivationRadius = 9f;
    public bool useCameraPosition = true;

    public string message = "Поздравляю";
    public Font textFont;

    public bool showGizmos = true;

    public bool useCameraChildForXR = false;

    private Transform _player;
    private Camera _camera;
    private bool _isActive;
    private GameObject _canvasRoot;
    private Canvas _canvas;

    void Start()
    {
        if (useCameraPosition)
        {
            _camera = Camera.main;
            if (_camera == null)
                _camera = Object.FindFirstObjectByType<Camera>();
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
        }

    }

    void Update()
    {
        //if (!IsRobotDead())
        //{
        //    if (_isActive)
        //        HideCongratulations();
        //    return;
        //}

        Vector3 checkPosition = GetCheckPosition();
        if (float.IsNaN(checkPosition.x)) return;

        float distance = Vector3.Distance(transform.position, checkPosition);

        if (!_isActive && distance <= activationRadius)
            ShowCongratulations();
        else if (_isActive && distance > deactivationRadius)
            HideCongratulations();
    }

    static bool _robotWasKilled;

    bool IsRobotDead()
    {
        var hp = Object.FindFirstObjectByType<HpManager>();
        if (hp != null && !hp.IsAlive)
            _robotWasKilled = true;
        return _robotWasKilled || (hp != null && !hp.IsAlive);
    }

    Vector3 GetCheckPosition()
    {
        if (useCameraPosition)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                    _camera = Object.FindFirstObjectByType<Camera>();
            }
            return _camera != null ? _camera.transform.position : new Vector3(float.NaN, float.NaN, float.NaN);
        }
        return _player != null ? _player.position : new Vector3(float.NaN, float.NaN, float.NaN);
    }

    void ShowCongratulations()
    {
        _isActive = true;
        if (_canvasRoot == null)
            CreateCongratulationsUI();
        if (_canvasRoot != null)
            _canvasRoot.SetActive(true);
    }

    void HideCongratulations()
    {
        _isActive = false;
        if (_canvasRoot != null)
            _canvasRoot.SetActive(true);
    }

    void CreateCongratulationsUI()
    {
        GameObject cam = GameObject.FindWithTag("MainCamera");

        _canvasRoot = new GameObject("CongratulationsCanvas");
         _canvasRoot.transform.SetParent(cam.transform, false);
        _canvasRoot.transform.localPosition = Vector3.zero + new Vector3(0.0f, 0.0f, 2.0f);
        _canvasRoot.transform.localRotation = Quaternion.identity;
        _canvasRoot.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
     
        _canvas = _canvasRoot.AddComponent<Canvas>();
      
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.planeDistance = 0.5f;
   
        _canvas.sortingOrder = 32767;
        _canvas.pixelPerfect = false;

        var scaler = _canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        _canvasRoot.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = _canvasRoot.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        // Плашка по центру экрана (остаётся в центре при любом повороте камеры)
        GameObject panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(_canvasRoot.transform, false);

        Image panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500f, 120f);
        panelRect.anchoredPosition = Vector2.zero;

        // Текст
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(panelGo.transform, false);

        Text text = textGo.AddComponent<Text>();
        text.text = message;

        Font font = textFont;
        if (font == null)
            font = Resources.Load<Font>("Fonts/LiberationSans");
        if (font == null)
            font = Resources.Load<Font>("LiberationSans");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 56);
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("Segoe UI", 56);
        }
        if (font != null)
            text.font = font;
        
        text.fontSize = 56;
        text.color = new Color(1f, 1f, 1f, 1f);
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        Shadow shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, 2f);

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(30f, 30f);
        textRect.offsetMax = new Vector2(-30f, -30f);

        _canvasRoot.SetActive(true);
    }

    void OnDestroy()
    {
        if (_canvasRoot != null)
            Destroy(_canvasRoot);
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deactivationRadius);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying) return;

        Gizmos.color = _isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
