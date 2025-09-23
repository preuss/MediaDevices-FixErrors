using System;

namespace MediaDevices.Internal
{
    internal class KeyAttribute : Attribute
    {
        public KeyAttribute()
        {
            Guid = Guid.Empty;
            Id = 0;
        }

        public KeyAttribute(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k, uint id)
        {
            Guid = new Guid(a, b, c, d, e, f, g, h, i, j, k);
            Id = id;
        }

        public Guid Guid { get; private set; }

        public uint Id { get; private set; }

        public PropertyKey PropertyKey { get { return new PropertyKey() { fmtid = Guid, pid = Id }; } }
    }
}
