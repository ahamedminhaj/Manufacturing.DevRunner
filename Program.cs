﻿using System;
using System.Reflection;
using Bootstrap;
using Bootstrap.Locator;
using Bootstrap.StructureMap;
using log4net;
using Manufacturing.DataCollector;
using Manufacturing.DataCollector.Api;
using Manufacturing.DataCollector.Datasources;
using Manufacturing.DataPusher;
using Manufacturing.FacilityDataProcessor;
using Manufacturing.Framework.Configuration;
using Manufacturing.Framework.Logging;
using StructureMap;

namespace Manufacturing.DevRunner
{
    public class Program
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static IContainer _container;

        #endregion

        #region Static Methods

        private static void DisplayMenu()
        {
            char key;

            do
            {
                Console.WriteLine("0. Exit");
                Console.WriteLine("9. Start all");
                Console.WriteLine("L. Show unified real-time logs");
                Console.WriteLine("--------------------------");

                Console.WriteLine("1. Start WebAPI Host (to receive records)");
                Console.WriteLine("2. Start Datasource Scheduler (to generate records)");
                Console.WriteLine("3. Start Data Pusher (to send data from your local queue to Azure");
                Console.WriteLine("4. Start Event Hub Processors");

                key = Console.ReadKey().KeyChar;
                Console.WriteLine();

                if (key == 'l' || key == 'L' || key == '9')
                {
                    const string url = "http://log4stuff.com/app/1E899147-44CC-43D9-8F38-0D1EB5CD1D39";
                    System.Diagnostics.Process.Start(url);
                }

                if (key == '1' || key == '9')
                {
                    var apiHost = _container.GetInstance<ApiHost>();
                    apiHost.Run();
                }
                if (key == '2' || key == '9')
                {
                    var datasourceScheduler = _container.GetInstance<DatasourceScheduler>();
                    datasourceScheduler.Run();
                }
                if (key == '3' || key == '9')
                {
                    var pusher = _container.GetInstance<Pusher>();
                    pusher.Run();
                }
                if (key == '4' || key == '9')
                {
                    var eventHubProcessor = _container.GetInstance<EventHubProcessor>();
                    eventHubProcessor.Run();
                }

            } while (key != '0');
        }

        private static void Main(string[] args)
        {
            LoggingUtils.InitializeLogging(true);

            Console.WriteLine("Loading Configuration...");
            Bootstrapper.With.StructureMap()
                .And.ServiceLocator()
                .LookForTypesIn.ReferencedAssemblies()
                .Including.Assembly(Assembly.GetAssembly(typeof(FrameworkContainer)))
                .AndAssembly(Assembly.GetAssembly(typeof(DataCollectorContainer)))
                .AndAssembly(Assembly.GetAssembly(typeof(DataPusherContainer)))
                .AndAssembly(Assembly.GetAssembly(typeof(WorkerRole)))
                .AndAssembly(Assembly.GetAssembly(typeof(FaclityDataProcessorContainer)))
                //.With.StartupTasks()
                .Start();

            _container = (IContainer)Bootstrapper.Container;

            //This is just for intellisense when stepping through...
            var what = _container.WhatDoIHave();

            //Ensure our IoC is happy
            //_container.AssertConfigurationIsValid();
            Log.Debug("IoC configuration is valid");

            DisplayMenu();
        }

        #endregion
    }
}