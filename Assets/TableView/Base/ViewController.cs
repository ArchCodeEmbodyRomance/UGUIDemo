using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewControllerT <T,V>: MonoBehaviour {
    public TableView<T, V> View;

    protected virtual void Awake()
    {
        if (View.Inited)
        {
            RegiesterEvent();
        }
        else
        {
            View.AfterViewInited += RegiesterEvent;
        }
    }

    protected virtual void  RegiesterEvent()
    {

    }
}
