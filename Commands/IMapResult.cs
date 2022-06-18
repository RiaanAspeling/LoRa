using System.Collections.Generic;

namespace LoRa.Commands
{
    public interface IMapResult<T>
    {
        T GetSettingsResult(byte[] rawSettings);
    }
}