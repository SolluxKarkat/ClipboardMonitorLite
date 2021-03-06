﻿using System;
using SettingsLib;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using CloudConnectionLib.Messages;
using Microsoft.AspNetCore.SignalR.Client;

namespace CloudConnectionLib
{
    /// <summary>
    /// Handles all the interactions using SignalR with the servers and
    /// the API, will also change the inboundMessage or act
    /// accordingly when the outgoing message changes.
    /// Inbound messages are received using SignalR, and outbound
    /// messages are sent using the API.
    /// </summary>
    public class CloudInteractions
    {
        private Settings _settings;
        private static HubConnection connection;
        private SignalRMessage _inboundMessage;
        private SignalRMessage _outgoingMessage;
        private ApiFunction _function;
        private OnlineSettings _onlineSettings;

        public CloudInteractions(SignalRMessage inboundMessage, SignalRMessage outgoingMessage,
            Settings settings, OnlineSettings onlineSettings)
        {
            //connection = new HubConnectionBuilder()
            //    .WithUrl("https://clipmanagerweb.azurewebsites.net/broadcast").Build();
            var baseUrl = "https://clipboardmonitorlitefunctions.azurewebsites.net";
            var connection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/api")
                .Build();
            _settings = settings;
            _inboundMessage = inboundMessage;
            _outgoingMessage = outgoingMessage;
            _onlineSettings = onlineSettings;
            _function = new ApiFunction(_onlineSettings);
            

            OnlineState.ConnectionLife = connection.State.ToString();
            if (_settings.OnlineMode)
            {
                StartListening();
            }
            _settings.PropertyChanged += OnlineModeChanged;
            _outgoingMessage.PropertyChanged += NewMessageToSend;
        }

        private async void NewMessageToSend(object sender, PropertyChangedEventArgs e)
        {
            if ((_settings.LimitTraffic && _settings.SendOnly) || (!_settings.LimitTraffic)
                && (OnlineState.ConnectionLife.Equals("Connected")))
            {
                if (e.PropertyName.Equals("MachineName"))
                {
                    await _function.Call(ApiFunction.Function.SendMessage, _outgoingMessage.Content);
                }
            }
        }

        private void MessageArrived(SignalRMessage message)
        {
            if ((_settings.LimitTraffic && !_settings.SendOnly) || (!_settings.LimitTraffic))
            {
                _inboundMessage.Content = message.Content;
                _inboundMessage.MachineName = message.MachineName;
            }
        }

        private void OnlineModeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("OnlineMode"))
            {
                if (_settings.OnlineMode)
                    StartListening();
                else
                    StopConnection();
            }
        }

        private async void StopConnection()
        {
            await connection.StopAsync();
        }

        private async void StartListening()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl($"https://clipboardmonitorlitefunctions.azurewebsites.net/api")
                .Build();
            await _function.Call(ApiFunction.Function.Register);
            try
            {
                await connection.StartAsync();
                connection.On<string, string>("broadcastMessage", (user, message) =>
                {
                    var newMessage = JsonConvert.DeserializeObject<SignalRMessage>(message);
                    if (!newMessage.Content.Equals(_inboundMessage.Content))
                    {
                        if (!newMessage.MachineName.Equals(Environment.MachineName))
                        {
                            MessageArrived(newMessage);
                        }
                    }
                });
                connection.Closed += Connection_Closed;
                OnlineState.ConnectionLife = connection.State.ToString();
            }
            catch
            {
                await RetryConnection(0);
            }
        }

        private async Task Connection_Closed(Exception arg)
        {
            OnlineState.ConnectionLife = connection.State.ToString();
            if (_settings.OnlineMode)
            {
                await RetryConnection(0);
                OnlineState.ConnectionLife = connection.State.ToString();
            }
        }

        private async Task RetryConnection(int retries)
        {
            int delay = _settings.RetryConnectionAfter;
            retries++;
            try
            {
                await Task.Delay(delay);
                await connection.StartAsync();
            }
            catch
            {
                await RetryConnection(retries);
            }
        }
    }
}