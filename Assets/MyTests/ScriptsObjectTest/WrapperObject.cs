using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

[Serializable]
public abstract class BaseDataInfo:UnityEngine.Object{
    int id ;
    string name;
}


[Serializable]
public class DataInfo:BaseDataInfo{
    string test = "123";
}

[Serializable]
public class UnityWrapperObject: ScriptableObject
{
    [SerializeField]
    public UnityEngine.Object d;

    // [CreateAssetMenu(fileName = "new TestWrapped" ,menuName = "Test/tmpWrapped")]
    // public class WrappedData: ScriptableObject{
    //     public T d;
    // }

    // public WrappedData GetWrappedData(){
    //     return new WrappedData();
    // }

}
