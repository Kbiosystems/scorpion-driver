using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kbiosystems
{
    [Serializable]
    public class ScorpionRequestException : Exception
    {
        public ScorpionRequestException()
            : base ("Error sending request to machine. Please make sure the instrument is on and the connection information is correct.")
        { }
    }
}
