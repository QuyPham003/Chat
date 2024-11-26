using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        TcpListener server;
        List<TcpClient> connectedClients = new List<TcpClient>();
        Dictionary<TcpClient, NetworkStream> clientStreams = new Dictionary<TcpClient, NetworkStream>();
        Dictionary<string, NetworkStream> clients = new Dictionary<string, NetworkStream>();
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            StartServer();
        }
        private void StartServer()
        {
            server = new TcpListener(IPAddress.Any, 8888);
            server.Start();
            Thread acceptClientThread = new Thread(AcceptClient);
            acceptClientThread.Start();
        }
        private void AcceptClient()
        {
            while (true)
            {
                BroadcastClientList();
                TcpClient client = server.AcceptTcpClient();
                connectedClients.Add(client);
                NetworkStream stream = client.GetStream();
                clientStreams[client] = stream;

                // Show the new client in the UI client list (assuming you have a ListBox named listBoxClients)
                Invoke((MethodInvoker)(() => listBoxClients.Items.Add(client.Client.RemoteEndPoint.ToString())));

                Thread receiveThread = new Thread(() => ReceiveMessage(client));
                receiveThread.Start();
            }
        }
        private void ReceiveMessage(TcpClient client)
        {
            NetworkStream stream = clientStreams[client];
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string fullMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Kiểm tra nếu thông điệp là tệp tin
                        if (fullMessage.StartsWith("FILE:"))
                        {
                            // Phân tích header tệp tin
                            string[] fileHeader = fullMessage.Split(':');
                            if (fileHeader.Length >= 3)
                            {
                                string fileName = fileHeader[1];
                                long fileSize = long.Parse(fileHeader[2]);

                                // Nhận nội dung file
                                byte[] fileData = new byte[fileSize];
                                int totalBytesRead = 0;
                                while (totalBytesRead < fileSize)
                                {
                                    int read = stream.Read(fileData, totalBytesRead, fileData.Length - totalBytesRead);
                                    if (read <= 0) break;
                                    totalBytesRead += read;
                                }

                                // Lưu tệp tin vào thư mục đích
                                string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReceivedFiles", fileName);
                                Directory.CreateDirectory(Path.GetDirectoryName(savePath)); // Tạo thư mục nếu chưa tồn tại
                                File.WriteAllBytes(savePath, fileData);

                                Invoke((MethodInvoker)(() =>
                                {
                                    AddMessageToPanel(flowLayoutKhungChat, client.Client.RemoteEndPoint.ToString(), $"Đã nhận tệp: {fileName} (lưu tại: {savePath})", false);
                                }));
                            }

                        }
                        else
                        {
                            // Hiển thị tin nhắn thông thường
                            Invoke((MethodInvoker)(() =>
                            {
                                AddMessageToPanel(flowLayoutKhungChat, client.Client.RemoteEndPoint.ToString(), fullMessage, false);
                            }));

                            // Xử lý gửi tin nhắn thông thường
                            int separatorIndex = fullMessage.IndexOf(": ");
                            if (separatorIndex > -1)
                            {
                                string targetClient = fullMessage.Substring(0, separatorIndex);
                                string actualMessage = fullMessage.Substring(separatorIndex + 2);

                                // Tìm client đích
                                var target = connectedClients.FirstOrDefault(c => c.Client.RemoteEndPoint.ToString() == targetClient);
                                if (target != null)
                                {
                                    SendMessageToClient($"{client.Client.RemoteEndPoint}: {actualMessage}", target);
                                }
                                else
                                {
                                    SendMessageToClient("Client đích không tồn tại.", client);
                                }
                            }
                            else
                            {
                                SendMessageToClient("Định dạng tin nhắn không hợp lệ.", client);
                            }
                        }
                    }
                }
                catch
                {
                    // Xử lý khi client ngắt kết nối
                    connectedClients.Remove(client);
                    clientStreams.Remove(client);
                    Invoke((MethodInvoker)(() => listBoxClients.Items.Remove(client.Client.RemoteEndPoint.ToString())));
                    client.Close();
                    BroadcastClientList(); // Cập nhật danh sách client sau khi xóa
                    break;
                }
            }
        }
        private void BroadcastClientList()
        {
            string clientList = string.Join(";", connectedClients.Select(c => c.Client.RemoteEndPoint.ToString()));
            byte[] data = Encoding.UTF8.GetBytes($"CLIENT_LIST: {clientList}");

            foreach (var client in connectedClients)
            {
                try
                {
                    clientStreams[client].Write(data, 0, data.Length);
                }
                catch
                {
                    // Xử lý khi client ngắt kết nối
                    connectedClients.Remove(client);
                    clientStreams.Remove(client);
                    client.Close();
                }
            }
        }
        private void BroadcastMessage(string message, TcpClient sender)
        {
            foreach (var client in connectedClients)
            {
                try
                {
                    // Kiểm tra nếu client hiện tại là người gửi, không gửi lại tin nhắn cho chính nó
                    if (client != sender)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        clientStreams[client].Write(data, 0, data.Length);
                    }
                }
                catch
                {
                    // Xử lý khi client ngắt kết nối
                    connectedClients.Remove(client);
                    clientStreams.Remove(client);
                    client.Close();
                }
            }
        }
        private void SendMessageToSelectedClients(string message)
        {
            var selectedClients = listBoxClients.SelectedItems.Cast<string>();
            foreach (var selectedClient in selectedClients)
            {
                var client = connectedClients.FirstOrDefault(c => c.Client.RemoteEndPoint.ToString() == selectedClient);
                if (client != null)
                {
                    SendMessageToClient(message, client);
                }
            }
        }
        private void SendMessageToClient(string message, TcpClient client)
        {
            if (clientStreams.ContainsKey(client))
            {
                string fullMessage = $"HoangQuy: {message}";
                byte[] data = Encoding.UTF8.GetBytes(fullMessage);
                clientStreams[client].Write(data, 0, data.Length);
            }
        }
        private void AddMessageToPanel(FlowLayoutPanel panel, string sender, string message, bool isSender)
        {
            Panel messagePanel = new Panel
            {
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 10, 0),
                Margin = new Padding(5),
                Padding = new Padding(10),
                BackColor = isSender ? Color.LightBlue : Color.LightGray,
                BorderStyle = BorderStyle.None,
            };

            Label contentLabel = new Label
            {
                Text = message,
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Regular),
                MaximumSize = new Size(panel.Width - 50, 0),
            };

            // Container cho các nút

            // Thêm nút xóa vào tin nhắn
            Button deleteButton = new Button
            {
                Text = "Xóa",
                Size = new Size(50, 30),
                Font = new Font("Arial", 8),
                Margin = new Padding(0, 15, 0, 0),
                Visible = true, // Hiển thị nút xóa cho cả người gửi và người nhận
            };

            deleteButton.Click += (s, ev) =>
            {
                // Xóa tin nhắn khỏi panel
                panel.Controls.Remove(messagePanel);

                // Gửi thông báo đến client để xóa tin nhắn trên các client khác (nếu cần)
                DeleteMessageOnClients(message, sender);
            };

            messagePanel.Controls.Add(contentLabel);
            messagePanel.Controls.Add(deleteButton);
            messagePanel.Dock = isSender ? DockStyle.Right : DockStyle.Left;
            panel.Controls.Add(messagePanel);

            Label timeLabel = new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss"),
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 10, 10),
                Anchor = AnchorStyles.Right,
                Dock = DockStyle.Bottom
            };

            panel.Controls.Add(timeLabel);
            panel.ScrollControlIntoView(messagePanel);
        }
        private void btnSendServer_Click(object sender, EventArgs e)
        {
            string message = txtnhaptinnhanServer.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            lbtennguoidung.Text = "Server gửi";
            AddMessageToPanel(flowLayoutKhungChat, "", message, true);

            // Send the message to all selected clients
            var selectedClients = listBoxClients.SelectedItems.Cast<string>();
            foreach (var selectedClient in selectedClients)
            {
                var client = connectedClients.FirstOrDefault(c => c.Client.RemoteEndPoint.ToString() == selectedClient);
                if (client != null)
                {
                    SendMessageToClient(message, client);
                }
            }

            txtnhaptinnhanServer.Clear();
        }
        List<string> emojis = new List<string> {  "😀", "😂", "😍", "😎", "😢", "😡", "👍", "👎", "❤️", "🔥",
    "😱", "😅", "🤔", "😜", "😏", "💪", "🤗", "😇", "😴", "💖",
    "🥺", "😜", "🤩", "🥳", "😬", "🤭", "🙌", "💃", "🕺", "🌟",
    "🎉", "🍀", "🌈", "🎂", "🎁", "🎈", "🥳", "💥", "💫", "💎",
    "🌻", "🌼", "🌸", "🌺", "🍎", "🍓", "🍉", "🍍", "🍒", "🍓" };

        private void btnShowEmojiServer_Click(object sender, EventArgs e)
        {
            FlowLayoutPanel emojiPanel = new FlowLayoutPanel
            {
                AutoSize = false, // Không tự động thay đổi kích thước
                Size = new Size(200, 100), // Kích thước cố định của danh sách emoji
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.LightGray,
                Padding = new Padding(2),
                AutoScroll = true, // Bật cuộn
            };

            foreach (var emoji in emojis)
            {
                Button emojiButton = new Button
                {
                    Text = emoji,
                    Size = new Size(40, 40), // Kích thước cố định cho từng nút emoji
                    Font = new Font("Arial", 14), // Font chữ đủ lớn để hiển thị emoji rõ ràng
                    Padding = new Padding(2),
                    Margin = new Padding(2),
                };

                emojiButton.Click += (s, ev) =>
                {
                    txtnhaptinnhanServer.Text += emojiButton.Text;
                    emojiPanel.Hide();
                };

                emojiPanel.Controls.Add(emojiButton);
            }

            this.Controls.Add(emojiPanel);
            emojiPanel.BringToFront();
            emojiPanel.Location = new Point(btnShowEmojiServer.Left, btnShowEmojiServer.Bottom + 5);

        }
        private void DeleteMessageOnClients(string message, string sender)
        {
            string deleteMessage = $"DELETE: {sender}: {message}";

            // Gửi thông báo xóa tin nhắn đến tất cả client
            foreach (var client in connectedClients)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(deleteMessage);
                    clientStreams[client].Write(data, 0, data.Length);
                }
                catch
                {
                    // Xử lý khi client ngắt kết nối
                    connectedClients.Remove(client);
                    clientStreams.Remove(client);
                    client.Close();
                }
            }
        }

        private void btnDelServer_Click(object sender, EventArgs e)
        {
            // Xóa tất cả tin nhắn trong flowLayoutTinNhanChat
            var confirmResult = MessageBox.Show("Bạn có chắc muốn xóa tất cả tin nhắn không?",
                                                "Xác nhận xóa",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                flowLayoutKhungChat.Controls.Clear(); // Xóa toàn bộ các tin nhắn
            }
        }

        
    }
}
