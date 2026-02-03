using UnityEngine.UI;
using UnityEngine;

public class SimpleSphereDestroyer : MonoBehaviour
{
    private float HP = 100f;
    public Image Bar;
    bool isDead = false;

    public AudioClip soundDestroy;
    public AudioClip soundNotDestroy;

    public Font MyFont;


    void OnTriggerEnter(Collider other)
    {
        if (gameObject.CompareTag("Player"))
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                Debug.Log("Triggered");
                HP -= 2;

                if (HP < 0)
                {
                    if (isDead) return;
                    isDead = true;
                    GameObject cam = GameObject.FindWithTag("MainCamera");

                    var _canvasRoot = new GameObject("CongratulationsCanvas");
                    _canvasRoot.transform.SetParent(cam.transform, false);
                    _canvasRoot.transform.localPosition = Vector3.zero + new Vector3(0.0f, 0.0f, 2.0f);
                    _canvasRoot.transform.localRotation = Quaternion.identity;
                    _canvasRoot.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

                    var _canvas = _canvasRoot.AddComponent<Canvas>();

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
                    text.text = "GAME OVER";

            
                    text.font = MyFont;

                    text.fontSize = 56;
                    text.color = new Color(1f, 0f, 0f, 1f);
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
                Bar.fillAmount = HP / 100;
            }
        }

        if (gameObject.CompareTag("SphereTag") && other.gameObject.CompareTag("SphereTag"))
        {
            return;
        }

        if (gameObject.CompareTag("Hands"))
        {
  

            if (other.gameObject.CompareTag("SphereTag"))
            {
                if (other.gameObject.GetComponent<Transform>().localScale.x <= 0.1)
                {
                    Debug.Log("Нет попадания");
                    AudioSource.PlayClipAtPoint(soundNotDestroy, transform.position);
                    // useless but its okay i guess
                }
                else
                {
                    Debug.Log("Попадание!");
                    AudioSource.PlayClipAtPoint(soundDestroy, transform.position);
                    Destroy(other.gameObject);
                    ApplyDamageToRobot();
                }
            }
        }
      
        //Debug.Log("����� � �������: " + other.gameObject.name);
       
    }
    
    void ApplyDamageToRobot()
    {
        Transform robotTransform = transform;
        while (robotTransform != null)
        {
            HpManager hpManager = robotTransform.GetComponent<HpManager>();
            if (hpManager != null)
            {
                hpManager.TakeDamage(1f);
                return;
            }
            hpManager = robotTransform.GetComponentInChildren<HpManager>();
            if (hpManager != null)
            {
                hpManager.TakeDamage(1f);
                return;
            }
            robotTransform = robotTransform.parent;
        }

        HpManager[] allHpManagers = FindObjectsOfType<HpManager>();
        if (allHpManagers.Length > 0)
            allHpManagers[0].TakeDamage(1f);
        else
            Debug.LogWarning("No HpManager component found in scene!");
    }
}