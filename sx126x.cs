using System;
using System.IO;
using System.IO.Ports;
using System.Device.Gpio;
using System.Collections.Generic;

public class sx126x
{
    int M0 = 22;
    int M1 = 27;
    // if the header is 0xC0, then the LoRa register settings dont lost when it poweroff, and 0xC2 will be lost. 
    // cfg_reg = [0xC0,0x00,0x09,0x00,0x00,0x00,0x62,0x00,0x17,0x43,0x00,0x00]
    byte[] cfg_reg = new byte[] {0xC2,0x00,0x09,0x00,0x00,0x00,0x62,0x00,0x12,0x43,0x00,0x00};
    byte[] get_reg = new byte[12];
    bool rssi = false;
    int addr = 65535;
    string serial_n = "";
    int addr_temp = 0;
    // start frequence of two lora module
    //
    // E22-400T22S           E22-900T22S
    // 410~493MHz      or    850~930MHz
    int start_freq = 850;
    // offset between start and end frequence of two lora module
    //
    // E22-400T22S           E22-900T22S
    // 410~493MHz      or    850~930MHz
    int offset_freq = 18;   // 850(start_freq) + 18(offset_freq) = 868MHz
                            // 410(start_freq) + 23(offset_freq) = 433MHz
    // power = 22
    // air_speed =2400

    enum SX126X_UART_BAUDRATE {
        _1200 = 0x00,
        _2400 = 0x20,
        _4800 = 0x40,
        _9600 = 0x60,
        _19200 = 0x80,
        _38400 = 0xA0,
        _57600 = 0xC0,
        _115200 = 0xE0
    }

    enum SX126X_PACKAGE_SIZE {
        _240_BYTE = 0x00,
        _128_BYTE = 0x40,
        _64_BYTE = 0x80,
        _32_BYTE = 0xC0
    }

    enum SX126X_Power {
        _22dBm = 0x00,
        _17dBm = 0x01,
        _13dBm = 0x02,
        _10dBm = 0x03
    }

    Dictionary<int,byte> lora_air_speed_dic =  new Dictionary<int,byte> {
        {1200, 0x01},
        {2400, 0x02},
        {4800, 0x03},
        {9600, 0x04},
        {19200, 0x05},
        {38400, 0x06},
        {62500, 0x07}
    };

    Dictionary<int,byte> lora_power_dic = new Dictionary<int, byte> {
        {22, 0x00},
        {17, 0x01},
        {13, 0x02},
        {10, 0x03}
    };

    Dictionary<int,SX126X_PACKAGE_SIZE> lora_buffer_size_dic = new Dictionary<int, SX126X_PACKAGE_SIZE> {
        {240, SX126X_PACKAGE_SIZE._240_BYTE},
        {128, SX126X_PACKAGE_SIZE._128_BYTE},
        {64, SX126X_PACKAGE_SIZE._64_BYTE},
        {32, SX126X_PACKAGE_SIZE._32_BYTE}
    };

    // Not used?
    int freq = 0;
    int power = 22;
    int send_to = 0;
    GpioController _controller;
    SerialPort _serialPort;

    public sx126x(string serial_num, int freq, int addr, int power, bool rssi, int air_speed = 2400, int net_id = 0, int buffer_size = 240, byte crypt = 0, bool relay = false) //, bool lbt = false, bool wor = false)
    {
        this.serial_n = serial_num;
        this.freq = freq;
        this.addr = addr;
        this.power = power;
        this.rssi = rssi;

        // Initial the GPIO for M0 and M1 Pin
        _controller = new GpioController ();
        //GPIO.setmode(GPIO.BCM)
        //GPIO.setwarnings(False)
        //GPIO.setup(self.M0,GPIO.OUT)
        _controller.OpenPin(M0, PinMode.Output);
        //GPIO.setup(self.M1,GPIO.OUT)
        _controller.OpenPin(M1, PinMode.Output);
        //GPIO.output(self.M0,GPIO.LOW)
        _controller.Write(M0, PinValue.Low);
        //GPIO.output(self.M1,GPIO.HIGH)
        _controller.Write(M1, PinValue.High);

        // The hardware UART of Pi3B+,Pi4B is /dev/ttyS0
        //self.ser = serial.Serial(serial_num,9600)
        _serialPort = new SerialPort();
        _serialPort.PortName = serial_n;
        _serialPort.BaudRate = 9600;
        _serialPort.ReadTimeout = 500;
        _serialPort.WriteTimeout = 500;
        //self.ser.flushInput()
        _serialPort.Open();

        //self.set(freq,addr,power,rssi,air_speed,net_id,buffer_size,crypt,relay,lbt,wor)
        Set(freq,addr,power,rssi,air_speed,net_id,buffer_size,crypt,relay); //,lbt,wor);
    }

