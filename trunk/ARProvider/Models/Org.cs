using Castle.ActiveRecord;

namespace ARProvider.Models
{
    [ActiveRecord]
    public class Org : BaseObj<Org>
    {
        public Org()
        {
        }

        [Property(Unique = true)]
        public virtual string Name { get; set; }   
    }

    public class OrgService : BaseService<Org>
    {
        private static Org _systemOrg;

        public OrgService(Token token) : base(token)
        {
            
        }

        public void Save(Org org)
        {
            PrepareSave(org);
            ActiveRecordMediator<Org>.SaveAndFlush(org);
            
        }

        public static Org GetSystemOrg()
        {
        	return _systemOrg ?? (_systemOrg = ActiveRecordMediator<Org>.FindAllByProperty("Name", "OIF")[0]);
        }

    	new public Org[] FindAll()
        {
        	return t.User.ProviderUserKey.ToString() == Token.AdminToken.BTTUser.GUID.ToString() ? ActiveRecordMediator<Org>.FindAll() : new[] { t.BTTUser.Organization };
        }

    	public Org FindByName(string name)
        {
            Org[] orgs = FindAllByProperty("Name", name);
            return orgs.Length == 1 ? orgs[0] : null;
        }

    }
}
