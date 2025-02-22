﻿//using System;
//using System.Net.Sockets;
//using System.Threading.Tasks;
//using NModbus;

//public class ModbusMaster
//{
//    private readonly string ipAddress;
//    private readonly int port;
//    private IModbusMaster? master;
//    private TcpClient? tcpClient;

//    public ModbusMaster(string ipAddress = "modbus-slave", int port = 502)
//    {
//        this.ipAddress = ipAddress;
//        this.port = port;
//    }

//    public async Task ConnectAsync()
//    {
//        try
//        {
//            tcpClient = new TcpClient();
//            await tcpClient.ConnectAsync(ipAddress, port);
//            var factory = new ModbusFactory();
//            master = factory.CreateMaster(tcpClient);
//            Console.WriteLine($"Successfully connected to Modbus slave at {ipAddress}:{port}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Failed to connect to Modbus slave: {ex.Message}");
//            throw;
//        }
//    }

//    public async Task ReadRegistersAsync()
//    {
//        try
//        {
//            byte slaveId = 1;

//            ushort startAddress = 30;

//            ushort numRegisters = 10;

//            while (true)
//            {
//                if (master != null)
//                {
//                    ushort[] registers = await master.ReadHoldingRegistersAsync(slaveId, startAddress, numRegisters);

//                    for (int i = 0; i < numRegisters; i++)
//                    {
//                        Console.WriteLine($"Register {startAddress + i} = {registers[i]}");
//                    }
//                }
//                else
//                {
//                    Console.WriteLine("Modbus master is not connected.");
//                }

//                await Task.Delay(5000);
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Failed to read registers: {ex.Message}");
//            throw;
//        }
//    }

//    public static async Task Main(string[] args)
//    {
//        var master = new ModbusMaster();
//        try
//        {
//            await master.ConnectAsync();
//            await Task.Delay(500);

//            Console.WriteLine("Starting to read registers...");
//            await master.ReadRegistersAsync();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Master error: {ex.Message}");
//        }
//        finally
//        {
//            Console.WriteLine("Disconnected from Modbus slave");
//        }
//    }
//}

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using NModbus;
using UserInputs.Model;

public class ModbusMaster
{
    private readonly string apiUrl = "http://192.168.0.220:5181/api/UserInput";
    private readonly string ipAddress;
    private readonly int port;
    private IModbusMaster? master;
    private TcpClient? tcpClient;
    private int startAddress;
    private int numRegisters;

    //public ModbusMaster(string ipAddress = "192.168.0.220", int port = 502)
    public ModbusMaster(string ipAddress = "modbus-slave", int port = 502)
    {
        this.ipAddress = ipAddress;
        this.port = port;
    }

    private async Task FetchInputFromApi()
    {
        try
        {
            using HttpClient client = new HttpClient();

            while (true)
            {
                var userInput = await client.GetFromJsonAsync<UserInput>(apiUrl);

                if (userInput != null && userInput.NumRegisters >= 1 && userInput.NumRegisters <= 125)
                {
                    startAddress = userInput.StartAddress;
                    numRegisters = userInput.NumRegisters;
                    Console.WriteLine($"Fetched from API: StartAddress={startAddress}, NumRegisters={numRegisters}");
                    break;
                }

                Console.WriteLine("Waiting for valid input from API...");
                await Task.Delay(5000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch input from API: {ex.Message}");
        }
    }

    public async Task ConnectAsync()
    {
        try
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ipAddress, port);
            var factory = new ModbusFactory();
            master = factory.CreateMaster(tcpClient);
            Console.WriteLine($"Successfully connected to Modbus slave at {ipAddress}:{port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to Modbus slave: {ex.Message}");
        }
    }

    public async Task ReadRegistersAsync()
    {
        try
        {
            byte slaveId = 1;

            while (true)
            {
                if (master != null)
                {
                    ushort[] registers = await master.ReadHoldingRegistersAsync(slaveId, (ushort)startAddress, (ushort)numRegisters);

                    for (int i = 0; i < numRegisters; i++)
                    {
                        Console.WriteLine($"Register {startAddress + i} = {registers[i]}");
                    }
                }
                else
                {
                    Console.WriteLine("Modbus master is not connected.");
                }

                await Task.Delay(5000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to read registers: {ex.Message}");
        }
    }

    public static async Task Main(string[] args)
    {
        var master = new ModbusMaster();

        Console.WriteLine("Please provide input using the Web API (POST to /api/UserInput)");
        await master.FetchInputFromApi();

        try
        {
            await master.ConnectAsync();
            await Task.Delay(500);

            Console.WriteLine("Starting to read registers...");
            await master.ReadRegistersAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Master error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Disconnected from Modbus slave");
        }
    }
}

