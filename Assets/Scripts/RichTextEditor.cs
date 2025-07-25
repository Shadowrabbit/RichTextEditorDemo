// ******************************************************************
//       /\ /|       @file       RichTextEditor.cs
//       \ V/        @brief      
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-07-23 12:45:02
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class RichTextEditor : MonoBehaviour
{
    [Header("UI 组件")] public TMP_InputField textInput;
    [Header("富文本按钮")] public Button boldButton;
    public Button italicButton;
    public Button underlineButton;
    public Button strikethroughButton;
    private int _selectionStartPos = -1;
    private int _selectionEndPos = -1;
    private List<CharacterData> _characters = new();

    private void Start()
    {
        SetupEventListeners();
    }

    private void SetupEventListeners()
    {
        // 文本输入事件
        if (textInput != null)
        {
            textInput.onValueChanged.AddListener(OnTextChanged);
        }

        // 富文本按钮事件
        if (boldButton != null)
            boldButton.onClick.AddListener(ApplyBold);
        if (italicButton != null)
            italicButton.onClick.AddListener(ApplyItalic);
        if (underlineButton != null)
            underlineButton.onClick.AddListener(ApplyUnderline);
        if (strikethroughButton != null)
            strikethroughButton.onClick.AddListener(ApplyStrikethrough);
    }

    private void Update()
    {
        // 实时检查文本选择状态
        CheckTextSelection();
    }

    private void OnTextChanged(string newText)
    {
        CheckTextSelection();
    }

    private void CheckTextSelection()
    {
        if (textInput == null)
        {
            return;
        }

        // 获取选中的文本
        var start = textInput.selectionAnchorPosition;
        var end = textInput.selectionFocusPosition;
        // 确保 start <= end
        if (start > end)
        {
            (start, end) = (end, start);
        }

        // 检查选择范围是否有效
        if (start == end || start < 0 || end > textInput.text.Length)
        {
            // 只有当之前有选择时才清空状态
            if (_selectionStartPos == -1 && _selectionEndPos == -1)
            {
                return;
            }

            _selectionStartPos = -1;
            _selectionEndPos = -1;
            Debug.Log("选择状态清空");
            return;
        }

        // 只有当选择范围真正改变时才更新状态
        if (_selectionStartPos == start && _selectionEndPos == end)
        {
            return;
        }

        _selectionStartPos = start;
        _selectionEndPos = end;
    }

    private static void ParseCharacters(string textInputText, ref List<CharacterData> characters)
    {
        // 清空之前的字符数据
        characters.Clear();
        if (string.IsNullOrEmpty(textInputText))
        {
            return;
        }

        var tagStack = new Stack<RichTextTagType>(); // 标签栈，用于管理当前激活的标签
        var currentTags = (byte)RichTextTagType.None; // 当前字符的标签状态
        var charIndex = 0; // 字符在原始文本中的位置
        for (var i = 0; i < textInputText.Length; i++)
        {
            var ch = textInputText[i];
            if (ch != '<')
            {
                // 普通字符，创建字符数据并记录当前标签状态
                AddCharacterWithTags(ch, charIndex, currentTags, ref characters);
                charIndex++;
                continue;
            }

            // 处理标签
            var newIndex = ProcessTag(textInputText, i, ref tagStack, ref currentTags);
            i = newIndex;
        }

        // 调试输出，方便查看解析结果
        LogParseResults(characters);
    }

    /// <summary>
    /// 添加带标签的字符到字符列表
    /// </summary>
    private static void AddCharacterWithTags(char ch, int charIndex, byte currentTags,
        ref List<CharacterData> characters)
    {
        var charData = new CharacterData(ch, charIndex);
        charData.SetTags(currentTags);
        characters.Add(charData);
    }

    /// <summary>
    /// 处理标签，返回新的索引位置
    /// </summary>
    private static int ProcessTag(string text, int startIndex, ref Stack<RichTextTagType> tagStack,
        ref byte currentTags)
    {
        // 寻找标签结束位置 '>'
        var tagEndIndex = text.IndexOf('>', startIndex);
        if (tagEndIndex == -1)
        {
            // 没有找到结束标签，当作普通字符处理
            return startIndex;
        }

        // 提取标签内容
        var tagContent = text.Substring(startIndex + 1, tagEndIndex - startIndex - 1);
        // 处理标签内容
        ProcessTagContent(tagContent, ref tagStack, ref currentTags);
        // 返回标签结束位置
        return tagEndIndex;
    }

    /// <summary>
    /// 处理标签内容
    /// </summary>
    private static void ProcessTagContent(string tagContent, ref Stack<RichTextTagType> tagStack, ref byte currentTags)
    {
        if (!tagContent.StartsWith("/"))
        {
            // 开始标签，添加到栈中
            ProcessStartTag(tagContent, ref tagStack, ref currentTags);
        }
        else
        {
            // 结束标签，从栈中移除对应的开始标签
            ProcessEndTag(tagContent, ref tagStack, ref currentTags);
        }
    }

    /// <summary>
    /// 处理开始标签
    /// </summary>
    private static void ProcessStartTag(string tagContent, ref Stack<RichTextTagType> tagStack, ref byte currentTags)
    {
        var tagType = RichTextDef.GetTagType(tagContent);
        if (!tagType.HasValue) return;
        tagStack.Push(tagType.Value);
        currentTags = RichTextDef.AddTag(currentTags, tagType.Value);
    }

    /// <summary>
    /// 处理结束标签
    /// </summary>
    private static void ProcessEndTag(string tagContent, ref Stack<RichTextTagType> tagStack, ref byte currentTags)
    {
        var tagName = tagContent[1..];
        var tagType = RichTextDef.GetTagType(tagName);
        if (!tagType.HasValue)
        {
            return;
        }

        // 从当前标签状态中移除该标签
        currentTags = RichTextDef.RemoveTag(currentTags, tagType.Value);
        // 从栈中移除对应的开始标签
        RemoveTagFromStack(tagType.Value, ref tagStack);
    }

    /// <summary>
    /// 从栈中移除指定标签
    /// </summary>
    private static void RemoveTagFromStack(RichTextTagType tagType, ref Stack<RichTextTagType> tagStack)
    {
        if (tagStack.Count <= 0)
        {
            return;
        }

        var stackTag = tagStack.Peek();
        if (stackTag == tagType)
        {
            tagStack.Pop();
        }
    }

    /// <summary>
    /// 输出解析结果日志
    /// </summary>
    private static void LogParseResults(List<CharacterData> characters)
    {
        Debug.Log($"解析完成！共解析出 {characters.Count} 个字符");
        for (var i = 0; i < Mathf.Min(characters.Count, 10); i++) // 只显示前10个字符的调试信息
        {
            Debug.Log($"字符 {i}: {characters[i]}");
        }
    }

    // 富文本应用方法
    private void ApplyBold()
    {
        if (textInput == null || _selectionStartPos < 0 || _selectionEndPos < 0)
        {
            Debug.LogWarning("没有有效的文本选择范围喵～");
            return;
        }

        // 解析当前文本获取字符数据
        ParseCharacters(textInput.text, ref _characters);
        // 获取选中范围内的字符
        var selectedCharacters = GetSelectedCharacters();
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("选中范围内没有字符喵～");
            return;
        }

        // 检查是否所有字符都有bold标签
        var allHaveBold = selectedCharacters.All(c => c.HasTag(RichTextTagType.Bold));
        // 根据情况添加或移除bold标签
        foreach (var character in selectedCharacters)
        {
            if (allHaveBold)
            {
                // 所有字符都有bold，全部移除
                character.RemoveTag(RichTextTagType.Bold);
            }
            else
            {
                // 不是所有字符都有bold，给没有的添加
                if (!character.HasTag(RichTextTagType.Bold))
                {
                    character.AddTag(RichTextTagType.Bold);
                }
            }
        }

        // 格式化输出结果
        var newText = FormatCharactersToRichText(_characters);
        Debug.Log($"newText:{newText}");
        textInput.text = newText;
        // 重新聚焦到输入框，避免丢失焦点
        textInput.ActivateInputField();
        Debug.Log($"Bold应用完成喵！操作: {(allHaveBold ? "移除" : "添加")}bold标签");
    }

    /// <summary>
    /// 将字符数据格式化为富文本
    /// </summary>
    private static string FormatCharactersToRichText(List<CharacterData> characters)
    {
        if (characters.Count == 0)
        {
            return string.Empty;
        }

        var result = new System.Text.StringBuilder();
        var currentTags = (byte)RichTextTagType.None; // 当前激活的标签状态
        foreach (var character in characters)
        {
            // 检查需要添加的标签
            var tagsToAdd = (byte)(character.tags & ~currentTags);
            if (tagsToAdd != 0)
            {
                result.Append(FormatTagsToString(tagsToAdd, false)); // 开始标签
            }

            // 检查需要移除的标签
            var tagsToRemove = (byte)(currentTags & ~character.tags);
            if (tagsToRemove != 0)
            {
                result.Append(FormatTagsToString(tagsToRemove, true)); // 结束标签
            }

            // 添加字符
            result.Append(character.character);
            // 更新当前标签状态
            currentTags = character.tags;
        }

        // 在最后关闭所有剩余的标签
        if (currentTags != 0)
        {
            result.Append(FormatTagsToString(currentTags, true));
        }

        return result.ToString();
    }

    private void ApplyItalic()
    {
    }

    private void ApplyUnderline()
    {
    }

    private void ApplyStrikethrough()
    {
    }

    /// <summary>
    /// 将标签格式化为字符串
    /// </summary>
    private static string FormatTagsToString(byte tags, bool isEndTag)
    {
        var result = new System.Text.StringBuilder();
        var prefix = isEndTag ? "</" : "<";
        const string suffix = ">";
        // 按优先级顺序处理标签（Bold -> Italic -> Underline -> Strikethrough）
        if ((tags & (byte)RichTextTagType.Bold) != 0)
        {
            result.Append($"{prefix}b{suffix}");
        }

        if ((tags & (byte)RichTextTagType.Italic) != 0)
        {
            result.Append($"{prefix}i{suffix}");
        }

        if ((tags & (byte)RichTextTagType.Underline) != 0)
        {
            result.Append($"{prefix}u{suffix}");
        }

        if ((tags & (byte)RichTextTagType.Strikethrough) != 0)
        {
            result.Append($"{prefix}s{suffix}");
        }

        return result.ToString();
    }

    /// <summary>
    /// 获取选中范围内的字符
    /// </summary>
    private List<CharacterData> GetSelectedCharacters()
    {
        return _characters.Where(character =>
            character.position >= _selectionStartPos && character.position < _selectionEndPos).ToList();
    }
}