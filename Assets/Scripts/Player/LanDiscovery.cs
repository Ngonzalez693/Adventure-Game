using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// Sistema simple de descubrimiento LAN basado en broadcast UDP.
// El HOST envía paquetes UDP a la red local cada cierto tiempo,
// indicando su puerto de juego. Los CLIENTES escuchan en el mismo
// puerto de descubrimiento y, cuando reciben un paquete, obtienen
// la IP del host y se pueden conectar automáticamente.
public class LanDiscovery : MonoBehaviour
{
    // Puerto fijo donde se hace broadcast/escucha del "anuncio" del host.
    // No es el puerto del juego — es solo para descubrir.
    private const int DiscoveryPort = 47777;

    // Cadena mágica para validar que el paquete es de nuestro juego
    // y no basura de otra aplicación que usa el mismo puerto.
    private const string Magic = "FUTPROTO";

    // Cliente UDP para enviar broadcasts (host) o recibir (cliente).
    private UdpClient _broadcaster;
    private UdpClient _listener;

    // Hilos en background para no bloquear el hilo principal de Unity.
    private Thread _broadcastThread;
    private Thread _listenThread;

    private volatile bool _isRunning;

    // Variables compartidas entre hilo de escucha y main thread.
    // 'volatile' garantiza que los cambios se vean entre hilos.
    private volatile string _foundIp;
    private volatile int _foundPort;

    // Callback que dispara el main thread cuando encuentra un host.
    // Se invoca desde Update() para que sea seguro tocar Unity APIs.
    public Action<string, ushort> OnHostFound;

    // ────────────────────────────────────────────────
    //  HOST: comienza a anunciarse en la LAN
    // ────────────────────────────────────────────────
    public void StartBroadcasting(ushort gamePort)
    {
        Stop();
        _isRunning = true;

        _broadcaster = new UdpClient { EnableBroadcast = true };

        _broadcastThread = new Thread(() => BroadcastLoop(gamePort)) { IsBackground = true };
        _broadcastThread.Start();

        Debug.Log($"[LanDiscovery] Broadcast iniciado anunciando puerto {gamePort}.");
    }

    // ────────────────────────────────────────────────
    //  CLIENTE: escucha hasta encontrar un host
    // ────────────────────────────────────────────────
    public void StartListening()
    {
        Stop();
        _isRunning = true;

        try
        {
            _listener = new UdpClient(DiscoveryPort);
        }
        catch (SocketException e)
        {
            Debug.LogError($"[LanDiscovery] No se pudo abrir el puerto {DiscoveryPort}: {e.Message}");
            return;
        }

        _listenThread = new Thread(ListenLoop) { IsBackground = true };
        _listenThread.Start();

        Debug.Log("[LanDiscovery] Escuchando hosts en la LAN...");
    }

    public void Stop()
    {
        _isRunning = false;

        try { _broadcaster?.Close(); } catch { }
        try { _listener?.Close(); } catch { }

        _broadcaster = null;
        _listener = null;
        _broadcastThread = null;
        _listenThread = null;
    }

    // ────────────────────────────────────────────────
    //  Loops en hilos secundarios
    // ────────────────────────────────────────────────
    private void BroadcastLoop(ushort gamePort)
    {
        var endpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
        var message = Encoding.ASCII.GetBytes($"{Magic}:{gamePort}");

        while (_isRunning)
        {
            try
            {
                _broadcaster.Send(message, message.Length, endpoint);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LanDiscovery] Broadcast falló: {e.Message}");
            }

            // Anunciar cada 1 segundo
            Thread.Sleep(1000);
        }
    }

    private void ListenLoop()
    {
        var any = new IPEndPoint(IPAddress.Any, 0);

        while (_isRunning)
        {
            try
            {
                byte[] data = _listener.Receive(ref any);
                string msg = Encoding.ASCII.GetString(data);

                // Validar que sea de nuestro juego
                if (!msg.StartsWith(Magic + ":")) continue;

                string portStr = msg.Substring(Magic.Length + 1);
                if (!ushort.TryParse(portStr, out ushort port)) continue;

                // Guardar resultado para que el main thread lo procese
                _foundIp = any.Address.ToString();
                _foundPort = port;
                _isRunning = false; // dejamos de escuchar tras encontrar uno
            }
            catch (Exception)
            {
                // Socket cerrado o error — salimos del loop
                break;
            }
        }
    }

    // ────────────────────────────────────────────────
    //  Update: dispara el callback en el main thread
    // ────────────────────────────────────────────────
    private void Update()
    {
        if (_foundIp != null && OnHostFound != null)
        {
            string ip = _foundIp;
            ushort port = (ushort)_foundPort;
            _foundIp = null;

            OnHostFound.Invoke(ip, port);
        }
    }

    private void OnDestroy() => Stop();
    private void OnApplicationQuit() => Stop();
}
