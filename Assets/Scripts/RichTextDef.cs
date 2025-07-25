﻿// ******************************************************************
//       /\ /|       @file       RichTextTagHelper.cs
//       \ V/        @brief      
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-07-24 05:20:56
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 富文本标签工具类
/// </summary>
public static class RichTextDef
{
    /// <summary>
    /// 标签映射表
    /// </summary>
    private static readonly Dictionary<RichTextTagType, string> TagMapping = new()
    {
        { RichTextTagType.Bold, "b" },
        { RichTextTagType.Italic, "i" },
        { RichTextTagType.Underline, "u" },
        { RichTextTagType.Strikethrough, "s" }
    };

    /// <summary>
    /// 根据标签名获取标签类型
    /// </summary>
    public static RichTextTagType? GetTagType(string tagName)
    {
        foreach (var kvp in TagMapping.Where(kvp => kvp.Value == tagName))
        {
            return kvp.Key;
        }

        return null;
    }
}