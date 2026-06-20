using UnityEngine;
using UnityEngine.UI;

public class SceneJumpButton : MonoBehaviour
{
    public enum JumpTarget
    {
        Main, AIBattle, BuildDeck, TestCard, Victory, Defeat,Quit
    }

    [Header("跳转目标")]
    public JumpTarget target;

    void Start()
    {
        Button btn = GetComponent<Button>();
        // 动态绑定点击事件，永远指向全局单例
        btn.onClick.AddListener(OnJump);
    }

    void OnJump()
    {
        if (UISceneLoader.Instance == null)
        {
            Debug.LogError("场景管理器单例丢失，请检查初始化逻辑");
            return;
        }

        switch (target)
        {
            case JumpTarget.Main:
                UISceneLoader.Instance.GoToMainScene();
                break;
            case JumpTarget.AIBattle:
                UISceneLoader.Instance.GoToAIBattleScene();
                break;
            case JumpTarget.BuildDeck:
                UISceneLoader.Instance.GoToBuildDeckScene();
                break;
            case JumpTarget.TestCard:
                UISceneLoader.Instance.GoToTestCardScene();
                break;
            case JumpTarget.Victory:
                UISceneLoader.Instance.GoToVictoryScene();
                break;
            case JumpTarget.Defeat:
                UISceneLoader.Instance.GoToDefeatScene();
                break;
            case JumpTarget.Quit:
                UISceneLoader.Instance.ExitGame();
                break;
        }
    }
}