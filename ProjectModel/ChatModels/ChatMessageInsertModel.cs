using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectModel.ChatModels
{
    public class ChatMessageInsertModel
    {
        public long ChatSessionId { get; set; }
        public string SentId { get; set; }
        public string ReceivedId { get; set; }
        public string Message { get; set; }
    }
}
