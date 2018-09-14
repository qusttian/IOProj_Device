using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

// 本结构蓝牙连接过程：
// 1、先根据设备配置类，将所有的过滤关键字传入IOS 或 Android 处理类中
// 2、启动蓝牙并开始扫描，根据传入的过滤关键字，将过滤后的蓝牙名字显示出并保存对应的identifier
// 3、用户点击蓝牙列表的某一项，根据所点击的蓝牙名字，在配置表中找出所以点击配置的UUID和对应的设备议处理类名
// 4、将UUID传入IOS 或 Android,为下一步的连接查找服务做准备
// 5、根据步骤3得到的设备协议类名创建对应的设备类，用于解析蓝牙传入的数据


//连接状态
public enum ConnectStatus
{
    Unconnected,   //未连接
    Connecting, //连接中
    Connected,  //已连接
    ConnectOff, //连接中断
}


//提示信息的显示时长
public enum IOSToastLength
{
    LENGTH_SHORT = 0,
    LENGTH_LONG = 1
}


public abstract class Connection:MonoBehaviour
{
    //-----------------------蓝牙面板相关--------------------------

    
    private string curBleMacAddress;
    private string curBleUUID;
    private string curDeviceClassName;
    private bool autoConnect = true;   //是否自动连接至保存的蓝牙


    private Camera mCamera;
    private Transform mUIParent = null;//蓝牙父对象
    private GameObject mBluetoothUI; //蓝牙UI


    //蓝牙UI显示隐藏
    public delegate void DeleBluetoothActive(bool active);
    public DeleBluetoothActive OnBluetoothActive;   //蓝牙界面显隐的回调
    //private Dictionary<ConnectStatus, Image> dictStatusImg = new Dictionary<ConnectStatus, Image>();    //显示蓝牙连接状态的图片
    //------------------------------------------------------------


    public ConnectStatus CurStatus = ConnectStatus.Unconnected;    //当前的连接状态
    public static Connection _Instance;

    private string bleName="NoName"; 

    public abstract void InitBleFilter(string filters);
    public abstract void StartBle();
    public abstract void SetBleUUID(string uuids);
    public abstract void DiscoverBle();
    public abstract void ConnectBle(string identifier);
    public abstract void WriteDataToBle(byte[] data);
    public abstract void DisConnectBle();
    public abstract void DisposeBle();


    public void InitBluetoothUI()
    {
        mCamera = Camera.main != null ? Camera.main : GameObject.Find("Head").GetComponent<Camera>();
        mUIParent = GameObject.Find("Canvas").transform;
        CreateBluetoothUI();
    }

    // 通过反射利用类名实例化对应的类对象
    public void SetDevice(string className)    
    {
        curDeviceClassName = className;
        Type type = Type.GetType(className, true);
        if(Device._Instance!=null)
        {
            Device._Instance = null;
        }
        Device._Instance = Activator.CreateInstance(type) as Device;
    }

   

    // 引用Device类数据更新函数
    public void SetDataListener(string dataStr)
    {
        Device._Instance.UpdateMotionData(dataStr);
    }

    //----------------------IOS回调方法--------------------------

    //接收IOS传入的蓝牙初始化信息
    public void CallbackByIOSForBLEInitStatus(string arg)
    {
        int bleInitStatus = int.Parse(arg[0].ToString());
        switch (bleInitStatus)
        {
            case -1:
                Debug.Log("Unity=>设备不支持蓝牙功能！");
                if (UIBluetooth._Instance != null)
                {
                    UIBluetooth._Instance.ShowTip("该设备不支持蓝牙功能");
                }
                break;
            case 0:
                Debug.Log("Unity=>蓝牙功能没有开启");
                if (UIBluetooth._Instance != null)
                {
                    UIBluetooth._Instance.ShowTip("蓝牙功能没有开启");
                }
                break;
            case 1:
                Debug.Log("Unity=>蓝牙成功初始化");
                if (UIBluetooth._Instance != null)
                {
                    UIBluetooth._Instance.ShowTip("蓝牙成功初始化");
                    UIBluetooth._Instance.SearchBLE(); 
                }
                break;
        }
    }

