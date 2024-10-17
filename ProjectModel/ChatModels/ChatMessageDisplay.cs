namespace ProjectModel.ChatModels 
{
    public class ChatMessageDisplay 
    {
        public DateTime ChatDate { get; set; }
        public List<ChatMessageModel> ListOfChatMessage { get; set; }
    }
}