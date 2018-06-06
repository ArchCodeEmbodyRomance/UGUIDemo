using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1)]
public class TableView<T, V> : MonoBehaviour,IDisposable
{
    //在行初始化之后代理执行注册事件等方法的回调
    public Action AfterViewInited;

    //行的父物体，一般为content
    public RectTransform Content;
    //行模板
    public GameObject CanvasRow;
    //关闭按钮，
    [Tooltip("可以为空")]
    public Button CloseBtn;
    //默认行数
    public int DefaultRow;

    protected List<TableRow<T, V>> RowPool = new List<TableRow<T, V>>();
    protected Dictionary<T, TableRow<T, V>> ActivedTableRows = new Dictionary<T, TableRow<T, V>>();
    protected Dictionary<string, Action<T>> RowEventDic = new Dictionary<string, Action<T>>();    

    /// <summary>
    /// 是否初始化完成，一般在Awake之后肯定完成的
    /// 在Table初始化之后Table代理才可以调用RegisterRowEvent方法开始注册事件
    /// </summary>
    public bool Inited { get; protected set; }
    /// <summary>
    /// 表中活动的行数
    /// </summary>
    public int Count
    {
        get { return ActivedTableRows.Count; }
    }
    /// <summary>
    /// 表的容量
    /// </summary>
    public int Capacity
    {
        get { return RowPool.Count; }
    }

    private void Awake()
    {
        if (DefaultRow <= 0)
        {
            DefaultRow = 10;
        }
        if (CloseBtn != null)
        {
            CloseBtn.onClick.AddListener(HideTableView);
        }
        Init();
    }

    /// <summary>
    /// 程序开始时根据最大数量初始化RowPool
    /// </summary>

