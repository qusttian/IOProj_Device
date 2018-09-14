using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device_Treadmill : Device
{

    public Device_Treadmill() 
    {
        
        //当平台为IOS时
       

        //当平台为Android时
       

    }
    public override void Dispose()
    {
        Debug.Log("执行了 Device_Treadmill 的 Dispose()");
    }

    public override void UpdateMotionData(string hexData)
    {
        Debug.Log("Device_Treadmill UpdataMotionData :" + hexData);
    }

    public override void ClearMotionData()
    {
        Debug.Log("执行了 Device_Treadmill 的 WriteData()");
    }
}
