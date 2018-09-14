using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeviceConfig
{
    private static  Dictionary<string,string> deviceList = new Dictionary<string,string>();
    private static DeviceConfig _instance;
    private static object syncRoot = new object();

    public static DeviceConfig Instance
    {
        get 
        {
            if(_instance==null)
            {
                lock(syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new DeviceConfig();
                    }
                }
            }
            return _instance;
        }
    }

    //UUID 第一个为服务的UUID，第二个为特征的读UUID,第三个为特性写UUID
    public DeviceConfig()
    {
        deviceList.Add("fitman", "FFE0&FFE1&FFE1$Device_Fitmanbike");
        deviceList.Add("boo", "1816&2A5B&2A5B$Device_TaPinQi");
        deviceList.Add("Ibiking", "1816&2A5B&2A5B$Device_TaPinQi");
        deviceList.Add("fitship", "FFE0&FFE1&FFE1$Device_RowingMachine");
    }


    //获取所有设备的过滤关键字并拼接成 abc&def 形式
    public  string  GetAllDeviceFilter()
    {
        string tmpFilter = "";
        foreach (string filter in deviceList.Keys)
        {
            if(tmpFilter.Length==0)
            {
                tmpFilter = filter;
            }
            else
            {
                tmpFilter = tmpFilter + "&" + filter;
            }
        }
        return tmpFilter;
    }

    //根据蓝牙名字获取对应的UUID
    public  string GetUUIDByBleName(string bleName)
    {
        
        string[] tmpStr = deviceList[GetFilterFromName(bleName)].Split('$');

        return  tmpStr[0];
    }

    //根据蓝牙名字获取对应的解析类名
    public  string GetClassNameByBleName(string bleName)
    {
        string[] tmpStr = deviceList[GetFilterFromName(bleName)].Split('$');
        return tmpStr[1];
    }


    //根据蓝牙名字获取对应的过滤关键字
    private  string GetFilterFromName(string bleName)
    {
        string tmpFilter = "";
        foreach (string item in deviceList.Keys)
        {
            if (bleName.ToUpper().Contains(item.ToUpper()))
            {
                tmpFilter = item;
                break;
            }
        }
        return tmpFilter;
    }

}
