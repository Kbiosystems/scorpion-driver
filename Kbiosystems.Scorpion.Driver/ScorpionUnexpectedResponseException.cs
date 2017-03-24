using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kbiosystems
{
    [Serializable]
    public class ScorpionUnexpectedResponseException : Exception
    {
        public string Request { get; private set; }
        public string Response { get; private set; }

        public ScorpionUnexpectedResponseException(string request, string response)
            : base("Unexpected result returned when sending " + request + ": " + response)
        { }
    }
}
