using System.Buffers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace WebServerMVCv2.Services.TCP
{
    public class RemoteTcpClient
    {
        //Properties
        private string? IpAddress { get; init; } = "192.168.4.81";
        private int Port { get; init; } = 5001;
        string? Message { get; set; } = "";
        public JsonDocument? Response { get; private set; }
        public string? rawStringResponse { get; private set; } = string.Empty;
        public byte[] VideoBuffer { get; set; } = Array.Empty<byte>();

        public DatabaseRequest? DbRequest { get; set; }

        #region constructors and destructors
        //Constructor
        public RemoteTcpClient()
        {
            //default constructor
        }
        public RemoteTcpClient(string ipAddress, int port, string message)
        {
            this.IpAddress = ipAddress;
            this.Port = port;
            this.Message = message;

        }

        public RemoteTcpClient(string message)
        {
            this.Message = message;
        }

        ~RemoteTcpClient()
        {
            //destructor
        }
        #endregion

        //Methods
        #region Getters and Setters
        public void SetMessage(string message)
        {
            Message = message;
        }

        public void SetJsonResponse(string rawString)
        {
            try
            {

                this.Response = JsonDocument.Parse(rawString);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                Response = null;
            }
        }

        public string GetJsonResponseAsString()
        {
            return this.Response?.RootElement.ToString() ?? string.Empty;
        }
        #endregion

        #region EXPERIMENTAL METHODS

        #endregion
        //used only for requests not streams
        public async Task ConnectAsync()
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(IpAddress, Port);
                using NetworkStream stream = client.GetStream();

                // Send message
                //byte[] dataToSend = Encoding.UTF8.GetBytes(Message + '\0');
                byte[] dataToSend = DatabaseRequest.BuildMessage(
                    auth: "my_secure_auth_token",
                    isStream: false,
                    videoPosition: 0,
                    requestLength: 0,
                    request: this.Message ?? string.Empty
                );

                Console.Write($"Message sent to server: {Message}");
                Console.WriteLine();
                await stream.WriteAsync(dataToSend, 0, dataToSend.Length);

                //Read until server closes
                using var memoryStream = new MemoryStream();
                byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesRead);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                string jsonResp = Encoding.UTF8.GetString(memoryStream.ToArray()).TrimEnd('\0');

                //might be able to replace var jsonResp with this.RawStringResponse
                this.rawStringResponse = jsonResp;
                Console.WriteLine($"Received response from server:");
                Console.WriteLine(jsonResp);

                SetJsonResponse(jsonResp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

        }


        //This is for TCP Video playing. Not used for normal requests. Want to migrate to UDP later.
        public async Task ConnectVideoAsync(long range, long amount)
        {
            using TcpClient vidClient = new TcpClient();
            await vidClient.ConnectAsync(IpAddress, Port);
            using NetworkStream vidStream = vidClient.GetStream();

            //prep and send the video request message with range
            byte[] dataTosend = DatabaseRequest.BuildMessage(
                auth: "my_secure_auth_token",
                isStream: true,
                requestLength: amount,
                videoPosition: range, // Pass the range parameter here
                request: this.Message ?? string.Empty
            );

            await vidStream.WriteAsync(dataTosend, 0, dataTosend.Length);

            //Read incoming video data
            //Note: The incoming data will not be a JSON. It will be raw video bytes.
            using var videoMemoryStream = new MemoryStream();
            int bufferSize = (1024 * 1024); // 1MB buffer
            byte[] videoBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            //Might considering caching video since the we need to account for network latency
            // From browser to server then server to database and back.
            //and buffering. But for now, just stream directly.

            try
            {
                int bytesRead;
                while ((bytesRead = await vidStream.ReadAsync(videoBuffer, 0, videoBuffer.Length)) > 0)
                {
                    videoMemoryStream.Write(videoBuffer, 0, bytesRead);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(videoBuffer);
            }

            Console.WriteLine($"Received video data: {videoMemoryStream.Length} bytes");
            if (!(videoMemoryStream.Length == 0))
            {
                this.VideoBuffer = videoBuffer;
            }

            
        }
    }
}

