using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectBleByAndroid : Connection
{
    private AndroidJavaObject jo = null;    //安卓交互

    public void Awake()
    {
        _Instance = this;
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.LogWarning("非Android平台，无法初始化蓝牙");
        }
        else
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");  //获取指定Activity对象
        }
        InitBluetoothUI();
        InitBleFilter(DeviceConfig.Instance.GetAllDeviceFilter());  //设置过滤关键字
        StartBle();
    }

    public void Init()
    {
        _Instance = this;
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.LogWarning("非Android平台，无法初始化蓝牙");
        }
        else
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");  //获取指定Activity对象
        }
        InitBluetoothUI();
        InitBleFilter(DeviceConfig.Instance.GetAllDeviceFilter());  //设置过滤关键字
        StartBle();
    }

    //初始化蓝牙的过滤字库,以字符串形式进行传递，不同关键字用&符号连接，比如fit&htc,不区分大小写
    public override void InitBleFilter(string filters)
    {
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非Android平台不能调用InitBleFilter操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
        {
            Debug.Log("设置过滤关键字->" + filters);
            jo.Call("InitBleFilter", filters);
        }
    }

    //开始蓝牙
    public override void StartBle()
    {
        //初始化蓝牙
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非Android平台不能调用StartBle操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
        {
            jo.Call("initBluetooth");
        }
    }

    //设置蓝牙的UUID，以字符串形式进行传递，不同UUID用&符号进行连接，比如 FFE0&FFE1,大小写区分待定
    public override void SetBleUUID(string uuids)
    {
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非Android平台不能调用SetBleUUID操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
        {
            SetCurBleUUID(uuids);
            string[] uids = uuids.Split('&');
            jo.Call("SetBleUUID", uids[0], uids[1], uids[2]);
        }
    }

    /// 扫描蓝牙设备
    public override void DiscoverBle()
    {
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非Android平台不能调用SetBleUUID操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
        {
            if (jo.Call<bool>("DoDiscovery"))
            {
                Debug.Log("开始扫描蓝牙设备……");
            }
        }

    }

    //连接至蓝牙设备
    public override void ConnectBle(string identifier)
    {
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非Android平台不能调用SetBleUUID操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
        {
            if (jo.Call<bool>("ConnectBlt", identifier)) //连接蓝牙设备
            {
                Debug.Log("正在连接蓝牙设备=>" + identifier);
            }
            else
            {
                Debug.LogError("无法连接蓝牙设备->" + identifier);
            }
        }
    }


    //写入蓝牙数据到 IOS
    public override void WriteDataToBle(byte[] data)
    {
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非Android平台不能调用WriteDataToBle操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
        {
            jo.Call("Write", data);
        }
    }

    public override void DisConnectBle()
    {
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非Android平台不能调用DisConnectBle操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
        {
            jo.Call("DisConnect");
        }
    }

    //释放资源
    public override void DisposeBle()
    {
        if (Application.platform != RuntimePlatform.Android)
            Debug.LogWarning("非安卓平台不能调用安卓释放操作");
        else if (jo == null)
            Debug.Log("JavaObject没有初始化");
        else
            jo.Call("Dispose");
    }

}
