namespace Performance.Logger.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialcreation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.APITraces",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        RunId = c.Guid(nullable: false),
                        CorrelationId = c.String(),
                        Method = c.String(),
                        Url = c.String(),
                        Status = c.String(),
                        DurationInMS = c.Long(nullable: false),
                        Argument = c.String(),
                        Body = c.String(),
                        Response = c.String(),
                        CreatedOn = c.DateTime(nullable: false, defaultValueSql: "GetDate()"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PerformanceExecutions", t => t.RunId, cascadeDelete: true)
                .Index(t => t.RunId);
            
            CreateTable(
                "dbo.PerformanceExecutions",
                c => new
                    {
                        RunId = c.Guid(nullable: false),
                        ParallelProcess = c.Int(nullable: false),
                        Iterations = c.Int(nullable: false),
                        Specs = c.String(nullable: false),
                        Environment = c.String(),
                        ExecutingMachine = c.String(),
                        StartedAt = c.DateTime(),
                        CompletedAt = c.DateTime(),
                        DurationInMS = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, defaultValueSql: "GetDate()"),
                    })
                .PrimaryKey(t => t.RunId);
            
            CreateTable(
                "dbo.TraceDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Operation = c.String(nullable: false),
                        CorrelationId = c.String(nullable: false),
                        StartedAt = c.DateTime(),
                        CompletedAt = c.DateTime(),
                        DurationInMS = c.Long(nullable: false),
                        RunId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, defaultValueSql: "GetDate()"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PerformanceExecutions", t => t.RunId, cascadeDelete: true)
                .Index(t => t.RunId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TraceDetails", "RunId", "dbo.PerformanceExecutions");
            DropForeignKey("dbo.APITraces", "RunId", "dbo.PerformanceExecutions");
            DropIndex("dbo.TraceDetails", new[] { "RunId" });
            DropIndex("dbo.APITraces", new[] { "RunId" });
            DropTable("dbo.TraceDetails");
            DropTable("dbo.PerformanceExecutions");
            DropTable("dbo.APITraces");
        }
    }
}
