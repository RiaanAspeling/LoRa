using System;
using System.Device.Gpio;
using System.Collections.Generic;
using System.Threading;
using LoRa.Commands;
using LoRa.Utils;
using System.Text;

namespace LoRa
{
    public class LoRa_SX126X
    {
        private SPHandler _comm;
        private GpioController _gpio;
        private int _frequency = 433;
        private int _m0_Pin = 22;
        private int _m1_Pin = 27;
        private SX126X_Communication_Mode _commsMode;

        // public Dictionary<int, int> SX126X_UART_Baudrate = new Dictionary<int, int>() { {0x00, 1200}, {0x20, 2400}, {0x40, 4800}, {0x60, 9600}, {0x80, 19200}, {0xA0, 38400}, {0xC0, 57600}, {0xE0, 115200} };

        public enum SX126X_Communication_Mode {_RaspberryPi = 0x00, _PC = 0x01 }

        public bool RSSI_Enabled { get; set; } = false;
        public int Address { get; set; } = 65535;
        public int Frequency { 
            get { 
                return _frequency; 
            } 
            set {
                if ((value < 410) || (value > 493 && value < 850) || (value > 930))
                    throw new Exception($"Frequency {value} is not supported. Only frequencies 410~493MHz or 850~930MHz are supported.");
                _frequency = value;
        } 
        }

        public List<string> GetAvailablePorts { get { return _comm.GetAvailablePorts();}}

        public LoRa_SX126X(SX126X_Communication_Mode commsMode = SX126X_Communication_Mode._RaspberryPi, int m0_Pin = 22, int m1_Pin = 27)
        {
            _comm = new SPHandler();

            _commsMode = commsMode;

            if (_commsMode == SX126X_Communication_Mode._RaspberryPi)
            {
                // For the RaspberryPi we need to setup the pins to control M0 and M1
                // Get M0 and M1 pins
                _m0_Pin = m0_Pin;
                _m1_Pin = m1_Pin;
                
                // Setup GPIO
                _gpio = new GpioController();
                _gpio.OpenPin(_m0_Pin, PinMode.Output);
                _gpio.OpenPin(_m1_Pin, PinMode.Output);
            }
        }

        ~LoRa_SX126X()
        {
            CloseComms();
            _gpio = null;
        }

        public bool OpenComms(string serialPort, int baudRate = 9600, int timeout = 1000)
        {
            try
            {
                if (!_comm.GetAvailablePorts().Contains(serialPort))
                    throw new Exception($"The port {serialPort} doesn't exist. Please call GetAvailablePorts for a list.");

                // Initialize Gpio's for normal opperation
                ConfigureModuleNormalMode();
                
                // Open Com Port
                _comm.SetPort(serialPort, baudRate, timeout);
                return _comm.Open();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
            return _comm.IsOpen();
        }

        public void CloseComms()
        {
            ConfigureModuleNormalMode();
            _comm.Close();
        }

        public LoRa_SX126X_Configuration? GetConfig()
        {
            if (!_comm.IsOpen()) 
                return null;
            ConfigureModuleForSettingsMode();
            var response = SendCommandAndGetResponse<LoRa_SX126X_Configuration>(sx126xCommands.GetSettings);
            ConfigureModuleNormalMode();
            return response;
        }

        public bool SetConfig(LoRa_SX126X_Configuration newConfig)
        {
            if (!_comm.IsOpen()) 
                return false;
            return true;
        }

        public void SetupModule(LoRa_SX126X_Configuration config)
        {
            if (!_comm.IsOpen())
                throw new Exception("Please OpenComms before trying any setup.");
            ConfigureModuleForSettingsMode();
            ConfigureModuleNormalMode();
        }

        private void ConfigureModuleForSettingsMode()
        {
            if (_commsMode == SX126X_Communication_Mode._PC) return;
            // We should pull up M1_Pin when setting up the module
            _gpio.Write(_m0_Pin, PinValue.Low);
            _gpio.Write(_m1_Pin, PinValue.High);
            Thread.Sleep(100);
        }

        private void ConfigureModuleNormalMode()
        {
            if (_commsMode == SX126X_Communication_Mode._PC) return;
            // Both M0 and M1 needs to be down for normal opperation
            _gpio.Write(_m0_Pin, PinValue.Low);
            _gpio.Write(_m1_Pin, PinValue.Low);
            Thread.Sleep(100);
        }

        private T SendCommandAndGetResponse<T>(String output) where T: IMapResult<T>
        {
            return SendCommandAndGetResponse<T>(Encoding.ASCII.GetBytes(output));
        }

        private  T SendCommandAndGetResponse<T>(byte[] data) where T: IMapResult<T>
        {
            try {
                var response = _comm.ExecuteCommand(data);
                var rtn = Activator.CreateInstance<T>().GetSettingsResult(response);
                return rtn;

            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return default;
        }
    }
}