using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentAID.Model;

namespace StudentAID.View.Selectors
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IncomingMessageTemplate { get; set; }
        public DataTemplate OutgoingMessageTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var message = item as Message;
            return message?.IsReceived ?? false ? IncomingMessageTemplate : OutgoingMessageTemplate;
        }
    }
}
