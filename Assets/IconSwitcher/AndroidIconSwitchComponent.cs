using IconSwitch;
using UnityEngine;

public class AndroidIconSwitchComponent : MonoBehaviour
{
    private static GameObject _ins = null;

    public static void Create()
    {
#if !UNITY_EDITOR
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
#endif

        if (null == _ins)
        {
            var obj = new GameObject("IconSwitcher");
            obj.AddComponent<AndroidIconSwitchComponent>();
            DontDestroyOnLoad(obj);
            _ins = obj;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"[Icon Switcher][前后台切换] 进行图标切换刷新");
        if (IconSwicher.refreshEnable)
        {
            IconSwicher.Refresh();
            Destroy(gameObject);
        }
    }
}