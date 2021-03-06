USE [CSETWeb]
GO
/****** Object:  Table [dbo].[USERS]    Script Date: 11/14/2018 3:57:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[USERS](
	[PrimaryEmail] [varchar](150) NOT NULL,
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Password] [varchar](250) NOT NULL,
	[Salt] [varchar](250) NOT NULL,
	[IsSuperUser] [bit] NOT NULL,
	[PasswordResetRequired] [bit] NOT NULL,
	[FirstName] [varchar](150) NULL,
	[LastName] [varchar](150) NULL,
	[Id] [uniqueidentifier] NULL,
 CONSTRAINT [PK_USERS_1] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_USERS]    Script Date: 11/14/2018 3:57:31 PM ******/
ALTER TABLE [dbo].[USERS] ADD  CONSTRAINT [IX_USERS] UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[USERS] ADD  CONSTRAINT [DF_USERS_IsSuperUser]  DEFAULT ((0)) FOR [IsSuperUser]
GO
ALTER TABLE [dbo].[USERS] ADD  CONSTRAINT [DF_USERS_PasswordResetRequired]  DEFAULT ((1)) FOR [PasswordResetRequired]
GO
ALTER TABLE [dbo].[USERS]  WITH CHECK ADD  CONSTRAINT [FK_USERS_USER_DETAIL_INFORMATION] FOREIGN KEY([Id])
REFERENCES [dbo].[USER_DETAIL_INFORMATION] ([Id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[USERS] CHECK CONSTRAINT [FK_USERS_USER_DETAIL_INFORMATION]
GO
