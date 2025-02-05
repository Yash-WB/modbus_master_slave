using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NModbus;
using NModbus.Data;
using NModbus.Device;

public class ModbusSlave
{
    private readonly string ipAddress;
    private readonly int port;
    private TcpListener? tcpListener;
    private IModbusSlaveNetwork? network;
    private CancellationTokenSource? cancellationSource;
    private readonly ushort[] registerValues;

    public ModbusSlave(string ipAddress = "0.0.0.0", int port = 502)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.registerValues = new ushort[100];
        InitializeRegisters();
    }

    private void InitializeRegisters()
    {
        Random random = new Random();
        for (int i = 0; i < registerValues.Length; i++)
        {
            registerValues[i] = (ushort)(i + random.Next(100));
        }
    }

    public async Task StartAsync()
    {
        try
        {
            var address = IPAddress.Parse(ipAddress);
            tcpListener = new TcpListener(address, port);
            tcpListener.Start();

            var factory = new ModbusFactory();
            network = factory.CreateSlaveNetwork(tcpListener);

            var dataStore = new DefaultSlaveDataStore();
            dataStore.HoldingRegisters.WritePoints(0, registerValues);

            var slave = factory.CreateSlave(1, dataStore);
            network.AddSlave(slave);

            cancellationSource = new CancellationTokenSource();
            Console.WriteLine($"Modbus Slave started on {ipAddress}:{port}");
            await network.ListenAsync(cancellationSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start Modbus slave: {ex.Message}");
            throw;
        }
    }

    public static async Task Main(string[] args)
    {
        var slave = new ModbusSlave();
        try
        {
            await slave.StartAsync();
            Console.WriteLine("Press any key to stop the slave...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Slave error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Modbus Slave stopped");
        }
    }
}