/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.PdfKit
*文件名： TextLocationListener
*版本号： V1.0.0.0
*唯一标识：3e45b91d-1a85-4e3e-b3ed-38786b703bef
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/11 17:56:31
*描述：自定义文本位置监听器
*
*=================================================
*修改标记
*修改时间：2025/8/11 17:56:31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自定义文本位置监听器
*
*****************************************************************************/


namespace LuBan.PdfKit;

/// <summary>
/// 自定义文本位置监听器
/// </summary>
public class TextLocationListener : LocationTextExtractionStrategy
{
    private readonly string _searchText;
    public List<TextChunk> FoundTextChunks { get; } = new();

    private readonly Queue<(string text, TextRenderInfo info)> _recentChunks = new();

    bool _begin = false;
    static TextChunk _textChunk;

    public TextLocationListener(string searchText)
    {
        _searchText = searchText;
    }

    public override void EventOccurred(IEventData data, EventType type)
    {
        if (type == EventType.RENDER_TEXT)
        {
            var renderInfo = (TextRenderInfo)data;
            string text = renderInfo.GetText();
            //结束
            if (text == _searchText.Substring(_searchText.Length - 1, 1) && _begin)
            {
                _begin = false;
                _textChunk.Width = renderInfo.GetAscentLine().GetEndPoint().Get(0) - _textChunk.Left;
                _textChunk.Height = renderInfo.GetAscentLine().GetEndPoint().Get(1) - _textChunk.Bottom;
                _recentChunks.Enqueue((text, renderInfo));
                // 拼接窗口内所有字符
                var combined = string.Concat(_recentChunks.Select(c => c.text));
                if (combined == _searchText)
                {
                    FoundTextChunks.Add(_textChunk);
                    _recentChunks.Clear();
                    return;
                }
            }
            //开始
            if (text == _searchText.Substring(0, 1) && !_begin)
            {
                _recentChunks.Clear();
                _begin = true;
                _textChunk = new TextChunk()
                {
                    Left = renderInfo.GetBaseline().GetStartPoint().Get(0),
                    Bottom = renderInfo.GetBaseline().GetStartPoint().Get(1),
                };
            }
            //拼接
            if (_begin)
            {
                _recentChunks.Enqueue((text, renderInfo));
            }
        }
        base.EventOccurred(data, type);
    }
}
