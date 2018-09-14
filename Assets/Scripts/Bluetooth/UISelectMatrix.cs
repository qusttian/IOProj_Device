using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

//UI按钮矩阵
public class UISelectMatrix : MonoBehaviour
{
    private static UISelectMatrix instance = null;
    public static UISelectMatrix _Instance
    {
        get
        {
            return instance;
        }
    }


    private const int Row = 10; //行数
    private const int Col = 10; //列数

    /// <summary>
    /// 设置当前界面共有64个按钮，不可能全用上
    /// </summary>
    private Button[] ButtonGrid = new Button[Row * Col];
    private Button selectButton = null; //选中的按钮
    private int indexer = -1;   //当前选择的位置， -1表示没有
    private RectTransform handler = null;   //手指

    [NonSerialized]
    public bool isStart = false;   //是否进行选择的标志位
#if UNITY_EDITOR
    private string layoutFilePath;   //测试用，保存当前的界面布局
#endif

    /// <summary>
    /// 上下左右移动：0-左、1-右、2-上、3-下
    /// </summary>
    [HideInInspector]
    public int[] moveDir = { -1, 1, -10, 10 };

    public void Init(Transform canvas)
    {
        instance = this;
        handler = Instantiate(Resources.Load<GameObject>("UIBluetooth/Handler"), canvas, false).transform as RectTransform;
        handler.gameObject.SetActive(false);    //初始隐藏
        handler.anchorMax = new Vector2(0.518f, 0.53f);
        handler.anchorMin = new Vector2(0.482f, 0.47f);

#if UNITY_EDITOR
        layoutFilePath = Application.dataPath + "/listenerLayout.txt";
#endif

        ClearButton();
    }

    void Update()
    {
        if (isStart)
        {
            //if (InputController.input.GetRBtn2_DOWN() || Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.JoystickButton0))  //确定键、回车键
            //{
            //    OnClick();
            //    //BleAndroidBike._Instance.ShowToast("点击确定键", ToastLength.LENGTH_SHORT);
            //}
            //else if (Input.GetKeyDown(KeyCode.LeftArrow) || InputController.input.GetLLeftDir_DOWN() || InputController.input.GetRLeftDir_DOWN())   //左侧
            //    NextButton(0);
            //else if (Input.GetKeyDown(KeyCode.RightArrow) || InputController.input.GetLRightDir_DOWN() || InputController.input.GetRRightDir_DOWN())   //右侧
            //    NextButton(1);
            //else if (Input.GetKeyDown(KeyCode.UpArrow) || InputController.input.GetLAboveDir_DOWN() || InputController.input.GetRAboveDir_DOWN())   //上部
            //    NextButton(2);
            //else if (Input.GetKeyDown(KeyCode.DownArrow) || InputController.input.GetLUnderDir_DOWN() || InputController.input.GetRUnderDir_DOWN())   //下部
                //NextButton(3);
        }
    }

