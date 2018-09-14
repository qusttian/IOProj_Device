using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : IInput 
{
    private float curRideSpeed = 0; //当前速度
    private float maxRideSpeed = 40;    //最大骑行速度(km)

    //每帧调用，用于按键和飞轮的脉冲检测，不含旋转角度部分
    public void HandleBtnData(){}

    public  DeviceType GetDeviceType() 
    {
        return DeviceType.Bike; 
    }

    public  string GetCurBleName() 
    { 
        return "NoBLE"; 
    }

    public  ConnectStatus GetStatus() 
    { 
        return ConnectStatus.Unconnected; 
    }

    public  int GetHeartRate() 
    { 
        return 0; 
    }

    public  int GetMotionTime() 
    { 
        return 0; 
    }
    public  int GetRollSpeed() 
    { 
        if (Input.GetKey(KeyCode.UpArrow))
            curRideSpeed = Mathf.MoveTowards(curRideSpeed, maxRideSpeed, 10 * Time.fixedDeltaTime);
        else
            curRideSpeed = Mathf.MoveTowards(curRideSpeed, 0, 10 * Time.fixedDeltaTime);
        return (int)curRideSpeed;
    }
    public  void AddOneLevelResistance() { }
    public  void DecreaseOneLevelResistance() { }
    public virtual int GetRollDistance() { return 0; }
    public virtual int GetCalories() { return 0; }
    public virtual float GetHorizontalAngle() { return 0f; }
    public virtual void ClearMotionData() { }
    public virtual int GetResistance() { return 1; }
    public virtual void SetResistance(int resistance) { }
    public virtual void SetTreadmillSpeed(int speed) { }
    public virtual void Dispose() { }

    //获取左键的按下、按住、抬起三个状态
    public virtual bool GetLeftBtn_DOWN() { return false; }
    public virtual bool GetLeftBtn_PRESSED() { return false; }
    public virtual bool GetLeftBtn_UP(){ return false; }

    //获取右键的按下、按住、抬起三个状态
    public virtual bool GetRightBtn_DOWN(){ return false; }
    public virtual bool GetRightBtn_PRESSED(){ return false; }
    public virtual bool GetRightBtn_UP(){ return false; }
    public virtual bool GetWheelPulse() { return false; }

}
