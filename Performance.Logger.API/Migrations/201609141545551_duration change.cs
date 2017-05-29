namespace Performance.Logger.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class durationchange : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.APIErrorLogs",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Error = c.String(),
                        RunId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false,defaultValueSql:"GetDate()"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PerformanceExecutions", t => t.RunId, cascadeDelete: true)
                .Index(t => t.RunId);
            
            AlterColumn("dbo.APITraces", "DurationInMS", c => c.Double(nullable: false));
            AlterColumn("dbo.PerformanceExecutions", "DurationInMS", c => c.Double(nullable: false));
            AlterColumn("dbo.TraceDetails", "DurationInMS", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.APIErrorLogs", "RunId", "dbo.PerformanceExecutions");
            DropIndex("dbo.APIErrorLogs", new[] { "RunId" });
            AlterColumn("dbo.TraceDetails", "DurationInMS", c => c.Long(nullable: false));
            AlterColumn("dbo.PerformanceExecutions", "DurationInMS", c => c.Long(nullable: false));
            AlterColumn("dbo.APITraces", "DurationInMS", c => c.Long(nullable: false));
            DropTable("dbo.APIErrorLogs");
        }
    }
}
