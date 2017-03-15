using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;              //Library for serial stuff

namespace CSharpToArduino
{
    class Program
    {
        static SerialPort port;
        static void Main(string[] args)
        {
            int baud;
            string name;
            /*Console.WriteLine("Welcome, enter parameters to begin");
            Console.WriteLine(" ");
            Console.WriteLine("Available ports:");
            if (SerialPort.GetPortNames().Count() >= 0)
            {
                foreach (string p in SerialPort.GetPortNames())
                {
                    Console.WriteLine(p);
                }
            }
            else
            {
                Console.WriteLine("No Ports available, press any key to exit.");
                Console.ReadLine();
                // Quit
                return;
            }
            Console.WriteLine("Port Name:");
            name = Console.ReadLine();
            Console.WriteLine(" ");
            Console.WriteLine("Baud rate:");
            baud = GetBaudRate();

            Console.WriteLine(" ");
            Console.WriteLine("Beging Serial...");*/
            //BeginSerial(baud, name);
            BeginSerial(9600, "COM4");
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.Open();
            Console.WriteLine("Serial Started.");
            Console.WriteLine(" ");
            Console.WriteLine("Ctrl+C to exit program");
            Console.WriteLine("Send:");

            for (; ; )
            {
                /*Console.WriteLine(" ");
                Console.WriteLine("> ");
                port.WriteLine(Console.ReadLine());*/
                port.WriteLine("H");
            }
        }

        static void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            for (int i = 0; i < (10000 * port.BytesToRead) / port.BaudRate; i++)
                ;       //Delay a bit for the serial to catch up
            Console.Write(port.ReadExisting());
            Console.WriteLine("");
            Console.WriteLine("> ");
        }

        static void BeginSerial(int baud, string name)
        {
            port = new SerialPort(name, baud);
        }

        /*static int GetBaudRate()
        {
            try
            {
                return int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Invalid integer.  Please try again:");
                return GetBaudRate();
            }
        }*/
    }
}
