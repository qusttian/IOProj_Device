using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{

    public static PlayerControl gsPlayerControl;

    public Transform canvas;

    #region UnityBackCall
    private void Awake()
    {
        gsPlayerControl = this;
        if (InputController.Instance == null)
        {

#if UNITY_EDITOR
            gameObject.AddComponent<InputController>().Init(InputType.PortRowingMachine);
#elif UNITY_STANDALONE_WIN
            gameObject.AddComponent<InputController>().Init(InputType.PortRowingMachine);
            Debug.Log("开始执行 Win 平台下的初始化函数");
#elif UNITY_IPHONE
            Debug.Log("开始执行 IOS 平台下的初始化函数");
            gameObject.AddComponent<InputController>().Init(InputType.DeviceBle);
            Debug.Log("已执行 IOS 平台下的初始化函数");
#elif UNITY_ANDROID
            Debug.Log("开始执行 Android 平台下的初始化函数");
            gameObject.AddComponent<InputController>().Init(InputType.DeviceBle);
            Debug.Log("已执行 Android 平台下的初始化函数");
#endif
        }

    }

    private void Start()
    {

    }

    private void Update()
    {
        if(InputController.Input!=null)
        {
            InputController.Input.HandleBtnData();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
#endregion

}
