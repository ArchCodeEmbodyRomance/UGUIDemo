using UnityEngine;


public class TableRow<T,V> : MonoBehaviour
{
    public int Index { get; protected set; }
    public T Key { get; protected set; }
    protected TableView<T,V> View;
    [HideInInspector]
    public RectTransform RectTrans { get; protected set; }

    // Use this for initialization
    void Start()
    {

    }

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
    public virtual void SetActive()
    {
        gameObject.SetActive(true);
    }
    public virtual void SetDisactive(bool isrecycle = false)
    {
        gameObject.SetActive(false);
        if (isrecycle)
        {
            Clear();
        }
    }
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
    /// <summary>
    /// 清除
    /// </summary>
    protected virtual void Clear()
    {
        Key = default(T);
    }
    /// <summary>
    /// 执行事件名称为eventname的事件
    /// </summary>
    /// <param name="eventname">事件名称</param>
    /// <param name="values"></param>
    public virtual void RowExecuteEvent(string eventname,params object[] values)
    {

    }
    #region 具体的事件的实现在子类中写并注册到button的OnClick事件中

    //public void OnExportBtnClick()
    //{
    //        View.ExecuteEvent(RowEventName.ReadCanvas,Index);
    //}
    #endregion
}
