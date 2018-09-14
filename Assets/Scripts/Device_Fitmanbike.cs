using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Device_Fitmanbike : Device
{


    private byte[] buffer = null;  //蓝牙传入的数据是分为两部分传入的，使用buffer保存第一部分，等待读取第二部分

    private int heartRate = 0;
    private int motionTime = 0;
    private int rollSpeed = 0;
    private int rollDistance = 0;
    private int calories = 0;
    private int maxResistance = 10;
    private int minResistance = 0;
    private int curResistance = 1;

    private float horizontalAngle = 0f;
    private float targetHorizontalAngle = 0f;


    //记录上次的状态
    private int lastLeftBtn = 0;
    private int lastRightBtn = 0;
    private int lastWheelPulse = 0;

    //记录当前返回值
    private int nowLeftBtn = 0;
    private int nowRightBtn = 0;
    private int nowWheelPulse = 0;


    public Device_Fitmanbike()
    {
        speedScale = 1;
        _connection = Connection._Instance;
        _Instance = this;
        DATA_BUFFER_SIZE = 22;
        Debug.Log("执行完成 Device_Fitmanbike的构造函数");
    }

    //获取设备的类型
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

	//获取心率
	public override int GetHeartRate() 
    { 
        return heartRate; 
    }

    //获取运动时长
    public override int GetMotionTime() 
    { 
        return motionTime; 
    }

    //获取骑行速度
    public override int GetRollSpeed() 
    {
        return (int)(rollSpeed * speedScale);
    }

    //获取骑行距离
    public override int GetRollDistance() 
    {
        return rollDistance;
    }

	//获取卡路里
	public override int GetCalories()
	{
        return calories;
	}

    //------------获取左手按钮的三个状态按住----------------
    public override bool GetLeftBtn_DOWN()
    {
        if (nowLeftBtn == 1 && nowLeftBtn!=lastLeftBtn)
            return true;
        return false;
    }

	public override bool GetLeftBtn_PRESSED()
	{
        if (nowLeftBtn == 1 && nowLeftBtn == lastLeftBtn)
            return true;
        return false;
	}

	public override bool GetLeftBtn_UP()
	{
        if (nowLeftBtn == 0 && nowLeftBtn != lastLeftBtn)
            return true;
        return false;
	}

    //------------获取右手按钮的三个状态按住----------------
    public override bool GetRightBtn_DOWN()
    {
        if (nowRightBtn == 1 && nowRightBtn != lastRightBtn)
            return true;
        return false;
    }

    public override bool GetRightBtn_PRESSED()
    {
        if (nowRightBtn == 1 && nowRightBtn == lastRightBtn)
            return true;
        return false;
    }

    public override bool GetRightBtn_UP()
    {
        if (nowRightBtn == 0 && nowRightBtn != lastRightBtn)
            return true;
        return false;
    }



	//获取脉冲状态-----true有；false无
	public override bool GetWheelPulse()
    {
        if (nowWheelPulse==1 && lastWheelPulse != nowWheelPulse)
            return true;
        return false;
    }


	//获取水平转角 
	public override float GetHorizontalAngle() 
    {
        return horizontalAngle = Mathf.Lerp(horizontalAngle, targetHorizontalAngle * 1.5f, 5 * Time.deltaTime);
    }


    //清除运动数据
    public override void ClearMotionData()
    {
        byte[] data = Portcmd_A2C.GetRequestCtr(1,Portcmd_A2C.ClearDataID);
        SendCmd(data);
    }


    //阻力+1
    public override void AddOneLevelResistance()
    {
        SetResistance(curResistance + 1);
    }

    //阻力-1
    public override void DecreaseOneLevelResistance()
    {
        SetResistance(curResistance - 1);
    }


    //获取车子阻力
    public override int GetResistance()
    {
        return curResistance;
    }

    //设置车子阻力
    public override void SetResistance(int resistance) 
    {
        if (resistance > maxResistance)
        {
            resistance = maxResistance;
        }
        else if (resistance < minResistance)
        {
            resistance = minResistance;
        }
        if (curResistance != resistance)
        {
            byte[] data = Portcmd_A2C.GetRequestCtr(resistance, 0x00);
            SendCmd(data);
        }
    }

    //设置跑步机速度
    public override void SetTreadmillSpeed(int speed) { }

    //断开设备连接
    public override void DisConnect()
    {
        _connection.DisConnectBle();
        Debug.Log("执行了Device_Fitmanbike的 Dispose()");
    }




	//*****************************************************************************

    //每帧调用，用于按键和飞轮的脉冲检测，不含旋转角度部分
	public override void HandleBtnData()
	{
        if (motionData != null)
        {
            //左侧按键更新
            if (lastLeftBtn != nowLeftBtn)
                lastLeftBtn = nowLeftBtn;
            nowLeftBtn = motionData[MotionDataOrder.LeftBtn];

            //右侧按键更新
            if (lastRightBtn != nowRightBtn)
                lastRightBtn = nowRightBtn;
            nowRightBtn = motionData[MotionDataOrder.RightBtn];

            //飞轮脉冲
            if (lastWheelPulse != nowWheelPulse)
                lastWheelPulse = nowWheelPulse;
            nowWheelPulse = motionData[MotionDataOrder.WheelPulse];
        }
	}


	//根据蓝牙传入的值进行调用，调用频率大于Update的频率
	public override void UpdateMotionData(string hexData)
    {
        HandleHexString(hexData);
    }

    //对校验后的数据进行解析
    private float f = 0f;//声明在外面是为了频繁的寻址，减少CPU消耗
    private void AnalyzeMotionData()
    {
       
        rollSpeed = motionData[MotionDataOrder.Speed];
        rollDistance = motionData[MotionDataOrder.DistanceH] * 256 + motionData[MotionDataOrder.DistanceL];
        curResistance = motionData[MotionDataOrder.CurrentResistance];
        calories = motionData[MotionDataOrder.CaloriesH] * 256 + motionData[MotionDataOrder.CaloriesL];

        //水平角度整数部分
        targetHorizontalAngle = motionData[MotionDataOrder.HorizontalAngle];
        //水平角度小数部分  
        f = motionData[MotionDataOrder.HorizontalAngleFloat] / 10f;
        targetHorizontalAngle = targetHorizontalAngle >= 90 ? 90 - targetHorizontalAngle - f : targetHorizontalAngle + f;
    }


    //初始十六进制指令字符串

    private void HandleHexString(string hexString)
    {
        //Debug.Log("HexString = " + hexString);
        if (string.IsNullOrEmpty(hexString))
        {
            Debug.LogWarning("处理的指令字符串为空");
            return;
        }
        //将16进制中的小写字母转换成大写字母，防止因大小写原因导致转换错误
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


        if (bytes[0] == MotionDataOrder.Start1 && bytes[1] == MotionDataOrder.Start2)
        {
            buffer = bytes;
            return;
        }
        //else
        //{
            //Debug.Log("收到一次不完整数据，已 return");
            //return;
        //}

        else if (buffer != null)
        {
            buffer = buffer.Concat(bytes).ToArray();    //后面的
        }

        //对接收到的数据进行校验和解析
        CheckoutAndAnalyzeData(buffer);
    }

    PortMessage msg = new PortMessage(); //规避 PortMessage结构体对象的重复创建，减小开支
    public override void CheckoutAndAnalyzeData(byte[] bytes)
    {
        if (msg.isHandleBytes(bytes))
        {
            this.motionData = msg.data;    //保存数据
            //Debug.Log("Unity=> 已将校验后的值更新到 motionData = " + motionData);
            AnalyzeMotionData();
        }
    }



    //发送指令
    private void SendCmd(byte[] data)
    {
         _connection.WriteDataToBle(data);
    }
}

