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

                        // Xử lý thông điệp là hình ảnh
                        if (fullMessage.StartsWith("IMAGE:"))
                        {
                            string[] imageHeader = fullMessage.Split(':');
                            if (imageHeader.Length >= 3)
                            {
                                string imageName = imageHeader[1];
                                long imageSize = long.Parse(imageHeader[2]);

                                // Nhận nội dung hình ảnh
                                byte[] imageData = new byte[imageSize];
                                int totalBytesRead = 0;
                                while (totalBytesRead < imageSize)
                                {
                                    int read = stream.Read(imageData, totalBytesRead, imageData.Length - totalBytesRead);
                                    if (read <= 0) break;
                                    totalBytesRead += read;
                                }

                                // Lưu hình ảnh vào thư mục đích
                                string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReceivedImages", imageName);
                                Directory.CreateDirectory(Path.GetDirectoryName(savePath)); // Tạo thư mục nếu chưa tồn tại
                                File.WriteAllBytes(savePath, imageData);

                                // Thông báo đã nhận hình ảnh
                                Invoke((MethodInvoker)(() =>
                                {
                                    AddMessageToPanel(flowLayoutChat, client.Client.RemoteEndPoint.ToString(), $"Đã nhận hình ảnh: {imageName} (lưu tại: {savePath})", false);
                                }));
                            }
                        }
                        // Xử lý thông điệp là tệp tin
                        else if (fullMessage.StartsWith("FILE:"))
                        {
                            string[] fileHeader = fullMessage.Split(':');
                            if (fileHeader.Length >= 3)
                            {
                                string fileName = fileHeader[1];
                                long fileSize = long.Parse(fileHeader[2]);

                                // Nhận nội dung tệp tin
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

                                // Thông báo đã nhận tệp tin
                                Invoke((MethodInvoker)(() =>
                                {
                                    AddMessageToPanel(flowLayoutChat, client.Client.RemoteEndPoint.ToString(), $"Đã nhận tệp: {fileName} (lưu tại: {savePath})", false);
                                }));
                            }
                        }
                        // Xử lý tin nhắn văn bản thông thường
                        else
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                AddMessageToPanel(flowLayoutChat, client.Client.RemoteEndPoint.ToString(), fullMessage, false);
                            }));

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
            // Create the message container panel
            Panel messagePanel = new Panel
            {
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 10, 0),
                Margin = new Padding(5),
                Padding = new Padding(10),
                BackColor = isSender ? Color.LightBlue : Color.LightGray,
                BorderStyle = BorderStyle.None,
            };

            // Create the label for the message content
            Label contentLabel = new Label
            {
                Text = message,
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Regular),
                MaximumSize = new Size(panel.Width - 50, 0),
            };

            // Create a new panel to hold the buttons below the message content
            Panel buttonPanel = new Panel
            {
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(5),
                Dock = DockStyle.Bottom
            };

            // Create delete button
            Button deleteButton = new Button
            {
                Text = "Xóa",
                Size = new Size(50, 30),
                Font = new Font("Arial", 8),
                Margin = new Padding(0),
                Visible = true, // Visible for both sender and receiver
            };

            deleteButton.Click += (s, ev) =>
            {
                // Remove the message panel from the flow layout panel
                panel.Controls.Remove(messagePanel);

                // Optionally, send a message to clients to delete the message if needed
                DeleteMessageOnClients(message, sender);
            };

            // Create forward button
            Button forwardButton = new Button
            {
                Text = "Chuyển tiếp",
                Size = new Size(80, 30),
                Font = new Font("Arial", 8),
                Margin = new Padding(5, 0, 0, 0),
                Visible = true, // Visible for both sender and receiver
            };

            forwardButton.Click += (s, ev) =>
            {
                // Handle forward functionality
                ForwardMessageToClients(message, sender);
            };

            // Add buttons to the button panel
            buttonPanel.Controls.Add(deleteButton);
            buttonPanel.Controls.Add(forwardButton);

            // Check if the message starts with a specific keyword for an image
            if (message.StartsWith("Đã nhận hình ảnh:"))
            {
                try
                {
                    // Lấy đường dẫn hình ảnh từ tin nhắn
                    string imagePath = message.Substring("Đã nhận hình ảnh:".Length).Split('(')[0].Trim();

                    // Tạo thumbnail
                    Image thumbnail = CreateThumbnail(imagePath, 50, 50);

                    // Tạo PictureBox để hiển thị thumbnail
                    PictureBox pictureBox = new PictureBox
                    {
                        Image = thumbnail,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        MaximumSize = new Size(panel.Width - 50, 200)
                    };

                    // Thêm pictureBox vào messagePanel
                    messagePanel.Controls.Add(pictureBox);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error displaying image: {ex.Message}");
                }
            }
            // Check if the message starts with a specific keyword for a file
            else if (message.StartsWith("Đã nhận tệp:"))
            {
                // Handle file message
                string filePath = message.Substring("Đã nhận tệp:".Length).Split('(')[0].Trim();
                string fileName = Path.GetFileName(filePath);

                // Create a link label for the file
                LinkLabel fileLinkLabel = new LinkLabel
                {
                    Text = fileName,
                    AutoSize = true,
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };

                fileLinkLabel.Click += (s, ev) =>
                {
                    // Open the file when clicked
                    System.Diagnostics.Process.Start(filePath);
                };

                // Add the file link label to the message panel
                messagePanel.Controls.Add(fileLinkLabel);
            }
            else
            {
                // Handle regular text message
                messagePanel.Controls.Add(contentLabel);
            }

            // Add the button panel below the message content
            messagePanel.Controls.Add(buttonPanel);

            // Set the docking of the message panel based on the sender
            messagePanel.Dock = isSender ? DockStyle.Right : DockStyle.Left;
            panel.Controls.Add(messagePanel);

            // Add the timestamp label at the bottom of the panel
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

            // Add time label to the panel
            panel.Controls.Add(timeLabel);

            // Scroll the panel to the newly added message panel
            panel.ScrollControlIntoView(messagePanel);
        }

        // Define this method to handle forwarding the message to clients
        private void ForwardMessageToClients(string message, string sender)
        {
            // Logic for forwarding the message (e.g., notifying other clients)
            // This can be implemented in several ways depending on your architecture (e.g., via SignalR, sockets, etc.)

            try
            {
                // Example of forwarding the message to other clients
                // This part depends on how you want to forward the message (server-side or directly to other users)

                // Assuming you have some way to broadcast or notify other clients
                Console.WriteLine($"Forwarding message from {sender}: {message}");

                // If you're using something like SignalR to notify clients:
                // Clients.All.SendAsync("ReceiveMessage", sender, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error forwarding message: {ex.Message}");
            }
        }



        private Image CreateThumbnail(string imagePath, int width, int height)
        {
            try
            {
                Image originalImage = Image.FromFile(imagePath);

                // Tính toán kích thước mới giữ nguyên tỷ lệ khung hình
                int newWidth, newHeight;
                double ratio = (double)originalImage.Width / originalImage.Height;
                if (originalImage.Width > originalImage.Height)
                {
                    newWidth = width;
                    newHeight = (int)(width / ratio);
                }
                else
                {
                    newHeight = height;
                    newWidth = (int)(height * ratio);
                }

                return new Bitmap(originalImage, new Size(newWidth, newHeight));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating thumbnail: {ex.Message}");
                return null;
            }
        }



        private void btnSendServer_Click(object sender, EventArgs e)
        {
            string message = txtnhaptinnhanServer.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            lbtennguoidung.Text = "Server gửi";
            AddMessageToPanel(flowLayoutChat, "", message, true);

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
                flowLayoutChat.Controls.Clear(); // Xóa toàn bộ các tin nhắn
            }
        }

        private void btnSendImageServer_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif";
            if (openFileDialog.ShowDialog() == DialogResult.OK)

            {
                string imagePath = openFileDialog.FileName;

                string imageName = Path.GetFileName(imagePath);
                byte[] imageData = File.ReadAllBytes(imagePath);

                foreach (var client in connectedClients)
                {
                    try
                    {
                        NetworkStream stream = clientStreams[client];
                        string header = $"IMAGE:{imageName}:{imageData.Length}";
                        byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                        stream.Write(headerBytes, 0, headerBytes.Length);
                        stream.Write(imageData, 0, imageData.Length);
                    }
                    catch
                    {
                        // Xử lý lỗi
                    }
                }
            }
        }
        

        private void btnSendFileServer_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog()
         == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                byte[] fileData = File.ReadAllBytes(filePath);

                foreach (var client in connectedClients)
                {
                    try
                    {
                        NetworkStream stream = clientStreams[client];
                        string header = $"FILE:{fileName}:{fileData.Length}";
                        byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                        stream.Write(headerBytes, 0, headerBytes.Length);
                        stream.Write(fileData, 0, fileData.Length);
                    }
                    catch
                    {
                        // Xử lý lỗi
                    }
                }
            }
        }
    }
}
