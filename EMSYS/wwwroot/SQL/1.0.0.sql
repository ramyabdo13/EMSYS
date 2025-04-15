
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 12/24/2022 7:29:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 12/24/2022 7:29:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
    [Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[CreatedBy] [nvarchar](128) NULL,
	[CreatedOn] [datetime2](7) NULL,
	[ModifiedBy] [nvarchar](128) NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[SystemDefault] [bit] NOT NULL,
	[IsoUtcCreatedOn] [nvarchar](max) NULL,
	[IsoUtcModifiedOn] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 12/24/2022 7:29:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 12/24/2022 7:29:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 12/24/2022 7:29:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 12/24/2022 7:29:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[LockoutEndDateUtc] [datetime2](7) NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 12/24/2022 7:29:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](128) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO


BEGIN
CREATE TABLE [dbo].[GlobalOptionSet] (
    [Id]            NVARCHAR (128)   NOT NULL,
    [Code]          NVARCHAR (256)    NULL,
    [DisplayName]   NVARCHAR (256)   NULL,
    [Type]          NVARCHAR (256)   NULL,
    [Status]        NVARCHAR (256)   NULL,
    [OptionOrder]   int NULL,
    [CreatedBy] NVARCHAR (128) NULL,
    [CreatedOn] datetime NULL,
    [ModifiedBy] NVARCHAR (128) NULL,
    [ModifiedOn] datetime NULL,
    [SystemDefault]   bit default(0) not null,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.GlobalOptionSet] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_GlobalOptionSetType] ON [dbo].[GlobalOptionSet]([Type] ASC);
END

BEGIN
CREATE TABLE [dbo].[UserProfile] (
    [Id]                NVARCHAR (128)    NOT NULL,
    [AspNetUserId]     NVARCHAR (128)  NULL,
    [FirstName]           NVARCHAR (256)  NULL,
    [LastName]          NVARCHAR (256)  NULL,
    [FullName]          NVARCHAR (256)  NULL,
    [IDCardNumber]          NVARCHAR (256)  NULL,
    [DateOfBirth]          DateTime  NULL,
    [GenderId]     NVARCHAR (128)  NULL,
    [CountryName]     NVARCHAR(256)  NULL,
    [Address]     NVARCHAR (max)  NULL,
    [PostalCode]     NVARCHAR (128)  NULL,
    [PhoneNumber]     NVARCHAR (256)  NULL,
    [UserStatusId]     NVARCHAR (128)  NULL,
    [CreatedBy]         NVARCHAR (128)   NULL,
    [CreatedOn]         datetime  NULL,
    [ModifiedBy]        NVARCHAR (128)   NULL,
    [ModifiedOn]        datetime   NULL,
    [IsoUtcDateOfBirth] NVARCHAR (128) NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL,
    [IntakeYear] int null,
    [Code] NVARCHAR (128) null
    CONSTRAINT [PK_dbo.UserProfile] PRIMARY KEY CLUSTERED ([Id] ASC),
    --When delete user from AspNetUsers, delete the record from UserProfile as well
    CONSTRAINT [FK_dbo.UserProfile_dbo.AspNetUsers_AspNetUserId] FOREIGN KEY ([AspNetUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_UserProfileUserStatusId] ON [dbo].[UserProfile]([UserStatusId] ASC);
END

BEGIN
CREATE TABLE [dbo].[UserAttachment] (
    [Id]                NVARCHAR (128)    NOT NULL,
    [UserProfileId]     NVARCHAR (128)  NULL,
    [FileUrl]           NVARCHAR (max)  NULL,
    [FileName]          NVARCHAR (256)  NULL,
    [UniqueFileName]    NVARCHAR (256)  NULL,
    [AttachmentTypeId]  NVARCHAR (128)     NULL,
    [CreatedBy]         NVARCHAR (128)   NULL,
    [CreatedOn]         datetime  NULL,
    [ModifiedBy]        NVARCHAR (128)   NULL,
    [ModifiedOn]        datetime   NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.UserAttachment] PRIMARY KEY CLUSTERED ([Id] ASC),
    --When delete user from [UserProfile], delete the record from [UserAttachment] as well
    CONSTRAINT [FK_dbo.UserAttachment_dbo.UserProfile_UserProfileId] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfile] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_UserAttachmentUserProfileId] ON [dbo].[UserAttachment]([UserProfileId] ASC);
