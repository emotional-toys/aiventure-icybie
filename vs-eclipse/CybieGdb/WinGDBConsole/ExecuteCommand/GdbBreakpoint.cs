using System.Collections.Generic;

namespace Commands
{
    public class GdbBreakpoint
    {
        public readonly int Id;
        public readonly int Address;
        public IDictionary<string, string> RawInfo;

        private const string IdKey = "number";
        private const string AddressKey = "addr";

        public GdbBreakpoint(IDictionary<string, string> rawInfo)
        {
            RawInfo = rawInfo;
            Id = int.Parse(rawInfo[IdKey]);
            Address = int.Parse(rawInfo[AddressKey].Substring(2),System.Globalization.NumberStyles.HexNumber); 
            // for whatever reason the leading "0x" is a problem, even when specifying NumberStyles.AllowHexSpecifier, so we use Substring instead...
            
        }
    }
}
