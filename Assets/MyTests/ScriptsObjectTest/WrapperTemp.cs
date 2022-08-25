using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

[Serializable]
public class UnitySoWrapper<T> :ScriptableObject
{
    public T d;

    // [CreateAssetMenu(fileName = "new TestWrapped" ,menuName = "Test/tmpWrapped")]
    // public class WrappedData: ScriptableObject{
    //     public T d;
    // }

    // public WrappedData GetWrappedData(){
    //     return new WrappedData();
    // }


    public class WrapperClass : UnitySoWrapper<T>
    {

    }

    
    public WrapperClass GetWrapperedClass(){
        return  (WrapperClass)this;
    }
  

}
