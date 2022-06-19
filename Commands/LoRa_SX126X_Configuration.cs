using System.Collections.Generic;

namespace LoRa.Commands
{
    public struct LoRa_SX126X_Configuration: IMapResult<LoRa_SX126X_Configuration>
    {
        private Dictionary<int, int> SX126X_Power = new Dictionary<int, int>() { {0x00, 22}, {0x01, 17}, {0x02, 13}, {0x03, 10} };
        private Dictionary<int, int> SX126X_AirSpeed = new Dictionary<int, int>() { {0x01, 1200}, {0x02, 2400}, {0x03, 4800}, {0x04, 9600}, {0x05, 19200}, {0x06, 38400}, {0x07, 62500} };
        private Dictionary<int, int> SX126X_Buffer_Size = new Dictionary<int, int>() { {0x00, 240}, {0x40, 128}, {0x80, 64}, {0xC0, 32} };

        public int Address { get; set; } = 0;
        public int NetworkId { get; set; } = 0;
        public int AirSpeed { get; set; } = 0;
        public int Power { get; set; } = 0;
        public int BufferSize { get; set; } = 0;
        public int Frequency { get; set; } = 0;
        public bool RSSI { get; set; } = false;
        public bool Relay { get; set; } = false;
        public int Encryption { get; set; } = 0;

        public LoRa_SX126X_Configuration() { }

        // Possible response:
        // C1-00-09-00-00-00-62-20-17-C3-00-00

        public LoRa_SX126X_Configuration GetSettingsResult(byte[] rawSettings)
        {
                if (rawSettings[0] != 0xC1 || rawSettings[2] != 0x09)
                    return default;
                var rtn = new LoRa_SX126X_Configuration();
                rtn.Address = (rawSettings[3] << 8) + rawSettings[4];
                rtn.NetworkId = rawSettings[5];
                rtn.AirSpeed = SX126X_AirSpeed[rawSettings[6] & 0x07];
                rtn.Power = SX126X_Power[rawSettings[7] & 0x03];
                rtn.BufferSize = SX126X_Buffer_Size[rawSettings[7] & 0xC0];
                rtn.Frequency = 410 + rawSettings[8];
                rtn.RSSI = (rawSettings[9] & (1 << 7)) > 0;
                rtn.Relay = ((rawSettings[9] & (1 << 6)) > 0);
                rtn.Encryption = (rawSettings[10] << 8) + rawSettings[11];
                return rtn;
        }

        public override string ToString()
        {
            return $"Address = {Address}, NetworkId = {NetworkId}, AirSpeed = {AirSpeed}, Power = {Power}, BufferSize = {BufferSize}, Frequency = {Frequency}, RSSI = {RSSI}, Relay = {Relay}, Encryption = {Encryption}";
        }
    }
}