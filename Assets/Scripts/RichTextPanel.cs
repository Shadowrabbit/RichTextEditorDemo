// ******************************************************************
//       /\ /|       @file       RichTextPanel.cs
//       \ V/        @brief      富文本编辑器面板
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 15:30:45
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 富文本编辑器面板
/// 提供富文本编辑的UI界面和交互功能喵～
/// </summary>
public class RichTextPanel : MonoBehaviour
{
    public TMP_InputField textInput;
    public Button boldButton;
    public Button italicButton;
    public Button underlineButton;
    public Button strikethroughButton;

    private void Start()
    {
        SetupEventListeners();
    }

    private void SetupEventListeners()
    {
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

    // 富文本应用方法
    private void ApplyBold()
    {
        ApplyRichTextTag(RichTextTagType.Bold);
    }

    private void ApplyItalic()
    {
        ApplyRichTextTag(RichTextTagType.Italic);
    }

    private void ApplyUnderline()
    {
        ApplyRichTextTag(RichTextTagType.Underline);
    }

    private void ApplyStrikethrough()
    {
        ApplyRichTextTag(RichTextTagType.Strikethrough);
    }

    /// <summary>
    /// 应用富文本标签的通用方法
    /// </summary>
    /// <param name="tagType">要应用的标签类型</param>
    private void ApplyRichTextTag(RichTextTagType tagType)
    {
        if (textInput == null)
        {
            Debug.LogWarning("textInput为空喵～");
            return;
        }

        var start = textInput.selectionAnchorPosition;
        var end = textInput.selectionFocusPosition;
        // 确保 start <= end
        if (start > end)
        {
            (start, end) = (end, start);
        }

        if (start < 0 || end < 0)
        {
            Debug.LogWarning("没有有效的文本选择范围喵～");
            return;
        }

        // 使用工具类应用标签
        var newText = RichTextUtils.ApplyRichTextTag(textInput.text, start, end, tagType);
        // 更新文本内容
        textInput.text = newText;
        // 重新聚焦到输入框，避免丢失焦点
        textInput.ActivateInputField();
        textInput.selectionAnchorPosition = start;
        textInput.selectionFocusPosition = end;
        Debug.Log($"标签应用完成喵！标签类型: {tagType}, 选择范围: {start}-{end}");
    }
}