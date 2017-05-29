namespace Performance.Logger.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtestcasescallidcolumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APITestCases", "APICallId", c => c.Guid(nullable: false));
        }

        
        public override void Down()
        {
            DropColumn("dbo.APITestCases", "APICallId");
        }
    }
}
