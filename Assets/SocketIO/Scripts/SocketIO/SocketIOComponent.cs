﻿#if UNITY_EDITOR
#region UnityCode
#region License
/*
 * SocketIO.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 Fabio Panettieri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

#endregion

//#define SOCKET_IO_DEBUG			// Uncomment this for debug
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

namespace SocketIO {
    public class SocketIOComponent : MonoBehaviour
	{
#region Public Properties
        
        public string HoloHubWS = "ws://192.168.0.103:3000/socket.io/?transport=websocket"; //  = Args.HOLOHUB_WEBSOCKET_ADDRESS;
        public bool autoConnect = true;
		public int reconnectDelay = 5;
		public float ackExpirationTime = 1800f;
		public float pingInterval = 25f;
		public float pingTimeout = 60f;

		public WebSocket socket { get { return ws; } }
		public string sid { get; set; }
		public bool IsConnected { get { return connected; } }

#endregion

#region Private Properties

		private volatile bool connected;
		private volatile bool thPinging;
		private volatile bool thPong;
		private volatile bool wsConnected;

		private Thread socketThread;
		private Thread pingThread;
		private WebSocket ws;

		private Encoder encoder;
		private Decoder decoder;
		private Parser parser;

		private Dictionary<string, List<Action<SocketIOEvent>>> handlers;
		private List<Ack> ackList;

		private int packetId;

		private object eventQueueLock;
		private Queue<SocketIOEvent> eventQueue;

		private object ackQueueLock;
		private Queue<Packet> ackQueue;

#endregion

#if SOCKET_IO_DEBUG
		public Action<string> debugMethod;
#endif

#region Unity interface

		public void Awake()
		{
            encoder = new Encoder();
			decoder = new Decoder();
			parser = new Parser();
			handlers = new Dictionary<string, List<Action<SocketIOEvent>>>();
			ackList = new List<Ack>();
			sid = null;
			packetId = 0;

			ws = new WebSocket(HoloHubWS);
			ws.OnOpen += OnOpen;
			ws.OnMessage += OnMessage;
			ws.OnError += OnError;
			ws.OnClose += OnClose;
			wsConnected = false;

			eventQueueLock = new object();
			eventQueue = new Queue<SocketIOEvent>();

			ackQueueLock = new object();
			ackQueue = new Queue<Packet>();

			connected = false;

#if SOCKET_IO_DEBUG
			if(debugMethod == null) { debugMethod = Debug.Log; };
#endif
		}

		public void Start()
		{
			if (autoConnect) { Connect(); }
		}

		public void Update()
		{
			lock(eventQueueLock){ 
				while(eventQueue.Count > 0){
					EmitEvent(eventQueue.Dequeue());
				}
			}

			lock(ackQueueLock){
				while(ackQueue.Count > 0){
					InvokeAck(ackQueue.Dequeue());
				}
			}

			if(wsConnected != ws.IsConnected){
				wsConnected = ws.IsConnected;
				if(wsConnected){
					EmitEvent("connect");
				} else {
					EmitEvent("disconnect");
				}
			}

			// GC expired acks
			if(ackList.Count == 0) { return; }
			if(DateTime.Now.Subtract(ackList[0].time).TotalSeconds < ackExpirationTime){ return; }
			ackList.RemoveAt(0);
		}

		public void OnDestroy()
		{
			if (socketThread != null) 	{ socketThread.Abort(); }
			if (pingThread != null) 	{ pingThread.Abort(); }
		}

		public void OnApplicationQuit()
		{
			Close();
		}

#endregion

#region Public Interface
		
		public void Connect()
		{
            
			connected = true;

			socketThread = new Thread(RunSocketThread);
			socketThread.Start(ws);

			pingThread = new Thread(RunPingThread);
			pingThread.Start(ws);
		}

		public void Close()
		{
			EmitClose();
			connected = false;
		}

		public void On(string ev, Action<SocketIOEvent> callback)
		{
			if (!handlers.ContainsKey(ev)) {
				handlers[ev] = new List<Action<SocketIOEvent>>();
			}
			handlers[ev].Add(callback);
		}

		public void Off(string ev, Action<SocketIOEvent> callback)
		{
			if (!handlers.ContainsKey(ev)) {
#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] No callbacks registered for event: " + ev);
#endif
				return;
			}

			List<Action<SocketIOEvent>> l = handlers [ev];
			if (!l.Contains(callback)) {
#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] Couldn't remove callback action for event: " + ev);
#endif
				return;
			}

			l.Remove(callback);
			if (l.Count == 0) {
				handlers.Remove(ev);
			}
		}

		public void Emit(string ev)
		{
			EmitMessage(-1, string.Format("[\"{0}\"]", ev));
		}

		public void Emit(string ev, Action<JSONObject> action)
		{
			EmitMessage(++packetId, string.Format("[\"{0}\"]", ev));
			ackList.Add(new Ack(packetId, action));
		}

		public void Emit(string ev, JSONObject data)
		{
			EmitMessage(-1, string.Format("[\"{0}\",{1}]", ev, data));
		}

		public void Emit(string ev, JSONObject data, Action<JSONObject> action)
		{
			EmitMessage(++packetId, string.Format("[\"{0}\",{1}]", ev, data));
			ackList.Add(new Ack(packetId, action));
		}

#endregion

#region Private Methods

		private void RunSocketThread(object obj)
		{
			WebSocket webSocket = (WebSocket)obj;
			while(connected){
				if(webSocket.IsConnected){
					Thread.Sleep(reconnectDelay);
				} else {
					webSocket.Connect();
				}
			}
			webSocket.Close();
		}

		private void RunPingThread(object obj)
		{
			WebSocket webSocket = (WebSocket)obj;

			int timeoutMilis = Mathf.FloorToInt(pingTimeout * 1000);
			int intervalMilis = Mathf.FloorToInt(pingInterval * 1000);

			DateTime pingStart;

			while(connected)
			{
				if(!wsConnected){
					Thread.Sleep(reconnectDelay);
				} else {

                    Debug.Log("CONNECT");
					thPinging = true;
					thPong =  false;
					
					EmitPacket(new Packet(EnginePacketType.PING));
					pingStart = DateTime.Now;
					
					while(webSocket.IsConnected && thPinging && (DateTime.Now.Subtract(pingStart).TotalSeconds < timeoutMilis)){
						Thread.Sleep(200);
					}
					
					if(!thPong){
						webSocket.Close();
					}

					Thread.Sleep(intervalMilis);
				}
			}
		}

		private void EmitMessage(int id, string raw)
		{
			EmitPacket(new Packet(EnginePacketType.MESSAGE, SocketPacketType.EVENT, 0, "/", id, new JSONObject(raw)));
		}

		private void EmitClose()
		{
			EmitPacket(new Packet(EnginePacketType.MESSAGE, SocketPacketType.DISCONNECT, 0, "/", -1, new JSONObject("")));
			EmitPacket(new Packet(EnginePacketType.CLOSE));
		}

		private void EmitPacket(Packet packet)
		{
#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] " + packet);
#endif
			
			try {
				ws.Send(encoder.Encode(packet));
			} catch(SocketIOException ex) {
#if SOCKET_IO_DEBUG
				debugMethod.Invoke(ex.ToString());
#endif
			}
		}

		private void OnOpen(object sender, EventArgs e)
		{
			EmitEvent("open");
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] Raw message: " + e.Data);
#endif
			Packet packet = decoder.Decode(e);

			switch (packet.enginePacketType) {
				case EnginePacketType.OPEN: 	HandleOpen(packet);		break;
				case EnginePacketType.CLOSE: 	EmitEvent("close");		break;
				case EnginePacketType.PING:		HandlePing();	   		break;
				case EnginePacketType.PONG:		HandlePong();	   		break;
				case EnginePacketType.MESSAGE: 	HandleMessage(packet);	break;
			}
		}

		private void HandleOpen(Packet packet)
		{
#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] Socket.IO sid: " + packet.json["sid"].str);
#endif
			sid = packet.json["sid"].str;
			EmitEvent("open");
		}

		private void HandlePing()
		{
			EmitPacket(new Packet(EnginePacketType.PONG));
		}

		private void HandlePong()
		{
			thPong = true;
			thPinging = false;
		}
		
		private void HandleMessage(Packet packet)
		{
			if(packet.json == null) { return; }

			if(packet.socketPacketType == SocketPacketType.ACK){
				for(int i = 0; i < ackList.Count; i++){
					if(ackList[i].packetId != packet.id){ continue; }
					lock(ackQueueLock){ ackQueue.Enqueue(packet); }
					return;
				}

#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] Ack received for invalid Action: " + packet.id);
#endif
			}

			if (packet.socketPacketType == SocketPacketType.EVENT) {
				SocketIOEvent e = parser.Parse(packet.json);
				lock(eventQueueLock){ eventQueue.Enqueue(e); }
			}
		}

		private void OnError(object sender, ErrorEventArgs e)
		{
			EmitEvent("error");
		}

		private void OnClose(object sender, CloseEventArgs e)
		{
			EmitEvent("close");
		}

		private void EmitEvent(string type)
		{
			EmitEvent(new SocketIOEvent(type));
		}

		private void EmitEvent(SocketIOEvent ev)
		{
			if (!handlers.ContainsKey(ev.name)) { return; }
			foreach (Action<SocketIOEvent> handler in this.handlers[ev.name]) {
				try{
					handler(ev);
				} catch(Exception ex){
#if SOCKET_IO_DEBUG
					debugMethod.Invoke(ex.ToString());
#endif
				}
			}
		}

		private void InvokeAck(Packet packet)
		{
			Ack ack;
			for(int i = 0; i < ackList.Count; i++){
				if(ackList[i].packetId != packet.id){ continue; }
				ack = ackList[i];
				ackList.RemoveAt(i);
				ack.Invoke(packet.json);
				return;
			}
		}

#endregion
	}
}
#endregion
#else
#region HoloLensCode

using System.Collections;
using UnityEngine;
//TRY TO SET UP WEB SOCKET
using Windows.Networking.Sockets;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace SocketIO {
    public class SocketIOComponent : MonoBehaviour {
        private Dictionary<string, List<Action<SocketIOEvent>>> handlers;
        public string HoloHubWS = "ws://192.168.0.103:3000/socket.io/?transport=websocket"; // = Args.HOLOHUB_WEBSOCKET_ADDRESS;
        public MessageWebSocket websocket;
        public DataWriter writer;
        private bool isConnected = false;
		private bool isConnecting = false;
        public bool autoConnect = true;
        private object eventQueueLock;

       

        private Queue<SocketIOEvent> eventQueue;
        //do nothing i don't care
        private void Awake() {
            eventQueueLock = new object();
            eventQueue = new Queue<SocketIOEvent>();
            handlers = new Dictionary<string, List<Action<SocketIOEvent>>>();
        }

        public void Connect()
        {
            ConnectWebsocket();
        }

        async void Start () {
            //TRY TO SET UP WEB SOCKET
            
            if (autoConnect) {
                Debug.Log("Loaded This One");
                await ConnectWebsocket();
            }
            //await SendAsync("beep");
        }

         async void Update() {
            lock ( eventQueueLock ) {
                while ( eventQueue.Count > 0 ) {
                    EmitEvent(eventQueue.Dequeue());
                }
            }
            if ( !isConnected && !isConnecting ) {
                await ConnectWebsocket();
            }
        }

        private async Task ConnectWebsocket() {
			isConnecting = true;
            websocket = new MessageWebSocket();
            Uri server = new Uri(HoloHubWS);

            websocket.Control.MessageType = SocketMessageType.Utf8;
            websocket.MessageReceived += Websocket_MessageReceived;
            websocket.Closed += Websocket_Closed;

            try {
                await websocket.ConnectAsync(server);
				isConnecting = false;
                isConnected = true;
                
                writer = new DataWriter(websocket.OutputStream);
                
            }
            catch ( Exception ex ) // For debugging
            {
				isConnecting = false;
                // Error happened during connect operation.
                if (websocket != null ) {
                    websocket.Dispose();
                } 
                websocket = null;
                Debug.Log("[SocketIOComponent] " + ex.Message);
                
                if ( ex is COMException ) {
                    Debug.Log("Send Event to User To tell them we are unable to connect to Pi");
                }
                return;
            }
        }

        private void Websocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args) {
            if ( websocket == sender ) {
                CloseSocket();
            }
            isConnected = false;
        }
        private void CloseSocket() {
            if ( writer != null ) {
                // In order to reuse the socket with another DataWriter, the socket's output stream needs to be detached.
                // Otherwise, the DataWriter's destructor will automatically close the stream and all subsequent I/O operations
                // invoked on the socket's output stream will fail with ObjectDisposedException.
                //
                // This is only added for completeness, as this sample closes the socket in the very next code block.
                writer.DetachStream();
                writer.Dispose();
                writer = null;
            }

            if ( websocket != null ) {
                try {
                    websocket.Close(1000, "Closed due to user request.");
                }
                catch ( Exception ex ) {
                    Debug.Log(ex.Message);
                }
                websocket = null;
            }
        }

        

        private void Websocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args) {
            try {
                using ( DataReader reader = args.GetDataReader() ) {
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                    try {
                        string read = reader.ReadString(reader.UnconsumedBufferLength);
                        //read = Regex.Unescape(read);
                        VentanaSocketData socc = VentanaSocketData.ParseFromString(read);
                        if (socc != null ) {
                            Debug.Log(socc.ToString());
                            SocketIOEvent e = new SocketIOEvent(socc.channel, new JSONObject( socc.jsonPayload));
                            lock ( eventQueueLock ) { eventQueue.Enqueue(e); }
                        }
                    }
                    catch ( Exception ex ) {
                        Debug.Log(ex.Message);
                    }
                }
            } catch (Exception ex ) {
                Debug.Log(ex.Message);
            }
        
        }
        public void On(string ev, Action<SocketIOEvent> callback) {
            if ( !handlers.ContainsKey(ev) ) {
                handlers[ev] = new List<Action<SocketIOEvent>>();
            }
            handlers[ev].Add(callback);
        }

        public void Off(string ev, Action<SocketIOEvent> callback) {
            if ( !handlers.ContainsKey(ev) ) {
#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] No callbacks registered for event: " + ev);
#endif
                return;
            }

            List<Action<SocketIOEvent>> l = handlers[ev];
            if ( !l.Contains(callback) ) {
#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] Couldn't remove callback action for event: " + ev);
#endif
                return;
            }

            l.Remove(callback);
            if ( l.Count == 0 ) {
                handlers.Remove(ev);
            }
        }

        private void EmitEvent(SocketIOEvent ev) {
            if ( !handlers.ContainsKey(ev.name) ) { return; }
            foreach ( Action<SocketIOEvent> handler in this.handlers[ev.name] ) {
                try {
                    handler(ev);
                }
                catch ( Exception ex ) {
                    Debug.Log(ex.Message);
#if SOCKET_IO_DEBUG
					debugMethod.Invoke(ex.ToString());
#endif
                }
            }
        }

        async Task SendAsync(string msg) {
            string message = msg;

            Debug.Log("Sending Message: " + message);

            // Buffer any data we want to send.
            writer.WriteString(message);

            try {
                // Send the data as one complete message.
                await writer.StoreAsync();
            }
            catch ( Exception ex ) {
                Debug.Log(ex.Message);
                return;
            }
            Debug.Log("Send Complete");
        }
    }
}
#endregion
#endif