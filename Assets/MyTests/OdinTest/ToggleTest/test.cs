using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

[Serializable]
class ListToggleClass{
     [TableList(AlwaysExpanded = true)]
    public List<toggleClass> m_data = new List<toggleClass>();

}

[Serializable]
class toggleClass{
// Simple Toggle Group
    [ToggleGroup("MyToggle")]
    public bool MyToggle;

    [ToggleGroup("MyToggle")]
    public float A;

    [ToggleGroup("MyToggle")]
    [HideLabel, Multiline]
    public string B;

    // Toggle for custom data.
    // [ToggleGroup("EnableGroupOne", "$GroupOneTitle")]
    // public bool EnableGroupOne = true;

    // [ToggleGroup("EnableGroupOne")]
    // public string GroupOneTitle = "One";

    // [ToggleGroup("EnableGroupOne")]
    // public float GroupOneA;

    // [ToggleGroup("EnableGroupOne")]
    // public float GroupOneB;

    // Toggle for individual objects.
    // [Toggle("Enabled")]
    // public MyToggleObject Three = new MyToggleObject();

    // [Toggle("Enabled")]
    // public MyToggleA Four = new MyToggleA();

    // [Toggle("Enabled")]
    // public MyToggleB Five = new MyToggleB();

    [ToggleGroup("MyToggle1")]
    public bool MyToggle1;

   [TableList(AlwaysExpanded = true)][ToggleGroup("MyToggle1")]
    public MyToggleC[] ToggleList = new MyToggleC[]
    {
        new MyToggleC(){ Test = 2f, Enabled = true, },
        new MyToggleC(){ Test = 5f, },
        new MyToggleC(){ Test = 7f, },
    };

    // [Serializable]
    // public class MyToggleObject
    // {
    //     public bool Enabled;

    //     [HideInInspector]
    //     public string Title;

    //     public int A;
    //     public int B;
    // }

    // [Serializable]
    // public class MyToggleA : MyToggleObject
    // {
    //     public float C;
    //     public float D;
    //     public float F;
    // }

    // [Serializable]
    // public class MyToggleB : MyToggleObject
    // {
    //     public string Text;
    // }

    [Serializable]
    public class MyToggleC
    {
        [ToggleGroup("Enabled", "$Label")]
        public bool Enabled;

        public string Label { get { return this.Test.ToString(); } }

        [ToggleGroup("Enabled")]
        public float Test;

        public string t = "test";
    }
}