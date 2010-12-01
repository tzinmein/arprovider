using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;

namespace BeechtreeTech.ARProviderExample.BLL
{
    [ActiveRecord]
    public class Address : BaseObj<Address>
    {
        [Property(NotNull = true)]
        public virtual string Address1 { get; set; }

        [Property]
        public virtual string Address2 { get; set; }

        [Property(NotNull = true)]
        public virtual string City { get; set; }

        [Property(NotNull = true)]
        public virtual string State { get; set; }

        [Property]
        public virtual string Zip { get; set; }

    }

    public class AddressService : BaseService<Address>
    {
        public AddressService(Token token) : base(token)
        {
            
        }

        public void Save(Address address)
        {
            PrepareSave(address);
            ActiveRecordMediator<Address>.SaveAndFlush(address);
        }
    }
}
