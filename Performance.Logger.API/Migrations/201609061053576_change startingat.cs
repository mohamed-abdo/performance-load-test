namespace Test.Performance.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changestartingat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PerformanceExecutions", "StartedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.TraceDetails", "StartedAt", c => c.DateTime());
            DropColumn("dbo.PerformanceExecutions", "StartingAt");
            DropColumn("dbo.TraceDetails", "StartingAt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TraceDetails", "StartingAt", c => c.DateTime());
            AddColumn("dbo.PerformanceExecutions", "StartingAt", c => c.DateTime(nullable: false));
            DropColumn("dbo.TraceDetails", "StartedAt");
            DropColumn("dbo.PerformanceExecutions", "StartedAt");
        }
    }
}
