namespace LoRa.Utils
{
    public static class BitHandler
    {
        public static int ReadBit(this byte value, int bitPosistion)
        {
            return (value & (1 << bitPosistion)) > 0 ? 1 : 0;
        }

        public static int ReadBitRange(this byte value, int startPosition, int length)
        {
            return (value & BuildBitMask(startPosition, length)) >> startPosition;
        }

        public static int BuildBitMask(int startPos, int length)
        {
            var rtn = 0;
            for (int i = 0; i <= length - 1; i++)
            {
                rtn += (1 << i);
            }
            return rtn << startPos;
        }
    }
}