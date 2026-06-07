using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuVideoController : MonoBehaviour
{
    private const string MenuSceneName = "MainMenu";
    private const string TargetSceneName = "SampleScene";
    private const string VideoFileName = "Cinematic_Game_Cover_Animation.mp4";
    private const string ButtonImageName = "Jogar_Imagem.png";
    private const string BuiltInFontName = "LegacyRuntime.ttf";

    private static bool menuLoadRequested;

    private VideoPlayer videoPlayer;
    private RawImage videoImage;
    private Button playButton;
    private CanvasGroup buttonGroup;
    private RenderTexture renderTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureMenuSceneLoaded()
    {
#if UNITY_EDITOR
        // No editor, permite iniciar qualquer cena diretamente sem redirecionar para o MainMenu.
        return;
#endif
        if (menuLoadRequested)
        {
            return;
        }

        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && activeScene.name == MenuSceneName)
        {
            return;
        }

        menuLoadRequested = true;
        SceneManager.LoadScene(MenuSceneName);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != MenuSceneName)
        {
            return;
        }

        if (FindObjectOfType<MainMenuVideoController>() != null)
        {
            return;
        }

        var controller = new GameObject("MainMenuVideoController");
        controller.AddComponent<MainMenuVideoController>();
    }

    private void Start()
    {
        BuildUi();
        SetupVideo();
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    private void BuildUi()
    {
        EnsureEventSystem();
        EnsureMenuCamera();

        var canvasObject = new GameObject("MainMenuCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var videoObject = new GameObject("Video");
        videoObject.transform.SetParent(canvasObject.transform, false);
        videoImage = videoObject.AddComponent<RawImage>();
        videoImage.color = Color.white;
        var videoRect = videoImage.rectTransform;
        videoRect.anchorMin = Vector2.zero;
        videoRect.anchorMax = Vector2.one;
        videoRect.offsetMin = Vector2.zero;
        videoRect.offsetMax = Vector2.zero;

        var buttonObject = new GameObject("PlayButton");
        buttonObject.transform.SetParent(canvasObject.transform, false);
        buttonGroup = buttonObject.AddComponent<CanvasGroup>();
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;
        var buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = Color.white;

        var sprite = LoadButtonSprite();
        if (sprite != null)
        {
            buttonImage.sprite = sprite;
            buttonImage.preserveAspect = true;
        }

        playButton = buttonObject.AddComponent<Button>();
        playButton.targetGraphic = buttonImage;
        
        var colors = playButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        playButton.colors = colors;

        playButton.onClick.AddListener(LoadGameScene);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.15f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.15f);
        
        if (sprite != null)
        {
            float aspect = (float)sprite.texture.width / sprite.texture.height;
            float height = 240f;
            buttonRect.sizeDelta = new Vector2(height * aspect, height);
        }
        else
        {
            buttonRect.sizeDelta = new Vector2(260f, 60f);
        }
        buttonRect.anchoredPosition = Vector2.zero;
    }

    private Sprite LoadButtonSprite()
    {
        var streamingPath = Path.Combine(Application.streamingAssetsPath, ButtonImageName);
        if (!File.Exists(streamingPath))
        {
            streamingPath = Path.Combine(Application.dataPath, ButtonImageName);
        }

        if (File.Exists(streamingPath))
        {
            byte[] bytes = File.ReadAllBytes(streamingPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }

    private void SetupVideo()
    {
        var videoObject = new GameObject("VideoPlayer");
        videoObject.transform.SetParent(transform, false);
        videoPlayer = videoObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        var audioSource = videoObject.AddComponent<AudioSource>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        renderTexture = CreateRenderTexture();
        videoPlayer.targetTexture = renderTexture;
        videoImage.texture = renderTexture;

        var url = ResolveVideoUrl();
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("Menu video not found. Ensure the file exists in StreamingAssets.");
            playButton.gameObject.SetActive(true);
            return;
        }

        videoPlayer.url = url;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnLoopPointReached;
        videoPlayer.Prepare();
    }

    private RenderTexture CreateRenderTexture()
    {
        var width = Mathf.Max(1, Screen.width);
        var height = Mathf.Max(1, Screen.height);
        return new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
        {
            useMipMap = false,
            autoGenerateMips = false
        };
    }

    private string ResolveVideoUrl()
    {
        var streamingPath = Path.Combine(Application.streamingAssetsPath, VideoFileName);
        if (File.Exists(streamingPath))
        {
            return streamingPath;
        }

        var editorPath = Path.Combine(Application.dataPath, VideoFileName);
        if (File.Exists(editorPath))
        {
            return editorPath;
        }

        return null;
    }

    private void OnPrepared(VideoPlayer player)
    {
        player.Play();
    }

    private void OnLoopPointReached(VideoPlayer player)
    {
        player.Pause();
        StartCoroutine(FadeInButton());
    }

    private IEnumerator FadeInButton()
    {
        const float fadeDuration = 0.6f;
        float elapsed = 0f;

        buttonGroup.gameObject.SetActive(true);
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            buttonGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        buttonGroup.alpha = 1f;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private void EnsureMenuCamera()
    {
        if (Camera.main != null)
        {
            return;
        }

        var cameraObject = new GameObject("MenuCamera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.depth = -10;
        camera.tag = "MainCamera";
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(TargetSceneName);
    }
}
