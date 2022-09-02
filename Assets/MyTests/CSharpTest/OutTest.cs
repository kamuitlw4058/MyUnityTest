using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutTest : MonoBehaviour
{
    Dictionary<int,int> dic = new Dictionary<int, int>();
    // Start is called before the first frame update
    void Start()
    {
        dic[1] = 10;
        Debug.Log($"dic1:{dic[1]}");
        if(dic.TryGetValue(1,out int ret)){
            ret = 100;
            dic[1] =100;
        }
        Debug.Log($"ret:{ret},dic1:{dic[1]}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
