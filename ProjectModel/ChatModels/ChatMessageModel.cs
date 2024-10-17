using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectModel.ChatModels
{
    public class ChatMessageModel
    {
        public long Id { get; set; }
        public string SentUserId { get; set; }
        public string ChatMessage { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}
