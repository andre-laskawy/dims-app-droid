using Dims.Common.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core.Logging;
using Nanomite;
using Nanomite.Core.Network;
using Nanomite.Core.Network.Common;
using Nanomite.Core.Network.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Dims.App.Droid
{
    public partial class MainPage : ContentPage
    {
        NanomiteClient client;

        public MainPage()
        {
            InitializeComponent();
            LogEditor.Text = "";
           
        }

        private async void InitBrokerConnection(string brokerAddress, string clientId, string user, string pass, string secret)
        {
            try
            {
                client = NanomiteClient.CreateGrpcClient(brokerAddress, clientId);
                client.OnConnected = async () =>
                {
                    SubscriptionMessage subscriptionMessage = new SubscriptionMessage() { Topic = LogLevel.Debug.ToString() };
                    await client.SendCommandAsync(subscriptionMessage, StaticCommandKeys.Subscribe);

                    subscriptionMessage = new SubscriptionMessage() { Topic = LogLevel.Info.ToString() };
                    await client.SendCommandAsync(subscriptionMessage, StaticCommandKeys.Subscribe);

                    subscriptionMessage = new SubscriptionMessage() { Topic = LogLevel.Warning.ToString() };
                    await client.SendCommandAsync(subscriptionMessage, StaticCommandKeys.Subscribe);

                    subscriptionMessage = new SubscriptionMessage() { Topic = LogLevel.Error.ToString() };
                    await client.SendCommandAsync(subscriptionMessage, StaticCommandKeys.Subscribe);
                };
                client.OnCommandReceived = (cmd, c) =>
                {
                    switch (cmd.Topic)
                    {
                        case "Info":
                        case "Debug":
                        case "Warning":
                        case "Error":
                            Device.BeginInvokeOnMainThread(() =>
                           {
                               var message = cmd.Data[0].CastToModel<LogMessage>();
                               LogEditor.Text += message.Message + Environment.NewLine;
                           });
                            break;
                    }
                };
                await client.ConnectAsync(user, pass, secret, true);
            } 
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void LoggingOnClicked(object sender, EventArgs e)
        {
            var cmd = new Nanomite.Core.Network.Common.Command() { Type = CommandType.Action, Topic = "SetLogLevel" };
            LogLevelInfo logMessage = new LogLevelInfo()
            {
                Level = LogLevel.Debug.ToString(),
            };
            cmd.Data.Add(Any.Pack(logMessage));

            await client.SendCommandAsync(cmd);
        }

        private async void LoggingOffClicked(object sender, EventArgs e)
        {
            var cmd = new Nanomite.Core.Network.Common.Command() { Type = CommandType.Action, Topic = "SetLogLevel" };
            LogLevelInfo logMessage = new LogLevelInfo()
            {
                Level = LogLevel.Off.ToString(),
            };
            cmd.Data.Add(Any.Pack(logMessage));

            await client.SendCommandAsync(cmd);
        }

        private async void LightOn(object sender, EventArgs e)
        {
            var cmd = new Nanomite.Core.Network.Common.Command() { Type = CommandType.Action, Topic = "LivingRoomLightOn" };
            await client.SendCommandAsync(cmd);
        }

        private async void LightOff(object sender, EventArgs e)
        {
            var cmd = new Nanomite.Core.Network.Common.Command() { Type = CommandType.Action, Topic = "LivingRoomLightOff" };
            await client.SendCommandAsync(cmd);
        }
    }
}
