using System;

namespace AzureSQL.ToDo {
    public class ToDoItem {
        public Guid Id { get; set; }
        public int? order { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public bool? completed { get; set; }
    }
}
