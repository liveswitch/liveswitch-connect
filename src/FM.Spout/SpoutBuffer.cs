
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using OpenGL;
using Spout.Interop;
using System;



namespace FM.Spout
{
    public class SpoutThread
    {
        private string name;
        private uint width;
        private uint height;
        private byte[] data;

        public SpoutThread(string name, uint width, uint height)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.data = new byte[width * height * 4];
        }

        public unsafe void Start()
        {
            using (DeviceContext deviceContext = DeviceContext.Create()) // Create the DeviceContext
            {
                IntPtr glContext = IntPtr.Zero;
                glContext = deviceContext.CreateContext(IntPtr.Zero);
                deviceContext.MakeCurrent(glContext); // Make this become the primary context
                SpoutSender sender = new SpoutSender();
                sender.CreateSender("CsSender", width, height, 0); // Create the sender
                byte[] internalData = new byte[width * height * 4];
                int i = 0;
                fixed (byte* pData = internalData) // Get the pointer of the byte array
                    while (true)
                    {
                        for (int j = 0; j < internalData.Length; j+=4)
                        {
                            internalData[j]   = data[j];
                            internalData[j+1] = data[j+1];
                            internalData[j+2] = data[j+2];
                            internalData[j+3] = data[j+3];
                        }
                        sender.SendImage(
                            pData, // Pixels
                            width, // Width
                            height, // Height
                            Gl.BGRA, // GL_RGBA
                            false, // B Invert
                            0 // Host FBO
                            );
                        Thread.Sleep(32);
                    }
            }
        }

        public void Write(byte[] bytes)
        {
            bytes.Skip(128).Take(data.Length).ToArray().CopyTo(data, 0);
        }
    }

    public class SpoutBuffer //: IDisposable
    {
        private SpoutThread spoutThread;
        private Thread thread;

        public SpoutBuffer(string name, uint width, uint height)
        {
            spoutThread = new SpoutThread(name, width, height);
            thread = new Thread(spoutThread.Start);
            thread.Start();
        }


        public unsafe bool TryWrite(byte[] bytes)
        {
            try
            {
                spoutThread.Write(bytes);
                return true;
            } catch (Exception ex)
            {
                Console.WriteLine("Unable to write to thread", ex);
            }
            return false;
        }

       /* public void Dispose()
        {
            deviceContext.Dispose();
        }*/
    }
}
