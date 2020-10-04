using DotNetty.Common.Internal.Logging;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RidoPnPDevice
{
    class Device
    {
        // long refreshInterval = 5;
        const string modelId = "dtmi:dev:tests:AComplexStory;1";
        
        ILogger logger;
        DeviceClient deviceClient;

        public async Task RunAsync(string connectionString, ILogger logger, CancellationToken quitSignal)
        {
            this.logger = logger;

            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt, new ClientOptions { ModelId = modelId }); // await DeviceClientFactory.CreateDeviceClientAsync(connectionString, logger, modelId);
            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback, null, quitSignal);                                                                                                                                    // await deviceClient.SetMethodDefaultHandlerAsync(DefaultCommandHadlerAsync, null, quitSignal);
                                                                                                                                                                                                                                                          // await deviceClient.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback, null, quitSignal);

            //var reported = new TwinCollection();
            //await deviceClient.UpdateReportedPropertiesAsync(reported);
            var reportedComp01 = new TwinCollection();
            reportedComp01["comp01"] = new
            {
                __t = "c",
                AComplexProperty = new
                {
                    ac = 200,
                    av = 1,
                    value = new {

                        customerName = "Contoso Corp.",
                        customerId = "123"
                    }
                }
            };
            Console.WriteLine(reportedComp01.ToJson());
            await deviceClient.UpdateReportedPropertiesAsync(reportedComp01);

            while(!quitSignal.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }

        
        private async Task DesiredPropertyUpdateCallback(TwinCollection desiredProperties, object userContext)
        {
            this.logger.LogWarning($"Received desired updates [{desiredProperties.ToJson()}]");

            var reportedComp01 = new TwinCollection();
            reportedComp01["comp01"] = new
            {
                __t = "c",
                AComplexProperty = new
                {
                    ac = 200,
                    av = desiredProperties.Version,
                    value = new
                    {

                        customerName = desiredProperties["comp01"]["AComplexProperty"]["customerName"],
                        customerId = desiredProperties["comp01"]["AComplexProperty"]["customerId"]
                    }
                }
            };
            await deviceClient.UpdateReportedPropertiesAsync(reportedComp01);
        }
    }
}

