using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 基础的委托
/// </summary>
public delegate void BaseEventHandler();


/// <summary>
/// 基础的事件接口
/// </summary>
public interface IBaseEvent
{
    void AddListener(MyBtnEvent eventType,BaseEventHandler call);
    void RemoveListener(MyBtnEvent eventType,BaseEventHandler call);
    void TriggerEvent(MyBtnEvent eventType);
}

/// <summary>
/// 计时器的事件结构体
/// </summary>
public struct BtnEvent : IBaseEvent
{
    private BaseEventHandler btnEventHandler_LeftDown;
    private BaseEventHandler btnEventHandler_LeftPressed;
    private BaseEventHandler btnEventHandler_LeftUp;
    private BaseEventHandler btnEventHandler_RightDown;
    private BaseEventHandler btnEventHandler_RightPressed;
    private BaseEventHandler btnEventHandler_RightUp;

    //注册此事件的监听  回调函数
    public void AddListener(MyBtnEvent myBtnEvent,BaseEventHandler call)
    {
        Debug.Log("执行事件注册函数");
        switch(myBtnEvent)
        {
            case MyBtnEvent.Left_DOWN:
                btnEventHandler_LeftDown += call;
                Debug.Log("执行完 LeftDown  事件注册");
                break;
            case MyBtnEvent.Left_PRESSED:
                btnEventHandler_LeftPressed += call;
                break;
            case MyBtnEvent.Left_UP:
                btnEventHandler_LeftUp += call;
                break;
            case MyBtnEvent.Right_DOWN:
                btnEventHandler_RightDown += call;
                break;
            case MyBtnEvent.Right_PRESSED:
                btnEventHandler_RightPressed += call;
                break;
            case MyBtnEvent.Right_UP:
                btnEventHandler_RightUp += call;
                break;
        }

    }

    //移除此事件的监听  回调函数
    public void RemoveListener(MyBtnEvent myBtnEvent,BaseEventHandler call)
    {
        switch (myBtnEvent)
        {
            case MyBtnEvent.Left_DOWN:
                btnEventHandler_LeftDown -= call;
                break;
            case MyBtnEvent.Left_PRESSED:
                btnEventHandler_LeftPressed -= call;
                break;
            case MyBtnEvent.Left_UP:
                btnEventHandler_LeftUp -= call;
                break;
            case MyBtnEvent.Right_DOWN:
                btnEventHandler_RightDown -= call;
                break;
            case MyBtnEvent.Right_PRESSED:
                btnEventHandler_RightPressed -= call;
                break;
            case MyBtnEvent.Right_UP:
                btnEventHandler_RightUp -= call;
                break;
        }
    }

    //触发此事件
    public void TriggerEvent(MyBtnEvent eventType)
    {
        switch (eventType)
        {
            case MyBtnEvent.Left_DOWN:
                if (btnEventHandler_LeftDown != null)
                {
                    btnEventHandler_LeftDown();
                }
                else
                {
                    Debug.Log("btnEventHandler_LeftDown is Empty");
                }
                break;
            case MyBtnEvent.Left_PRESSED:
                if (btnEventHandler_LeftPressed != null)
                {
                    btnEventHandler_LeftPressed();
                }
                else
                {
                    Debug.Log("btnEventHandler_LeftPressed is Empty");
                }
                break;
            case MyBtnEvent.Left_UP:
                if (btnEventHandler_LeftUp != null)
                {
                    btnEventHandler_LeftUp();
                }
                else
                {
                    Debug.Log("btnEventHandler_LeftUp is Empty");
                }
                break;
            case MyBtnEvent.Right_DOWN:
                if (btnEventHandler_RightDown != null)
                {
                    btnEventHandler_RightDown();
                }
                else
                {
                    Debug.Log("btnEventHandler_RightDown is Empty");
                }
                break;
            case MyBtnEvent.Right_PRESSED:
                if (btnEventHandler_RightPressed != null)
                {
                    btnEventHandler_RightPressed();
                }
                else
                {
                    Debug.Log("btnEventHandler_RightPressed is Empty");
                }
                break;
            case MyBtnEvent.Right_UP:
                if (btnEventHandler_RightUp != null)
                {
                    btnEventHandler_RightUp();
                }
                else
                {
                    Debug.Log("btnEventHandler_RightUp is Empty");
                }
                break;
        }
    }
}

//自定义的事件种类
public enum MyBtnEvent
{
    Left_DOWN,
    Left_PRESSED,
    Left_UP,
    Right_DOWN,
    Right_PRESSED,
    Right_UP,
}