﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Colors.Net;
using NCli;
using WebJobs.Script.Cli.Common;
using WebJobs.Script.Cli.Interfaces;
using static WebJobs.Script.Cli.Common.OutputTheme;

namespace WebJobs.Script.Cli.Verbs
{
    internal abstract class BaseVerb : IVerb, IVerbError, IVerbPostRun
    {
        protected readonly ITipsManager _tipsManager;

        [Option("quiet", DefaultValue = false, HelpText = "Disable all logging", ShowInHelp = false)]
        public bool Quiet { get; set; }

        [Option("cli-dev", DefaultValue = false, HelpText = "Display exceptions for reporting issues", ShowInHelp = false)]
        public bool CliDev { get; set; }

        [Option("help", DefaultValue = false, HelpText = "Show this help")]
        public bool Help { get; set; }

        public string OriginalVerb { get; set; }

        public IDependencyResolver DependencyResolver { get; set; }

        public BaseVerb(ITipsManager tipsManager)
        {
            _tipsManager = tipsManager;
        }

        public abstract Task RunAsync();

        public Task OnErrorAsync(Exception e)
        {
            if (e is CliException)
            {
                var cliException = e as CliException;
                if (cliException.Handled)
                {
                    return Task.CompletedTask;
                }
            }

            if (CliDev)
            {
                ColoredConsole
                    .Error
                    .WriteLine(ErrorColor(e.ToString()));
            }
            else
            {
                ColoredConsole
                    .Error
                    .WriteLine(ErrorColor($"Error: {e.Message}"));

                ColoredConsole
                    .WriteLine($"You can run the same command passing {ExampleColor("--cli-dev")} and report an issue on https://github.com/azure/azure-webjobs-sdk-script/issues");
            }
            return Task.CompletedTask;
        }
        public Task PostRunVerbAsync(bool failed)
        {
            try
            {
                _tipsManager.Record(failed);
            }
            catch { }

            return Task.CompletedTask;
        }
    }
}