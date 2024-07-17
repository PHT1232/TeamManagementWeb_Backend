namespace ProjectModel.ChatModels 
{
    public class ChatMessageDisplay 
    {
        public long Id { get; set; }
        public string ChatMessage { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}