using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Cryptography;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash_Essentials_Core.Queues;

namespace PepperDash.Essentials.Displays
{
    /// <summary>
    /// Plugin device template for third party devices that use IBasicCommunication
    /// </summary>
    /// <remarks>
    /// Rename the class to match the device plugin being developed.
    /// </remarks>
    /// <example>
    /// "EssentialsPluginDeviceTemplate" renamed to "SamsungMdcDevice"
    /// </example>
    public class PanasonicProjectorController : TwoWayDisplayBase, IBridgeAdvanced
    {
        private readonly PanasonicProjectorControllerConfig _config;

        private readonly MD5CryptoServiceProvider _md5Provider;

        private readonly GenericQueue _rxQueue;

        private readonly CrestronQueue<string> _txQueue;
        private eInputTypes _currentInput;

        private string _hash;

        #region IBasicCommunication Properties and Constructor.  Remove if not needed.

        private readonly ICommandBuilder _commandBuilder;

        private readonly IBasicCommunication _comms;

        /// <summary>
        /// Set this value to that of the delimiter used by the API (if applicable)
        /// </summary>
        private readonly string _commsDelimiter;

        private readonly CommunicationGather _commsGather;
        private readonly GenericCommunicationMonitor _commsMonitor;

        private string _currentCommand;

        private bool _powerIsOn;

        /// <summary>
        /// Plugin device constructor for devices that need IBasicCommunication
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <param name="comms"></param>
        public PanasonicProjectorController(string key, string name, PanasonicProjectorControllerConfig config,
            IBasicCommunication comms)
            : base(key, name)
        {
            Debug.Console(0, this, "Constructing new {0} instance", name);

            _rxQueue = new GenericQueue(String.Format("{0}-rxQueue", Key));
            _txQueue = new CrestronQueue<string>(50);

            _config = config;

            ConnectFeedback = new BoolFeedback(() => Connect);
            OnlineFeedback = new BoolFeedback(() => _commsMonitor.IsOnline);
            StatusFeedback = new IntFeedback(() => (int) _commsMonitor.Status);

            _comms = comms;
            _commandBuilder = GetCommandBuilder(_config);

            if (_commandBuilder == null)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error,
                    "Command builder not created. Unable to continue. Please correct the configuration to use either 'com' or 'tcpip' as the control method");
                return;
            }

            _commsDelimiter = _commandBuilder.Delimiter;

            _commsMonitor = new GenericCommunicationMonitor(this, _comms, 10000, 15000, 30000, Poll);

            var socket = _comms as ISocketStatus;
            if (socket != null)
            {
                // device comms is IP **ELSE** device comms is RS232
                socket.ConnectionChange += socket_ConnectionChange;

                _md5Provider = new MD5CryptoServiceProvider();
                _md5Provider.Initialize();
            }

            // Only one of the below handlers should be necessary.  

            // _comms gather for any API that has a defined delimiter
            _commsGather = new CommunicationGather(_comms, _commsDelimiter);
            _commsGather.LineReceived += Handle_LineRecieved;

