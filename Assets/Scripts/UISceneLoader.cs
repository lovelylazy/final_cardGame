using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 全场景跳转管理器（单例，全局唯一）
/// 适配：主界面、AI战斗、卡组构筑、卡牌测试、胜利/失败界面
/// </summary>
public class UISceneLoader : MonoBehaviour
{
    // 单例（全局调用，无需重复挂载）
    public static UISceneLoader Instance;

    // ===================== 【可修改】你的场景名称 =====================
    [Header("场景名称（必须和Unity里一致）")]
    public string mainScene = "MainScene";             // 主菜单场景
    public string aiBattleScene = "AIBattle";     // AI对战场景
    public string buildDeckScene = "BuilDeck";   // 卡组构筑场景
    public string testCardScene = "TestCardData"; // 卡牌抽取场景
    public string victoryScene = "VictoryScene";       // 胜利界面
    public string defeatScene = "DefeatScene";         // 失败界面

    private void Awake()
    {
        // 单例初始化 + 跨场景不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===================== 场景跳转方法（按钮直接绑定） =====================
    /// <summary> 跳转到主菜单 </summary>
    public void GoToMainScene()
    {
        SceneManager.LoadScene(mainScene);
        Debug.Log("按钮被点击了");
    }

    /// <summary> 跳转到AI对战 </summary>
    public void GoToAIBattleScene()
    {
        SceneManager.LoadScene(aiBattleScene);
        Debug.Log("按钮被点击了");
    }

    /// <summary> 跳转到卡组构筑 </summary>
    public void GoToBuildDeckScene()
    {
        SceneManager.LoadScene(buildDeckScene);
        Debug.Log("按钮被点击了");
    }

    /// <summary> 跳转到卡牌测试 </summary>
    public void GoToTestCardScene()
    {
        SceneManager.LoadScene(testCardScene);
        Debug.Log("按钮被点击了");
    }

    /// <summary> 跳转到胜利界面 </summary>
    public void GoToVictoryScene()
    {
        SceneManager.LoadScene(victoryScene);
        Debug.Log("按钮被点击了");
    }

    /// <summary> 跳转到失败界面 </summary>
    public void GoToDefeatScene()
    {
        SceneManager.LoadScene(defeatScene);
        Debug.Log("按钮被点击了");
    }

    /// <summary> 重新开始AI对战（战斗失败/胜利后常用） </summary>
    public void RestartAIBattle()
    {
        SceneManager.LoadScene(aiBattleScene);
        Debug.Log("按钮被点击了");
    }

    /// <summary> 退出游戏 </summary>
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("游戏已退出");
        Debug.Log("按钮被点击了");
    }
}