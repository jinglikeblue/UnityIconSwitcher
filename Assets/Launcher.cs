using System.Collections.Generic;
using IconSwitch;
using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviour
{
    List<Button> buttons = new List<Button>();
    private Text textInfo;
    
    void Start()
    {
        var btnsT = GameObject.Find("Buttons").transform;
        textInfo = GameObject.Find("TextInfo").GetComponent<Text>();
        textInfo.text = string.Empty;
        for (int i = 0; i < btnsT.childCount; i++)
        {
            var btn = btnsT.GetChild(i);
            buttons.Add(btn.GetComponent<Button>());
            var idx = i;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                SwitchIcon(idx);
            });
        }
    }

    void SwitchIcon(int idx)
    {
        textInfo.text = $"使用的Icon: {idx}";
        
        IconSwitcher.UseIcon(idx);
    }
}
