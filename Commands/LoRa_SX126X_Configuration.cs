using System.Collections.Generic;
using LoRa.Utils;

namespace LoRa.Commands
{
    public struct LoRa_SX126X_Configuration: IMapResult<LoRa_SX126X_Configuration>
    {
        private int[] SX126X_Power = new int[] { 22, 17, 13, 10 };
        private int[] SX126X_AirSpeed = new int[] { 300, 1200, 2400, 4800, 9600, 19200, 38400, 62500 };
        private int[] SX126X_Packet_Size = new int[] { 240, 128, 64, 32 };
		private int[] SX126X_WOR_Cycle = new int[] { 500, 1000, 1500, 2000, 2500, 3000, 3500, 4000 };

		public enum WORRoleEnum{
			Translate,
			Receive,
			NotSet
		}

		public enum TranModeEnum{
			Normal,
			Fixed,
			NotSet
		}

        public int Address { get; set; } = -1;
        public int NetworkId { get; set; } = -1;
        public int AirSpeed { get; set; } = -1;
        public int Power { get; set; } = -1;
		public bool ChannelRSSI { get; set; } = false;
        public int PacketSize { get; set; } = -1;
        public int ChannelOffset { get; set; } = -1;
		public int WORCycle { get; set; } = -1;
		public WORRoleEnum WORRole { get; set; } = WORRoleEnum.NotSet;
		public bool LBT { get; set; } = false;
		public bool Relay { get; set; } = false;
        public TranModeEnum TranMode { get; set; } = TranModeEnum.NotSet;
		public bool PacketRSSI { get; set; } = false;
        public int Key { get; set; } = -1;

        public LoRa_SX126X_Configuration() { }

        // Possible Response: (See Table 1.0 below for detail)
		// 0  1  2  3  4  5  6  7  8  9  10 11
        // C1-00-09-00-00-00-62-20-17-83-00-00
		//          |---- |- |- |- |- |- |----
		//          Address  |  |  |  |  |
		//                |  |  |  |  |  |
		//                NetworkId|  |  |
		//                   |  |  |  |  |
		//                   AirSpeed |  |
		//                      |  |  |  |
		//                      Power/ChannelRSSI/PacketSize
		//                         |  |  |
		//                         ChannelOffset
		//                            |  |
		//                            WOR Cycle/WOR Role/LBT/Relay/Tran Mode/PacketRSSI
		//                               |
		//                               Key

        public LoRa_SX126X_Configuration GetSettingsResult(byte[] rawSettings)
        {
                if (rawSettings[0] != 0xC1 || rawSettings[2] != 0x09)
                    return default;
                var rtn = new LoRa_SX126X_Configuration();
                rtn.Address = (rawSettings[3] << 8) + rawSettings[4];
                rtn.NetworkId = rawSettings[5];
                rtn.AirSpeed = SX126X_AirSpeed[rawSettings[6].ReadBitRange(0, 3)];
                rtn.Power = SX126X_Power[rawSettings[7].ReadBitRange(0, 2)];
				rtn.ChannelRSSI = rawSettings[7].ReadBitRange(5, 1) == 1;
                rtn.PacketSize = SX126X_Packet_Size[rawSettings[7].ReadBitRange(6, 2)];
                rtn.ChannelOffset = rawSettings[8];
				rtn.WORCycle = SX126X_WOR_Cycle[rawSettings[9].ReadBitRange(0, 3)];
				rtn.WORRole = (WORRoleEnum)rawSettings[9].ReadBitRange(3, 1);
				rtn.LBT = rawSettings[9].ReadBitRange(4, 1) == 1;
                rtn.Relay = rawSettings[9].ReadBitRange(5, 1) == 1;
				rtn.TranMode = (TranModeEnum)rawSettings[9].ReadBitRange(6, 1);
                rtn.PacketRSSI = rawSettings[9].ReadBitRange(7, 1) == 1;
                rtn.Key = (rawSettings[10] << 8) + rawSettings[11];
                return rtn;
        }

        public override string ToString()
        {
            return $"Address = {Address}, NetworkId = {NetworkId}, AirSpeed = {AirSpeed}, Power = {Power}, " +
				   $"ChannelRSSI = {ChannelRSSI}, PacketSize = {PacketSize}, ChannelOffset = {ChannelOffset}, " +
				   $"WORCycle = {WORCycle}, WORRole = {WORRole}, LBT = {LBT}, Relay = {Relay}, TranMode = {TranMode}, " +
				   $"PacketRSSI = {PacketRSSI}, Key = {Key}";
        }
    }
}

/*
Table 1.0: GetSettingsResult breakdown
--------------------------------------
Address		[3-4]			0x0000 -> 0xFFFF
NetworkId	[5]				0			0x00 -> 0000 0000
							255			0xFF -> 1111 1111
AirSpeed 	[6] 0..2		300:		0x60 -> 0110 0000
							1200:		0x61 -> 0110 0001
							2400:		0x62 -> 0110 0010
							4800:		0x63 -> 0110 0011
							9600:		0x64 -> 0110 0100
							19200:		0x65 -> 0110 0101
							38400:		0x66 -> 0110 0110
							62500:		0x67 -> 0110 0111
Power		[7]	0..1		22:			0x20 -> 0010 0000
							17:			0x21 -> 0010 0001
							13:			0x22 -> 0010 0010
							10:			0x23 -> 0010 0011
ChannelRSSI [7] 5..5		Disable		0x00 -> 0000 0000
							Enable		0x20 -> 0010 0000
PacketSize 	[7] 6..7		240:		0x20 -> 0010 0000 
							128:		0x60 -> 0110 0000
							64:			0xa0 -> 1010 0000
							32:			0xe0 -> 1110 0000
ChannelOffset [8]			0			0x00 -> 0000 0000
							84			0x54 -> 0101 0100
WOR Cycle	[9]	0..2		500			0x80 -> 1000 0000
							1000		0x81 -> 1000 0001
							1500		0x82 -> 1000 0010
							2000		0x83 -> 1000 0011
							2500		0x84 -> 1000 0100
							3000		0x85 -> 1000 0101
							3500		0x86 -> 1000 0110
							4000		0x87 -> 1000 0111
WOR Role	[9]	3..3		Translate	0x83 -> 1000 0011
							Receive		0x8B -> 1000 1011
LBT 		[9]	4..4		Disable		0x83 -> 1000 0011
							Enable		0x93 -> 1001 0011
Relay		[9] 5..5		Disable		0x83 -> 1000 0011
							Enable		0xA3 -> 1010 0011
Tran Mode	[9]	6..6		Normal		0x83 -> 1000 0011
							Fixed		0xC3 -> 1100 0011
PacketRSSI	[9]	7..7		Disable		0x03 -> 0000 0011
							Enable		0x83 -> 1000 0011
Key			[10-11]			0x0000 -> 0xFFFF
*/