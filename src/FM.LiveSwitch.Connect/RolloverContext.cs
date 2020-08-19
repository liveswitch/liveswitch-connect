using System;

namespace FM.LiveSwitch.Connect
{
    class RolloverContext
    {
        public int Bits
        {
            get { return _Bits; }
            set
            {
                _Bits = value;
                _RolloverSize = (int)MathAssistant.Pow(2, _Bits);
                _RolloverSize_2 = _RolloverSize / 2;
            }
        }
        private int _Bits;

        public long RolloverCounter { get; private set; }

        public long HighestValue { get; private set; }

        private long _RolloverSize;
        private long _RolloverSize_2;

        public RolloverContext(int bits)
        {
            if (bits < 2)
            {
                throw new Exception("Minimum bits is 2.");
            }

            Bits = bits;
            RolloverCounter = 0;
            HighestValue = -1;
        }

        public long GetIndex(long value)
        {
            long roc = 0;
            return GetIndex(value, out roc);
        }

        public long GetIndex(long value, out long rolloverCounter) // i
        {
            if (HighestValue == -1)
            {
                HighestValue = value;
                rolloverCounter = RolloverCounter;
                return value;
            }

            long v;
            if (HighestValue < _RolloverSize_2)
            {
                // use > instead of >= to favour forward movement
                if (value - HighestValue > _RolloverSize_2)
                {
                    v = (RolloverCounter - 1) % 4294967296;
                }
                else
                {
                    v = RolloverCounter;

                    HighestValue = MathAssistant.Max(HighestValue, value);
                }
            }
            else
            {
                // use >= instead of > to favour forward movement
                if (HighestValue - _RolloverSize_2 >= value)
                {
                    v = (RolloverCounter + 1) % 4294967296;

                    HighestValue = value;
                    RolloverCounter = v;
                }
                else
                {
                    v = RolloverCounter;

                    HighestValue = MathAssistant.Max(HighestValue, value);
                }
            }
            rolloverCounter = v;
            return _RolloverSize * v + value;
        }
    }
}
