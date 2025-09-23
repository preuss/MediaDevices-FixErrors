using System;

namespace MediaDevices.Internal
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    internal class EnumGuidAttribute : Attribute
    {
        public EnumGuidAttribute()
        {
            Guid = Guid.Empty;
        }

        public EnumGuidAttribute(string v)
        {
            Guid = new Guid(v);
        }

        public EnumGuidAttribute(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            Guid = new Guid(a, b, c, d, e, f, g, h, i, j, k); 
        }

        public Guid Guid { get; private set; }
    }
}
