using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentAID.Model
{
    public class Message
    {
        public string Text { get; set; }
        public bool IsReceived { get; set; } // True for messages from ChatGPT, False for user messages
    }
}
