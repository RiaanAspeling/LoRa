using System.Collections.Generic;

namespace LoRa.Commands
{
    public struct LoRa_SX126X_Configuration: IMapResult<LoRa_SX126X_Configuration>
    {
        private Dictionary<int, int> SX126X_Power = new Dictionary<int, int>() { {0x00, 22}, {0x01, 17}, {0x02, 13}, {0x03, 10} };
        private Dictionary<int, int> SX126X_AirSpeed = new Dictionary<int, int>() { {0x01, 1200}, {0x02, 2400}, {0x03, 4800}, {0x04, 9600}, {0x05, 19200}, {0x06, 38400}, {0x07, 62500} };
        private Dictionary<int, int> SX126X_Buffer_Size = new Dictionary<int, int>() { {0x00, 240}, {0x40, 128}, {0x80, 64}, {0xC0, 32} };

        public int Address { get; set; } = -1;
        public int NetworkId { get; set; } = -1;
        public int AirSpeed { get; set; } = -1;
        public int Power { get; set; } = -1;
        public int BufferSize { get; set; } = -1;
        public int Frequency { get; set; } = -1;
        public bool RSSI { get; set; } = false;
        public bool Relay { get; set; } = false;
        public int Encryption { get; set; } = -1;

        public LoRa_SX126X_Configuration() { }

        // Possible response:
        // C1-00-09-00-00-00-62-20-17-83-00-00

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

/*

AirSpeed	Packet Size		Power	Relay		PacketRSSI	ChannelRSSI		Address		Channel		NetworkId	Key		Data
2400		240				22		Disable		Enabled		Enable			0			23			0			0		0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x83 0x00 0x00

300																														0xc0 0x00 0x09 0x00 0x00 0x00 0x60 0x20 0x17 0x83 0x00 0x00
1200																													0xc0 0x00 0x09 0x00 0x00 0x00 0x61 0x20 0x17 0x83 0x00 0x00
4800																													0xc0 0x00 0x09 0x00 0x00 0x00 0x63 0x20 0x17 0x83 0x00 0x00
62500																													0xc0 0x00 0x09 0x00 0x00 0x00 0x67 0x20 0x17 0x83 0x00 0x00
			128																											0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x60 0x17 0x83 0x00 0x00
			64																											0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0xa0 0x17 0x83 0x00 0x00
			32																											0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0xe0 0x17 0x83 0x00 0x00
							17																							0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x21 0x17 0x83 0x00 0x00
							13																							0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x22 0x17 0x83 0x00 0x00
							10																							0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x23 0x17 0x83 0x00 0x00
									Enable																				0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0xa3 0x00 0x00
												Disable																	0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x03 0x00 0x00
															Disable														0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x00 0x17 0x83 0x00 0x00
																			FF											0xc0 0x00 0x09 0x00 0xff 0x00 0x62 0x20 0x17 0x83 0x00 0x00
																			FF00										0xc0 0x00 0x09 0xff 0x00 0x00 0x62 0x20 0x17 0x83 0x00 0x00
																						84								0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x54 0x83 0x00 0x00
																						0								0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x00 0x83 0x00 0x00
																									FF					0xc0 0x00 0x09 0x00 0x00 0xff 0x62 0x20 0x17 0x83 0x00 0x00
																												FF		0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x83 0x00 0xff
																												FF00	0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x83 0xff 0x00

WOR Role (Translate) As first line
WOR Rold (Receive)																										0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x8b 0x00 0x00
WOR Cycle (2000ms) As first line
WOR Cycle (500ms)																										0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x80 0x00 0x00
500ms++ increments
WOR Cycle (4000ms)																										0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x87 0x00 0x00
Tran Mode (Normal) As first line
Tran Mode (Fixed)																										0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0xc3 0x00 0x00
LBT (Disable) As first line
LBT (Enable)																											0xc0 0x00 0x09 0x00 0x00 0x00 0x62 0x20 0x17 0x93 0x00 0x00

AirSpeed 	[6] : 	  300:		0x60 -> 0110 0000
					  1200:		0x61 -> 0110 0001
					  2400:		0x62 -> 0110 0010
					  4800:		0x63 -> 0110 0011
					  9600:		0x64 -> 0110 0100
					  19200:	0x65 -> 0110 0101
					  38400:	0x66 -> 0110 0110
					  62500:	0x67 -> 0110 0111
PacketSize 	[7] 	: 240:		0x20 -> 0010 0000 
					  128:		0x60 -> 0110 0000
					  64:		0xa0 -> 1010 0000
					  32:		0xe0 -> 1110 0000
Power		[7]		: 22:		0x20 -> 0010 0000
					  17:		0x21 -> 0010 0001
				      13:		0x22 -> 0010 0010
					  10:		0x23 -> 0010 0011
Relay		[9] 	: Disable	0x83 -> 1000 0011
					  Enable	0xA3 -> 1010 0011
PacketRSSI	[9]		: Disable	0x03 -> 0000 0011
					: Enable	0x83 -> 1000 0011
ChannelRSSI [7] 	: Disable	0x00 -> 0000 0000
					: Enable	0x20 -> 0010 0000
Address		[3-4]	:
Channel		[8]		: 0			0x00 -> 0000 0000
					  84		0x54 -> 0101 0100
NetworkId	[5]		: 0			0x00 -> 0000 0000
					: 255		0xFF -> 1111 1111
Key			[10-11]	:
WOR Role	[9]		: Translate	0x83 -> 1000 0011
					: Receive	0x8B -> 1000 1011
WOR Cycle	[9]		: 500		0x80 -> 1000 0000
					: 1000		0x81 -> 1000 0001
					: 1500		0x82 -> 1000 0010
					: 2000		0x83 -> 1000 0011
					: 2500		0x84 -> 1000 0100
					: 3000		0x85 -> 1000 0101
					: 3500		0x86 -> 1000 0110
					: 4000		0x87 -> 1000 0111
Tran Mode	[9]		: Normal	0x83 -> 1000 0011
					: Fixed		0xC3 -> 1100 0011
LBT 		[9]		: Disable	0x83 -> 1000 0011
					: Enable	0x93 -> 1001 0011
*/