﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectModel.ChatModels
{
    public class ChatMessage
    {
        public string SentId { get; set; }
        public string ReceivedId { get; set; }
        public string Message { get; set; }
    }
}
