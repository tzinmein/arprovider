using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;

namespace BeechtreeTech.ARProviderExample.BLL
{
    [ActiveRecord]
    public class BTTRole : BaseObj<BTTRole>
    {
        private IList assignedUsers = new ArrayList();
        
        [Property(Unique = true)]
        public string Name { get; set; }

        [HasAndBelongsToMany(typeof(BTTUser),
        Table = "UserRoles", ColumnKey = "roleid", ColumnRef = "userid")]
        public IList AssignedUsers
        {
            get { return assignedUsers; }
            set { assignedUsers = value; }
        }
    }

    public class BTTRoleService : BaseService<BTTRole>
    {
        public BTTRoleService(Token token)
            : base(token)
        {

        }

        public void Save(BTTRole role)
        {
            PrepareSave(role);
            ActiveRecordMediator<BTTRole>.SaveAndFlush(role);
        }
    }
}
