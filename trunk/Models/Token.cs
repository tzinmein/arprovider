using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;
using System.Web;
using System.Web.Security;

namespace BeechtreeTech.ARProviderExample.BLL
{
    public class Token
    {
        private MembershipUser _user;
        private static Token _adminToken;
        private static Token _anonPatientToken;
        private BTTUser _BTTUser;

        public Token()
        {

        }

        public Token(MembershipUser user)
        {

            _user = user;
            _BTTUser = (BTTUser)HttpContext.Current.Cache["user" + user.UserName];
            if (_BTTUser == null)
                _BTTUser = ActiveRecordMediator<BTTUser>.FindByPrimaryKey(new Guid(user.ProviderUserKey.ToString()));
        }


        public MembershipUser User
        {
            get { return _user; }
        }

        public BTTUser BTTUser
        {
            get { return _BTTUser; }
        }


        public void Save()
        {

            _BTTUser.ModifiedDate = DateTime.UtcNow;
            _BTTUser.ModifiedBy = Token.AdminToken.BTTUser.GUID;

            ActiveRecordMediator<BTTUser>.Save(_BTTUser);
            

        }

        
        public DateTime GetLocalTimeFromUTC(DateTime dateTime)
        {
            return GetLocalTimeFromUTC(dateTime, true);
        }

        public DateTime GetLocalTimeFromUTC(DateTime dateTime, bool isUtc)
        {
            DateTime dt;

            // I rteally don't like this, but the stupid-ass membership orovider insists on claiming
            // the UTC times I put in the DB are local
            if (dateTime.Kind == DateTimeKind.Local && isUtc)
            {
                dt = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
            }
            else if (dateTime.Kind == DateTimeKind.Utc || isUtc)
            {
                dt = dateTime;
            }
            else if (dateTime.Kind == DateTimeKind.Local)
            {
                dt = new DateTime(dateTime.ToUniversalTime().Ticks, DateTimeKind.Utc);
            }
            else
            {
                throw new Exception("Cannot convert DateTime with DateTimeKind=Unspecified and flag isUtc set to false.");
            }
            TimeZoneInfo clientZone = TimeZoneInfo.FindSystemTimeZoneById(_BTTUser.Profile.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(dt, clientZone);

        }

        public static Token AdminToken
        {
            get
            {
                if (_adminToken == null)
                {
                    _adminToken = new Token(Membership.GetUser("admin"));
                }
                return _adminToken;
            }
        }

        public static Token AnonPatientToken
        {
            get
            {
                if (_anonPatientToken == null)
                {
                    _anonPatientToken = new Token(Membership.GetUser("anonpatient"));
                }
                return _anonPatientToken;
            }
        }

    }
}
