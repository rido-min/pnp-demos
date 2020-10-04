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

        public async Task RunAsync(string connectionString, ILogger logger)
        {
            this.logger = logger;

            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt, new ClientOptions { ModelId = modelId }); // await DeviceClientFactory.CreateDeviceClientAsync(connectionString, logger, modelId);
                                                                                                                                                   // await deviceClient.SetMethodDefaultHandlerAsync(DefaultCommandHadlerAsync, null, quitSignal);
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

            //var reportedInterface02 = new TwinCollection();
            //reportedInterface02["myinterface02"] = new
            //{
            //    __t = "c",
            //    serialNumber = serialNumber
            //};
            //await deviceClient.UpdateReportedPropertiesAsync(reportedInterface02);

            
        }

        
        private async Task DesiredPropertyUpdateCallback(TwinCollection desiredProperties, object userContext)
        {
            this.logger.LogWarning($"Received desired updates [{desiredProperties.ToJson()}]");
            //var desiredPropertyValue = GetPropertyValue<long>(desiredProperties, "refreshInterval");
            //if (desiredPropertyValue > 0)
            //{
            //    refreshInterval = desiredPropertyValue;
            //    await AckDesiredPropertyReadAsync("refreshInterval", desiredPropertyValue, 200, "property synced", desiredProperties.Version);
            //}
        }

        T GetPropertyValue<T>(TwinCollection collection, string propertyName)
        {
            T result = default(T);
            if (collection.Contains(propertyName))
            {
                var propertyJson = collection[propertyName] as JObject;
                if (propertyJson != null)
                {
                    if (propertyJson.ContainsKey("value"))
                    {
                        var propertyValue = propertyJson["value"];
                        result = propertyValue.Value<T>();
                    }
                }
                else
                {
                    try
                    {
                        result = collection[propertyName].Value;
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, ex.Message);
                    }
                }
            }
            return result;
        }


        public async Task AckDesiredPropertyReadAsync(string propertyName, object payload, int statuscode, string description, long version)
        {
            var ack = CreateAck(propertyName, payload, statuscode, version, description);
            await deviceClient.UpdateReportedPropertiesAsync(ack);
        }

        private TwinCollection CreateAck(string propertyName, object value, int statusCode, long statusVersion, string statusDescription = "")
        {
            TwinCollection ackProp = new TwinCollection();
            ackProp[propertyName] = new
            {
                value = value,
                ac = statusCode,
                av = statusVersion,
                ad = statusDescription
            };
            return ackProp;
        }

        private TwinCollection CreateAck(string componentName, string propertyName, object value, int statusCode, long statusVersion, string statusDescription = "")
        {
            TwinCollection ackProp = new TwinCollection();
            ackProp[propertyName] = new
            {
                value = value,
                ac = statusCode,
                av = statusVersion,
                ad = statusDescription
            };
            TwinCollection compAck = new TwinCollection();
            compAck[componentName] = new
            {
                __t = "c", 
                ackProp
            };
            return compAck;
        }
    }
}

