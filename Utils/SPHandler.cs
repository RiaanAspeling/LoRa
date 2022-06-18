using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace LoRa.Utils
{
    public class SPHandler
    {
        private SerialPort _serialPort;
        private int _timeOut;
        private bool _waitingForResponse;
        private byte[] _response;

        public bool IsOpen()
        {
            return _serialPort != null ? _serialPort.IsOpen : false;
        }

        public SPHandler()
        {
            _waitingForResponse = false;
        }

        public List<string> GetAvailablePorts()
        {
            return new List<string>(SerialPort.GetPortNames());
        }

        public void SetPort(string portName, int baudRate, int timeOutMs = 1000)
        {
            _timeOut = timeOutMs;
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Parity = Parity.None;
            _serialPort.Handshake = Handshake.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.RtsEnable = true;
            _serialPort.DtrEnable = true;
            _serialPort.WriteTimeout = _timeOut;
            _serialPort.ReadTimeout = _timeOut;
        }

        public bool Open()
        {
            try
            {
                if (_serialPort != null && !_serialPort.IsOpen)
                {
                    _serialPort.DataReceived += _serialPort_DataReceived;
                    _serialPort.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = new byte[_serialPort.BytesToRead];
            _serialPort.Read(data, 0, data.Length);
            _response = data;
            _waitingForResponse = false;
            Console.WriteLine($"<<<<#### RECEIVING [{data.Length}] : {BitConverter.ToString(data)}");
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            _serialPort.DataReceived -= _serialPort_DataReceived;
        }

        public byte[] ExecuteCommand(byte[] cmd)
        {
            Console.WriteLine($"####>>>> SENDING [{cmd.Length}] : {BitConverter.ToString(cmd)}");
            _waitingForResponse = true;
            _response = null;
            _serialPort.DiscardOutBuffer();
            _serialPort.DiscardInBuffer();
            _serialPort.Write(cmd, 0, cmd.Length);

            var timeout = DateTime.Now.AddMilliseconds(_timeOut);
            while (_waitingForResponse && DateTime.Now < timeout)
                Thread.Sleep(10);
            
            if (_waitingForResponse)
            {
                // We had a timeout!
                _waitingForResponse = false;
                throw new TimeoutException($"Timeout waiting for response. Current timeout set to {_timeOut}ms.");
            }

            return _response;
        }
    }
}