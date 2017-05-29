namespace Test.Performance.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCorrelationID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APITraces", "CorrelationId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APITraces", "CorrelationId");
        }
    }
}
