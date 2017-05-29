namespace Performance.Logger.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtestcasesrenamecolumne : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APITestCases", "APITraceCallId", c => c.Guid(nullable: false));
            DropColumn("dbo.APITestCases", "APICallId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.APITestCases", "APICallId", c => c.Guid(nullable: false));
            DropColumn("dbo.APITestCases", "APITraceCallId");
        }
    }
}
