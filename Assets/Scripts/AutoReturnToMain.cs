using UnityEngine;
using System.Collections;

public class AutoReturnToMain : MonoBehaviour
{
    [Header("自动延时时间（秒）")]
    public float delayTime = 3f;

    void Start()
    {
        // 场景加载后，自动启动延时返回（原功能保留）
        StartCoroutine(ReturnToMainAfterDelay());
    }

    // ==============================================
    // 【新增】给按钮绑定的公开方法（无参数，Unity可识别）
    // ==============================================
    public void OnClickReturnToMain()
    {
        // 直接调用返回逻辑
        DoReturnToMain();
    }

    // 延时返回的协程
    IEnumerator ReturnToMainAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        DoReturnToMain();
    }

    // 统一的返回逻辑（避免重复代码）
    private void DoReturnToMain()
    {
        if (UISceneLoader.Instance != null)
        {
            UISceneLoader.Instance.GoToMainScene();
        }
        else
        {
            Debug.LogError("UISceneLoader 单例不存在！请在主场景挂载UISceneLoader");
            // 兜底跳转
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }
}