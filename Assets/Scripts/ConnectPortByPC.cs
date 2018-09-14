using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.IO.Ports;

public class ConnectPortByPC:Connection
{
    private Thread tPortRead;//读取串口数据的线程
    private bool stopTread = false; //停止线程的标志位

    private const int baudRate = 38400;//波特率
    private SerialPort curSP = null;    //当前串口
    private List<SerialPort> listExistSP = new List<SerialPort>();  //当前所有的串口
    private byte[] recvBuff=new byte[100];    //读取到的数据

    //初始化
    public void Init()
    {
        _Instance = this;
        
        tPortRead = new Thread(HandleData);
        tPortRead.IsBackground = true;
        tPortRead.Start();
    }

    //写入数据
    public override void WriteDataToBle(byte[] data)
    {
        if (curSP != null && curSP.IsOpen)
            curSP.Write(data, 0, data.Length);
    }

    //释放资源
    public void Dispose()
    {
        stopTread = true;
        if (curSP != null)
        {
            if (curSP.IsOpen)
                curSP.Close();
            curSP.Dispose();
        }
        tPortRead.Abort();  //尝试终止线程
        tPortRead.Join();   //等待线程终止
        Debug.Log("执行关闭端口和关闭线程");
    }

    //线程操作

    private void HandleData()
    {
        while (!stopTread)
        {
            switch (CurStatus)
            {
                case ConnectStatus.Unconnected: //未连接，重新尝试连接
                case ConnectStatus.ConnectOff:  //连接中断
                    if (listExistSP.Count == 0)
                    {
                        string[] sPortsName = SerialPort.GetPortNames();//获取计算机当前所有端口，以数组形式返回
                        for (int i = 0; i < sPortsName.Length; i++)
                        {
                            SerialPort tempSp = new SerialPort(sPortsName[i], baudRate, Parity.None, 8, StopBits.One);
                            tempSp.Handshake = Handshake.None;
                            tempSp.RtsEnable = true;
                            
                            listExistSP.Add(tempSp);
                        }
                    }
                    CurStatus = ConnectStatus.Connecting;
                    break;
                case ConnectStatus.Connecting:  //连接中...
                    CheckConnect();
                    Thread.Sleep(100);  //等待
                    break;
                case ConnectStatus.Connected:   //已连接
                    RecieveData();
                    break;
            }
            Thread.Sleep(10);  //等待
        }
    }


    //检查连接
    private void CheckConnect()
    {
        if (curSP != null)
        {
            if (curSP.IsOpen)
                curSP.Close();
            curSP.Dispose();
            curSP = null;
        }

        Debug.Log("尝试连接 =>串口数:" + listExistSP.Count);

        for (int i = 0; i < listExistSP.Count; i++)
        {
            SerialPort sp = listExistSP[i];
            try
            {
                if (sp.IsOpen)
                {
                    sp.Close();
                }
                sp.Open();
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.ReadTimeout = 500;

                try
                {
                    int res = sp.ReadByte();
                    if (res == RawingMachineDataOrder.Start1 || res > 0)
                    {
                        CurStatus = ConnectStatus.Connected;    //连接成功
                        curSP = sp;
                        curSP.Read(new byte[50], 0,50);    //将剩余的所有数据读取出来，实际读取的byte数量不到22
                        Debug.Log("checkConnect() 收到端口传入数据");
                     
                        SetCurBleName(curSP.PortName);
                        listExistSP.RemoveAt(i); //将正确的端口从已扫描到的所有端口数组中移除
                        break;
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(" read 异常：======数据读取错误" + e.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.Log(" open 异常：" + e.ToString());
            }
        }

        if (curSP == null)
            CurStatus = ConnectStatus.Unconnected;  //未连接成功

        //释放所有其他非正确端口
        for (int i = 0; i < listExistSP.Count; i++)
        {
            if (listExistSP[i].IsOpen)
                listExistSP[i].Close();
            listExistSP[i].Dispose();
        }
        listExistSP.Clear();

    }

//接收数据
private void RecieveData()
    {
        try
        {
            //读取数据
            if (!curSP.IsOpen)
                return;

            recvBuff[0] = (byte)curSP.ReadByte();
            if (recvBuff[0] == RawingMachineDataOrder.Start1)                //获取到包头的第一个开始字节
            {

                for (int i = 1; i < Device._Instance.DATA_BUFFER_SIZE; i++)
                    recvBuff[i] = (byte)curSP.ReadByte();          //读取到的数据，不包含Start1，应该包含21个字节
                Debug.Log("收到端口传入数据：" + recvBuff.ToString());
                Device._Instance.CheckoutAndAnalyzeData(recvBuff);
            }
            else
            {
                Debug.Log("收到非头数据：" + recvBuff[0] + "第一次检查时");
            }
        }
        catch (Exception e)
        {
            Debug.Log("----------------------------------------------------发生读取异常！");
        }
    }


    private void OnApplicationQuit()
    {
        Dispose();
    }

    public override void InitBleFilter(string filters)
    {
        throw new NotImplementedException();
    }

    public override void StartBle()
    {
        throw new NotImplementedException();
    }

    public override void SetBleUUID(string uuids)
    {
        throw new NotImplementedException();
    }

    public override void DiscoverBle()
    {
        throw new NotImplementedException();
    }

    public override void ConnectBle(string identifier)
    {
        throw new NotImplementedException();
    }

    public override void DisConnectBle()
    {
        throw new NotImplementedException();
    }

    public override void DisposeBle()
    {
        Dispose();
    }
}

