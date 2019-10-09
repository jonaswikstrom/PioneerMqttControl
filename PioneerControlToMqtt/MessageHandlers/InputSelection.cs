namespace PioneerControlToMqtt.MessageHandlers
{
    public class InputSelection
    {

        private InputSelection(int inputNumber, string name)
        {
            Number = inputNumber;
            Name = name;
        }

        public static InputSelection Parse(string inputValue)
        {
            return Create(int.Parse(inputValue.Replace("FN", "")));
        }

        public static InputSelection Create(int inputNumber)
        {
            switch (inputNumber)
            {
                case 0:
                    return new InputSelection(0, "PHONO");
                case 1:
                    return new InputSelection(1, "CD");
                case 2:
                    return new InputSelection(1, "TUNER");
                case 3:
                    return new InputSelection(3, "CD-R/TAPE");
                case 4:
                    return new InputSelection(4, "DVD");
                case 5:
                    return new InputSelection(5, "TV");
                case 6:
                    return new InputSelection(6, "SAT/CBL");
                case 10:
                    return new InputSelection(10, "VIDEO1");
                case 14:
                    return new InputSelection(14, "VIDEO2");
                case 15:
                    return new InputSelection(15, "DVR/BDR");
                case 17:
                    return new InputSelection(17, "iPod/USB");
                case 19:
                    return new InputSelection(19, "HDMI1");
                case 20:
                    return new InputSelection(20, "HDMI2");
                case 21:
                    return new InputSelection(21, "HDMI3");
                case 22:
                    return new InputSelection(22, "HDMI4");
                case 23:
                    return new InputSelection(23, "HDMI5");
                case 24:
                    return new InputSelection(24, "HDMI6");
                case 25:
                    return new InputSelection(25, "BD");
                case 26:
                    return new InputSelection(26, "INTERNET RADIO");
                case 33:
                    return new InputSelection(33, "NO ADP");
                case 38:
                    return new InputSelection(38, "INTERNET RADIO");
                case 44:
                    return new InputSelection(44, "MEDIA SERVER");
                case 45:
                    return new InputSelection(45, "FAVOURITES");
                case 48:
                    return new InputSelection(48, "HDMI/MHL");
                case 49:
                    return new InputSelection(49, "GAME");
                case 53:
                    return new InputSelection(53, "SPOTIFY");
                default:
                    return new InputSelection(-1, "UNKNOWN DEVICE");
            }
        }

        public string Name { get; }
        public int Number { get; }
    }
}