            SetUpInputPorts();
        }

        public bool PowerIsOn
        {
            get { return _powerIsOn; }
            set
            {
                if (value == _powerIsOn)
                {
                    return;
                }

                _powerIsOn = value;

                PowerIsOnFeedback.FireUpdate();
            }
        }


        /// <summary>
        /// Connects/disconnects the comms of the plugin device
        /// </summary>
        /// <remarks>
        /// triggers the _comms.Connect/Disconnect as well as thee comms monitor start/stop
        /// </remarks>
        public bool Connect
        {
            get { return _comms.IsConnected; }
            set
            {
                if (value)
                {
                    _comms.Connect();
                    _commsMonitor.Start();
                }
                else
                {
                    _comms.Disconnect();
                    _commsMonitor.Stop();
                }
            }
        }

        /// <summary>
        /// Reports connect feedback through the bridge
        /// </summary>
        public BoolFeedback ConnectFeedback { get; private set; }

        /// <summary>
        /// Reports online feedback through the bridge
        /// </summary>
        public BoolFeedback OnlineFeedback { get; private set; }

        /// <summary>
        /// Reports socket status feedback through the bridge
        /// </summary>
        public IntFeedback StatusFeedback { get; private set; }

        private void SetUpInputPorts()
        {
            var computer1 = new RoutingInputPort(String.Format("{0}-computer1", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(() => SetInput(eInputTypes.Rg1)), this);

            var computer2 = new RoutingInputPort(String.Format("{0}-computer2", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Vga, new Action(() => SetInput(eInputTypes.Rg2)), this);

            var video = new RoutingInputPort(String.Format("{0}-video", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Composite, new Action(() => SetInput(eInputTypes.Vid)), this);

            var sVideo = new RoutingInputPort(String.Format("{0}-s-video", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Component, new Action(() => SetInput(eInputTypes.Svd)), this);

            var dvi = new RoutingInputPort(String.Format("{0}-dvi", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Dvi, new Action(() => SetInput(eInputTypes.Dvi)), this);

            var hdmi1 = new RoutingInputPort(String.Format("{0}-hdmi-1", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(() => SetInput(eInputTypes.Hd1)), this);

            var hdmi2 = new RoutingInputPort(String.Format("{0}-hdmi-2", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(() => SetInput(eInputTypes.Hd2)), this);

            var sdi = new RoutingInputPort(String.Format("{0}-sdi", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.Sdi, new Action(() => SetInput(eInputTypes.Sd1)), this);

            var digitalLink = new RoutingInputPort(String.Format("{0}-dl", Key), eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, new Action(() => SetInput(eInputTypes.Dl1)), this);

            InputPorts.Add(computer1);
            InputPorts.Add(computer2);
            InputPorts.Add(video);
            InputPorts.Add(sVideo);
            InputPorts.Add(dvi);
            InputPorts.Add(hdmi1);
            InputPorts.Add(hdmi2);
            InputPorts.Add(sdi);
            InputPorts.Add(digitalLink);
        }

        private ICommandBuilder GetCommandBuilder(PanasonicProjectorControllerConfig config)
        {
            if (config.Control.Method == eControlMethod.Com)
            {
                return new SerialCommandBuilder(config.Id);
            }

            if (config.Control.Method == eControlMethod.Tcpip)
            {
                return new IpCommandBuilder();
            }

            Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Control method {0} isn't valid for this plugin.",
                config.Control.Method);
            return null;
        }


        private void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs args)
        {
            if (ConnectFeedback != null)
            {
                ConnectFeedback.FireUpdate();
            }

            if (StatusFeedback != null)
            {
                StatusFeedback.FireUpdate();
            }
        }

        private void Handle_LineRecieved(object sender, GenericCommMethodReceiveTextArgs args)
        {
            _rxQueue.Enqueue(new QueueMessage(() => ParseResponse(args.Text)));
        }


        /// <summary>
        /// Sends text to the device plugin comms
        /// </summary>
        /// <remarks>
        /// Can be used to test commands with the device plugin using the DEVPROPS and DEVJSON console commands
        /// </remarks>
        /// <param name="text">Command to be sent</param>		
        public void SendText(string text)
        {
            _txQueue.Enqueue(text);

            if (!_comms.IsConnected)
            {
                _comms.Connect();
            }
        }

        /// <summary>
        /// Polls the device
        /// </summary>
        /// <remarks>
        /// Poll method is used by the communication monitor.  Update the poll method as needed for the plugin being developed
        /// </remarks>
        public void Poll()
        {
            SendText(_commandBuilder.GetCommand("QPW", ""));
        }

        private void ParseResponse(string response)
        {
            //need to calculate hash
            if (response.Contains("ntcontrol 1"))
            {
                _hash = GetHash(response);
                DequeueAndSend();
                return;
            }

            if (response.Contains("ntcontrol 0"))
            {
                DequeueAndSend();
                return;
            }

            if (String.IsNullOrEmpty(_currentCommand))
            {
                return;
            }

            //power query
            if (_currentCommand.ToLower().Contains("qpw"))
            {
                PowerIsOn = response.Contains("0001");
                return;
            }

            if (_currentCommand.ToLower().Contains("iis"))
            {
                CurrentInput = response.Trim();
            }
        }

        private void DequeueAndSend()
        {
            if (_txQueue.IsEmpty)
            {
                return;
            }

            var cmdToSend = _txQueue.Dequeue(10);

            if (String.IsNullOrEmpty(cmdToSend))
            {
                Debug.Console(1, this, "Unable to get command to send");
                return;
            }

            _currentCommand = cmdToSend;

            _comms.SendText(String.IsNullOrEmpty(_hash) ? cmdToSend : String.Format("{0}{1}", _hash, cmdToSend));
        }

        private string GetHash(string randomNumber)
        {
            //response is of the form ntcontrol 1 {random}
            var randomString = randomNumber.Split(' ')[2];

            var stringToHash = String.Format("{0}:{1}:{2}", _config.Control.TcpSshProperties.Username,
                _config.Control.TcpSshProperties.Password, randomString);

            var bytes = Encoding.UTF8.GetBytes(stringToHash);

            var hash = _md5Provider.ComputeHash(bytes);

            return Encoding.UTF8.GetString(hash, 0, hash.Length);
        }

        public void SetInput(eInputTypes input)
        {
            SendText(_commandBuilder.GetCommand("IIS", input.ToString().ToUpper()));

            CurrentInput = input.ToString();
        }

        #endregion

        #region Overrides of DisplayBase

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get { return () => PowerIsOn; }
        }

        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return () => false; }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return () => false; }
        }

        public override void PowerOn()
        {
            SendText(_commandBuilder.GetCommand("PON"));
        }

        public override void PowerOff()
        {
            SendText(_commandBuilder.GetCommand("POF"));
        }

        public override void PowerToggle()
        {
            SendText(_commandBuilder.GetCommand(PowerIsOn ? "POF" : "PON"));
        }

        public override void ExecuteSwitch(object selector)
        {
            var handler = selector as Action;

            if (handler == null)
            {
                Debug.Console(1, this, "Unable to switch using selector {0}", selector);
                return;
            }

            handler();
        }

        #endregion

        #region Overrides of TwoWayDisplayBase

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => _currentInput.ToString(); }
        }

        #endregion

        #region Implementation of IBridgeAdvanced

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #endregion

        public string CurrentInput
        {
            get { return _currentInput.ToString(); }
            set
            {
                if (_currentInput.ToString() == value)
                {
                    return;
                }

                try
                {
                    _currentInput = (eInputTypes) Enum.Parse(typeof (eInputTypes), value, true);
                }
                catch
                {
                    _currentInput = eInputTypes.None;
                }

                CurrentInputFeedback.FireUpdate();
            }
        }
    }
}