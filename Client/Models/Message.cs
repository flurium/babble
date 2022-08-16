using System.Collections.Generic;

namespace Client.Models
{
    internal struct Message
    {
        public bool IsIncoming { get; set; }
        public string? Text { get; set; }
        public List<MessageFile>? Files { get; set; }
        public string Time { get; set; }
        public string? User { get; set; }
    }

    internal struct MessageFile
    {
        public static readonly List<string> ImageExtentions = new() { ".bmp", ".jpeg", ".png", ".tiff", ".gif", ".icon", ".jpg" };
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsImage { get; set; }
    }
}