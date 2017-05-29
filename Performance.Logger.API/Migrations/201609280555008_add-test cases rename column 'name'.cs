namespace Performance.Logger.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtestcasesrenamecolumnname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APITestCases", "API", c => c.String());
            DropColumn("dbo.APITestCases", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.APITestCases", "Name", c => c.String());
            DropColumn("dbo.APITestCases", "API");
        }
    }
}
