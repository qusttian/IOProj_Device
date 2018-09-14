using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Reflection;


public class ConnectBleByIOS : Connection
{


    //固定写法,用于调用XCODE中的OC函数

    [DllImport("__Internal")]
    private static extern void _InitBLEFilter(string str);

    [DllImport("__Internal")]
    private static extern void _StartBLE();

    [DllImport("__Internal")]
    private static extern void _SetBLEUUID(string str);

    [DllImport("__Internal")]
    private static extern void _DiscoverBLE();

    [DllImport("__Internal")]
    private static extern void _ConnectBLE(string identifier);

    [DllImport("__Internal")]
    private static extern void _WriteToBLE(string str);

    [DllImport("__Internal")]
    private static extern void _DisConnectBLE();

    [DllImport("__Internal")]
    private static extern void _DisposeBLE();

    // 本结构蓝牙连接过程：
    // 1、先根据设备配置类，将所有的过滤关键字传入IOS 或 Android 处理类中
    // 2、启动蓝牙并开始扫描，根据传入的过滤关键字，将过滤后的蓝牙名字显示出并保存对应的identifier
    // 3、用户点击蓝牙列表的某一项，根据所点击的蓝牙名字，在配置表中找出所以点击配置的UUID和对应的设备议处理类名
    // 4、将UUID传入IOS 或 Android,为下一步的连接查找服务做准备
    // 5、根据步骤3得到的设备协议类名创建对应的设备类，用于解析蓝牙传入的数据



    public ConnectBleByIOS()
    {
        _Instance = this;
        this.Init();
        Debug.Log("执行完成 ConnectBleByIOS 的构造函数");
    }
	public  void Init()
    {
        
        Debug.Log("DeviceConfig.GetAllDeviceFilter()=" + DeviceConfig.Instance.GetAllDeviceFilter());
        InitBleFilter(DeviceConfig.Instance.GetAllDeviceFilter());
        InitBluetoothUI();
        _StartBLE();

	}


	//初始化蓝牙的过滤字库,以字符串形式进行传递，不同关键字用&符号连接，比如fit&htc,不区分大小写
	public override void InitBleFilter(string filters)
	{
        _InitBLEFilter(filters);
        Debug.Log("Unity=> 执行 _InitBLEFilter(" + filters + ")");
	}

	//开始蓝牙
	public override void StartBle()
	{
        _StartBLE();
	}

	//设置蓝牙的UUID，以字符串形式进行传递，不同UUID用&符号进行连接，比如 FFE0&FFE1,大小写区分待定
	public override void SetBleUUID(string uuids)
	{
        _SetBLEUUID(uuids);
        SetCurBleUUID(uuids);
	}

	// 扫描蓝牙设备 
	public override void DiscoverBle()
    {
        _DiscoverBLE();
    }

    //连接至蓝牙设备
    public override void ConnectBle(string identifier)
    {
        _ConnectBLE(identifier);//连接蓝牙设备
    }

    //连接至蓝牙设备
    public override void DisConnectBle()
    {
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            Debug.LogWarning("非IOS平台不能调用断开连接操作");
        }
        else
        {
            Debug.Log("开始执行断开连接函数");
            _DisConnectBLE();
        }
    }



    //写入蓝牙数据到 IOS
    public override void WriteDataToBle(byte[] data)
    {
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            Debug.LogWarning("非IOS平台无法写入数据");
        }
        else
        {
            _WriteToBLE(BitConverter.ToString(data));
        }
    }



    //释放资源
    public override void DisposeBle()
    {
        
         Debug.Log("开始执行释放资源函数");

    }
}
