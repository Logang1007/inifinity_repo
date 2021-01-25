GO

SELECT [BetID]
      ,[TicketNumber]
      ,[FK_UserID]
      ,[FK_BranchID] FROM [BetGames].[bg].[Bet]
WHERE TicketNumber='@TicketNumber'

GO