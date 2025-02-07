using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace GdbControl
{
    public class GdbBreakpoint
    {
        public event Action<GdbBreakpoint> PreInfoChange;

        private readonly object RawInfoChangeLock = new object();
        private readonly object DeletionLock = new object();

        private void InvokePreInfoChange()
        {
            if (PreInfoChange == null)
                return;
            PreInfoChange(this);
        }

        public event Action<GdbBreakpoint> InfoChanged;

        private void InvokeInfoChanged()
        {
            if (InfoChanged == null)
                return;
            InfoChanged(this);
        }

        private string locationIdentifier;
        public string LocationIdentifier
        {
            get
            {
                var acknowledgedIdentifier = (string)RawInfoEntry(LocationIdentifierKey);
                if (acknowledgedIdentifier == null) // not acknowledged yet...
                    return locationIdentifier; // take the given one
                
                return acknowledgedIdentifier; // take the acknowledged one
            }
            private set { locationIdentifier = value; }
        }

        private const string LocationIdentifierKey = "original-location";
        private const string IdKey = "number";
        private const string AddressKey = "addr";
        private const string PendingKey = "pending";
        private const string TimesHitKey = "times";

        private IDictionary<string, object> rawInfo;
        public IDictionary<string, object> RawInfo
        {
            get { return rawInfo; }
            /*internal*/ set
            {
                InvokePreInfoChange();
                lock (RawInfoChangeLock)
                {
                    rawInfo = value;
                    Monitor.PulseAll(RawInfoChangeLock);
                }
                InvokeInfoChanged();
            }
        }

        public int? Id { get { return RawInfoEntryAsInt(IdKey); } }

        public uint? Address { get { return RawInfoEntryAsHexUint(AddressKey); } }

        public bool Pending { get { return RawInfoEntry(PendingKey) != null; } }

        private bool deleted;
        public bool Deleted
        {
            get { return deleted; }
            /*internal*/ set
            {
                lock (DeletionLock)
                {
                    deleted = value;
                    Monitor.PulseAll(DeletionLock);
                }
            }
        }

        public bool Acknowledged { get { return RawInfo != null; } }

        public int HitCount { get { return RawInfoEntryAsInt(TimesHitKey) ?? 0; } }

        public void WaitForHitsToExceed(int numHitsToExceed)
        {
            lock (RawInfoChangeLock)
            {
                while (HitCount <= numHitsToExceed)
                    Monitor.Wait(RawInfoChangeLock);
            }
        }

        public void WaitForAcknowledgement()
        {
            lock (RawInfoChangeLock)
            {
                while (!Acknowledged)
                    Monitor.Wait(RawInfoChangeLock);
            }
        }

        public void WaitForDeletion()
        {
            lock (DeletionLock)
            {
                while (!Deleted)
                    Monitor.Wait(DeletionLock);
            }
        }

        public GdbBreakpoint(string locationIdentifier)
        {
            LocationIdentifier = locationIdentifier;
        }

        private object RawInfoEntry(string key)
        {
            if (!Acknowledged)
                return null;

            object dictEntry;
            if (!RawInfo.TryGetValue(key, out dictEntry))
                return null;

            return dictEntry;
        }

        private int? RawInfoEntryAsInt(string key)
        {
            var rawValue = (string)RawInfoEntry(key);
            if (rawValue == null)
                return null;

            int result;
            if (!int.TryParse(rawValue, out result))
                return null;

            return result;
        }

        private int? RawInfoEntryAsHexInt(string key)
        {
            var rawValue = (string)RawInfoEntry(key);
            if (rawValue == null)
                return null;

            int result;
            // For whatever reason the leading "0x" is a problem, even when specifying NumberStyles.AllowHexSpecifier, so we use Substring instead
            if (!int.TryParse(rawValue.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
                return null;

            return result;
        }

        private uint? RawInfoEntryAsHexUint(string key)
        {
            var rawValue = (string)RawInfoEntry(key);
            if (rawValue == null)
                return null;

            uint result;
            // For whatever reason the leading "0x" is a problem, even when specifying NumberStyles.AllowHexSpecifier, so we use Substring instead
            if (!uint.TryParse(rawValue.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
                return null;

            return result;
        }

    }

}