//App => Device
public class Portcmd_A2C
{
    
    private const int CmdResuestCtr = 0x02;//要求设备控制
    private const int CmdRequestReset = 0x03;//要求车把复位
    public const int ClearDataID = 0x01;  //清除运动资料


    //获得请求清空数据命令
    public static byte[] GetRequestCtr(int resistanceForce, int cmd)
    {
        PortMessage pm = new PortMessage();
        pm.cmd = CmdResuestCtr;
        pm.len = 0x02;
        pm.data = new int[] { resistanceForce, cmd };
        pm.checksum = (pm.cmd + pm.len + resistanceForce + cmd) & 0xFF;
        return pm.GetBytes();
    }


    //获得请求车把复位命令
    public static byte[] GetRequestReset()
    {
        PortMessage pm = new PortMessage();
        pm.cmd = CmdRequestReset;
        pm.len = 0x01;
        pm.data = new int[] { 0x00 };
        pm.checksum = (pm.cmd + pm.len + 0x00) & 0xFF;
        return pm.GetBytes();
    }

}

//Device => App
public class Portcmd_C2A
{
    public const int HFYDZL = 0x12;//回覆运动数据
}

[Serializable]
public struct PortMessage
{
    public int cmd;
    public int len;
    public int[] data;
    public int checksum;

