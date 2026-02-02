using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class hpBarView : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 20f, 0f);
    [SerializeField] private float barWidth = 3f;
    [SerializeField] private float barHeight = 0.3f;

    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color fillColor = new Color(0.2f, 0.8f, 0.2f, 1f);

    [SerializeField] private bool rotation = true;

    private HpManager _hpManager;
    private Canvas _canvas;
    private GameObject _canvasRoot;
    private Image _fillImage;
    private static Sprite _whiteSprite;

    void Awake()
    {
        _hpManager = GetComponent<HpManager>();
        if (_hpManager == null)
            _hpManager = GetComponentInParent<HpManager>();
        if (_hpManager == null)
        {
            Debug.LogWarning("HpBarView: HpManager not found on this object or parent.");
            enabled = false;
            return;
        }
        CreateSphere robot = GetComponent<CreateSphere>();
        if (robot == null)
            robot = GetComponentInParent<CreateSphere>();
        if (robot == null)
        {
            enabled = false;
            return;
        }
        Camera mainCam = Camera.main;
        if (mainCam != null && (transform == mainCam.transform || transform.IsChildOf(mainCam.transform)))
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        if (_hpManager == null || !enabled) return;
        CreateBar();
    }

    static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null) return _whiteSprite;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        return _whiteSprite;
    }

    static Camera GetDisplayCamera()
    {
        if (Camera.main != null) return Camera.main;
        var cam = Camera.current;
        if (cam != null) return cam;
        cam = Object.FindFirstObjectByType<Camera>();
        return cam;
    }

    void CreateBar()
    {
        Camera cam = GetDisplayCamera();
        if (cam == null)
        {
            Debug.LogWarning("HpBarView: No camera found. Bar will be created when camera is available.");
            return;
        }

        Sprite white = GetWhiteSprite();

        GameObject canvasGo = new GameObject("HPBarCanvas");
        _canvasRoot = canvasGo;
        canvasGo.transform.SetParent(transform);
        canvasGo.transform.localPosition = offset;
        canvasGo.transform.localRotation = Quaternion.identity;
        canvasGo.transform.localScale = Vector3.one;

        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.worldCamera = cam;
        _canvas.sortingOrder = 100;

        canvasGo.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);

        // Фон бара
        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(canvasGo.transform, false);
        Image bgImage = bgGo.AddComponent<Image>();
        bgImage.sprite = white;
        bgImage.color = backgroundColor;
        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Заполнение
        GameObject fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(bgGo.transform, false);
        _fillImage = fillGo.AddComponent<Image>();
        _fillImage.sprite = white;
        _fillImage.color = fillColor;
        _fillImage.type = Image.Type.Filled;
        _fillImage.fillMethod = Image.FillMethod.Horizontal;
        _fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        RectTransform fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
    }

    void LateUpdate()
    {
        if (_hpManager != null && !_hpManager.IsAlive)
        {
            if (_canvasRoot != null)
                _canvasRoot.SetActive(false);
            return;
        }

        if (_canvasRoot == null && _hpManager != null)
        {
            CreateBar();
            return;
        }

        if (_canvas != null && _canvas.worldCamera == null)
            _canvas.worldCamera = GetDisplayCamera();

        Camera cam = GetDisplayCamera();
        if (cam != null && rotation && _canvasRoot != null)
        {
            _canvasRoot.transform.LookAt(_canvasRoot.transform.position + cam.transform.forward, cam.transform.up);
        }

        if (_hpManager == null || _fillImage == null) return;

        float maxHp = _hpManager.MaxHp;
        if (maxHp <= 0f) return;

        float fillAmount = Mathf.Clamp01(_hpManager.HP / maxHp);
        _fillImage.fillAmount = fillAmount;
        _fillImage.color = fillColor;
    }

    void OnDestroy()
    {
        if (_canvasRoot != null)
            Destroy(_canvasRoot);
    }
}
