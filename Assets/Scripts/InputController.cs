using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

//输入控制
public enum InputType
{
    Keyboard,   //键盘
    DeviceBle,  //设备蓝牙
    PortRowingMachine, //划船器串口连接
    PortFitmanOldBike, //Fitman老车子串口连接
    PortFitmanNewBike, //Fitman新车子串口连接
}

/// <summary>
/// 输入控制器
/// </summary>
public class InputController : MonoBehaviour
{

    public InputController()
    {
        _instance = this;
    }

    private static InputController _instance = null;
    public static InputController Instance
    {
        get
        {
            return _instance;
        }
    }

    //输入类型
    public InputType type = InputType.DeviceBle;

    private Coroutine quitCor = null;   //退出的协程操作

    private static IInput input = null; //输入控制
    public static IInput Input
    {
        get 
        {
            if(input==null)
            {
                input = Device._Instance; 
            }
            return input;
        }
    }

    //初始化操作
    public void Init(InputType type = InputType.DeviceBle)
    {
        this.type = type;
          Debug.Log("初始化输入控制器 =>" + type.ToString());
        Screen.sleepTimeout = 0;    //禁用睡眠
        DontDestroyOnLoad(gameObject);  //切换场景不删除




#if TV
        gameObject.AddComponent<UISelectMatrix>().Init(args[0] as Transform);
#endif
        switch (type)
        {
            case InputType.Keyboard:
                input = new KeyboardInput();
                break;

            case InputType.PortRowingMachine:
                gameObject.AddComponent<ConnectPortByPC>().Init();
                Device._Instance = new Device_RowingMachine();
                input = Device._Instance;
                break;

            case InputType.DeviceBle:
                if (Application.platform == RuntimePlatform.Android)
                {
                    gameObject.AddComponent<ConnectBleByAndroid>();
                    input = Device._Instance;
                }

                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    gameObject.AddComponent<ConnectBleByIOS>();
                    input = Device._Instance;
                }

                break;
        }
    }

    public void ReInitInput()
    {
        if(Device._Instance!=null)
        {
            input = Device._Instance;
        }
        else
        {
            Debug.Log("重新初始化 input 时，Device._Instance为空");
        }
    }
    public void Restart()
    {
        //input.ClearConsoleData();   //清空数据
        //input.SetResistance(c_StartForce);   //设置阻力为5
    }

    //是否按下返回键的操作
    //public bool GetEscape()
    //{
    //    return Input.GetKeyDown(KeyCode.Escape);
    //}

    void Update()
    {
        if (input != null)
        {
            //input.HandleData();
        }
            
    }

    //退出应用
    public void QuitApp()
    {
        if (quitCor != null) return;
        quitCor = StartCoroutine(WaitQuit());
    }

    private IEnumerator WaitQuit()
    {
        if (type == InputType.DeviceBle)
        {
            input.Dispose();
            while (Connection._Instance.CurStatus == ConnectStatus.Connected)
            {
                yield return null;
            }
            yield return null;
        }
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