    /// <summary>
    /// 点击静听的按钮
    /// </summary>
    public void OnClick()
    {
        if (selectButton == null) return;
        ExecuteEvents.Execute(selectButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    /// <summary>
    /// 选择某一方向的监听器
    /// </summary>
    /// <param name="indexDir">方向：0向左；1：向右；2：向上；3：向下</param>
    public void NextButton(int indexDir)
    {
        int start = indexer;    //初始位置
        int col = start % Col;    //当前列
        int index = -1;
        switch (indexDir)
        {
            case 0: //向左选择
                while (0 < col && col <= Col - 1) //由当前位置向左向右遍历，直到第0列或第倒数第二列
                {
                    col += moveDir[indexDir];
                    start += moveDir[indexDir];    //先移动下标，因为当前的监听器的列不需要返回
                    index = FindRelativeBtn(start, false);
                    if (index != -1)   //遍历所有监听器
                        break;
                }
                break;
            case 1: //向右选择
                while (0 <= col && col < Col - 1) //由当前位置向左向右遍历，直到第0列或倒数第二列
                {
                    col += moveDir[indexDir];
                    start += moveDir[indexDir];
                    index = FindRelativeBtn(start, false);
                    if (index != -1)
                        break;
                }
                break;
            case 2: //向上选择
                while (Col - 1 < start && start <= Row * Col - 1) //由当前位置向上向下遍历，直到第0行或倒数第二行
                {
                    start += moveDir[indexDir];
                    index = FindRelativeBtn(start, true);
                    if (index != -1)
                        break;
                }
                break;
            case 3: //向下选择
                while (0 <= start && start < (Row - 1) * Col) //由当前位置向上向下遍历，直到第0行或倒数第二行
                {
                    start += moveDir[indexDir];
                    index = FindRelativeBtn(start, true);
                    if (index != -1)
                        break;
                }
                break;
            default:
                Debug.LogError("The Direction is not allowed! ===>>" + indexDir);
                break;
        }

        SetSelectedButton(index);
    }

    /// <summary>
    /// 根据位置索引，判断是否找到了最近的监听器，找到了则获取下标
    /// </summary>
    /// <param name="nowIndex">当前位置索引</param>
    /// <param name="isRow">是遍历行还是遍历列</param>
    /// <returns>存在则返回true</returns>
    private int FindRelativeBtn(int nowIndex, bool isRow)
    {
        int L_up_Index = nowIndex; //向左或向上遍历的下标
        int R_down_Index = nowIndex; //向右或向下遍历的下标
        int index = -1; //查找到的按钮下标
        if (isRow)  //向左右两边遍历该行的每一列
        {
            int L_col = nowIndex % Col; //向左遍历的列数
            int R_col = nowIndex % Col; //向又右遍历的列数

            while (L_col >= 0 || R_col <= Col - 1)  //列数在0 - (Col - 1)之间共Col列
            {
                if (L_col >= 0)
                {
                    if (ButtonGrid[L_up_Index] != null) //找到监听器
                    {
                        index = L_up_Index;
                        break;
                    }
                }
                if (R_col <= Col - 1)
                {
                    if (ButtonGrid[R_down_Index] != null) //找到监听器
                    {
                        index = R_down_Index;
                        break;
                    }
                }

                L_col += moveDir[0];    //列数左移
                R_col += moveDir[1];    //列数右移
                L_up_Index += moveDir[0];   //下标左移
                R_down_Index += moveDir[1]; //下标右移
            }
        }
        else    //向上下两边遍历该列的每一行
        {
            while (L_up_Index >= 0 || R_down_Index <= Row * Col - 1)
            {
                if (L_up_Index >= 0)
                {
                    if (ButtonGrid[L_up_Index] != null) //找到监听器
                    {
                        index = L_up_Index;
                        break;
                    }
                }
                if (R_down_Index <= Row * Col - 1)
                {
                    if (ButtonGrid[R_down_Index] != null) //找到监听器
                    {
                        index = R_down_Index;
                        break;
                    }
                }

                L_up_Index += moveDir[2];
                R_down_Index += moveDir[3];
            }
        }

        return index;
    }

    /// <summary>
    /// 设置选择框
    /// </summary>
    private void SetSelectedButton(int nowIndex)
    {
        if (nowIndex == -1) return;
        Button button = ButtonGrid[nowIndex];
        if (button == null || !button.interactable || !button.gameObject.activeInHierarchy) return;
        indexer = nowIndex;
        selectButton = button;
        handler.transform.SetAsLastSibling();   //最后一层
        handler.gameObject.SetActive(true);
        //handler.transform.SetParent(selectButton.transform, true);
        handler.position = selectButton.transform.position;
        EventSystem.current.SetSelectedGameObject(selectButton.gameObject);
    }

    /// <summary>
    /// 移除指定的监听器
    /// </summary>
    /// <param name="btn"></param>
    public void RemoveButton(Button btn)
    {
        for (int i = 0; i < ButtonGrid.Length; i++)
        {
            if (ButtonGrid[i] == btn)
            {
                ButtonGrid[i] = null;
                if (indexer == i)    //如果当前选择的是指定的
                    StartNearestButton(indexer); //选择下一个
                break;
            }
        }
    }

    /// <summary>
    /// 移除指定位置的监听器
    /// </summary>
    /// <param name="row">行数</param>
    /// /// <param name="col">列数</param>
    public void RemoveListener(int row, int col)
    {
        int index = row * Col + col;
        if (ButtonGrid[index] != null)
            ButtonGrid[index] = null;
        if (indexer == index)    //如果当前选择的是指定的
            StartNearestButton(index); //选择下一个
    }

    //获取某行某列的监听器
    public Button GetButton(int row, int col)
    {
        int index = row * Col + col;
        Button bt = ButtonGrid[index];
        return bt;
    }

    /// <summary>
    /// 获得离当前位置最近的监听器下标
    /// </summary>
    /// <param name="index"></param>
    public void StartNearestButton(int index)
    {
        int tempPos = -1;
        bool isGetted = false;
        for (int i = 1; i < ButtonGrid.Length - 1; i++)
        {
            tempPos = index - i;
            if (tempPos >= 0 && ButtonGrid[tempPos] != null)
            {
                isGetted = true;
                break;
            }

            tempPos = index + i;
            if (tempPos <= 63 && ButtonGrid[tempPos] != null)
            {
                isGetted = true;
                break;
            }
        }

        if (isGetted)   //找到最近的监听器
        {
            SetSelectedButton(tempPos);
        }
        else    //没有监听器了
        {
            isStart = false;
            indexer = -1;
            handler.gameObject.SetActive(false);
        }

    }

    /// <summary>
    /// 清除保存的监听器
    /// </summary>
    public void ClearButton()
    {
        isStart = false;

        for (int i = 0; i < ButtonGrid.Length; i++)
        {
            if (ButtonGrid[i] != null)
                ButtonGrid[i] = null;
        }

        indexer = -1;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        handler.gameObject.SetActive(false);    //初始隐藏
    }

    /// <summary>
    /// 重新开始选择新的监听按钮
    /// </summary>
    public void Restart(int row = -1, int col = -1)
    {
        isStart = true;
        indexer = -1;
        if (row > -1 && row < Row && col > -1 && col < Col)
        {
            indexer = Col * row + col;
        }
        else
        {
            for (int i = 0; i < ButtonGrid.Length; i++)
            {
                if (ButtonGrid[i] != null)
                {
                    indexer = i;
                    break;
                }
            }
        }

        if (indexer == -1)
            return;
        Debug.Log("启动按钮=>" + indexer);
        SetSelectedButton(indexer);
#if UNITY_EDITOR
        PrintLayout();    //打印当前布局界面
#endif
    }

    /// <summary>
    /// 向指定的行/列插入监听器
    /// </summary>
    /// <param name="row">行数</param>
    /// <param name="col">列数</param>
    /// <param name="btn">按钮</param>
    public void AddButton(int row, int col, Button btn)
    {
        int indexPos = Col * row + col;   //真正的一位数组下标
        if (indexPos < ButtonGrid.Length)  //判断一下
            ButtonGrid[indexPos] = btn;
        else
            Debug.LogError("The Index is out of Array ==>>" + indexPos);
    }

#if UNITY_EDITOR
    //测试用，打印当前的监听器布局********************************************************************************************************************
    public void PrintLayout()
    {
        if (File.Exists(layoutFilePath))    //先删除存在的
            File.Delete(layoutFilePath);

        string content = "";
        for (int i = 0; i < ButtonGrid.Length; i++)
        {
            if (i % Col == 0) //每Col个初始化一次，表示换行
            {
                WriteLine(content);
                content = "";
            }
            if (ButtonGrid[i] != null)
                content += "1  ";
            else
                content += "0  ";
        }
        WriteLine(content); //最后再写入一行
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    //打印一行
    void WriteLine(string str)
    {
        if (!File.Exists(layoutFilePath))
        {
            using (FileStream fs = File.Create(layoutFilePath))
            {
                fs.Flush();
            }
        }

        using (StreamWriter sw = new StreamWriter(layoutFilePath, true))
        {
            sw.WriteLine(str);
            sw.Flush();
        }
    }

#endif
}