    //校验数据,返回True表示数据解析正确
    public bool isHandleBytes(byte[] bytes)
    {
        //Debug.Log("开始执行数据校验指令:  "+BitConverter.ToString(bytes));
        if (bytes.Length == 0)
        {
            Debug.Log("开始执行数据校验指令----数据长度为0");
            return false;
        }
            
        //头部信息不正确
        if (bytes[0] != MotionDataOrder.Start1 || bytes[1] != MotionDataOrder.Start2)
        {
            Debug.Log("开始执行数据校验指令----头部信息不正确");
            return false;
        }


        cmd = bytes[2];
        len = bytes[3];
        //长度不够
        if (bytes.Length < len + 5)
        {
            Debug.Log("开始执行数据校验指令----数据长度不够");
            return false;
        }


        data = new int[len];
        checksum = cmd + len;
        for (int i = 0; i < len; i++)
        {
            data[i] = bytes[i + 4];
            checksum += data[i];
        }
        checksum &= 0xFF;   //校验码进行与运算
        if (checksum != bytes[len + 4])
        {
            Debug.Log("开始执行数据校验指令----数据校验错误");
            return false;
        }

        return true;
    }

    //解析成字节数据
    public byte[] GetBytes()
    {
        byte[] bys = new byte[data.Length + 5];
        bys[0] = MotionDataOrder.Start1;
        bys[1] = MotionDataOrder.Start2;
        bys[2] = (byte)cmd;
        bys[3] = (byte)len;
        for (int i = 0; i < data.Length; i++)
        {
            bys[4 + i] = (byte)data[i];
        }
        bys[bys.Length - 1] = (byte)checksum;

        return bys;
    }
}


//新车的数据规则
//[Serializable]
//public class MotionDataOrder
//{
//    public const int Start1 = 0xFE;    //协议头部消息
//    public const int Start2 = 0xAB;    //协议头部消息

//    public const int HorizontalAngle = 0;
//    public const int HorizontalAngleFloat = 1;
//    public const int LeftBtn = 2;
//    public const int RightBtn = 3;
//    public const int CurrentResistance = 4;
//    public const int WheelPulse =5;
//    public const int Speed = 6;
//    public const int DistanceH = 7;
//    public const int DistanceL = 8;
//    public const int CaloriesH = 9;
//    public const int CaloriesL = 10;

//}

//原车的数据规则
[Serializable]
public class MotionDataOrder
{
    public const int Start1 = 0xfe;    //协议头部消息
    public const int Start2 = 0xab;    //协议头部消息
    public const int HorizontalAngle = 0;
    public const int VerticAlangle = 1;
    public const int LbuttonL = 2;
    public const int LeftBtn = 3;
    public const int Ldirection = 4;
    public const int RbuttonL = 5;
    public const int RightBtn = 6;
    public const int Rdirection = 7;
    public const int CurrentResistance = 8;
    public const int WheelPulse = 9;
    public const int Speedh = 10;
    public const int Speed = 11;
    public const int DistanceH = 12;
    public const int DistanceL = 13;
    public const int CaloriesH = 14;
    public const int CaloriesL = 15;
    public const int Geartrate = 16;
    public const int HorizontalAngleFloat = 17;
    public const int VerticalAngleFloat = 18;
}
