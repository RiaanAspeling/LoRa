using System.Collections.Generic;

namespace LoRa.Commands
{
    public struct LoRa_SX126X_Configuration: IMapResult<LoRa_SX126X_Configuration>
    {
        private Dictionary<int, int> SX126X_Power = new Dictionary<int, int>() { {0x00, 22}, {0x01, 17}, {0x02, 13}, {0x03, 10} };
        private Dictionary<int, int> SX126X_AirSpeed = new Dictionary<int, int>() { {0x01, 1200}, {0x02, 2400}, {0x03, 4800}, {0x04, 9600}, {0x05, 19200}, {0x06, 38400}, {0x07, 62500} };

        public int Frequency { get; set; } = 433;
        public int Address { get; set; } = 0;
        public int Power { get; set; } = 22;
        public bool RSSI { get; set; } = false;
        public int AirSpeed { get; set; } = 2400;
        public int NetworkId { get; set; } = 0;
        public int BufferSize { get; set; } = 240;
        public byte Encryption { get; set; } = 0;
        public bool Relay { get; set; } = false;

        public LoRa_SX126X_Configuration() { }

        public LoRa_SX126X_Configuration GetSettingsResult(byte[] rawSettings)
        {
                if (rawSettings[0] != 0xC1 || rawSettings[2] != 0x09)
                    return default;
                var rtn = new LoRa_SX126X_Configuration();
                rtn.Frequency = 410 + rawSettings[8];
                rtn.Address = rawSettings[3] + rawSettings[4];
                rtn.AirSpeed = SX126X_AirSpeed[rawSettings[6] & 0x03];
                rtn.Power = SX126X_Power[rawSettings[7] & 0x03];
                return rtn;
        }
    }
}