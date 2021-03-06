﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SmartCode.App.BuildTasks
{
    public class ProcessBuildTask : IBuildTask
    {
        const string CREATE_NO_WINDOW = "CreateNoWindow";
        const string WORKING_DIRECTORY = "WorkingDirectory";
        const string FILE_NAME = "FileName";
        const string ARGS = "Args";
        const string TIMEOUT = "Timeout";
        private readonly ILogger<ProcessBuildTask> _logger;
        const int DEFAULT_TIME_OUT = 30 * 1000;
        const bool DEFAULT_CREATE_NO_WINDOW = true;
        public bool Initialized => true;

        public string Name => "Process";
        public ProcessBuildTask(ILogger<ProcessBuildTask> logger)
        {
            _logger = logger;
        }

        public Task Build(BuildContext context)
        {
            if (!context.Build.Paramters.Value(FILE_NAME, out string fileName))
            {
                throw new SmartCodeException($"Build:{context.BuildKey},Can not find Paramter:{FILE_NAME}!");
            }
            if (!context.Build.Paramters.Value(ARGS, out string args))
            {
                throw new SmartCodeException($"Build:{context.BuildKey},Can not find Paramter:{ARGS}!");
            }
            var process = new Process();
            var startInfo = process.StartInfo;
            startInfo.CreateNoWindow = DEFAULT_CREATE_NO_WINDOW;
            startInfo.FileName = fileName;
            startInfo.Arguments = args;
            if (context.Build.Paramters.Value(WORKING_DIRECTORY, out string workingDic))
            {
                startInfo.WorkingDirectory = workingDic;
            }
            if (context.Build.Paramters.Value(CREATE_NO_WINDOW, out bool createNoWin))
            {
                startInfo.CreateNoWindow = createNoWin;
            }
            _logger.LogDebug($"--------Process.FileName:{startInfo.FileName},Args:{startInfo.Arguments} Start--------");
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            try
            {
                process.Start();
                var timeOut = DEFAULT_TIME_OUT;
                if (context.Build.Paramters.Value(TIMEOUT, out int _timeout))
                {
                    timeOut = _timeout;
                }
                process.WaitForExit(timeOut);
                _logger.LogDebug($"--------Process.FileName:{startInfo.FileName},Args:{startInfo.Arguments} End--------");
            }
            finally
            {
                process.Dispose();
            }
            return Task.CompletedTask;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.LogDebug(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _logger.LogDebug(e.Data);
        }

        public void Initialize(IDictionary<string, object> paramters)
        {

        }
    }
}
