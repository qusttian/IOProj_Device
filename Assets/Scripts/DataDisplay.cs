using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour 
{
    public Text deviceType;
    public Text bleName;
    public Text bleConnStatus;
    public Text speed;
    public Text distance;
    public Text heartRate;
    public Text motionTime;
    public Text resistance;
    public Text calories;
    public Text horizontalAngle;
    public Text pulse;

    public Button Btn_ResistanceAdd;
    public Button Btn_ResistanceDecrease;
    public Button Btn_ClearData;
    public Button Btn_Disconnect;
    public Button Btn_BluetoothPanel;

    public Transform Gun;
    public Transform leftBtn;
    public Transform rightBtn;

    //按键按下时的位移量
    public Vector3 BtnDownVec = new Vector3(0, 0.5f, 0);
    //左右两边按键对应的初始位置
    private Vector3 leftBtnInitLocalPos;
    private Vector3 rightBtnInitLocalPos;

    //脉冲感应
    private int indexPulse = 0;
    private bool isBtnInit = true;


	// Use this for initialization
	void Start () 
    {
        Btn_ResistanceAdd.onClick.AddListener(delegate()
        {
            InputController.Input.AddOneLevelResistance();

        });

        Btn_ResistanceDecrease.onClick.AddListener(delegate()
        {

            InputController.Input.DecreaseOneLevelResistance();
        });

        Btn_ClearData.onClick.AddListener(delegate ()
        {

            ClearData();
        });

        Btn_Disconnect.onClick.AddListener(delegate ()
        {

            DisconnectBLE();
        });

        Btn_BluetoothPanel.onClick.AddListener(delegate ()
        {

            ShowBluetoothUI();
        });

        leftBtnInitLocalPos = leftBtn.localPosition;
        rightBtnInitLocalPos = rightBtn.localPosition;




		
	}
	



	// Update is called once per frame
	void Update () 
    {
        
        if (InputController.Input!=null)
        {
            deviceType.text = InputController.Input.GetDeviceType().ToString();
            bleName.text = InputController.Input.GetCurBleName();
            bleConnStatus.text = InputController.Input.GetStatus().ToString();
            speed.text = InputController.Input.GetRollSpeed().ToString();
            Debug.Log("Speed = " + InputController.Input.GetRollSpeed());
            distance.text = InputController.Input.GetRollDistance().ToString();
            heartRate.text = InputController.Input.GetHeartRate().ToString();
            motionTime.text = InputController.Input.GetMotionTime().ToString();
            resistance.text = InputController.Input.GetResistance().ToString();
            calories.text = InputController.Input.GetCalories().ToString();
            horizontalAngle.text = InputController.Input.GetHorizontalAngle().ToString();

            GunController();
            BtnController();

            if(InputController.Input.GetWheelPulse())
            {
                indexPulse++;
            }
            pulse.text = indexPulse.ToString();

        }


	}

    //控制枪的旋转
    private float curAngleX;
    private float targetAngleX;

    private void GunController()
    {
        targetAngleX = InputController.Input.GetHorizontalAngle();
        curAngleX = Mathf.Lerp(curAngleX, targetAngleX, 0.3f);
        Gun.transform.rotation = Quaternion.Euler(0, targetAngleX, 0);
    }

    private void BtnController()
    {
        if (InputController.Input.GetLeftBtn_DOWN())
            leftBtn.localPosition = leftBtnInitLocalPos + BtnDownVec;
        if (InputController.Input.GetLeftBtn_PRESSED())
            leftBtn.localPosition = leftBtnInitLocalPos + BtnDownVec;
        if (InputController.Input.GetLeftBtn_UP())
            leftBtn.localPosition = leftBtnInitLocalPos;

        if (InputController.Input.GetRightBtn_DOWN())
            rightBtn.localPosition = rightBtnInitLocalPos + BtnDownVec;
        if (InputController.Input.GetRightBtn_PRESSED())
            rightBtn.localPosition = rightBtnInitLocalPos + BtnDownVec;
        if (InputController.Input.GetRightBtn_UP())
            rightBtn.localPosition = rightBtnInitLocalPos;
    }


    public void ClearData()
    {
        Debug.Log("Unity=> 执行数据 清空 指令");
        if(InputController.Input!=null)
        {
            InputController.Input.ClearMotionData();
        }

    }
	public void DisconnectBLE()
    {
        Connection._Instance.DisConnectBle();
        Debug.Log("Unity=> 执行 蓝牙 断开指令");
    }
	public void ShowBluetoothUI()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (UIBluetooth._Instance == null)
            {
                Connection._Instance.CreateBluetoothUI();
                Connection._Instance.SetAutoConnect(false);
            }
            else
            {
                Connection._Instance.DestroyBluetoothUI();
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (UIBluetooth._Instance == null)
            {
                Connection._Instance.CreateBluetoothUI();
                Connection._Instance.SetAutoConnect(false);
            }
            else
            {
                Connection._Instance.DestroyBluetoothUI();
            }

        }
        else
        {
            Debug.Log("当前系统不支持,请使用 Android 或 IOS 平台");
        }

    }

}
