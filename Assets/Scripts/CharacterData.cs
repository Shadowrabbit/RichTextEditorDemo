// ******************************************************************
//       /\ /|       @file       CharacterData.cs
//       \ V/        @brief      富文本字符数据结构（位运算标签）
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-27 16:10:00
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using System.Collections.Generic;

public class CharacterData
{
    public readonly char character; // 字符
    public readonly int position; // 在原始文本中的位置
    public byte tags; // 标签状态，使用位运算存储

    public CharacterData(char ch, int pos)
    {
        character = ch;
        position = pos;
        tags = 0; // 初始化为无标签
    }

    public void AddTag(RichTextTagType tagType)
    {
        tags |= (byte)tagType;
    }

    public void RemoveTag(RichTextTagType tagType)
    {
        tags &= (byte)~tagType;
    }

    public bool HasTag(RichTextTagType tagType)
    {
        return (tags & (byte)tagType) != 0;
    }

    public bool HasAnyTag()
    {
        return tags != 0;
    }

    public void ClearAllTags()
    {
        tags = 0;
    }

    public void SetTags(byte newTags)
    {
        tags = newTags;
    }

    public override string ToString()
    {
        var tagsList = GetAllTags();
        var tagsStr = tagsList.Count > 0 ? string.Join(",", tagsList) : "None";
        return $"'{character}' pos:{position} tags:[{tagsStr}]";
    }

    private List<RichTextTagType> GetAllTags()
    {
        var result = new List<RichTextTagType>();
        if (HasTag(RichTextTagType.Bold)) result.Add(RichTextTagType.Bold);
        if (HasTag(RichTextTagType.Italic)) result.Add(RichTextTagType.Italic);
        if (HasTag(RichTextTagType.Underline)) result.Add(RichTextTagType.Underline);
        if (HasTag(RichTextTagType.Strikethrough)) result.Add(RichTextTagType.Strikethrough);
        return result;
    }
}