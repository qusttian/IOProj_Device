using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIBluetooth : MonoBehaviour
{
    /// <summary>
    /// 页码组件
    /// </summary>
    private class PageItem
    {
        public Transform parent;
        public string page;
        public Text pagetext;
    }
    /// <summary>
    /// 蓝牙组件
    /// </summary>
    private class BleItem
    {
        public string name = null;
        public string adress = null;
        public Transform clicked = null;
        public Button bleBtn = null;
        public Text nameText = null;
        public Transform parent = null;
    }
    /// <summary>
    /// 蓝牙数据
    /// </summary>
    private class BltData
    {
        public string adress = null;
        public string name = null;
        public bool dfsa = false;
    }

    public static UIBluetooth _Instance = null; //单例

    public int State = 1;//0:测试用，1：实际使用

    private Transform TransF_Load = null; //扫描中展示图片
    private Button Btn_Search = null; //扫描按钮
    //private RectTransform RecttransContent = null; //新的设备列表布局
    private Text tip = null;  //提示
    private Image frontBG = null;   //前端显示的遮挡图片
    private Transform TransF_PageGrid = null;//页码grid
    private Transform TransF_Grid = null; //蓝牙列表
    private Button Btn_NextPage, Btn_LastPage;//页码刷新
    private Transform TransF_NowPage;//当前页码显示
    private Transform TransF_NowBle;//当前连接的蓝牙
    private BleItem[] bluetoothArr = null;
    private Button Btn_Close;
    private PageItem[] pageItems = null;
    private float waitTime = 0;  //等待隐藏时间
    int curPage = 1;//当前页数
    int oldPage = 0;//之前页数
    int maxPage = 1;//最大页数
    int lastPage = 0;//当前页码的最后数字
    int firstPage = 0;//页码的第一个
    int showCount = 0;//蓝牙显示的个数
    Dictionary<int, Transform> pageDict = new Dictionary<int, Transform>();

    private List<string> SearchingAdressList = new List<string>();//搜索中的列表，可加入，数量不确定（地址）
    private List<string> SearchingNameList = new List<string>();//搜索中的列表，可加入，数量不确定（名称）

    private const int bluetoothCountInPerPage = 6;//每页蓝牙显示的最多个数
    Coroutine cor = null;


	private void Awake()
	{
        _Instance = this;
	}




	#region 初始化操作
	/// <summary>
	/// 初始化和重置操作
	/// </summary>
	void InitMes()
    {
        // 数据重置
        curPage = 1;
        oldPage = 0;
        maxPage = 1;
        lastPage = 0;
        firstPage = 0;
        showCount = 0;
        //组件重置
        TransF_NowPage.SetParent(transform);
        TransF_NowBle.SetParent(transform);
        //NewBltDic.Clear();
        ClearPage();
        TransF_NowPage.SetParent(TransF_PageGrid.GetChild(0));
        TransF_NowPage.SetAsFirstSibling();
        TransF_NowPage.localPosition = Vector3.zero;
        InitBltContent(0);
    }
    /// <summary>
    /// 初始化蓝牙组件
    /// </summary>
    /// <param name="mes"></param>
    void InitBltContent(int mes)
    {
        if (mes == 0) //组件全部 重置
        {
            for (int i = 0; i < bluetoothCountInPerPage; i++)//蓝牙列表重置
            {
                bluetoothArr[i].name = "";
                bluetoothArr[i].nameText.text = "";
                bluetoothArr[i].clicked.gameObject.SetActive(false);
                bluetoothArr[i].parent.gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < bluetoothCountInPerPage; i++)//蓝牙列表重置
            {
                bluetoothArr[i].clicked.gameObject.SetActive(false);
            }
        }


    }
    //初始化UIBluetooth
    public void Init()
    {

        TransF_Load = transform.Find("Searching");
        Btn_Search = transform.Find("Search").GetComponent<Button>();
        tip = transform.Find("Tip").GetComponent<Text>();
        frontBG = tip.transform.GetChild(0).GetComponent<Image>();
        frontBG.enabled = false;
        TransF_PageGrid = transform.Find("PageGrid");
        TransF_Grid = transform.Find("Grid");
        Btn_NextPage = transform.Find("Next").GetComponent<Button>();
        Btn_LastPage = transform.Find("Last").GetComponent<Button>();
        TransF_NowPage = transform.Find("NowPage");
        TransF_NowBle = transform.Find("NowChoosed");
        TransF_NowBle.gameObject.SetActive(false);
        Btn_Close = transform.Find("Close").GetComponent<Button>();
        //页码配置
        pageItems = new PageItem[5];
        for (int i = 0; i < TransF_PageGrid.childCount; i++)
        {
            PageItem item = new PageItem();
            item.parent = TransF_PageGrid.GetChild(i);
            item.page = "";
            item.pagetext = TransF_PageGrid.GetChild(i).Find("Text").GetComponent<Text>();
            if (i > 0) item.parent.gameObject.SetActive(false);
            pageItems[i] = item;
        }

        bluetoothArr = new BleItem[TransF_Grid.childCount];
        for (int i = 0; i < bluetoothArr.Length; i++)
        {
            BleItem item = new BleItem();
            item.parent = TransF_Grid.GetChild(i);
            item.nameText = TransF_Grid.GetChild(i).Find("Text").GetComponent<Text>();
            item.bleBtn = TransF_Grid.GetChild(i).GetComponent<Button>();
            item.clicked = TransF_Grid.GetChild(i).Find("Choiced");
            item.clicked.gameObject.SetActive(false);
            item.name = "";
            item.bleBtn.onClick.AddListener(delegate ()
            {
                InitBltContent(1);
                TransF_NowBle.gameObject.SetActive(true);
                item.clicked.gameObject.SetActive(true);
                TransF_NowBle.SetParent(item.parent);
                TransF_NowBle.SetAsFirstSibling();
                TransF_NowBle.localPosition = Vector3.zero;
                Debug.Log("Unity=> 请求连接蓝牙设备=>地址" + item.adress);
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    
                    Debug.Log("Unity=> 当前点击的项的名字为： "+item.name);
                    Connection._Instance.DisConnectBle();
                    Connection._Instance.SetCurBleName(item.name);
                    Connection._Instance.SetBleUUID(DeviceConfig.Instance.GetUUIDByBleName(item.name));
                    Connection._Instance.SetDevice(DeviceConfig.Instance.GetClassNameByBleName(item.name));
                    InputController.Instance.ReInitInput();
                    Connection._Instance.ConnectBle(item.adress);
                }
               
                else
                {
                        ShowTip("Unity=> 非正常平台，无法连接", true);
                }
            });
            bluetoothArr[i] = item;
        }
#if TV
        UISelectMatrix._Instance.AddButton(0, 3, Btn_Close);
        UISelectMatrix._Instance.AddButton(7, 0, Btn_LastPage);
        UISelectMatrix._Instance.AddButton(7, 2, Btn_NextPage);
        UISelectMatrix._Instance.AddButton(8, 1, Btn_Search);
        UISelectMatrix._Instance.Restart(8, 1);
#endif 
        Btn_Close.onClick.AddListener(delegate ()
        {
            if(Application.platform==RuntimePlatform.Android || Application.platform==RuntimePlatform.IPhonePlayer)
            {
                Connection._Instance.DestroyBluetoothUI();
            }
          
            else
            {
                Debug.Log("Unity=> 当前系统不支持，请使用Android 或 IOS");
            }

        });
        Btn_Search.onClick.AddListener(() => { SearchBLE(); });
        Btn_NextPage.onClick.AddListener(delegate () { changepage(1); });
        Btn_LastPage.onClick.AddListener(delegate () { changepage(-1); });
    }
    #endregion
    //----------------------测试---------------
    IEnumerator DoTest()
    {
        int a = 0;
        while (a < 30)
        {
            if (showCount < bluetoothCountInPerPage) showCount++;
            //NewBltDic.Add("地址" + a + "djasd", "蓝牙" + a);
            SearchingAdressList.Add("地址" + a + "djasd");
            SearchingNameList.Add("蓝牙" + a);
            UpdateNewGrid();
            if (SearchingAdressList.Count <= bluetoothCountInPerPage || SearchingAdressList.Count / bluetoothCountInPerPage <= curPage)
            {
                Debug.Log(showCount + "curPage" + curPage);
                DelBltMes(curPage * bluetoothCountInPerPage - bluetoothCountInPerPage, showCount);
            }
            yield return new WaitForSeconds(0.5f);
            a += 1;

        }
        yield return null;
        SearchingAdressList.RemoveRange(5, 15);
        SearchingNameList.RemoveRange(5, 15);
        UpdateNewGrid();
        ShowLoad(false);
        Debug.Log("扫描结束！");
    }
    //---------------------------------
    void Update()
    {
        //提示信息显示4s
        if (waitTime != -1 && tip.enabled && Time.realtimeSinceStartup - waitTime >= 4)
        {
            tip.enabled = false;  //4s后隐藏提示
        }
    }

    /// <summary>
    /// 查找蓝牙设备。。
    /// </summary>
    public void SearchBLE()
    {
        Debug.Log("Unity-UIBluetooth => 开始查找设备....");
        if (cor != null)
        {
            StopCoroutine(cor);
            cor = null;
        }
#if TV
        UISelectMatrix._Instance.Restart(7, 1); //自动聚焦到下一页的按钮上
#endif
        SearchingAdressList.Clear();
        SearchingNameList.Clear();
        TransF_NowBle.gameObject.SetActive(false);
        InitMes();
        if (State == 0)
            cor = StartCoroutine(DoTest());
        else
            UpdateNewGrid();
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            //Connection._Instance.Init();
            Connection._Instance.DiscoverBle();
            Debug.Log("Unity-UIBluetooth => 开始查找设备....执行完毕");
        }

        else
        {
            Debug.LogWarning("Unity=> 非安卓或iOS 设备无法搜索蓝牙！");
            ShowLoad(false);
        }
    }

    public void ShowLoad(bool active)
    {
        TransF_Load.gameObject.SetActive(active); //禁用启用加载图片
        Btn_Search.interactable = !active;     //禁用启用扫描按钮
        Btn_Search.targetGraphic.raycastTarget = !active;
        if (!active && SearchingAdressList.Count == 0)
            ShowTip("Unity=> 未扫描到可用蓝牙设备");
    }

    //更新新的蓝牙设备列表
    private void UpdateNewGrid()
    {
        if (SearchingAdressList.Count == 0)
        {
            //ShowTip("未扫描到可用蓝牙设备");
            ClearPage();
            return;
        }
        int newmaxpage = (SearchingAdressList.Count % 6) == 0 ? (SearchingAdressList.Count / bluetoothCountInPerPage) : (SearchingAdressList.Count / bluetoothCountInPerPage + 1);
        if (newmaxpage == 0) newmaxpage = 1;
        Debug.Log("共有页数：" + maxPage);
        DelMes(newmaxpage, curPage);
        if (State != 0)
        {
            Debug.Log(showCount + "curPage" + curPage);
            DelBltMes(curPage * bluetoothCountInPerPage - bluetoothCountInPerPage, showCount);
        }
    }

    //添加蓝牙设备名称
    public void AddNewBleDevice(string address, string name)
    {
        Debug.Log(string.Format("address:{0},name:{1}", address, name));
        if (SearchingAdressList.Contains(address))
        {
            Debug.Log("Unity=> 已存在相同地址的蓝牙设备=>" + address);
            return;
        }
        SearchingAdressList.Add(address);
        SearchingNameList.Add(name);
        //NewBltDic .Add(address, name);
        UpdateNewGrid();   //更新列表

        if (showCount < bluetoothCountInPerPage) showCount++;
        if (SearchingAdressList.Count <= bluetoothCountInPerPage || SearchingAdressList.Count / bluetoothCountInPerPage <= curPage)
        {
            Debug.Log(showCount + "curPage" + curPage);
            DelBltMes(curPage * bluetoothCountInPerPage - bluetoothCountInPerPage, showCount);
        }
    }

    /// <summary>
    /// 提示信息
    /// </summary>
    /// <param name="info">提示的信息文字</param>
    /// <param name="isRaycast">是否开启当前界面的射线检测</param>
    public void ShowTip(string info, bool isRaycast = true)
    {
        tip.text = info;
        tip.enabled = true;
        if (isRaycast)
            waitTime = Time.realtimeSinceStartup;
        else
            waitTime = -1;
        if (frontBG.enabled == isRaycast)
            frontBG.enabled = !isRaycast;
    }

    /// <summary>
    /// 处理蓝牙列表逻辑
    /// </summary>
    private void DelBltMes(int indexa = 0, int count = 0)
    {
        List<string> blts = new List<string>();
        for (int i = indexa; i < indexa + count; i++)
        {
            blts.Add(SearchingNameList[i]);
        }
        Debug.Log(blts.Count);
        BltShow(blts);
    }
    /// <summary>
    /// 显示蓝牙列表,名字
    /// </summary>
    /// <param name="blts"></param>
    private void BltShow(List<string> blts)
    {
        Debug.Log("Unity=> 收到了蓝牙个数：" + blts.Count);
        List<BltData> datas = new List<BltData>();
        for (int i = 0; i < blts.Count; i++)
        {
            BltData data = new BltData();
            data.adress = SearchingAdressList[i];
            data.name = blts[i];
            datas.Add(data);
        }
        for (int i = 0; i < bluetoothArr.Length; i++)
        {
            if (i < datas.Count)
            {
                bluetoothArr[i].parent.gameObject.SetActive(true);
                bluetoothArr[i].name = datas[i].name;
                bluetoothArr[i].adress = datas[i].adress;
                bluetoothArr[i].nameText.text = datas[i].name;
#if TV
                UISelectMatrix._Instance.AddButton(i + 1, 1, bluetoothArr[i].bleBtn);
#endif
                continue;
            }
#if TV
            UISelectMatrix._Instance.RemoveButton(bluetoothArr[i].bleBtn);
#endif
        }
    }
    #region 页码
    //---------------------------------------------------------------
    /// <summary>
    ///   页码显示
    /// </summary>
    /// <param name="nums"></param>
    /// <param name="index"></param>
    void PagesShow(List<int> nums, int index)
    {
        pageDict.Clear();
        ClearPage();
        for (int i = 0; i < nums.Count; i++)
        {
            pageItems[i].pagetext.text = nums[i].ToString();
            pageItems[i].parent.gameObject.SetActive(true);
            pageDict.Add(nums[i], pageItems[i].parent);
        }
        TransF_NowPage.SetParent(pageDict[index]);
        TransF_NowPage.SetAsFirstSibling();
        TransF_NowPage.localPosition = Vector3.zero;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="newmaxpage">最新最大页码</param>
    /// <param name="newpage">当前数据页码</param>
    void DelMes(int newmaxpage, int newpage)
    {
        Debug.Log(curPage + "max:" + maxPage + "new" + newmaxpage + "last" + lastPage + "newpage" + newpage);
        curPage = oldPage = newpage;
        if (newpage > lastPage || newpage < firstPage)//超出当前页
        {
            DelPage(newmaxpage);
        }
        else
        {
            if (oldPage > newmaxpage || maxPage != newmaxpage)//页码总个数发生变化
            {
                DelPage(newmaxpage);
            }
            else
            {
                TransF_NowPage.SetParent(pageDict[newpage]);
                TransF_NowPage.SetAsFirstSibling();
                TransF_NowPage.localPosition = Vector3.zero;
            }
        }
    }
    void DelPage(int newmaxpage)
    {
        Debug.Log(curPage + "max:" + maxPage + "new" + newmaxpage);
        List<int> nums = new List<int>();
        int a = newmaxpage % 5 == 0 ? newmaxpage / 5 : newmaxpage / 5 + 1;
        int b = curPage % 5 == 0 ? curPage / 5 : curPage / 5 + 1;
        bool islast = a == b ? true : false;  // a=b 最后一页
        int index = curPage - 4;
        if (curPage % 5 != 0) { index = curPage - curPage % 5 + 1; }
        int count = newmaxpage < 5 ? newmaxpage : 5;
        if (islast)
        {
            if (newmaxpage % 5 != 0)
                count = newmaxpage % 5;
        }
        for (int i = 0; i < count; i++)
        {
            nums.Add(index + i);
        }
        PagesShow(nums, curPage);
        lastPage = nums[nums.Count - 1];
        firstPage = index;
        oldPage = curPage;
        maxPage = newmaxpage;
    }
    /// <summary>
    /// 切换页码
    /// </summary>
    /// <param name="num"></param>
    void changepage(int num)
    {
        curPage += num;
        if (curPage <= 0 || curPage > maxPage)
        {
            curPage -= num;
            return;
        }
        InitBltContent(0);
        TransF_NowBle.gameObject.SetActive(false);
        Debug.Log("当前界面：" + curPage);
        DelMes(maxPage, curPage);
        showCount = SearchingAdressList.Count < bluetoothCountInPerPage ? SearchingAdressList.Count : bluetoothCountInPerPage;
        int index = (curPage * bluetoothCountInPerPage - bluetoothCountInPerPage);//显示的第一个下标
        Debug.Log("下标--" + index + "显示个数：--" + showCount);
        if (num == 1 && SearchingAdressList.Count - index <= 5)//下一页 最后的
        {
            if (SearchingAdressList.Count % bluetoothCountInPerPage != 0)
                showCount = SearchingAdressList.Count % bluetoothCountInPerPage;
        }
        //if (showCount == bluetoothCountInPerPage && NewBltDic.Count % bluetoothCountInPerPage == 0) showCount = 0;
        Debug.Log("index--" + index + "显示个数：--" + showCount);
        DelBltMes(index, showCount);
    }
    /// <summary>
    /// 页码重置
    /// </summary>
    void ClearPage()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i > 0)
            {
                pageItems[i].parent = TransF_PageGrid.GetChild(i);
                pageItems[i].page = "";
                pageItems[i].pagetext.text = "";
                pageItems[i].parent.gameObject.SetActive(false);

            }
            else
                pageItems[i].pagetext.text = "1";
        }

    }
    #endregion
}
