using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace LoRa
{
    class Program
    {
        // static bool _continue;
        // static SerialPort _serialPort;

        public static void Main()
        {
            // string name;
            // string message;
            // StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            // Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            //_serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            //_serialPort.PortName = "/dev/ttyS0"; //SetPortName(_serialPort.PortName);
            // _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
            // _serialPort.Parity = SetPortParity(_serialPort.Parity);
            // _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
            // _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

            // Set the read/write timeouts
            // _serialPort.ReadTimeout = 500;
            // _serialPort.WriteTimeout = 500;

            // _serialPort.Open();
            // _continue = true;
            // readThread.Start();

            //_serialPort.WriteLine("\x00\x00\x17\x00\x00\x17THIS IS A MESSAGE FROM THE RASPBERRY");

            //var lora = new sx126x("/dev/ttyS0", 433, 0, 22, true, 2400);
            //lora.Send("\x00\x00\x17\x00\x00\x17MESSAGE FROM CODE");

            var loraNew = new LoRa_SX126X(LoRa_SX126X.SX126X_Communication_Mode._RaspberryPi);
            if (!loraNew.OpenComms("/dev/ttyS0"))
            {
                Console.WriteLine("Unable to open port!");
                return;
            }

            var config = loraNew.GetConfig();

            Console.WriteLine($"Frequency = {config?.Frequency}, Address = {config?.Address}, Speed = {config?.Speed}, Power = {config?.Power}");

            Console.WriteLine("Press any key to close..");
            Console.ReadLine();

            loraNew.CloseComms();

            // Console.Read();

            // readThread.Join();
            // _serialPort.Close();
        }

        // public static void Read()
        // {
        //     while (_continue)
        //     {
        //         try
        //         {
        //             var message = _serialPort.ReadChar();
        //             Console.WriteLine($"Value={message}");
        //         }
        //         catch (TimeoutException) { }
        //         Console.Write(".");
        //     }
        // }

        // // Display Port values and prompt user to enter a port.
        // public static string SetPortName(string defaultPortName)
        // {
        //     string portName;

        //     Console.WriteLine("Available Ports:");
        //     foreach (string s in SerialPort.GetPortNames())
        //     {
        //         Console.WriteLine("   {0}", s);
        //     }

        //     Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
        //     portName = Console.ReadLine();

        //     if (portName == "" || !(portName.ToLower()).StartsWith("com"))
        //     {
        //         portName = defaultPortName;
        //     }
        //     return portName;
        // }
        // // Display BaudRate values and prompt user to enter a value.
        // public static int SetPortBaudRate(int defaultPortBaudRate)
        // {
        //     string baudRate;

        //     Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
        //     baudRate = Console.ReadLine();

        //     if (baudRate == "")
        //     {
        //         baudRate = defaultPortBaudRate.ToString();
        //     }

        //     return int.Parse(baudRate);
        // }

        // // Display PortParity values and prompt user to enter a value.
        // public static Parity SetPortParity(Parity defaultPortParity)
        // {
        //     string parity;

        //     Console.WriteLine("Available Parity options:");
        //     foreach (string s in Enum.GetNames(typeof(Parity)))
        //     {
        //         Console.WriteLine("   {0}", s);
        //     }

        //     Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
        //     parity = Console.ReadLine();

        //     if (parity == "")
        //     {
        //         parity = defaultPortParity.ToString();
        //     }

        //     return (Parity)Enum.Parse(typeof(Parity), parity, true);
        // }
        // // Display DataBits values and prompt user to enter a value.
        // public static int SetPortDataBits(int defaultPortDataBits)
        // {
        //     string dataBits;

        //     Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
        //     dataBits = Console.ReadLine();

        //     if (dataBits == "")
        //     {
        //         dataBits = defaultPortDataBits.ToString();
        //     }

        //     return int.Parse(dataBits.ToUpperInvariant());
        // }

        // // Display StopBits values and prompt user to enter a value.
        // public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        // {
        //     string stopBits;

        //     Console.WriteLine("Available StopBits options:");
        //     foreach (string s in Enum.GetNames(typeof(StopBits)))
        //     {
        //         Console.WriteLine("   {0}", s);
        //     }

        //     Console.Write("Enter StopBits value (None is not supported and \n" +
        //     "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
        //     stopBits = Console.ReadLine();

        //     if (stopBits == "" )
        //     {
        //         stopBits = defaultPortStopBits.ToString();
        //     }

        //     return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        // }
        // public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        // {
        //     string handshake;

        //     Console.WriteLine("Available Handshake options:");
        //     foreach (string s in Enum.GetNames(typeof(Handshake)))
        //     {
        //         Console.WriteLine("   {0}", s);
        //     }

        //     Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
        //     handshake = Console.ReadLine();

        //     if (handshake == "")
        //     {
        //         handshake = defaultPortHandshake.ToString();
        //     }

        //     return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        // }
    }
}