END

BEGIN
CREATE TABLE [dbo].[EmailTemplate] (
    [Id]            NVARCHAR (128)    NOT NULL,
    [Subject]        NVARCHAR (max)   NULL,
    [Body]        NVARCHAR (max)   NULL,
    [Type]        NVARCHAR (256)   NULL
    CONSTRAINT [PK_dbo.EmailTemplate] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

BEGIN
CREATE TABLE [dbo].[LoginHistory] (
    [Id]            NVARCHAR (128)    NOT NULL,
    [AspNetUserId]  NVARCHAR (128)   NULL,
    [LoginDateTime] datetime   NULL,
    [IsoUtcLoginDateTime] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.LoginHistory] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

BEGIN
insert into GlobalOptionSet values(NEWID(),'ProfilePicture','Profile Picture','UserAttachment','Active',1,null,null,null,null,1,null,null)
END

BEGIN
insert into GlobalOptionSet values(NEWID(),'Female','Female','Gender','Active',1,null,null,null,null,0,null,null)
insert into GlobalOptionSet values(NEWID(),'Male','Male','Gender','Active',2,null,null,null,null,0,null,null)
insert into GlobalOptionSet values(NEWID(),'Other','Other','Gender','Active',3,null,null,null,null,0,null,null)
END

BEGIN
insert into GlobalOptionSet values(NEWID(),'Registered','Registered','UserStatus','Active',1,null,null,null,null,1,null,null)
insert into GlobalOptionSet values(NEWID(),'Validated','Validated','UserStatus','Active',2,null,null,null,null,0,null,null)
insert into GlobalOptionSet values(NEWID(),'NotValidated','Not Validated','UserStatus','Active',3,null,null,null,null,0,null,null)
END

BEGIN
insert into AspNetRoles values (NEWID(),'System Admin','SYSTEM ADMIN',null,null,null,null,1,null,null,null)
insert into AspNetRoles values (NEWID(),'Teacher','TEACHER',null,null,null,null,1,null,null,null)
insert into AspNetRoles values (NEWID(),'Student','STUDENT',null,null,null,null,1,null,null,null)
END

BEGIN
insert into EmailTemplate values(NEWID(),'Confirm Your Email To Complete [WebsiteName] Account Registration','<p>Hi [Username],<br><br>Thanks for signning up an account on [WebsiteName].</p><p>Click <a href="[Url]">Here</a> to confirm your email in order to login. Thank You.</p><p>If you did not sign up an account on [WebsiteName], please ignore this email.</p><p><i>Do not reply to this email.</i></p><p>Regards,<br>[WebsiteName]</p>','ConfirmEmail')
insert into EmailTemplate values(NEWID(),'Password Reset For [WebsiteName] Account','<p>Hi [Username],<br><br>Kindly be informed that your password for the [WebsiteName] account has been reset by [ResetByName].</p><p>Below is your temporary new password to log in:<br><b>New Password:</b> [NewPassword]</p><p><b>NOTE:</b> As a safety precaution, you are advised to change your password after you log in later. Thank you.</p><p><i>Do not reply to this email.</i></p><p>Regards,<br>[WebsiteName]</p>','PasswordResetByAdmin')
insert into EmailTemplate values(NEWID(),'Password Reset For [WebsiteName] Account','<p>Hi [Username],<br><br>There was a request to reset your password on [WebsiteName].</p><p><a href="[Url]">Click Here</a> and follow the instructions to reset your password. Thank You.</p><p></p><p>If you did not make this request then please ignore this email.</p><p><i>Do not reply to this email.</i></p><p>Regards,<br>[WebsiteName]</p>','ForgotPassword')
END

BEGIN
CREATE TABLE [dbo].[Subject] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    [CreatedBy] NVARCHAR (128) NULL,
    [CreatedOn] datetime NULL,
    [ModifiedBy] NVARCHAR (128) NULL,
    [ModifiedOn] datetime NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.Subject] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

