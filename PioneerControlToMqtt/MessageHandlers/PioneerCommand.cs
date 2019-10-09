namespace PioneerControlToMqtt.MessageHandlers
{
    public static class PioneerCommand
    {
        public static string VolumeDown => "VD";
        public static string VolumeUp => "VU";
        public static string VolumeInfo => "?V";
        public static string MuteOnOff => "MZ";

        public static string FunctionInfo => "?F";
        public static string FunctionChange => "FN";
        public static string PowerStatus => "?P";
        public static string PowerOn => "PO";
        public static string PowerOff => "PF";
    }
}