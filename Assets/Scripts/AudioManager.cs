using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 全局音频管理器（单例，跨场景不销毁）
/// 负责背景音乐统一管理
/// </summary>
public class AudioManager : MonoSingleton<AudioManager>
{
    [Header("背景音乐播放器")]
    public AudioSource bgmSource;

    [Header("各场景对应BGM")]
    public AudioClip mainBGM;     // 主界面BGM
    public AudioClip battleBGM;   // 战斗场景BGM
    public AudioClip buildDeckBGM;// 卡组构筑BGM
    public AudioClip victoryBGM;  // 胜利界面BGM
    public AudioClip defeatBGM;   // 失败界面BGM

    [Header("音量设置")]
    [Range(0f, 1f)] public float defaultVolume = 0.5f;
    private bool _isMuted = false;

    void Start()
    {
        // 跨场景不销毁，全局唯一
        DontDestroyOnLoad(gameObject);

        // 自动添加AudioSource组件
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true;       // 循环播放
        bgmSource.playOnAwake = false;
        bgmSource.volume = defaultVolume;

        // 监听场景加载，自动切换对应BGM
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 启动默认播放主界面BGM
        PlayBGM(mainBGM);
    }

    /// <summary>
    /// 播放指定背景音乐
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        // 同一首歌不重复播放
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    /// <summary>
    /// 暂停BGM
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource.isPlaying) bgmSource.Pause();
    }

    /// <summary>
    /// 继续播放BGM
    /// </summary>
    public void ResumeBGM()
    {
        if (!bgmSource.isPlaying) bgmSource.UnPause();
    }

    /// <summary>
    /// 停止BGM
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 调节音量（0~1）
    /// </summary>
    public void SetVolume(float volume)
    {
        defaultVolume = Mathf.Clamp01(volume);
        if (!_isMuted) bgmSource.volume = defaultVolume;
    }

    /// <summary>
    /// 切换静音
    /// </summary>
    public void ToggleMute()
    {
        _isMuted = !_isMuted;
        bgmSource.volume = _isMuted ? 0 : defaultVolume;
    }

    /// <summary>
    /// 场景加载完成后，自动切换对应BGM
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 场景名请和你项目里的实际名称完全一致
        switch (scene.name)
        {
            case "MainScene":
                PlayBGM(mainBGM);
                break;
            case "AIBattle":
                PlayBGM(battleBGM);
                break;
            case "BuildDeck":
                PlayBGM(buildDeckBGM);
                break;
            case "VictoryScene":
                PlayBGM(victoryBGM);
                break;
            case "DefeatScene":
                PlayBGM(defeatBGM);
                break;
        }
    }

    private void OnDestroy()
    {
        // 移除事件监听，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}