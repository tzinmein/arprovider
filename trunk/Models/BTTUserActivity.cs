using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;

namespace BeechtreeTech.ARProviderExample.BLL
{
    [ActiveRecord]
    public class BTTUserActivity : BaseObj<BTTUserActivity>
    {
        private DateTime? lastActivityDate;
        private DateTime? lastLoginDate;
        private DateTime? lastPasswordChangeDate;

        [BelongsTo]
        public BTTUser AUser { get; set; }
        [Property()]
        public int FailedLogins { get; set; }
        [Property()]
        public bool IsLockedOut { get; set; }

        [Property]
        public DateTime LastLoginDate
        {
            get
            {
                if (lastLoginDate == null)
                    return new DateTime(1970, 1, 1);
                else
                    return lastLoginDate.Value;
            }
            set { lastLoginDate = value; }
        }


        [Property]
        public DateTime LastActivityDate
        {
            get
            {
                if (lastActivityDate == null)
                    return new DateTime(1970, 1, 1);
                else
                    return lastActivityDate.Value;
            }
            set { lastActivityDate = value; }
        }



        [Property]
        public DateTime LastPasswordChangeDate
        {
            get
            {
                if (lastPasswordChangeDate == null)
                    return new DateTime(1970, 1, 1);
                else
                    return lastPasswordChangeDate.Value;
            }
            set { lastPasswordChangeDate = value; }
        }
    }

    public class BTTUserActivityService : BaseService<BTTUserActivity>
    {
        public BTTUserActivityService(Token token)
            : base(token)
        {

        }

        public BTTUserActivity GetRecentActivityForUser(BTTUser user)
        {

            BTTUserActivity BTTUserActivity = (BTTUserActivity)HttpContext.Current.Cache["activity" + user.UserName];
            if (BTTUserActivity != null)
                return BTTUserActivity;

            SimpleQuery<BTTUserActivity> q = new SimpleQuery<BTTUserActivity>(
                @"from BTTUserActivity a where a.AUser.Organization.GUID = ? 
                AND a.AUser.GUID = ? 
                ORDER BY a.LastActivityDate DESC", t.BTTUser.Organization.GUID, user.GUID);

            BTTUserActivity[] activity = q.Execute();
            if( activity.Length != 1)
                return null;
            
            return activity[0];

        }

        public BTTUserActivity[] GetActivitiesForOrg()
        {

            SimpleQuery<BTTUserActivity> q = new SimpleQuery<BTTUserActivity>(
                @"from BTTUserActivity a where a.AUser.Organization.GUID = ?  
                ORDER BY a.AUser.UserName DESC", t.BTTUser.Organization.GUID);

            return q.Execute();
        }

        public void Save(BTTUserActivity activity)
        {
            PrepareSave(activity);
            ActiveRecordMediator<BTTUserActivity>.SaveAndFlush(activity);
        }
    }
}
