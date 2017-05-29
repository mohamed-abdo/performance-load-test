using Performance.Logger.API.Integration.Model;
using Performance.Logger.API.Models;
using Performance.Logger.API.Reports;
using Performance.Logger.API.Repository;
using Performance.Logger.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Performance.Logger.API.Controllers
{
    [RoutePrefix("api/PreformanceTracer")]
    public class TracerController : ApiController
    {

        [HttpGet]
        [Route("report/{runId?}")]
        public IHttpActionResult Report(Guid runId)
        {
            var loader = new TracerService();
            var report = loader.GetReport(runId);
            if (report != null)
                return Json(report);
            else
                return new StatusCodeResult(System.Net.HttpStatusCode.InternalServerError, this.Request);
        }

        [HttpPost]
        [Route("initialize")]
        public IHttpActionResult CreateTracing([FromBody]Tracer Tracer)
        {
            if (Tracer == null)
                return new StatusCodeResult(System.Net.HttpStatusCode.BadRequest, this.Request);
            var loader = new TracerService();
            var success = loader.InitializeMaster(Tracer);
            if (success)
                return Ok();
            else
                return new StatusCodeResult(System.Net.HttpStatusCode.InternalServerError, this.Request);
        }
        [HttpPost]
        [Route("finalize/{RunId?}/{CompletedAt?}")]
        public IHttpActionResult TracingReport([FromUri]Guid RunId, [FromUri] string CompletedAt)
        {
            var loader = new TracerService();
            var report = loader.TracingReport(RunId, CompletedAt);
            if (report != null)
                return Json(report);
            else
                return new StatusCodeResult(System.Net.HttpStatusCode.InternalServerError, this.Request);
        }
        [HttpPost]
        [Route("APIErrorLogger/{RunId?}")]
        public IHttpActionResult APIErrorLogger([FromUri]Guid RunId, [FromBody] string Error)
        {
            var loader = new TracerService();
            var success = loader.APIErrorLogger(RunId, Error);
            if (success)
                return Ok();
            else
                return new StatusCodeResult(System.Net.HttpStatusCode.InternalServerError, this.Request);
        }
        [HttpPost]
        [Route("APICalls")]
        public IHttpActionResult APICalls([FromBody] APIPerformance APIEndtoEnd)
        {
            if (APIEndtoEnd == null)
                return new StatusCodeResult(System.Net.HttpStatusCode.BadRequest, this.Request);
            var loader = new TracerService();
            var result = loader.APIPerformace(new APITrace()
            {
                RunId = APIEndtoEnd.runId,
                CallId = APIEndtoEnd.callId,
                DurationInMS = APIEndtoEnd.responseTime,
                Method = APIEndtoEnd.method,
                Url = APIEndtoEnd.url,
                CorrelationId = APIEndtoEnd.correlationId,
                Status = APIEndtoEnd.statusCode,
                Argument = APIEndtoEnd.argument,
                Body = APIEndtoEnd.body,
                Response = APIEndtoEnd.response
            });
            if (result)
                return Ok();
            else
                return new StatusCodeResult(System.Net.HttpStatusCode.InternalServerError, this.Request);
        }
        [HttpPost]
        [Route("APITestCase")]
        public IHttpActionResult APITestCase([FromBody] Integration.Model.APITestCase APITestCase)
        {
            if (APITestCase == null)
                return new StatusCodeResult(System.Net.HttpStatusCode.BadRequest, this.Request);
            var loader = new TracerService();
            var result = loader.APITestCase(new Models.APITestCase()
            {
                APITraceCallId = APITestCase.callId,
                API = APITestCase.api,
                Url = APITestCase.url,
                FailedTestCase = APITestCase.failedTestCase
            });
            if (result)
                return Ok();
            else
                return new StatusCodeResult(System.Net.HttpStatusCode.InternalServerError, this.Request);
        }
    }

}
