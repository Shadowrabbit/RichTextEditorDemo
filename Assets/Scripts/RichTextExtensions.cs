// ******************************************************************
//       /\ /|       @file       RichTextExtensions.cs
//       \ V/        @brief      富文本扩展方法
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-29 15:35:12
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

/// <summary>
/// 富文本扩展方法类
/// 提供富文本相关的扩展功能喵～
/// </summary>
public static class RichTextExtensions
{
    /// <summary>
    /// 从标签集合中移除指定标签
    /// </summary>
    public static byte RemoveTag(byte tags, RichTextTagType tagType)
    {
        return (byte)(tags & ~(byte)tagType);
    }

    /// <summary>
    /// 添加标签到标签集合
    /// </summary>
    public static byte AddTag(byte tags, RichTextTagType tagType)
    {
        return (byte)(tags | (byte)tagType);
    }
}