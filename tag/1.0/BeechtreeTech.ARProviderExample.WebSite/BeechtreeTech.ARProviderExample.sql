
    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK8C1490CB6A27DED1]') AND parent_object_id = OBJECT_ID('Address'))
alter table Address  drop constraint FK8C1490CB6A27DED1

 GO 


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK2A0A9B1F87CB58B7]') AND parent_object_id = OBJECT_ID('UserRoles'))
alter table UserRoles  drop constraint FK2A0A9B1F87CB58B7

 GO 


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK2A0A9B1FDCC94280]') AND parent_object_id = OBJECT_ID('UserRoles'))
alter table UserRoles  drop constraint FK2A0A9B1FDCC94280

 GO 


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FKC1F33ACC6A27DED1]') AND parent_object_id = OBJECT_ID('Message'))
alter table Message  drop constraint FKC1F33ACC6A27DED1

 GO 


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK34A6DCFDCF9E65EE]') AND parent_object_id = OBJECT_ID('BTTUserActivity'))
alter table BTTUserActivity  drop constraint FK34A6DCFDCF9E65EE

 GO 


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK9A6E564F6DD7DAB5]') AND parent_object_id = OBJECT_ID('BTTUser'))
alter table BTTUser  drop constraint FK9A6E564F6DD7DAB5

 GO 


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK9A6E564FB00DDEE3]') AND parent_object_id = OBJECT_ID('BTTUser'))
alter table BTTUser  drop constraint FK9A6E564FB00DDEE3

 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'Address') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Address
 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'BTTRole') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table BTTRole
 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'UserRoles') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table UserRoles
 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'Org') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Org
 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'Message') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Message
 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'BTTUserActivity') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table BTTUserActivity
 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'UserProfile') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table UserProfile
 GO 


    if exists (select * from dbo.sysobjects where id = object_id(N'BTTUser') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table BTTUser
 GO 


    create table Address (
        GUID UNIQUEIDENTIFIER not null,
       Version INT not null,
       CreatedDate DATETIME null,
       ModifiedDate DATETIME null,
       DeletedDate DATETIME null,
       ModifiedBy UNIQUEIDENTIFIER null,
       Address1 NVARCHAR(255) not null,
       Address2 NVARCHAR(255) null,
       City NVARCHAR(255) not null,
       State NVARCHAR(255) not null,
       Zip NVARCHAR(255) null,
       UserProfileID UNIQUEIDENTIFIER null,
       primary key (GUID)
    )
 GO 


    create table BTTRole (
        GUID UNIQUEIDENTIFIER not null,
       Version INT not null,
       CreatedDate DATETIME null,
       ModifiedDate DATETIME null,
       DeletedDate DATETIME null,
       ModifiedBy UNIQUEIDENTIFIER null,
       Name NVARCHAR(255) null unique,
       primary key (GUID)
    )
 GO 


    create table UserRoles (
        roleid UNIQUEIDENTIFIER not null,
       userid UNIQUEIDENTIFIER not null
    )
 GO 


    create table Org (
        GUID UNIQUEIDENTIFIER not null,
       Version INT not null,
       CreatedDate DATETIME null,
       ModifiedDate DATETIME null,
       DeletedDate DATETIME null,
       ModifiedBy UNIQUEIDENTIFIER null,
       Name NVARCHAR(255) null unique,
       primary key (GUID)
    )
 GO 


    create table Message (
        GUID UNIQUEIDENTIFIER not null,
       Version INT not null,
       CreatedDate DATETIME null,
       ModifiedDate DATETIME null,
       DeletedDate DATETIME null,
       ModifiedBy UNIQUEIDENTIFIER null,
       Text NVARCHAR(255) null,
       Controller NVARCHAR(255) null,
       Action NVARCHAR(255) null,
       Id NVARCHAR(255) null,
       Force BIT null,
       Ordinal INT null,
       UserProfileID UNIQUEIDENTIFIER null,
       primary key (GUID)
    )
 GO 


    create table BTTUserActivity (
        GUID UNIQUEIDENTIFIER not null,
       Version INT not null,
       CreatedDate DATETIME null,
       ModifiedDate DATETIME null,
       DeletedDate DATETIME null,
       ModifiedBy UNIQUEIDENTIFIER null,
       FailedLogins INT null,
       IsLockedOut BIT null,
       LastLoginDate DATETIME null,
       LastActivityDate DATETIME null,
       LastPasswordChangeDate DATETIME null,
       AUser UNIQUEIDENTIFIER null,
       primary key (GUID)
    )
 GO 


    create table UserProfile (
        GUID UNIQUEIDENTIFIER not null,
       Version INT not null,
       CreatedDate DATETIME null,
       ModifiedDate DATETIME null,
       DeletedDate DATETIME null,
       ModifiedBy UNIQUEIDENTIFIER null,
       FirstName NVARCHAR(255) null,
       LastName NVARCHAR(255) null,
       Phone1 NVARCHAR(255) null,
       Phone2 NVARCHAR(255) null,
       TimeZoneId NVARCHAR(255) null,
       primary key (GUID)
    )
 GO 


    create table BTTUser (
        GUID UNIQUEIDENTIFIER not null,
       Version INT not null,
       PasswordSalt NVARCHAR(255) null,
       CreatedDate DATETIME null,
       ModifiedDate DATETIME null,
       DeletedDate DATETIME null,
       ModifiedBy UNIQUEIDENTIFIER null,
       UserName NVARCHAR(255) null unique,
       Password NVARCHAR(255) null,
       Email NVARCHAR(255) null unique,
       PasswordQuestion NVARCHAR(255) null,
       PasswordAnswer NVARCHAR(255) null,
       Comment NVARCHAR(255) null,
       Organization UNIQUEIDENTIFIER null,
       Profile UNIQUEIDENTIFIER null,
       primary key (GUID)
    )
 GO 


    alter table Address 
        add constraint FK8C1490CB6A27DED1 
        foreign key (UserProfileID) 
        references UserProfile
 GO 


    alter table UserRoles 
        add constraint FK2A0A9B1F87CB58B7 
        foreign key (userid) 
        references BTTUser
 GO 


    alter table UserRoles 
        add constraint FK2A0A9B1FDCC94280 
        foreign key (roleid) 
        references BTTRole
 GO 


    alter table Message 
        add constraint FKC1F33ACC6A27DED1 
        foreign key (UserProfileID) 
        references UserProfile
 GO 


    alter table BTTUserActivity 
        add constraint FK34A6DCFDCF9E65EE 
        foreign key (AUser) 
        references BTTUser
 GO 


    alter table BTTUser 
        add constraint FK9A6E564F6DD7DAB5 
        foreign key (Organization) 
        references Org
 GO 


    alter table BTTUser 
        add constraint FK9A6E564FB00DDEE3 
        foreign key (Profile) 
        references UserProfile
 GO 

