using System.Collections;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;

namespace ARProvider.Models
{
    [ActiveRecord]
    public class BTTUser : BaseObj<BTTUser>
    {
        
        private IList roles = new ArrayList();
        

        [Property(Unique = true)]
        public string UserName { get; set; }
        [Property]
        public string Password { get; set; }

        [Field] 
        public string PasswordSalt;
        [Property(Unique = true)]
        public string Email { get; set; }

        public BTTUserActivity Activity
        {
            get
            {
                SimpleQuery<BTTUserActivity> q = new SimpleQuery<BTTUserActivity>(@"from BTTUserActivity a where a.AUser.GUID = ?", this.GUID);
                return q.Execute()[0];
            }
        }
        

        [Property]
        public string PasswordQuestion { get; set; }
        [Property]
        public string PasswordAnswer { get; set; }
        [Property]
        public string Comment { get; set; }
        
        [BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
        public Org Organization { get; set; }
        [BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
        public UserProfile Profile { get; set; }

        
        [HasAndBelongsToMany(typeof(BTTRole),
        Table = "UserRoles", ColumnKey = "userid", ColumnRef = "roleid", Inverse = true)]
        public virtual IList Roles
        {
            get { return roles; }
            set { roles = value; }
        }
    }

    public class BTTUserService : BaseService<BTTUser>
    {
        public BTTUserService(Token token)
            : base(token)
        {

        }

        public void Save(BTTUser user)
        {
            PrepareSave(user);
            ActiveRecordMediator<BTTUser>.SaveAndFlush(user);
        }

        public BTTUser[] GetUsersInOrg()
        {

            SimpleQuery<BTTUser> q = new SimpleQuery<BTTUser>(
                @"from BTTUser u where u.Organization.GUID = ?", t.BTTUser.Organization.GUID);


            return q.Execute();
        }

        public BTTUser[] GetUsersInOrg(Org org)
        {

            SimpleQuery<BTTUser> q = new SimpleQuery<BTTUser>(
                @"from BTTUser u where u.Organization.GUID = ?", org.GUID);


            return q.Execute();
        }
    }
}