--originally wanted to use [dbo].[Class], but thinking that it will be confused with "public class Class", so changed to ClassHub
BEGIN
CREATE TABLE [dbo].[ClassHub] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    [IsActive] bit NULL,
    [CreatedBy] NVARCHAR (128) NULL,
    [CreatedOn] datetime NULL,
    [ModifiedBy] NVARCHAR (128) NULL,
    [ModifiedOn] datetime NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.ClassHub] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

BEGIN
CREATE TABLE [dbo].[StudentClass] (
    [StudentId] NVARCHAR (128) NOT NULL,
    [ClassId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.StudentClass] PRIMARY KEY CLUSTERED ([StudentId] ASC, [ClassId] ASC),
    CONSTRAINT [FK_dbo.StudentClass_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentClass_dbo.ClassHub_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [dbo].[ClassHub] ([Id]) ON DELETE CASCADE
    
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentId] ON [dbo].[StudentClass]([StudentId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_ClassId] ON [dbo].[StudentClass]([ClassId] ASC);
END

BEGIN
CREATE TABLE [dbo].[QuestionType] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Code] NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL
    CONSTRAINT [PK_dbo.QuestionType] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

BEGIN
insert into QuestionType values(NEWID(),'MCQ','Multiple Choice Question')
insert into QuestionType values(NEWID(),'SA','Short Answer')
END

BEGIN
CREATE TABLE [dbo].[Question] (
    [Id]   NVARCHAR (128) NOT NULL,
    [QuestionTitle] NVARCHAR (max) NULL,
    [SubjectId] NVARCHAR (128) NULL,
    [QuestionTypeId] NVARCHAR (128) NULL,
    [IsActive] bit NULL,
    [CreatedBy] NVARCHAR (128) NULL,
    [CreatedOn] datetime NULL,
    [ModifiedBy] NVARCHAR (128) NULL,
    [ModifiedOn] datetime NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.Question] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Question_dbo.Subject_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subject] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_dbo.Question_dbo.QuestionType_QuestionTypeId] FOREIGN KEY ([QuestionTypeId]) REFERENCES [dbo].[QuestionType] ([Id]) ON DELETE SET NULL
);
END

BEGIN
CREATE TABLE [dbo].[QuestionAttachment] (
    [Id]                NVARCHAR (128)    NOT NULL,
    [QuestionId]     NVARCHAR (128)  NULL,
    [FileUrl]           NVARCHAR (max)  NULL,
    [FileName]          NVARCHAR (256)  NULL,
    [UniqueFileName]    NVARCHAR (256)  NULL,
    [CreatedBy]         NVARCHAR (128)   NULL,
    [CreatedOn]         datetime  NULL,
    [ModifiedBy]        NVARCHAR (128)   NULL,
    [ModifiedOn]        datetime   NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.QuestionAttachment] PRIMARY KEY CLUSTERED ([Id] ASC),
    --When delete user from [UserProfile], delete the record from [QuestionAttachment] as well
    CONSTRAINT [FK_dbo.QuestionAttachment_dbo.Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[Question] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_QuestionAttachmentQuestionId] ON [dbo].[QuestionAttachment]([QuestionId] ASC);
END

BEGIN
CREATE TABLE [dbo].[Answer] (
    [Id]   NVARCHAR (128) NOT NULL,
    [QuestionId] NVARCHAR (128) NULL,
    [AnswerText] NVARCHAR (max) NULL,
    [IsCorrect] bit default(0) not null,
    [AnswerOrder] int null,
    [CreatedBy] NVARCHAR (128) NULL,
    [CreatedOn] datetime NULL,
    [ModifiedBy] NVARCHAR (128) NULL,
    [ModifiedOn] datetime NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL
    CONSTRAINT [PK_dbo.Answer] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_AnswerQuestionId] ON [dbo].[Answer]([QuestionId] ASC);
END


