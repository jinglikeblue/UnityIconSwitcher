package lmd.fireframe;

import android.app.Activity;
import android.app.Application;
import android.content.ComponentName;
import android.content.pm.PackageManager;
import android.util.Log;

public class IconSwitcher {

    private static Activity _unityActivity;

    /**
     * 获取unity项目的上下文
     * @return
     */
    public static Activity getActivity(){
        if(null == _unityActivity) {
            try {
                Class<?> classtype = Class.forName("com.unity3d.player.UnityPlayer");
                Activity activity = (Activity) classtype.getDeclaredField("currentActivity").get(classtype);
                _unityActivity = activity;
            } catch (ClassNotFoundException e) {

            } catch (IllegalAccessException e) {

            } catch (NoSuchFieldException e) {

            }
        }
        return _unityActivity;
    }

    public static void updateAlias(String[] componentNames, int enableIndex){
        Activity activity = getActivity();

        for(int i = 0; i < componentNames.length; i++){
            ComponentName name0 = new ComponentName(activity, componentNames[i]);
            updateAlias(i == enableIndex?true:false, name0);
        }
    }

    /**
     * 更新别名显示
     * @param componentName componentName
     * @param enable 是否启用
     */
    private static void updateAlias(Boolean enable, ComponentName componentName) {
        Activity activity = getActivity();

        int state = enable? PackageManager.COMPONENT_ENABLED_STATE_ENABLED:PackageManager.COMPONENT_ENABLED_STATE_DISABLED;
        if(activity.getPackageManager().getComponentEnabledSetting(componentName) != state){
            activity.getPackageManager().setComponentEnabledSetting(componentName, state, PackageManager.DONT_KILL_APP);
        }
    }
}
