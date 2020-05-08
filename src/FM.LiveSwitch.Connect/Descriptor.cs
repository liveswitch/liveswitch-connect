using System;
using System.Linq;

namespace FM.LiveSwitch.Connect
{
    class Descriptor
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public Descriptor(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static string Format(Descriptor[] descriptors)
        {
            var maxNameLength = descriptors.OrderByDescending(x => x.Name.Length).First().Name.Length;
            return string.Join(Environment.NewLine, descriptors.Select(descriptor => $"  {descriptor.Name.PadRight(maxNameLength, ' ')} : {descriptor.Value}"));
        }
    }
}
