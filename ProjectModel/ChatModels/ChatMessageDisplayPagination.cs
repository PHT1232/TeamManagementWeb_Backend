namespace ProjectModel.ChatModels
{
    public class ChatMessageDisplayPagination
    {
        public DateTime LastChatDate { get; set; }
        public List<ChatMessageDisplay> chats { get; set; }
    }
}
