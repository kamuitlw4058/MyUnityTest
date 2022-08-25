using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;


public class FirstMenuWindow : OdinMenuEditorWindow
{
    [MenuItem("My Game/My Editor")]
    private static void OpenWindow()
    {
        GetWindow<FirstMenuWindow>().Show();
    }

    
 
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        var uwo =  new UnityWrapperObject();
        uwo.d = new DataInfo();
        tree.Add("TestMenu", new TestMenu());
        tree.Add("DetailedInfoBox", new DetailedInfoBox());





        tree.Add("原始类型",new Data());
        tree.Add("测试泛型So",new UnitySoWrapper<Data>());
        tree.Add("测试泛型So 2",new C1());
        tree.Add("测试Object So",uwo);
        tree.Add("普通分装泛型So",new WrapperData());
        tree.Add("测试List",new OdinListTest());


        var v = new ListToggleClass();
        v.m_data.Add(new toggleClass() );
        tree.Add("toggle test",v);
        var val = ScriptableObject.CreateInstance< UnitySoWrapper<Data>.WrapperClass>();
        if(val == null){
            Debug.Log($"init so failed!");
        }

        return tree;
    }
}



public class TestMenu
{
    [ButtonGroup]
    [GUIColor(255, 1, 1)]
    private void Apply()
    {
        Debug.Log("OnClick Apply");
    }
 
    [ButtonGroup]
    [GUIColor(1, 0.6f, 0.4f)]
    private void Cancel()
    {
        Debug.Log("OnClick Cancel");
    }
    [ButtonGroup]
    [GUIColor(1, 34f, 1f)]
    private void OK()
    {
        Debug.Log("OnClick OK");
    }
 
    [InfoBox("测试颜色变化文本！\n白日依山尽，\n黄河入海流。\n欲穷千里目，\n更上一层楼。\n")]
    [GUIColor("GetButtonColor")]
    [Button("I Am Fabulous", ButtonSizes.Gigantic)]
    private static void IAmFabulous()
    {
        Debug.Log("OnClick IAmFabulous");
    }
 
    [Button(ButtonSizes.Large)]
    [GUIColor("@Color.Lerp(Color.red, Color.green, Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup)))")]
    private static void Expressive()
    {
        Debug.Log("OnClick Expressive");
    }
 
#if UNITY_EDITOR 
    private static Color GetButtonColor()
    {
        Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
        return Color.HSVToRGB(Mathf.Cos((float)UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1, 1);
    }
#endif
}
 
public class DetailedInfoBox
{
    [DetailedInfoBox("点我查看详情...",
        "\n白日依山尽，\n黄河入海流。\n欲穷千里目，\n更上一层楼。\n")]
    public int Field;
}
