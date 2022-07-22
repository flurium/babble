﻿using System.Collections.Generic;

namespace Client.Models
{
    internal struct Message
    {
        public bool IsIncoming { get; set; }
        public string? Text { get; set; }
        public List<MessageFile>? Files { get; set; }
    }

    internal struct MessageFile
    {
        public static readonly List<string> ImageExtentions = new() { ".bmp", ".jpeg", ".png", ".tiff", ".gif", ".icon", ".jpg" };
        public string Path { get; set; }
        public bool IsImage { get; set; }
    }
}