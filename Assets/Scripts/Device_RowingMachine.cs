using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Device_RowingMachine : Device
{
    private byte[] buffer = null;
    private int pullSpeed = 0;
    private int pullTimes = 0;
    private float horizontalAngle = 0f;
    private float targetHorizontalAngle = 0f;


    public Device_RowingMachine()
    {
        speedScale = 1;
        _connection = Connection._Instance;
        _Instance = this;
        DATA_BUFFER_SIZE = 9;
        Debug.Log("执行完成 Device_RowingMachine 的构造函数");
    }


	//获取设备类型
	public override DeviceType GetDeviceType()
	{
        return DeviceType.RowingMachine;
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

	//获取拉拽速度
	public override int GetRollSpeed()
    {
        return (int)(pullSpeed * speedScale);
    }

    //获取累计次数
    public override int GetRollDistance()
    {
        return pullTimes;
    }

    //获取水平转角
    public override float GetHorizontalAngle()
    {
        return horizontalAngle = Mathf.Lerp(horizontalAngle, targetHorizontalAngle * 1.5f, 5 * Time.deltaTime);
    }


    //断开设备连接
    public override void DisConnect()
    {
        _connection.DisConnectBle();
        Debug.Log("执行了Device_RowingMachine 的 Dispose()");
    }

    //清空运动数据
    public override void ClearMotionData()
    {
        byte[] clearCmdData = new byte[6];
        clearCmdData[0] = 0xFE;
        clearCmdData[1] = 0xAB;
        clearCmdData[2] = 0x03;
        clearCmdData[3] = 0x01;
        clearCmdData[4] = 0x00;
        clearCmdData[5] = 0x04;
        _connection.WriteDataToBle(clearCmdData);
    }


    //更新运动数据
    public override void UpdateMotionData(string hexData)
    {
        HandleHexString(hexData);
    }

    //对校验后的数据进行解析
    private void AnalyzeMotionData()
    {

        pullSpeed = motionData[RawingMachineDataOrder.PullSpeed] ;
        pullTimes = motionData[RawingMachineDataOrder.PullTimes] ;

        //水平角度
        if(motionData[RawingMachineDataOrder.AngleSign]==0)
        {
            targetHorizontalAngle = motionData[RawingMachineDataOrder.AngleData];
        }
        if(motionData[RawingMachineDataOrder.AngleSign]==1)
        {
            targetHorizontalAngle = -motionData[RawingMachineDataOrder.AngleData];
        }

    }


    //初始十六进制指令字符串
    private void HandleHexString(string hexString)
    {
        Debug.Log("hexString = " + hexString);
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


        if (bytes[0] == RawingMachineDataOrder.Start1 && bytes[1] == RawingMachineDataOrder.Start2)
        {
            buffer = bytes;
            CheckoutAndAnalyzeData(buffer);
        }
        else
        {
            return;
        }
    }

    //对接收的数据进行校验和解析
    RawingMachineMsg msg = new RawingMachineMsg();
    public override void CheckoutAndAnalyzeData(byte[] bytes)
    {
        if (msg.isHandleBytes(bytes))
        {
            this.motionData = msg.data;    //保存数据
            Debug.Log("Unity=> 已将校验后的值更新到 motionData = " + motionData);
            AnalyzeMotionData();
        }
    }
}





[Serializable]
public struct RawingMachineMsg
{
    public int cmd;
    public int len;
    public int[] data;
    public int checksum;

    //校验数据,返回True表示数据解析正确
    public bool isHandleBytes(byte[] bytes)
    {
        //Debug.Log("开始执行数据校验指令:  "+BitConverter.ToString(bytes));
        //头部信息不正确
        if (bytes[0] !=   RawingMachineDataOrder.Start1 || bytes[1] != RawingMachineDataOrder.Start2)
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

   
}

[Serializable]
public class RawingMachineDataOrder
{
    public const int Start1 = 0xFE;     //协议头部消息
    public const int Start2 = 0xAB;     //协议头部消息
    public const int PullSpeed = 0;     //
    public const int PullTimes = 1;
    public const int AngleSign = 2;
    public const int AngleData = 3;

}