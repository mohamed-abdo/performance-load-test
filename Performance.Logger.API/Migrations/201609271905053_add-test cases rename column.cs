namespace Performance.Logger.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtestcasesrenamecolumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APITestCases", "FailedTestCase", c => c.String());
            DropColumn("dbo.APITestCases", "TestCase");
        }
        
        public override void Down()
        {
            AddColumn("dbo.APITestCases", "TestCase", c => c.String());
            DropColumn("dbo.APITestCases", "FailedTestCase");
        }
    }
}
