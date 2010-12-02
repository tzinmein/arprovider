using Castle.ActiveRecord;

namespace ARProvider.Models
{
    [ActiveRecord]
    public class Message : BaseObj<Message>
    {

        [Property]
        public virtual string Text { get; set; }

        [Property]
        public virtual string Controller { get; set; }

        [Property]
        public virtual string Action { get; set; }

        [Property]
        public virtual string Id { get; set; }

        [Property]
        public virtual bool Force { get; set; }

        [Property]
        public virtual int Ordinal { get; set; }

    }

    public class MessageService : BaseService<Message>
    {
        public MessageService(Token token)
            : base(token)
        {
        }

        public void Save(Message message)
        {
            PrepareSave(message);
            ActiveRecordMediator<Message>.SaveAndFlush(message);
        }
    }
}
