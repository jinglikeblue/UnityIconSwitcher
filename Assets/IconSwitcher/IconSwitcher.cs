using System;
using System.Collections.Generic;
using UnityEngine;

namespace IconSwitch
{
    /// <summary>
    /// 图标切换实用工具
    /// </summary>
    public static class IconSwicher
    {
        /// <summary>
        /// 图标名字配置
        /// </summary>
        public static readonly string[] IconNameConfigs = new string[] { "icon_1", "icon_2" };

        public static readonly string[] IconNames = null;

        /// <summary>
        /// 为false时Refresh不会生效。一般进入战斗时，不允许Refresh执行
        /// </summary>
        public static bool refreshEnable = true;


#if UNITY_IPHONE
        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern void SwitchAppIcon(string iconName);
#endif

        static string GetAndroidMainActivityName()
        {
            string name = string.Empty;

#if UNITY_ANDROID
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                // 获取当前活动的主活动
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // 获取类对象
                    using (AndroidJavaObject classObject = currentActivity.Call<AndroidJavaObject>("getClass"))
                    {
                        // 获取类名
                        string mainActivityName = classObject.Call<string>("getName");
                        Debug.Log("Main Activity: " + mainActivityName);
                        name = mainActivityName;
                    }
                }
            }
#endif
            return name;
        }

        static IconSwicher()
        {
#if UNITY_EDITOR
            IconNames = Array.Empty<string>();
            return;
#endif

            List<string> icons = new List<string>();

#if UNITY_ANDROID
            //构建Android的别名数组
            icons.Add(GetAndroidMainActivityName());
            foreach (var iconName in IconNameConfigs)
            {
                icons.Add($".{iconName}_Activity");
            }
#endif

#if UNITY_IPHONE
            //构建iOS的图标配置
            icons.Add(string.Empty);
            foreach (var iconName in IconNameConfigs)
            {
                icons.Add($"{iconName}");
            }
#endif
            IconNames = icons.ToArray();
            Debug.Log($"[Icon Switcher] 图标集合:{string.Join(',', icons)}");
        }

        private static int toUseIconIndex = -1;

        /// <summary>
        /// 实用指定索引对应的Icon
        /// </summary>
        /// <param name="alias"></param>
        public static void UseIcon(int index)
        {
            if (0 == IconNames.Length)
            {
                return;
            }

            if (index < 0 || index >= IconNames.Length)
            {
                index = 0;
            }

            toUseIconIndex = index;

#if UNITY_ANDROID
            AndroidIconSwitchComponent.Create();
#endif
            
#if UNITY_IPHONE
            SwitchAppIcon(IconNames[index]);
#endif
        }

        /// <summary>
        /// 建议在应用进入后台的时候调用，该操作会导致APP闪退
        /// </summary>
        public static void Refresh()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                //该方法只在Android下有用
                return;
            }

            Debug.Log("[Icon Switcher] 刷新图标显示");

            if (false == refreshEnable)
            {
                return;
            }

            if (-1 == toUseIconIndex)
            {
                return;
            }

            string[] componentNames = new string[IconNames.Length];
            for (int i = 0; i < IconNames.Length; i++)
            {
                if (IconNames[i].StartsWith("."))
                {
                    componentNames[i] = Application.identifier + IconNames[i];
                }
                else
                {
                    componentNames[i] = IconNames[i];
                }
            }


            Debug.Log($"[Icon Switcher] 使用的图标配置: {toUseIconIndex} : {componentNames[toUseIconIndex]}");

#if UNITY_ANDROID
            var javaCls = new AndroidJavaClass("lmd.fireframe.IconSwitcher");
            javaCls.CallStatic("updateAlias", componentNames, toUseIconIndex);
            toUseIconIndex = -1;
#endif
        }
    }
}