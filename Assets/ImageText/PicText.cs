using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PicText : Text
{
    // 用正则取<quad name=1 size=64 width=1/>只能通过size调整图片的高度，width 是size的倍数。通过这俩参数调整图片大小
    protected static readonly Regex _InputTagRegex = new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?)/>", RegexOptions.Singleline);
    //表情位置索引信息
    Dictionary<int, SpriteTagInfo> _SpriteTagInfos = new Dictionary<int, SpriteTagInfo>();
    //显示图片的Image
    protected List<Image> Images = new List<Image>();
    //数据库中取出所有图片
    public List<Texture2D> Textures = new List<Texture2D>();
    //根据texture生成的sprite列表
    protected List<Sprite> Sprites = new List<Sprite>();
    protected bool TextChanged = true;

    public override string text
    {
        get { return m_Text; }
        set
        {
            if (TextChanged)
            {
                base.text = value;
            }
            else
                Debug.LogWarning("图文混排不支持直接改text值，请调用： InputDate(string newtext ,List<Texture2D> textures)");
        }
    }

    protected override void Start()
    {
        ActiveText();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        ActiveText();
    }
#endif

    public void ActiveText()
    {
        //支持富文本
        supportRichText = true;
        //对齐几何
        alignByGeometry = true;
        //启动的是 更新顶点
        SetVerticesDirty();
    }

    public void InputDate(string newtext, List<Texture2D> newtextures = null)
    {
        Textures = newtextures;
        TextChanged = true;
        text = newtext;
        ParserText();
    }

    public void UpdateDate()
    {
        ParserText();
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;
        base.OnPopulateMesh(toFill);

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        //这个属性设置false后会造成变化text边框后 图片位置不刷新
        m_DisableFontTextureRebuiltCallback = true;

        //获取所有的UIVertex,绘制一个字符对应6个UIVertex，绘制顺序为012 203  
        List<UIVertex> verts = new List<UIVertex>();

        toFill.GetUIVertexStream(verts);
        /*      Unity 绘制UI界面坐标系：以左上角为原点，右下角为（1,1）
         *      text 绘制mesh
         *      uv（0,1）序号（0）         uv（1,1）序号（1）
         *      
         *      uv（0,0）序号（3）         uv（1,1）序号（2）
         *      
         *      顺序（0,1,2），（2,3,0）
         */

        //通过标签信息来设置需要绘制的图片的信息  
        for (int i = 0; i < _SpriteTagInfos.Count; i++)
        {
            //UGUI Text不支持<quad/>标签，表现为乱码，这里将他的uv全设置为0,清除乱码  
            for (int m = _SpriteTagInfos[i].Index * 6; m < _SpriteTagInfos[i].Index * 6 + 6; m++)
            {
                if (m < verts.Count)
                {
                    UIVertex tempVertex = verts[m];
                    //Debug.Log(verts.Count + "  m " + m);
                    tempVertex.uv0 = Vector2.zero;
                    verts[m] = tempVertex;
                }
            }
            if (_SpriteTagInfos[i].Index * 6 < verts.Count)
            {
                //设置Image
                var tagInfo = _SpriteTagInfos[i];
                var uiv = verts[_SpriteTagInfos[i].Index * 6];
                if (i < Images.Count)
                {
                    var rt = Images[i].GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(uiv.position.x + tagInfo.Size * tagInfo.Width / 2, uiv.position.y - tagInfo.Size / 2);
                }
                else
                {
                    Debug.LogError("图片多于image");
                    Debug.Log("标签数量" + _SpriteTagInfos.Count);
                    Debug.Log("Image数" + Images.Count);
                }
            }
            else
            {
                //处理超出边框的问题。
                Images[i].transform.position = new Vector3(10000, 0, 0);//.gameObject.SetActive(false);
            }
        }
        toFill.Clear();
        toFill.AddUIVertexTriangleStream(verts);

        m_DisableFontTextureRebuiltCallback = false;
    }

    //当前需求下将更新数据放在输入文本的时候才更新
    //正常来说将本方法放在SetVerticesDirty()就能实现随时更新。
    //根据正则规则更新文本
    protected void ParserText()
    {
        //根据正则解析txt获取所有的标签
        MatchCollection mc = _InputTagRegex.Matches(text);
        _SpriteTagInfos.Clear();
        for (int i = 0; i < mc.Count; i++)
        {
            SpriteTagInfo spriteTgeInfo = new SpriteTagInfo
            {
                Index = mc[i].Index,
                Name = mc[i].Groups[1].Value,
                Size = int.Parse(mc[i].Groups[2].Value),
                Width = int.Parse(mc[i].Groups[3].Value),
            };
            _SpriteTagInfos.Add(i, spriteTgeInfo);
        }

        //维护Images列表
        int imgindex = 0;
        while (imgindex < Images.Count)
        {
            if (Images[imgindex] == null)
            {
                Images.RemoveAt(imgindex);
            }
            else
            {
                imgindex++;
            }
        }
        if (Images.Count == 0)
        {
            //取不到disactive的gameobject的组件
            //transform.GetComponentsInChildren<Image>(Images);
            foreach (Transform item in transform)
            {
                if (item.GetComponent<Image>() != null)
                {
                    Images.Add(item.GetComponent<Image>());
                }
            }
        }
        for (int i = 0; i < _SpriteTagInfos.Count; i++)
        {
            if (i >= Images.Count)
            {
                var resources = new DefaultControls.Resources();
                var go = DefaultControls.CreateImage(resources);
                go.layer = gameObject.layer;
                go.transform.SetParent(transform);
                Images.Add(go.GetComponent<Image>());
            }
            Images[i].GetComponent<RectTransform>().sizeDelta = new Vector2(_SpriteTagInfos[i].Size * _SpriteTagInfos[i].Width, _SpriteTagInfos[i].Size);
            Images[i].gameObject.SetActive(true);
        }
        for (int i = _SpriteTagInfos.Count; i < Images.Count; i++)
        {
            Images[i].gameObject.SetActive(false);
        }

        Sprites.Clear();
        for (int i = 0; i < _SpriteTagInfos.Count; i++)
        {
            if (i < Textures.Count)
            {
                Rect rect = new Rect(Vector2.zero, new Vector2(Textures[i].width, Textures[i].height));
                Sprite sprite = Sprite.Create(Textures[i], rect, rect.center);
                Sprites.Add(sprite);
                Images[i].sprite = sprite;
            }
            else
                Debug.LogError("图文混排图片数量不足，标签数量：" + _SpriteTagInfos.Count + "  图片数量：  " + Sprites.Count);
        }
    }
    protected override void OnDestroy()
    {
        _SpriteTagInfos = null;
        Images = null;
        Textures = null;
        Sprites = null;
        base.OnDestroy();
    }
}

public class SpriteTagInfo
{
    //标签在文字中的位置
    public int Index;
    //标签名称 没想好要不要，目前来说没有用
    public string Name;
    //标签大小
    public int Size;
    //标签宽
    public int Width;

}



