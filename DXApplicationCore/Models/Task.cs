using System;
namespace DXApplicationCore.Models
{
    public class Task
    {
        public int ID { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Progress { get; set; }
    }
}

