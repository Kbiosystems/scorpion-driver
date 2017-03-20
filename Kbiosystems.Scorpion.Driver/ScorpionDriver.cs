using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kbiosystems
{
    public enum ScorpionStatus { Okay = 0, Error = 2, Busy = 4, RunFinished = 16 }
    public enum ScorpionError { None = 0, ArmUp = 3, ArmDown = 4, PlateTransferBack = 6, ArmHome = 7, LiftHome = 8, ConveyerJam = 9, PauseButton = 10 }

    public class ScorpionDriver : IDisposable
    {
        private const string _successfulResult = "ok";
        private const string _errorResult = "err";

        private SerialPort _port;
        private Action<string> _logAction;

        public bool Connected { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScorpionDriver"/> class.
        /// </summary>
        public ScorpionDriver()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScorpionDriver"/> class.
        /// </summary>
        /// <param name="logAction">The log action to execute.</param>
        public ScorpionDriver(Action<string> logAction)
        {
            _logAction = logAction;
        }

        public void Connect(string portName)
        {
            Disconnect();

            _port = new SerialPort(portName, 9600);
            _port.ReadTimeout = 3000;
            _port.NewLine = "\r";
            _port.Open();

            var result = GetVersion();

            Connected = true;
        }

        public void Disconnect()
        {
            if (_port != null && _port.IsOpen)
            {
                _port.Close();
                _port.Dispose();

                Connected = false;
            }
        }


        public Version GetVersion()
        {
            throw new NotImplementedException();
        }

        public ScorpionStatus QueryStatus()
        {
            string result = WriteCommand("?");

            if (result == _errorResult) { return ScorpionStatus.Error; }

            int status = -1;
            if (!int.TryParse(result, out status))
            {
                throw new Exception("Unexpected result returned when querying status: " + result);
            }

            if (!Array.Exists((int[])Enum.GetValues(typeof(ScorpionStatus)), i => i == status))
            {
                throw new Exception("Unexpected result returned when querying status: " + result);
            }

            return (ScorpionStatus)status;
        }

        public ScorpionError QueryError()
        {
            var result = WriteCommand("E");
            if (!result.StartsWith("E"))
            {
                throw new Exception("Unexpected result returned when querying error: " + result);
            }

            result = result.Replace("E", "");
            int error = -1;
            if (!int.TryParse(result, out error))
            {
                throw new Exception("Unexpected result returned when querying error: " + result);
            }

            if (!Array.Exists((int[])Enum.GetValues(typeof(ScorpionError)), i => i == error))
            {
                throw new Exception("Unknown error result returned: " + result);
            }

            return (ScorpionError)error;
        }

        public ScorpionStatus Initialize()
        {
            return WriteStandardCommand("I");
        }

        public ScorpionStatus GetPlate()
        {
            return WriteStandardCommand("G");
        }

        public ScorpionStatus ReplacePlate()
        {
            return WriteStandardCommand("R");
        }

        public ScorpionStatus FinishRun()
        {
            return WriteStandardCommand("PA");
        }

        public void Dispose()
        {
            Disconnect();
        }

        public string WriteCommand(string command)
        {
            if (!Connected) { throw new InvalidOperationException("Scorpion has not been connected."); }

            _port.WriteLine(command);
            Thread.Sleep(500);

            string result = _port.ReadLine();
            while (result == command || string.IsNullOrEmpty(result))
            {
                result = _port.ReadLine();
            }
            return result;
        }

        private ScorpionStatus WriteStandardCommand(string command)
        {
            Log("Writing command " + command);
            var result = WriteCommand(command);

            if (result != _successfulResult)
            {
                if (result == _errorResult)
                {
                    return ScorpionStatus.Error;
                }

                throw new Exception("Unexpected result returned when sending the " + command + " command: " + result);
            }

            return ScorpionStatus.Okay;
        }

        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }
    }
}