    protected virtual void Init()
    {
        TableRow<T, V>[] rows = Content.GetComponentsInChildren<TableRow<T, V>>(true);
        if (rows != null)
        {
            RowPool.AddRange(rows);
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].Init(i, this);
                rows[i].SetActive(false);
            }
        }
        for (int i = RowPool.Count; i < DefaultRow; i++)
        {
            RectTransform rt = GameObject.Instantiate(CanvasRow, Content).GetComponent<RectTransform>();
            RowPool.Add(rt.GetComponent<TableRow<T, V>>());
            RowPool[i].Init(i, this);
            RowPool[i].SetActive(false);
        }

        Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,0);
        //第一行注册事件
        RowPool[0].AddEvent();
        if (AfterViewInited != null)
        {
            AfterViewInited();
        }
        Inited = true;
    }
    protected void CreateNewRow()
    {
        RectTransform rt = GameObject.Instantiate(CanvasRow, Content).GetComponent<RectTransform>();
        RowPool.Add(rt.GetComponent<TableRow<T, V>>());
        RowPool[RowPool.Count-1].Init(RowPool.Count-1, this);
        RowPool[RowPool.Count - 1].SetActive(false);
    }
    /// <summary>
    /// 在末尾添加行
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Add(T key, V value)
    {
        if (ActivedTableRows.ContainsKey(key))
        {
            Debug.LogWarning("表中已经存在键为:/'" + key.ToString() + " /'的行", gameObject);
            return ActivedTableRows[key].ParseDate(key, value);
        }
        if (ActivedTableRows.Count >= RowPool.Count)
        {
            CreateNewRow();
        }
        TableRow<T, V> row = RowPool[ActivedTableRows.Count];
        if (!row.ParseDate(key, value))
        {
            Debug.LogError("填充表格内容失败", gameObject);
            return false;
        }
        ActivedTableRows.Add(key, row);
        row.SetActive(true);
        return true;
    }

    public bool Add(KeyValuePair<T, V> keyValue)
    {
        return Add(keyValue.Key, keyValue.Value);
    }

    /// <summary>
    /// 更新或添加行数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool UpdateRow(T key, V value)
    {

        TableRow<T, V> orange;
        if (!ActivedTableRows.TryGetValue(key, out orange))
        {
            Debug.LogWarning("表中不存在键为 /'" + key.ToString() + " /'的行", gameObject);
            return Add(key, value);
        }
        return ActivedTableRows[key].ParseDate(key, value);
    }

    /// <summary>
    /// 更新Table中的数据
    /// </summary>
    /// <param name="list">只有T设置为int时候才有效果</param>
    //public bool UpdateTableDate(List<V> list)
    //{
    //    //关闭的时候没有清空数据，所以默认是不需要清空的。
    //    if (list == null || list.Count == 0)
    //    {
    //        Debug.LogWarning("Table 数据为空");
    //        while (ActivedTableRows.Count > 0)
    //        {
    //            ActivedTableRows[0].SetDisactive(false);
    //            ActivedTableRows.RemoveAt(0);
    //        }
    //    }
    //    else
    //    {
    //        while (ActivedTableRows.Count > list.Count)
    //        {
    //            ActivedTableRows[list.Count].SetDisactive(false);
    //            ActivedTableRows.RemoveAt(list.Count);
    //        }

    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            if (i >= ActivedTableRows.Count)
    //            {
    //                if (i >= RowPool.Count)
    //                {
    //                    RectTransform rt = GameObject.Instantiate(CanvasRow, Content).GetComponent<RectTransform>();
    //                    rt.anchoredPosition = new Vector2(0, -(ContentHeight * i + ContentInterval * (i + 1)));
    //                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ContentHeight);
    //                    RowPool.Add(rt.GetComponent<TableRow>());
    //                    RowPool[i].Init(i, this);
    //                }
    //                ActivedTableRows.Add(RowPool[i]);
    //                ActivedTableRows[i].SetActive();
    //            }
    //            //解析转化为TableRow数据
    //            if (!ActivedTableRows[i].ParseDate(list[i]))
    //            {
    //                Debug.LogError("填充表格内容失败", gameObject);
    //                return false;
    //            }
    //        }
    //        Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (ContentHeight + ContentInterval) * list.Count);
    //    }

    //    gameObject.SetActive(true);
    //    return true;
    //}

    /// <summary>
    /// 删除表中行
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isclear">是否清空该行中数据</param>
    /// <returns></returns>
    public bool DelteRow(T key)
    {
        TableRow<T, V> row;
        if (!ActivedTableRows.TryGetValue(key, out row))
        {
            Debug.LogWarning("表中不存在键为：" + key.ToString() + " 的行；", gameObject);
            return false;
        }
        row.Dispose();
        //将之后的行赋值给前一个
        int index = RowPool.IndexOf(row);        
        for (int i = index; i < Count - 1; i++)
        {
            RowPool[i].Clone(RowPool[i + 1]);
        }
        RowPool[Count - 1].Dispose();
        return true;
    }
    public bool UpdateTable(Dictionary<T, V> dic)
    {
        foreach (var item in ActivedTableRows)
        {
            item.Value.Dispose();
        }
        if (dic.Count > Capacity)
        {
            RowPool = new List<TableRow<T, V>>(dic.Count);
            ActivedTableRows = new Dictionary<T, TableRow<T, V>>(dic.Count);
        }
        else
        {
            ActivedTableRows.Clear();
        }
        if (dic == null || dic.Count == 0)
        {
            //Debug.LogWarning("Table 数据为空",gameObject);
        }
        else
        {
            if (dic.Count >= RowPool.Count)
            {
                for (int row = RowPool.Count; row <= dic.Count; row++)
                {
                    CreateNewRow();
                }

            }
            foreach (var item in dic)
            {
                TableRow<T, V> row = RowPool[ActivedTableRows.Count];

                if (!row.ParseDate(item.Key, item.Value))
                {
                    return false;
                }
                ActivedTableRows.Add(item.Key, row);
                row.SetActive(true);
            }
        }
        return true;
    }
    public virtual void ShowTableView()
    {
        gameObject.SetActive(true);
        Content.parent.gameObject.SetActive(false);
        Content.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// 销毁并隐藏表，并销毁物体
    /// </summary>
    /// <param name="isrecycle">是否回收多余的行</param>
    public void Dispose(bool isrecycle)
    {
        gameObject.SetActive(false);

        foreach (var item in ActivedTableRows)
        {
            item.Value.Dispose();
        }
        ActivedTableRows.Clear();

        if (isrecycle)
        {
            while (RowPool.Count > DefaultRow)
            {
                TableRow<T, V> row = RowPool[DefaultRow];
                RowPool.Remove(row);
                GameObject.DestroyImmediate(row.gameObject);
            }
        }
    }
    public void Dispose()
    {
        Dispose(false);
    }
    /// <summary>
    /// 隐藏表
    /// </summary>
    public void HideTableView()
    {
        //Content.parent.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
    #region 行事件
    /// <summary>
    /// 给TableView的事件列表添加事件
    /// </summary>
    /// <param name="TableRow中包含的事件名称"></param>
    /// <param name="act"></param>
    public bool AddRowEvent(string name)
    {
        if (!RowEventDic.ContainsKey(name))
        {
            Action<T> act = null;
            RowEventDic.Add(name, act);
            return true;
        }
        else
        {
            Debug.LogError(string.Format("Table 重复注册名为： ‘{0}’ 的事件", name), gameObject);
            return false;
        }
    }
    /// <summary>
    /// 给TableView的事件列表减少事件
    /// </summary>
    /// <param name="TableRow中包含的事件名称"></param>
    public bool ReduceRowEvent(string name)
    {
        if (RowEventDic.ContainsKey(name))
        {
            RowEventDic.Remove(name);
            return true;
        }
        else
        {
            Debug.Log(string.Format("table中不存在名为 {0} 的事件", name), gameObject);
            return false;
        }
    }
    /// <summary>
    /// 执行事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="Key"></param>
    /// <returns></returns>
    public bool ExecuteRowEvent(string name, T Key)
    {
        Action<T> act;
        if (RowEventDic.TryGetValue(name, out act))
        {
            if (act != null)
            {
                act(Key);
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 清空事件字典
    /// </summary>
    public void ClearRowEventDic()
    {
        RowEventDic.Clear();
    }
    /// <summary>
    /// 注册事件字典中的事件
    /// </summary>
    /// <param name="事件名称"></param>
    /// <param name="注册的方法"></param>
    public bool RegisterRowEvent(string eventName, Action<T> act)
    {
        if (RowEventDic.ContainsKey(eventName))
        {
            RowEventDic[eventName] += act;
            return true;
        }
        Debug.Log(eventName + " 事件在table中注册失败", gameObject);
        return false;
    }
    /// <summary>
    /// 取消注册事件字典中的事件
    /// </summary>
    /// <param name="事件名称"></param>
    /// <param name="取消注册的方法"></param>
    public bool UnregisterRowEnent(string eventName, Action<T> act)
    {
        if (RowEventDic.ContainsKey(eventName))
        {
            RowEventDic[eventName] -= act;
            return true;
        }
        Debug.Log(eventName + " 事件在table中取消注册失败", gameObject);
        return false;
    }
    #endregion
    #region 行执行事件
    //泛型方法不能很好地传递泛型类型，暂时取消泛型，统一用object。

    /// <summary>
    /// 让表中每一行执行事件名称为eventname的事件，提供了泛型和object两种方法
    /// </summary>
    /// <param name="eventname"></param>
    /// <param name="values"></param>
    public void RowsExecuteEvent(string eventname, params object[] values)
    {
        foreach (var item in ActivedTableRows)
        {
            item.Value.RowExecuteEvent(eventname, values);
        }
    }

    /// <summary>
    /// 让键为Key值执行事件名称为eventname的事件
    /// 用法1.让行执行事件
    /// 用法2.更新部分在UpdateRow的时候不能进行赋值的操作
    /// </summary>
    /// <param name="key">执行的行的键</param>
    /// <param name="eventname">执行的事件名称</param>
    /// <param name="values">发送的值</param>
    public void TargetRowExecuteEvent(string eventname, T key, params object[] values)
    {
        TableRow<T, V> row;
        if (ActivedTableRows.TryGetValue(key, out row))
        {
            row.RowExecuteEvent(eventname, values);
        }
        else
        {
            Debug.LogWarning("当前表中不存在KEY为：" + key.ToString() + " 的行", gameObject);
        }
    }
    #endregion

    #region  排序
    //UI层只负责显示，在填充前先将数据排序，然后再填充表进行排序。

    #endregion
    #region 取值
    /// <summary>
    /// 根据顺序获得Key值
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T GetKeyByIndex(int index)
    {
        if (index >= Count)
            throw new IndexOutOfRangeException();
        else
            return RowPool[index].Key;
    }
    public int GetIndexByKey(T key)
    {
        TableRow<T, V> row;
        if (ActivedTableRows.TryGetValue(key,out row))
        {
            return row.Index;
        }
        Debug.LogError("TableView do not contains key:" + key, gameObject);
        return -1;
    }
    public TableRow<T, V> GetRowByIndex(int index)
    {
        if (index >= Count)
            throw new IndexOutOfRangeException();
        else
            return RowPool[index];
    }

    public TableRow<T, V> GetRowByKey(T key)
    {
        if (ActivedTableRows.ContainsKey(key))
        {
            return ActivedTableRows[key];
        }
        Debug.LogError("TableView do not contains key:" + key, gameObject);
        return null;
    }
    #endregion
}