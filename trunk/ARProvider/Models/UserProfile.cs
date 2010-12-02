using System.Collections;
using Castle.ActiveRecord;

namespace ARProvider.Models
{
    [ActiveRecord]
    public class UserProfile : BaseObj<UserProfile>
    {
        private IList addresses = new ArrayList();
        private IList messages = new ArrayList();

        [Property]
        public virtual string FirstName
        {
            get; set;
        }

        [Property]
        public virtual string LastName
        {
            get; set;
        }

        [Property]
        public virtual string Phone1
        {
            get; set;
        }

        [Property]
        public virtual string Phone2
        {
            get; set;
        }

        [Property]
        public virtual string TimeZoneId
        {
            get; set;
        }

        [HasMany(typeof(Address), Table = "Address", ColumnKey = "UserProfileID", Cascade = ManyRelationCascadeEnum.SaveUpdate)]
        public virtual IList Addresses
        {
            get { return addresses; }
            set { addresses = value; }
        }

        [HasMany(typeof(Message), Table = "Message", ColumnKey = "UserProfileID", Cascade = ManyRelationCascadeEnum.SaveUpdate)]
        public virtual IList Messages
        {
            get { return messages; }
            set { messages = value; }
        }


    }

    public class UserProfileService : BaseService<UserProfile>
    {

        public UserProfileService(Token token)
            : base(token)
        {

        }

        public void Save(UserProfile profile)
        {
            PrepareSave(profile);
            using (new SessionScope())
            {
                using (new TransactionScope())
                {
                    ActiveRecordMediator<UserProfile>.SaveAndFlush(profile);
                }
                
            }
        }
    }
}
