using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivix.Quark.Stream
{
    public interface IQuarkNetworkStreamResponse : IQuarkNetworkStreamGeneric
    {
        IQuarkNetworkPacketData StreamResponseDTO { get; }
        void StreamResponse(QuarkNetworkPacket packet);
    }
}