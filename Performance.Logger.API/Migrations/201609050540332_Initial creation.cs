namespace Test.Performance.API.Migrations
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
                        Method = c.String(),
                        Url = c.String(),
                        Status = c.String(),
                        DurationInMS = c.Long(nullable: false),
                        Argument = c.String(),
                        Body = c.String(),
                        Response = c.String(),
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
                        ExecutingMachine = c.String(),
                        StartingAt = c.DateTime(nullable: false),
                        CompletedAt = c.DateTime(nullable: false),
                        DurationInMS = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.RunId);
            
            CreateTable(
                "dbo.Environments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaymentAPI = c.String(),
                        IdentityAPI = c.String(),
                        BusinessAPI = c.String(),
                        ReportAPI = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TraceDetails",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Operation = c.String(nullable: false),
                        CorrelationId = c.String(nullable: false),
                        StartingAt = c.DateTime(),
                        CompletedAt = c.DateTime(),
                        DurationInMS = c.Long(nullable: false),
                        RunId = c.Guid(nullable: false),
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
            DropTable("dbo.Environments");
            DropTable("dbo.PerformanceExecutions");
            DropTable("dbo.APITraces");
        }
    }
}
