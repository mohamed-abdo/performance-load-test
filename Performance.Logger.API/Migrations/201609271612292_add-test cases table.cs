namespace Performance.Logger.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtestcasestable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.APITestCases",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        TestCase = c.String(),
                        Url = c.String(),
                        CreatedOn = c.DateTime(nullable: false,defaultValueSql:"GetDate()"),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.APITraces", "CallId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.APITraces", "CallId");
            DropTable("dbo.APITestCases");
        }
    }
}
