using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivix.Quark.Stream
{
    public interface IQuarkNetworkStreamEvent : IQuarkNetworkStreamGeneric
    {
        IQuarkNetworkPacketData StreamEventDTO { get; }
        void StreamEvent(QuarkNetworkPacket packet);
    }
}