    private void Set(int freq, int addr, int power, bool rssi, int air_speed = 2400, int net_id = 0, int buffer_size = 240, byte crypt = 0, bool relay = false) //, bool lbt = false, bool wor = false)
    {
        this.send_to = addr;
        this.addr = addr;
        // We should pull up the M1 pin when sets the module
        //GPIO.output(self.M0,GPIO.LOW)
        _controller.Write(M0, PinValue.Low);
        //GPIO.output(self.M1,GPIO.HIGH)
        _controller.Write(M1, PinValue.High);
        //time.sleep(0.1)
        System.Threading.Thread.Sleep(100);

        byte low_addr = (byte)(addr & 0xff);
        byte high_addr = (byte)(addr >> 8 & 0xff);
        byte net_id_temp = (byte)(net_id & 0xff);
        var freq_temp = -1;
        if (freq > 850) {
            freq_temp = freq - 850;
            start_freq = 850;
            offset_freq = freq_temp;
        } else if (freq >410) {
            freq_temp = freq - 410;
            start_freq  = 410;
            offset_freq = freq_temp;
        }

        var air_speed_temp = lora_air_speed_dic[air_speed];
        var buffer_size_temp = lora_buffer_size_dic[buffer_size];
        var power_temp = lora_power_dic[power];
        byte rssi_temp = 0x00;
        if (rssi) {
            //enable print rssi value 
            rssi_temp = 0x80;
        } else {
            // disable print rssi value
            rssi_temp = 0x00;  
        }
        byte l_crypt = (byte)(crypt & 0xff);
        byte h_crypt = (byte)(crypt >> 8 & 0xff);

        if (relay == false) {
            cfg_reg[3] = high_addr;
            cfg_reg[4] = low_addr;
            cfg_reg[5] = net_id_temp;
            cfg_reg[6] = (byte)((byte)SX126X_UART_BAUDRATE._9600 + air_speed_temp);
            // 
            // it will enable to read noise rssi value when add 0x20 as follow
            // 
            cfg_reg[7] = (byte)(buffer_size_temp + power_temp + 0x20);
            cfg_reg[8] = (byte)freq_temp;
            //
            // it will output a packet rssi value following received message
            // when enable eighth bit with 06H register(rssi_temp = 0x80)
            //
            cfg_reg[9] = (byte)(0x43 + rssi_temp);
            cfg_reg[10] = h_crypt;
            cfg_reg[11] = l_crypt;
        } else {
            cfg_reg[3] = 0x01;
            cfg_reg[4] = 0x02;
            cfg_reg[5] = 0x03;
            cfg_reg[6] = (byte)((byte)SX126X_UART_BAUDRATE._9600 + air_speed_temp);
            // 
            // it will enable to read noise rssi value when add 0x20 as follow
            // 
            cfg_reg[7] = (byte)((byte)buffer_size_temp + power_temp + 0x20);
            cfg_reg[8] = (byte)freq_temp;
            //
            // it will output a packet rssi value following received message
            // when enable eighth bit with 06H register(rssi_temp = 0x80)
            //
            cfg_reg[9] = (byte)(0x03 + rssi_temp);
            cfg_reg[10] = h_crypt;
            cfg_reg[11] = l_crypt;
        }

        _serialPort.Write(cfg_reg, 0, 12);
        System.Threading.Thread.Sleep(200);
        var rslt = _serialPort.ReadExisting();

        //GPIO.output(self.M0,GPIO.LOW)
        _controller.Write(M0, PinValue.Low);
        //GPIO.output(self.M1,GPIO.LOW)
        _controller.Write(M1, PinValue.Low);
        System.Threading.Thread.Sleep(100);

        if (rslt[0] != 0xC1)
        {
            Console.WriteLine("Setup failed!");
        }
    }

    public void GetSettings()
    {
        //the pin M1 of lora HAT must be high when enter setting mode and get parameters
        _controller.Write(M1, PinValue.High);
        System.Threading.Thread.Sleep(100);

        // send command to get setting parameters
        _serialPort.Write(new byte[] {0xC1,0x00,0x09}, 0, 3);
        System.Threading.Thread.Sleep(100);
        
        var rslt = _serialPort.ReadExisting();

        // check the return characters from hat and print the setting parameters
        if (rslt[0] == 0xC1 && rslt[2] == 0x09)
        {
            var fre_temp = rslt[8];
            var addr_temp = rslt[3] + rslt[4];
            var air_speed_temp = rslt[6] & 0x03;
            var power_temp = rslt[7] & 0x03;
            
            Console.WriteLine($"Frequence is {fre_temp}.125MHz.");
            Console.WriteLine($"Node address is {addr_temp}.");
            Console.WriteLine($"Air speed is {air_speed_temp} bps");
            Console.WriteLine($"Power is {power_temp} dBm");
            
            // GPIO.output(M1,GPIO.LOW)
        }
        _controller.Write(M1, PinValue.Low);
    }

    //
    // the data format like as following
    // "node address,frequence,payload"
    // "20,868,Hello World"
    public void Send(string data)
    {
        //GPIO.output(self.M1,GPIO.LOW)
        _controller.Write(M1, PinValue.Low);
        //GPIO.output(self.M0,GPIO.LOW)
        _controller.Write(M0, PinValue.Low);
        System.Threading.Thread.Sleep(100);

        _serialPort.Write(data);
        System.Threading.Thread.Sleep(100);
    }

    public string Receive()
    {
        if (_serialPort.BytesToRead < 1)
            return null;
        System.Threading.Thread.Sleep(500);
        var buff = _serialPort.ReadExisting();
        Console.WriteLine(buff);
        return buff;
    }

    public void GetRSSIChannel()
    {
        //GPIO.output(self.M1,GPIO.LOW)
        _controller.Write(M1, PinValue.Low);
        //GPIO.output(self.M0,GPIO.LOW)
        _controller.Write(M0, PinValue.Low);

        System.Threading.Thread.Sleep(100);
        _serialPort.DiscardInBuffer();
        _serialPort.Write(new byte[] {0xC0,0xC1,0xC2,0xC3,0x00,0x02}, 0, 6);
        System.Threading.Thread.Sleep(500);
        var buff = _serialPort.ReadExisting();
        if (buff[0] == 0xC1 && buff[1] == 0x00 && buff[2] == 0x02) {
            Console.WriteLine($"The current noise RSSI value is = {256 - buff[3]}");
        } else {
            Console.WriteLine("Failed to read RSSI value");
        }
        
    }
    
}