using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInput 
{

    //每帧调用，用于按键和飞轮的脉冲检测，不含旋转角度部分
    void HandleBtnData();

    //获取设备类型
    DeviceType GetDeviceType();

    //获取蓝牙名字
    string GetCurBleName();

    //获取连接状态
    ConnectStatus GetStatus();

    //获取心率
    int GetHeartRate();

    //获取运动时长
    int GetMotionTime();

    //获取骑行速度或跑步机速度
    int GetRollSpeed();

    //获取骑行距离或跑步机距离
    int GetRollDistance();

    //获取卡路里
    int GetCalories();

    //获取车子阻力或跑步机坡度
    int GetResistance();

    //获取车把水平角度
    float GetHorizontalAngle();

    //获取左键的按下、按住、抬起三个状态
    bool GetLeftBtn_DOWN();
    bool GetLeftBtn_PRESSED();
    bool GetLeftBtn_UP();

    //获取右键的按下、按住、抬起三个状态
    bool GetRightBtn_DOWN();
    bool GetRightBtn_PRESSED();
    bool GetRightBtn_UP();

    //获取车轮脉冲
    bool GetWheelPulse();

    //阻力+1 或 跑步机坡度+1
    void AddOneLevelResistance();

    //阻力-1 或 跑步机坡度-1
    void DecreaseOneLevelResistance();

    //设置车子阻力或跑步机坡度
    void SetResistance(int resistance);

    //设置跑步机速度
    void SetTreadmillSpeed(int speed);

    //清除运动数据
    void ClearMotionData();

    //断开释放设备
    void Dispose();

}
