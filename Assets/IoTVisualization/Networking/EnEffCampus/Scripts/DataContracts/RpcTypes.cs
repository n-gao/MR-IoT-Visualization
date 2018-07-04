namespace IoTVisualization.Networking.EnEffCampus.DataContracts
{
    /// <summary>
    /// This static class contins possible RPC commands for the energy router protocol.
    /// </summary>
    public static class RpcTypes
    {
        public const string Reply = "reply";
        public const string Ping = "ping";
        public const string Hello = "hello";
        public const string Subscribe = "subscribe";
        public const string Unsubscribe = "unsubscribe";
        public const string Data = "data";
        public const string SubscribeAnnouncement = "subscribe_announcement";
        public const string UnsubscribeAnnouncement = "unsubscribe_announcement";
        public const string Announce = "announce";
        public const string Unannounce = "unannounce";
        public const string History = "history";
        public const string SetMrPosition = "set_mr_position";
    }
}
