using UnityEngine;
using System.Collections.Generic;
using System;

public class TableRow<T,V> : MonoBehaviour,IDisposable
{
    public int Index { get; protected set; }
    public T Key { get; protected set; }
    protected TableView<T,V> View;
    [HideInInspector]
    public RectTransform RectTrans { get; protected set; }

    /// <summary>
    /// 初始化本行数据
    /// </summary>
    /// <param name="index">序号</param>
    /// <param name="view">行所在的表</param>
    public virtual void Init(int index, TableView<T,V> view)
    {
        //ReadBtn.onClick.AddListener(OnReadBtnClick);
        RectTrans = gameObject.transform as RectTransform;
        Index = index;
        View = view;
    }
    /// <summary>
    /// 添加事件，只有第一行添加
    /// </summary>
    public virtual void AddEvent()
    {
        //View.AddEvent(RowEventName.ReadCanvas);
    }
    public virtual void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
    //public virtual void SetDisactive()
    //{
    //    gameObject.SetActive(false);
    //}
    /// <summary>
    /// 解析数据到对应的表现是形式
    /// 必须手动将key赋值给字段Key
    /// </summary>
    /// <param name="key">行的键</param>
    /// <param name="value">行中的值</param>
    /// <returns></returns>
    public virtual bool ParseDate(T key, V value)
    {
        Key = key;
        Debug.Log(this.GetType());
        return true;
    }
    public virtual void Clone(T key,V value)
    {
        ParseDate(key, value);
    }
    /// <summary>
    /// 值复制，引用不变
    /// </summary>
    /// <param name="row"></param>
    public virtual void Clone(TableRow<T,V> row)
    {
        this.Key = row.Key;
    }
    
    /// <summary>
    /// 执行事件名称为eventname的事件
    /// </summary>
    /// <param name="eventname">事件名称</param>
    /// <param name="values"></param>
    public virtual void RowExecuteEvent(string eventname,params object[] values)
    {

    }
    /// <summary>
    /// 清空C#内存同时销毁Unity的物体释放内存
    /// </summary>
    public virtual void Dispose()
    {
        gameObject.SetActive(false);
        Key = default(T);
        //DestroyImmediate(Gameobje);
    }
    #region 具体的事件的实现在子类中写并注册到button的OnClick事件中

    //public void OnExportBtnClick()
    //{
    //        View.ExecuteEvent(RowEventName.ReadCanvas,Index);
    //}
    #endregion
}
