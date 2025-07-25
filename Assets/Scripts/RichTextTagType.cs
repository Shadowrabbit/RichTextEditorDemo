// ******************************************************************
//       /\ /|       @file       RichTextTagType.cs
//       \ V/        @brief      富文本标签类型定义（位运算优化）
//       | "")       @author     Catarina·RabbitNya, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2025-01-27 16:10:00
//    *(__\_\        @Copyright  Copyright (c) 2025, Shadowrabbit
// ******************************************************************

[System.Flags]
public enum RichTextTagType : byte
{
    None = 0, // 0000 0000
    Bold = 1, // 0000 0001 <b>
    Italic = 2, // 0000 0010 <i>
    Underline = 4, // 0000 0100 <u>
    Strikethrough = 8 // 0000 1000 <s>
}