using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using NModbus;

public class ModbusMaster
{
    private readonly string ipAddress;
    private readonly int port;
    private IModbusMaster? master;
    private TcpClient? tcpClient;

    public ModbusMaster(string ipAddress = "192.168.0.220", int port = 502)
    {
        this.ipAddress = ipAddress;
        this.port = port;
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
            throw;
        }
    }

    public async Task ReadRegistersAsync()
    {
        try
        {
            byte slaveId = 1;
            ushort startAddress = 40;
            ushort numRegisters = 10;

            if (master != null)
            {
                ushort[] registers = await master.ReadHoldingRegistersAsync(slaveId, startAddress, numRegisters);

                for (int i = 0; i < numRegisters; i++)
                {
                    Console.WriteLine($"Register {(startAddress + i)} = {registers[i]}");
                }
            }
            else
            {
                Console.WriteLine("Modbus master is not connected.");
            }
        }
        catch (NModbus.SlaveException ex)
        {
            Console.WriteLine($"SlaveException: {ex.Message} (Check slave address, function, or register address)");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to read registers: {ex.Message}");
            throw;
        }
    }

    public static async Task Main(string[] args)
    {
        var master = new ModbusMaster();
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