BEGIN
CREATE TABLE [dbo].[Exam] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NULL,
    [Description] NVARCHAR (max) NULL,
    [StartDate] datetime NULL,
    [IsoUtcStartDate] NVARCHAR (128) NULL,
    [EndDate] datetime NULL,
    [IsoUtcEndDate] NVARCHAR (128) NULL,
    [Duration] int NULL,
    [ReleaseAnswer] bit NULL,
    [IsActive] bit NULL,
    [MarksToPass] decimal (10,2) NULL,
    [RandomizeQuestions] bit NULL,
    [CreatedBy] NVARCHAR (128) NULL,
    [CreatedOn] datetime NULL,
    [ModifiedBy] NVARCHAR (128) NULL,
    [ModifiedOn] datetime NULL,
    [IsoUtcCreatedOn] NVARCHAR (128) NULL,
    [IsoUtcModifiedOn] NVARCHAR (128) NULL,
    [IsPublished]   bit NULL
    CONSTRAINT [PK_dbo.Exam] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

BEGIN
CREATE TABLE [dbo].[ExamClassHub] (
    [ExamId] NVARCHAR (128) NOT NULL,
    [ClassHubId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.ExamClassHub] PRIMARY KEY CLUSTERED ([ExamId] ASC, [ClassHubId] ASC),
    CONSTRAINT [FK_dbo.ExamClassHub_dbo.Exam_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[Exam] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.ExamClassHub_dbo.ClassHub_ClassHubId] FOREIGN KEY ([ClassHubId]) REFERENCES [dbo].[ClassHub] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_ExamClassHubExamId] ON [dbo].[ExamClassHub]([ExamId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_ExamClassHubClassHubId] ON [dbo].[ExamClassHub]([ClassHubId] ASC);
END

BEGIN
CREATE TABLE [dbo].[ExamSubject] (
    [ExamId] NVARCHAR (128) NOT NULL,
    [SubjectId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.ExamSubject] PRIMARY KEY CLUSTERED ([ExamId] ASC, [SubjectId] ASC),
    CONSTRAINT [FK_dbo.ExamSubject_dbo.Exam_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[Exam] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.ExamSubject_dbo.Subject_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subject] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_ExamSubjectExamId] ON [dbo].[ExamSubject]([ExamId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_ExamSubjectSubjectId] ON [dbo].[ExamSubject]([SubjectId] ASC);
END

BEGIN
CREATE TABLE [dbo].[ExamQuestion] (
    [ExamId] NVARCHAR (128) NOT NULL,
    [QuestionId] NVARCHAR (128) NOT NULL,
    [QuestionOrder] int NULL,
    [Mark] decimal (10,2)  NULL
    CONSTRAINT [PK_dbo.ExamQuestion] PRIMARY KEY CLUSTERED ([ExamId] ASC, [QuestionId] ASC),
    CONSTRAINT [FK_dbo.ExamQuestion_dbo.Exam_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[Exam] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.ExamQuestion_dbo.Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[Question] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_ExamQuestionExamId] ON [dbo].[ExamQuestion]([ExamId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_ExamQuestionQuestionId] ON [dbo].[ExamQuestion]([QuestionId] ASC);
END

BEGIN
CREATE TABLE [dbo].[StudentExam] (
    [Id]   NVARCHAR (128) NOT NULL,
    [StudentId]   NVARCHAR (128) NULL,
    [ExamId]   NVARCHAR (128) NULL,
    [StartedOn] datetime NULL,
    [EndedOn] datetime NULL,
    [IsoUtcStartedOn] NVARCHAR (128) NULL,
    [IsoUtcEndedOn] NVARCHAR (128) NULL,
    [Result] decimal NULL,
    [Passed] bit null
    CONSTRAINT [PK_dbo.StudentExam] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.StudentExam_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentExam_dbo.Exam_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[Exam] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentExamStudentId] ON [dbo].[StudentExam]([StudentId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentExamExamId] ON [dbo].[StudentExam]([ExamId] ASC);
END

BEGIN
CREATE TABLE [dbo].[StudentAnswer] (
    [Id]   NVARCHAR (128) NOT NULL,
    [StudentId]   NVARCHAR (128)  NULL,
    [ExamId]   NVARCHAR (128)  NULL,
    [QuestionId]   NVARCHAR (128)  NULL,
    [AnswerId]   NVARCHAR (128)  NULL,
    [AnswerText]   NVARCHAR (max)  NULL,
    [MarksObtained]   decimal  NULL
    CONSTRAINT [PK_dbo.StudentAnswer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.StudentAnswer_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentAnswer_dbo.Answer_AnswerId] FOREIGN KEY ([AnswerId]) REFERENCES [dbo].[Answer] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_dbo.StudentAnswer_dbo.Exam_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[Exam] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentAnswer_dbo.Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[Question] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentAnswerStudentId] ON [dbo].[StudentAnswer]([StudentId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentAnswerExamId] ON [dbo].[StudentAnswer]([ExamId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentAnswerQuestionId] ON [dbo].[StudentAnswer]([QuestionId] ASC);
END

BEGIN
CREATE TABLE [dbo].[StudentAnswerCloned] (
    [Id]   NVARCHAR (128) NOT NULL,
    [StudentId]   NVARCHAR (128)  NULL,
    [ExamId]   NVARCHAR (128)  NULL,
    [QuestionId]   NVARCHAR (128)  NULL,
    [AnswerId]   NVARCHAR (128)  NULL,
    [AnswerText]   NVARCHAR (max)  NULL,
    [MarksObtained]   decimal  NULL
    CONSTRAINT [PK_dbo.StudentAnswerCloned] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.StudentAnswerCloned_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentAnswerCloned_dbo.Answer_AnswerId] FOREIGN KEY ([AnswerId]) REFERENCES [dbo].[Answer] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_dbo.StudentAnswerCloned_dbo.Exam_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[Exam] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentAnswerCloned_dbo.Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[Question] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentAnswerClonedStudentId] ON [dbo].[StudentAnswerCloned]([StudentId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentAnswerClonedExamId] ON [dbo].[StudentAnswerCloned]([ExamId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentAnswerClonedQuestionId] ON [dbo].[StudentAnswerCloned]([QuestionId] ASC);
END


BEGIN
CREATE TABLE [dbo].[StudentQuestionOrder] (
    [Id]   NVARCHAR (128) NOT NULL,
    [StudentId]   NVARCHAR (128)  NULL,
    [ExamId]   NVARCHAR (128)  NULL,
    [QuestionOrder]   NVARCHAR (max)  NULL
    --[QuestionOrder]   int  NULL
    CONSTRAINT [PK_dbo.StudentQuestionOrder] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.StudentQuestionOrder_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentQuestionOrder_dbo.Exam_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[Exam] ([Id]) ON DELETE CASCADE
    --CONSTRAINT [FK_dbo.StudentQuestionOrder_dbo.Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[Question] ([Id])
);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentQuestionOrderStudentId] ON [dbo].[StudentQuestionOrder]([StudentId] ASC);
END

BEGIN
CREATE NONCLUSTERED INDEX [IX_StudentQuestionOrderExamId] ON [dbo].[StudentQuestionOrder]([ExamId] ASC);
END

--BEGIN
--CREATE NONCLUSTERED INDEX [IX_StudentQuestionOrderQuestionId] ON [dbo].[StudentQuestionOrder]([QuestionId] ASC);
--END

BEGIN
CREATE TABLE [dbo].[ErrorLog] (
    [Id]   NVARCHAR (128) NOT NULL,
    [UserId] NVARCHAR (128) NULL,
    [ErrorMessage] NVARCHAR (max) NULL,
    [ErrorDetails] NVARCHAR (max) NULL,
    [ErrorDate] datetime NULL
    CONSTRAINT [PK_dbo.ErrorLog] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ErrorLog_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
END


BEGIN
CREATE TABLE [dbo].[Governate] (
    [Id] int IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR (128) NULL
);
END

BEGIN
insert into Governate (Name) values 
('Alexandria'),('Aswan'),('Asyut'),('Beheira'),('Beni Suef'),('Cairo'),('Dakahlia'),('Damietta'),
('Faiyum'),('Gharbia'),('Giza'),('Ismailia'),('Kafr El Sheikh'),('Luxor'),('Matruh'),('Minya'),
('Monufia'),('New Valley'),('North Sinai'),('Port Said'),('Qalyubia'),('Qena'),('Red Sea'),
('Sharqia'),('Sohag'),('South Sinai'),('Suez')
END
