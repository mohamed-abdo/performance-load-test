(function () {
    console.log('Unit Test Executer started.');
    'use strict';
    let args = process.argv.slice(2);//get first argument as runId 
    let runId = '';
    let callbackUrl = '';
    let callbacTestCasesUrl = '';
    let callbackErrUrl = '';
    let UnitTestCollecion = '';
    let UnitTestEnvironment = '';
    if (args.length > 0) {
        UnitTestCollecion = args[0];
        UnitTestEnvironment = args[1];
        runId = args[2];
        callbackUrl = args[3];
        callbacTestCasesUrl= args[4];
        callbackErrUrl = args[5];
    }
    let newman = require('newman'),
        request = require('request'),
        _ = require('lodash');
    let testCaseCBHandler = function (failedTestCases) {
        if (failedTestCases == null)
            return;
        let req = '', resp = '';
        request({
            url: callbacTestCasesUrl,
            method: 'POST',
            headers: { 'content-type': 'application/json', 'Accept': 'application/json' },
            body: JSON.stringify(failedTestCases)
        }, function (err, response, body) {
            if (err)
                console.error(err);
        });
    };
    let errorHandler = function (err, args) {
        if ((err || args) == null)
            return;
        let req = '', resp = '';
        if (args) {
            args.request ? req = args.request : args;
            args.response ? resp = args.response : null;
        }
        let error = {
            error: `|${err}|`,
            request: req,
            response: resp
        };
        request({
            url: `${callbackErrUrl}?runId=${runId}`,
            method: 'POST',
            headers: { 'content-type': 'application/json', 'Accept': 'application/json' },
            json: true,
            body: JSON.stringify(error)
        }, function (err, response, body) {
            if (err)
                console.error(err);
        });
    };
    let onRequestDone = function (err, args) {
        let url = _.join(args.request.url.path, '/');
        let argument = '';
        let correlationId = !isNaN(parseInt(_.last(args.request.url.path))) ? _.last(args.request.url.path) : null;
        if (args.request.method == 'GET') {
            argument = url.replace(correlationId, '');//remove order id from url, for better grouping
            args.request.body.raw = null;
        }
        else {
            argument = JSON.parse(args.request.body.raw).apiOperation;
        }
        request({
            url: callbackUrl,
            method: 'POST',
            headers: { 'content-type': 'application/json', 'Accept': 'application/json' },
            json: true,
            body: {
                runId: runId,
                callId:args.item.id,
                method: args.request.method,
                statusCode: args.response.code,
                url: url,
                correlationId: correlationId,
                body: args.request.body.raw,
                response: args.response.body,
                argument: argument,
                responseTime: args.response.responseTime
            }
        }, function (error, response, body) {
            if (error) {
                errorHandler(error, response);
                console.error(error);
            }
        });
    };
    try {
    let emmiterResults = newman.run({
        collection: UnitTestCollecion,
        environment: UnitTestEnvironment,
        iterationCount: 1,
        delayRequest: 1,
        suppressExitCode: true,
        reporters: ['cli']
    }, function (err, summary) {
        errorHandler(err, _.isObject(summary)? summary.error: summary);
        console.log('collection run completed, with error:' + err);
    })
        .on('start', function (err, args) {
            console.log('running a collection...');
        })
        .on('request', function (err, args) {
            if (args.response.code)
                onRequestDone(err, args);
            else
                errorHandler(err, args);
        })
        .on('exception', function (err, args) {
            errorHandler(err, args);
        })
        .on('done', function (error, summary) {
            //to get succeeded test cases "assertions"
            //summary.run.executions.assertions
            if (error || summary.error) {
                errorHandler(error, summary.error);
                console.error('collection run encountered an error.');
            }
            else {
                let failedCases = _.map(summary.run.failures, function (item, idx) {
                return {
                    callId: item.source.id,
                    api: item.source.name,
                    failedTestCase: item.error.message,
                    url:`${item.source.request.method}: ${item.source.request.url}`
                    };
                });
                _.each(failedCases, function (failedCase, key) {
                 testCaseCBHandler(failedCase);
             });
             console.log('collection run completed.');
            }
        });
         } catch (error) {
          errorHandler(error);
          console.error('collection run encountered an internal error.');
    }
})();