using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        public bool Connected { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScorpionDriver"/> class.
        /// </summary>
        public ScorpionDriver()
        { }

        public bool Connect(string portName)
        {
            Disconnect();

            _port = new SerialPort(portName, 9600);
            _port.ReadTimeout = 3000;
            _port.NewLine = "\r";
            _port.Open();

            //get version info as connection test
            try
            {
                string version = QueryVersion();
                Connected = true;
            }
            catch (Exception ex)
            {
                Disconnect();
            }

            return Connected;
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

        public string QueryVersion()
        {
            return WriteQuery("V");
        }

        public ScorpionStatus QueryStatus()
        {
            string result = WriteQuery("?", string.Empty);

            int status = -1;
            if (!int.TryParse(result, out status) || !Array.Exists((int[])Enum.GetValues(typeof(ScorpionStatus)), i => i == status))
            {
                throw new ScorpionUnexpectedResponseException("?", result);
            }

            return (ScorpionStatus)status;
        }

        public ScorpionError QueryError()
        {
            var result = WriteQuery("E");

            int error = -1;
            if (!int.TryParse(result, out error) || !Array.Exists((int[])Enum.GetValues(typeof(ScorpionError)), i => i == error))
            {
                throw new ScorpionUnexpectedResponseException("E", result);
            }

            return (ScorpionError)error;
        }

        public int QueryTransferPosition()
        {
            var result = WriteQuery("P1", "P1=");

            int position = -1;
            if (!int.TryParse(result, out position) || position < 0)
            {
                throw new ScorpionUnexpectedResponseException("P1", result);
            }
            return position;
        }

        public int QueryArmSafePosition()
        {
            var result = WriteQuery("P4", "P4=");

            int position = -1;
            if (!int.TryParse(result, out position) || position < 0)
            {
                throw new ScorpionUnexpectedResponseException("P4", result);
            }
            return position;
        }

        public int QueryArmSpeed()
        {
            var result = WriteQuery("P5", "P5=");

            int speed = -1;
            if (!int.TryParse(result, out speed) || speed < 0)
            {
                throw new ScorpionUnexpectedResponseException("P5", result);
            }
            return speed;
        }

        public void Initialize()
        {
            WriteCommand("I");
        }

        public void GetPlate()
        {
            WriteCommand("G");
        }

        public void ReplacePlate()
        {
            WriteCommand("R");
        }

        public void FinishRun()
        {
            WriteCommand("PA");
        }

        public void SetTransferPosition(int position)
        {
            WriteCommand(string.Format(CultureInfo.InvariantCulture, "P1={0}", position));
        }

        public void SetArmSafePosition(int position)
        {
            WriteCommand(string.Format(CultureInfo.InvariantCulture, "P4={0}", position));
        }

        public void SetArmSpeed(int speed)
        {
            WriteCommand(string.Format(CultureInfo.InvariantCulture, "P4={0}", speed));
        }

        public void Dispose()
        {
            Disconnect();
        }

        public string Write(string request)
        {
            if (!Connected) { throw new InvalidOperationException("Scorpion has not been connected."); }

            _port.WriteLine(request);
            Thread.Sleep(500);

            string result = _port.ReadLine();
            while (result == request || string.IsNullOrEmpty(result))
            {
                result = _port.ReadLine();
            }
            return result;
        }

        private string WriteQuery(string query)
        {
            return WriteQuery(query, query);
        }

        private string WriteQuery(string query, string expectedResultStart)
        {
            string result = Write(query);

            if (result == _errorResult)
            {
                throw new ScorpionRequestException();
            }

            if (!string.IsNullOrEmpty(expectedResultStart) && !result.StartsWith(expectedResultStart))
            {
                throw new ScorpionUnexpectedResponseException(query, result);
            }

            return result.Replace(expectedResultStart, "");
        }

        private void WriteCommand(string command)
        {
            var result = Write(command);

            if (result != _successfulResult)
            {
                if (result == _errorResult)
                {
                    throw new ScorpionRequestException();
                }

                throw new ScorpionUnexpectedResponseException(command, result);
            }
        }
    }
}
