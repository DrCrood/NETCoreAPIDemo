using System;
using System.Collections.Generic;
using System.Linq;
using DotNET5API.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace DotNET5API.Services
{
    public class EmployeeStatusMonitor : BackgroundService
    {
        private readonly IServiceLog _logger;
        public EmployeeStatusMonitor(IServiceLog logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"CanvasSyncService started at: {DateTime.Now}");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Starting the EmployeeStatusMonitor ... ");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //how often we check the task queue
                    await Task.Delay(5000, stoppingToken);
                    Console.WriteLine("Watching the Employee Status ...");
                    _logger.Log(Level.Trace, "logging from EmployeeStatusMonitor");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in EmployeeStatusMonitor: " + e.Message);
                    await StopAsync(stoppingToken);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        private void OnEmployeeStatusChanged(object sender, EmployeeDataChangedEventArgs arg)
        {
            Console.WriteLine(arg.changeMessage);
        }
    }
}
