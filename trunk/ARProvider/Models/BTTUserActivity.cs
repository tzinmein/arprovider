using System;
using System.Web;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;

namespace ARProvider.Models
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
            get {
            	return lastLoginDate == null ? new DateTime(1970, 1, 1) : lastLoginDate.Value;
            }
        	set { lastLoginDate = value; }
        }


        [Property]
        public DateTime LastActivityDate
        {
            get {
            	return lastActivityDate == null ? new DateTime(1970, 1, 1) : lastActivityDate.Value;
            }
        	set { lastActivityDate = value; }
        }



        [Property]
        public DateTime LastPasswordChangeDate
        {
            get
            {
            	return lastPasswordChangeDate == null ? new DateTime(1970, 1, 1) : lastPasswordChangeDate.Value;
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