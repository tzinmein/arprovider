using System;
using System.Collections.Generic;
using Castle.ActiveRecord;

namespace ARProvider.Models
{
    public abstract class BaseObj<T> where T : class
    {
        public BaseObj()
        {
        }

        [PrimaryKey(PrimaryKeyType.GuidComb)]
        public virtual Guid GUID { get; set; }

        //[Property]
        //public int ID { get; set; }

        [Version("Version")]
        public virtual int Version { get; set; }

        [Property]
        public virtual DateTime? CreatedDate { get; set; }

        [Property]
        public virtual DateTime? ModifiedDate { get; set; }

        [Property]
        public virtual DateTime? DeletedDate { get; set; }

        [Property]
        public virtual Guid ModifiedBy { get; set; }

    }

    public class BaseService<T> where T : BaseObj<T>
    {
        protected Token t;

        private static Dictionary<string, bool> _permissions = new Dictionary<string, bool>();

        public BaseService()
        {
            
        }

        public BaseService(Token token)
        {
            t = token;
        }

        public T FindByGUID(Guid guid)
        {


            return ActiveRecordMediator<T>.FindByPrimaryKey(guid);
        }

        public T[] FindAll()
        {


            return ActiveRecordMediator<T>.FindAll();
        }

        public bool Exists(Guid guid)
        {
            return ActiveRecordMediator<T>.Exists(guid);
        }

        public T[] FindAllByProperty(string property, object value)
        {

            return ActiveRecordMediator<T>.FindAllByProperty(property, value);
        }

        protected void PrepareSave(T item)
        {


            if (item.CreatedDate == null)
            {
                item.CreatedDate = DateTime.UtcNow;
            }
            item.ModifiedDate = DateTime.UtcNow;
            item.ModifiedBy = new Guid(t.User.ProviderUserKey.ToString());
        }
    }

    public enum PermType
    {
        CanView,
        CanCreate,
        CanModify,
        CanDelete
    }
}

