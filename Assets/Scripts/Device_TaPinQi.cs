using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device_TaPinQi : Device
{


    //记录踏频器传入时每次的circle 和 circletime
    private int Data1Circle2 = 0;
    private int Data1Circle2Time = 0;
    private int Data2Circle2 = 0;
    private int Data2Circle2Time = 0;
    private int tempRPM = 0;
    private int RPM = 0;
    //private int lastRPM = 0;


    //构造函数
    public Device_TaPinQi()
    {
        speedScale = 0.5f;
        _connection = Connection._Instance;
        _Instance = this;
        Debug.Log("执行完成 Device_TaPinQi 的构造函数");
    }


    //返回设备类型，踏频器属于Bike类型
    public override DeviceType GetDeviceType()
    {
        return DeviceType.Bike;
    }

    //获取蓝牙名字
    public override string GetCurBleName()
    {
        return base.GetCurBleName();
    }

    //获取连接状态
    public override ConnectStatus GetStatus()
    {
        return base.GetStatus();
    }

	//获取骑行速度
	public override int GetRollSpeed()
	{
        return (int)(RPM*speedScale);
	}

    //更新运动数据
	public override void UpdateMotionData(string hexData)
    {
        HandleHexString(hexData);
    }

    //初始十六进制指令字符串
    public void HandleHexString(string hexString)
    {
        if (string.IsNullOrEmpty(hexString))
        {
            Debug.LogWarning("处理的指令字符串为空");
            return;
        }
        hexString = hexString.ToUpper();
        int length = hexString.Length / 2;
        char[] hexChars = hexString.ToCharArray();
        byte[] bytes = new byte[length];

        for (int i = 0; i < length; i++)
        {
            int pos = i * 2; // 两个字符对应一个byte
            int h = hexDigits.IndexOf(hexChars[pos]) << 4; // 将16进制字符对应的10进制值转换成二进制再左移4位
            int l = hexDigits.IndexOf(hexChars[pos + 1]); // 注2
            if (h == -1 || l == -1) // 非16进制字符
                return;
            bytes[i] = (byte)(h | l);  //将h和l 所对应的二进制按位或运算。

        }

        Data2Circle2 = bytes[8] * 256 + bytes[7];
        Data2Circle2Time = bytes[10] * 256 + bytes[9];

        tempRPM = (Data1Circle2 - Data2Circle2) * 60000 / (Data1Circle2Time - Data2Circle2Time);
        RPM = (tempRPM > 100 ? 30 : tempRPM);
        Debug.Log("计算得到的 RPM = " + RPM);
        Data1Circle2 = Data2Circle2;
        Data1Circle2Time = Data2Circle2Time;
    }
}
