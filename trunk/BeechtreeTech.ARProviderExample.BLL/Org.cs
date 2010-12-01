using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;

namespace BeechtreeTech.ARProviderExample.BLL
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
            if (_systemOrg == null)
                _systemOrg = ActiveRecordMediator<Org>.FindAllByProperty("Name", "OIF")[0];

            return _systemOrg;
        }

        new public Org[] FindAll()
        {
            if (t.User.ProviderUserKey.ToString() == Token.AdminToken.BTTUser.GUID.ToString())
            {
                return ActiveRecordMediator<Org>.FindAll();

            }
            else
            {
                return new Org[] { t.BTTUser.Organization };
            }
        }

        public Org FindByName(string name)
        {
            Org[] orgs = FindAllByProperty("Name", name);
            if (orgs.Length == 1)
                return orgs[0];
            return null;
        }

    }
}
