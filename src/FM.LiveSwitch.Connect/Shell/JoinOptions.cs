using CommandLine;

namespace FM.LiveSwitch.Connect.Shell
{
    [Verb("join", HelpText = "Joins a channel.")]
    class JoinOptions : IChannelOptions
    {
        public string SharedSecret { get; set; }

        [Value(0, Required = true, HelpText = "The channel ID.")]
        public string ChannelId { get; set; }
    }
}
