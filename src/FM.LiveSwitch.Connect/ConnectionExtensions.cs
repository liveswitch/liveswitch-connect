using System.Collections.Generic;
using System.Threading.Tasks;

namespace FM.LiveSwitch.Connect
{
    static class ConnectionExtensions
    {
        public static async Task<Task> Connect(this ManagedConnection connection)
        {
            var disconnectedSource = new TaskCompletionSource<ManagedConnection>();
            connection.OnStateChange += (c) =>
            {
                if (connection.State == ConnectionState.Closed ||
                    connection.State == ConnectionState.Failed)
                {
                    disconnectedSource.TrySetResult(connection);
                }
            };
            await connection.Open();
            return disconnectedSource.Task;
        }

        public static Task Disconnect(this ManagedConnection connection)
        {
            return connection.Close().AsTaskAsync();
        }

        public static Descriptor[] GetDescriptors(this ManagedConnection connection)
        {
            var descriptors = new List<Descriptor>();
            if (connection.Id != null)
            {
                descriptors.Add(new Descriptor("ID", connection.Id));
            }
            if (connection.Tag != null)
            {
                descriptors.Add(new Descriptor("Tag", connection.Tag));
            }
            if (connection.MediaId != null)
            {
                descriptors.Add(new Descriptor("Media ID", connection.MediaId));
            }
            return descriptors.ToArray();
        }
    }
}