    //搜索蓝牙的回调
    public void CallbackByIOSForBLEScanning(string arg)
    {
        int result = int.Parse(arg[0].ToString());

        switch (result)
        {
            case 0: //开始扫描
                Debug.Log("Unity=>扫描开始");
                break;
            case 1: //扫描到一个设备
                
                Debug.Log(arg);
                string[] strs = arg.Remove(0, 1).Split('&');
                //strs[0]为BLEMac地址，strs[1]为蓝牙名字
                Debug.Log("Unity=>发现蓝牙设备=>" + strs[1]);

                if (UIBluetooth._Instance != null)
                {
                    UIBluetooth._Instance.AddNewBleDevice(strs[0], strs[1]);
                }
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (GetBleAutoConnInfo().Contains(strs[0]) && autoConnect)
                    {
                        print("Unity=>执行到自动连接蓝牙函数");
                        string[] tempStr = GetBleAutoConnInfo().Split('$');
                        SetBleUUID(tempStr[0]);
                        SetDevice(tempStr[1]);
                        ConnectBle(tempStr[2]);
                        SetCurBleName(strs[1]);

                        if (UIBluetooth._Instance != null)
                        {
                            UIBluetooth._Instance.ShowTip("Unity=> 正在自动连接蓝牙……", false);
                        }
                    }
                }
                break;
            case 2: //扫描结束
                Debug.Log("Unity=>扫描结束");
                if (UIBluetooth._Instance != null)
                {
                    UIBluetooth._Instance.ShowLoad(false);
                }
                break;
        }
    }

    //连接蓝牙的回调
    public void CallbackByIOSForBLEConnecting(string arg)
    {
        int result = int.Parse(arg[0].ToString());
        switch (result)
        {
            case 0: //连接失败
                Debug.LogWarning("失败信息=>" + arg);
                if (UIBluetooth._Instance != null)
                    UIBluetooth._Instance.ShowTip("连接失败……请重试", true);
                break;
            case 1: //开始连接
                Debug.Log("连接开始");
                ChangeBleStatus(ConnectStatus.Connecting);
                if (UIBluetooth._Instance != null)
                    UIBluetooth._Instance.ShowTip("开始连接蓝牙...", false);
                break;
            case 2: //连接成功，但是无法准备订阅通知出现问题
            case 3:
                Debug.LogWarning("准备订阅通知失败信息=>" + arg);
                ChangeBleStatus(ConnectStatus.Unconnected);
                if (UIBluetooth._Instance != null)
                    UIBluetooth._Instance.ShowTip("无法获取蓝牙数据……请重试", true);
                break;
            case 4: //连接成功且订阅通知
                Debug.Log("成功连接到设备=>" + arg.Remove(0, 1) + "开始订阅通知……");
                break;
            case 5: //连接中断
                      
                ChangeBleStatus(ConnectStatus.ConnectOff);
                if (UIBluetooth._Instance != null)
                    UIBluetooth._Instance.ShowTip("连接中断……", true);
                break;
        }
    }


    //蓝牙通知回调
    public void CallbackByIOSForBLENotify(string arg)
    {
        int result = int.Parse(arg[0].ToString());
        switch (result)
        {
            case 0: //打开通知成功
                string macAddress = arg.Remove(0, 1);
                Debug.Log("Unity=>打开通知成功");
                if (UIBluetooth._Instance != null)
                    UIBluetooth._Instance.ShowTip("连接成功！", true);
                
                ChangeBleStatus(ConnectStatus.Connected);
                SaveBleAutoConnInfo(curBleUUID+"$"+curDeviceClassName+"$"+macAddress);
                print("保存的 BLE 自动连接信息为=> " + GetBleAutoConnInfo());

                if (!Device._Instance.isInit)
                {
                    Debug.Log("执行到：" + Device._Instance.isInit);
                    Device._Instance.ClearMotionData();   //清除运动资料
                    Device._Instance.isInit = true;
                } 
                //连接成功，销毁UI,设置为默认连接此设备 
                DestroyBluetoothUI();
                SetAutoConnect(true);
                break;
            case 1: //收到通知数据

                SetDataListener(arg.Remove(0, 1));
           
                break;
            case 2:    //订阅通知失败
                Debug.LogWarning("Unity=>通知失败=>" + arg);
                ChangeBleStatus(ConnectStatus.Unconnected);
                if (UIBluetooth._Instance != null)
                {
                    UIBluetooth._Instance.ShowTip("订阅通知失败……", true);
                }
                break;
        }
    }


    //写入数据的回调
    private void CallbackByIOSForBLEWrite(string arg)
    {
        int result = int.Parse(arg[0].ToString());
        switch (result)
        {
            case 0:    //写入失败
                Debug.LogWarning("Unity=>写入数据失败=>" + arg.Remove(0, 1));
                break;
            case 1: //写入成功
                //Debug.Log("写入数据成功！");
                break;
        }
    }

    //==================================
    //显示当前蓝牙的状态
    private void ChangeBleStatus(ConnectStatus status)
    {
        this.CurStatus = status;
        //foreach (var item in dictStatusImg)
        //{
        //    item.Value.enabled = item.Key == status ? true : false;
        //}
    }


    //提示信息
    public void ShowToast(string tipInfo, IOSToastLength type)
    {
        //if (jo == null) return;
        //jo.Call("ShowToast", tipInfo, (int)type);
    }

    /// <summary>
    /// 创建BluetoothUI
    /// </summary>
    public GameObject CreateBluetoothUI()
    {
        Debug.Log("Unity=>  创建蓝牙界面");
        if (OnBluetoothActive != null)
            OnBluetoothActive(true);
        mBluetoothUI = Instantiate(Resources.Load<GameObject>("UIBluetooth/UIBlutooth2D"), mUIParent, false);
        mBluetoothUI.AddComponent<UIBluetooth>().Init();
        return mBluetoothUI;
    }


    /// <summary>
    /// 销毁创建UI
    /// </summary>
    /// <param name="delayTime">延迟销毁的时间</param>
    public void DestroyBluetoothUI()
    {
        Debug.Log("销毁蓝牙界面");

        if (UIBluetooth._Instance != null)
        {
            Destroy(mBluetoothUI);//销毁UI
        }
        mBluetoothUI = null;

        if (OnBluetoothActive != null)
            OnBluetoothActive(false);
    }


    /// <summary>
    /// 保存已连接过的BLE信息
    /// </summary>
    /// <param name="bleAutoConnInfo"></param>
    public void SaveBleAutoConnInfo(string bleAutoConnInfo)
    {
        PlayerPrefs.SetString("FitmanBLE", bleAutoConnInfo);
    }

    //获取需要自动连接BLE时的信息
    public string GetBleAutoConnInfo()
    {
        return PlayerPrefs.GetString("FitmanBLE", "");
    }

    //设置当前连接的BLE的UUID
    public void SetCurBleUUID(string uuid)
    {
        curBleUUID = uuid;
    }

    //设置当前连接的BLE的设备类名
    public void SetCurDeviceClassName(string className)
    {
        curDeviceClassName = className;
    }

    //设置当前连接的BLE的蓝牙名字
    public void SetCurBleName(string bleName)
    {
        this.bleName = bleName;
    }

    // 获取当前连接的蓝牙名字
    public string GetCurBleName()
    {
        return bleName;
    }


    //设置是否自动连接
    public void SetAutoConnect(bool isAuto)
    {
        autoConnect = isAuto;
    }


}
