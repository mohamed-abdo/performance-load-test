namespace Test.Performance.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class datedefault : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APITraces", "CreatedOn", c => c.DateTime(nullable: false, defaultValueSql: "GetDate()"));
            AddColumn("dbo.PerformanceExecutions", "CreatedOn", c => c.DateTime(nullable: false, defaultValueSql: "GetDate()"));
            AddColumn("dbo.TraceDetails", "CreatedOn", c => c.DateTime(nullable: false, defaultValueSql: "GetDate()"));
        }

        public override void Down()
        {
            DropColumn("dbo.TraceDetails", "CreatedOn");
            DropColumn("dbo.PerformanceExecutions", "CreatedOn");
            DropColumn("dbo.APITraces", "CreatedOn");
        }
    }
}
