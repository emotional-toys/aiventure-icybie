using System.Collections.Generic;

namespace GdbApi
{
    public class GdbBreakpoint
    {
        public readonly int Id;
        public readonly int Address;
        public IDictionary<string, string> RawInfo;

        private const string IdKey = "number";
        private const string AddressKey = "addr";

        /// <summary>
        /// Initializes a new instance of the <see cref="GdbBreakpoint"/> class.
        /// </summary>
        /// <param name="rawInfo">The raw info.</param>
        /// <remarks>For whatever reason the leading "0x" is a problem, even when specifying NumberStyles.AllowHexSpecifier, 
        /// so we use Substring instead</remarks>
        public GdbBreakpoint(IDictionary<string, string> rawInfo)
        {
            RawInfo = rawInfo;
            Id = int.Parse(rawInfo[IdKey]);
            Address = int.Parse(rawInfo[AddressKey].Substring(2),System.Globalization.NumberStyles.HexNumber);
        }
    }
}
