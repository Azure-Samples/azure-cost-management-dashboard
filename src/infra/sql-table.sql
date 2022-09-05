CREATE TABLE [dbo].[billing](
	[date] [datetime] NOT NULL,
	[subscriptionId] [varchar](50) NOT NULL,
	[value] [float] NOT NULL,
	[lastupdated] [datetime] NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[billinglog](
	[id] [uniqueidentifier] NOT NULL,
	[date] [datetime] NOT NULL,
	[subscriptionId] [varchar](50) NOT NULL,
	[value] [float] NOT NULL,
	[valuechangepercent] [float] NOT NULL,
	[emailsent] [bit] NOT NULL
) ON [PRIMARY]
GO
