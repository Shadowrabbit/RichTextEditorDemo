// ******************************************************************
//       /\ /|       @file       RichTextUtils.cs
//       \ V/        @brief      富文本处理工具类
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 15:30:45
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 富文本处理工具类
/// 提供富文本解析、格式化、标签操作等功能喵～
/// </summary>
public static class RichTextUtils
{
    /// <summary>
    /// 对指定范围的文本应用富文本标签
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <param name="startIndex">开始位置</param>
    /// <param name="endIndex">结束位置</param>
    /// <param name="tagType">要应用的标签类型</param>
    /// <returns>应用标签后的富文本字符串</returns>
    public static string ApplyRichTextTag(string text, int startIndex, int endIndex, RichTextTagType tagType)
    {
        if (string.IsNullOrEmpty(text) || startIndex < 0 || endIndex < 0 || startIndex >= endIndex)
        {
            Debug.LogWarning("无效的文本或索引范围喵～");
            return text;
        }

        // 解析当前文本获取字符数据
        var characters = ParseRichText(text);
        // 获取选中范围内的字符
        var selectedCharacters = characters.Where(character =>
            character.position >= startIndex && character.position < endIndex).ToList();
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("选中范围内没有字符喵～");
            return text;
        }

        // 检查是否所有字符都有指定标签
        var allHaveTag = selectedCharacters.All(c => c.HasTag(tagType));
        // 根据情况添加或移除标签
        foreach (var character in selectedCharacters)
        {
            if (allHaveTag)
            {
                // 所有字符都有标签，全部移除
                character.RemoveTag(tagType);
            }
            else
            {
                // 不是所有字符都有标签，给没有的添加
                if (!character.HasTag(tagType))
                {
                    character.AddTag(tagType);
                }
            }
        }

        // 格式化输出结果
        var newText = FormatCharactersToRichText(characters);
        Debug.Log($"应用标签完成喵！标签类型: {tagType}, 操作: {(allHaveTag ? "移除" : "添加")}");
        return newText;
    }

    /// <summary>
    /// 解析富文本字符串，提取字符数据和标签信息
    /// </summary>
    /// <param name="text">要解析的富文本字符串</param>
    /// <returns>解析后的字符数据列表</returns>
    private static List<CharacterData> ParseRichText(string text)
    {
        var characters = new List<CharacterData>();
        if (string.IsNullOrEmpty(text))
        {
            return characters;
        }

        var tagStack = new Stack<RichTextTagType>(); // 标签栈，用于管理当前激活的标签
        var currentTags = (byte)RichTextTagType.None; // 当前字符的标签状态
        var charIndex = 0; // 字符在原始文本中的位置

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch != '<')
            {
                // 普通字符，创建字符数据并记录当前标签状态
                AddCharacterWithTags(ch, charIndex, currentTags, ref characters);
                charIndex++;
                continue;
            }

            // 处理标签
            var newIndex = ProcessTag(text, i, ref tagStack, ref currentTags);
            i = newIndex;
        }

        // 调试输出，方便查看解析结果
        LogParseResults(characters);
        return characters;
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
        currentTags = RichTextExtensions.AddTag(currentTags, tagType.Value);
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
        currentTags = RichTextExtensions.RemoveTag(currentTags, tagType.Value);
        // 从栈中移除对应的开始标签
        RemoveTagFromStack(tagType.Value, ref tagStack);
    }

    /// <summary>
    /// 将字符数据格式化为富文本字符串
    /// </summary>
    /// <param name="characters">字符数据列表</param>
    /// <returns>格式化后的富文本字符串</returns>
    private static string FormatCharactersToRichText(List<CharacterData> characters)
    {
        if (characters.Count == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        var currentTags = (byte)RichTextTagType.None; // 当前激活的标签状态
        foreach (var character in characters)
        {
            // 检查需要添加的标签
            var tagsToAdd = (byte)(character.tags & ~currentTags);
            // 找出当前字符需要但当前状态没有的标签
            if (tagsToAdd != 0)
            {
                result.Append(FormatTagsToString(tagsToAdd, false)); // 开始标签
            }

            // 检查需要移除的标签
            var tagsToRemove = (byte)(currentTags & ~character.tags);
            // 找出当前状态有但当前字符不需要的标签
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
    /// 将标签格式化为字符串
    /// </summary>
    private static string FormatTagsToString(byte tags, bool isEndTag)
    {
        var result = new StringBuilder();
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
    /// 输出解析结果日志
    /// </summary>
    private static void LogParseResults(List<CharacterData> characters)
    {
        Debug.Log($"解析完成喵！共解析出 {characters.Count} 个字符");
        for (var i = 0; i < Mathf.Min(characters.Count, 10); i++) // 只显示前10个字符的调试信息
        {
            Debug.Log($"字符 {i}: {characters[i]}");
        }
    }
}