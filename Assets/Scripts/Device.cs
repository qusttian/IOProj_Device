using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DeviceType
{
    Bike,//脚踏车类，游戏内会根据此决定实例化骑车角色还是跑步角色
    Treadmill,//跑步机类，
    RowingMachine,//划船器类
}

public abstract class Device:IInput
{

    public static Device _Instance;
    public int[] motionData;
    public Connection _connection;
    public string typeName;
    public bool isInit = false;    //是否初始化的标志位，第一次连接设备时，需要清空数据，断线重连时不需要清空
    protected string hexDigits = "0123456789ABCDEF"; //十六进制的字符，用于解析十六进制指令字符串
    protected float speedScale = 1.0f; //速度缩放比例，用于不同单车或不同跑步机兼容时，使不同单车传入的速度*速度缩放比例后得到统一标准的速度值
    public int DATA_BUFFER_SIZE = 0;//不同设备的协议数据长度

    //每帧调用，用于按键和飞轮的脉冲检测，不含旋转角度部分
    public virtual void HandleBtnData(){}
    public virtual DeviceType GetDeviceType() { return DeviceType.Bike; }
    public virtual string GetCurBleName() { return Connection._Instance.GetCurBleName(); }
    public virtual ConnectStatus GetStatus() { return _connection.CurStatus; }
    public virtual int GetHeartRate() { return 0; }
    public virtual int GetMotionTime() { return 0; }
    public virtual int GetRollSpeed() { return 0; }
    public virtual int GetRollDistance() { return 0; }
    public virtual int GetResistance() { return 1; }
    public virtual int GetCalories() { return 0; }

    //获取左键的按下、按住、抬起三个状态
    public virtual bool GetLeftBtn_DOWN() { return false; }
    public virtual bool GetLeftBtn_PRESSED() { return false; }
    public virtual bool GetLeftBtn_UP() { return false; }

    //获取右键的按下、按住、抬起三个状态
    public virtual bool GetRightBtn_DOWN() { return false; }
    public virtual bool GetRightBtn_PRESSED() { return false; }
    public virtual bool GetRightBtn_UP() { return false; }

    public virtual bool GetWheelPulse() { return false; }

    public virtual float GetHorizontalAngle() { return 0f; }
    public virtual void AddOneLevelResistance() { }
    public virtual void DecreaseOneLevelResistance() { }
    public virtual void ClearMotionData(){}
    public virtual void SetResistance(int resistance){}
    public virtual void SetTreadmillSpeed(int speed){}
    public virtual void DisConnect() {}//断开与设备的连接
    public virtual void Dispose(){}  //是否设备连接所占资源


    public virtual void CheckoutAndAnalyzeData(byte[] bytes) { Debug.Log("执行虚函数校验解析命名"); }
    public abstract void UpdateMotionData(string hexData);


	
}
