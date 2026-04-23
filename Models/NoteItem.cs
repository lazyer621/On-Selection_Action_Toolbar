using System;
using System.Collections.Generic;
namespace translation.Models
{
    public class NoteItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<string> Tags { get; set; } = new List<string>();
        public string AiSummary { get; set; } = string.Empty;
        public string NoteType { get; set; } = "highlight";
    }
}