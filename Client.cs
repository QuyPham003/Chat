using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_PHAMHOANGQUY2
{
    public partial class Client : Form
    {
        TcpClient client;
        NetworkStream stream;
        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            ConnectToServer();
        }
        private void ConnectToServer()
        {
            client = new TcpClient("127.0.0.1", 8888);
            stream = client.GetStream();
            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();
        }
        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string fullMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Xử lý khi nhận danh sách client
                        if (fullMessage.StartsWith("CLIENT_LIST: "))
                        {
                            string clientList = fullMessage.Substring("CLIENT_LIST: ".Length);
                            UpdateClientList(clientList);
                        }
                        // Xử lý hình ảnh
                        else if (fullMessage.StartsWith("IMAGE:"))
                        {
                            string[] imageInfo = fullMessage.Split(':');
                            string imageName = imageInfo[1];
                            long imageSize = long.Parse(imageInfo[2]);

                            byte[] imageData = new byte[imageSize];
                            int totalBytesReceived = 0;
                            while (totalBytesReceived < imageSize)
                            {
                                int bytes = stream.Read(imageData, totalBytesReceived, imageData.Length - totalBytesReceived);
                                if (bytes <= 0) break;
                                totalBytesReceived += bytes;
                            }

                            string savePath = Path.Combine(Application.StartupPath, "ReceivedImages", imageName);
                            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                            File.WriteAllBytes(savePath, imageData);

                            Invoke((MethodInvoker)(() =>
                            {
                                MessageBox.Show($"Đã nhận hình ảnh {imageName} và lưu tại: {savePath}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                AddMessageToPanel(flowLayoutTinNhanChat, "Server", $"Đã nhận hình ảnh: {imageName}", false);
                            }));
                        }
                        // Xử lý khi nhận file
                        else if (fullMessage.StartsWith("FILE:"))
                        {
                            string[] fileInfo = fullMessage.Split(':');
                            string fileName = fileInfo[1];
                            long fileSize = long.Parse(fileInfo[2]);

                            byte[] fileData = new byte[fileSize];
                            int totalBytesReceived = 0;
                            while (totalBytesReceived < fileSize)
                            {
                                int bytes = stream.Read(fileData, totalBytesReceived, fileData.Length - totalBytesReceived);
                                if (bytes <= 0) break;
                                totalBytesReceived += bytes;
                            }

                            string savePath = Path.Combine(Application.StartupPath, "ReceivedFiles", fileName);
                            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                            File.WriteAllBytes(savePath, fileData);

                            Invoke((MethodInvoker)(() =>
                            {
                                MessageBox.Show($"Đã nhận file {fileName} và lưu tại: {savePath}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                AddMessageToPanel(flowLayoutTinNhanChat, "Server", $"Đã nhận file: {fileName}", false);
                            }));
                        }
                        // Xử lý tin nhắn thông thường
                        else
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                AddMessageToPanel(flowLayoutTinNhanChat, "Server", fullMessage, false);
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi nhận tin nhắn: {ex.Message}");
                    break;
                }
            }
        }

        private void AddMessageToPanel(FlowLayoutPanel panel, string sender, string message, bool isSender)
        {
            // Panel chính chứa cả tin nhắn và nút
            Panel containerPanel = new Panel
            {
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 10, 0),
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent,
            };

            // Panel chứa nội dung tin nhắn
            Panel messagePanel = new Panel
            {
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 10, 0),
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = isSender ? Color.LightBlue : Color.LightGray,
                BorderStyle = BorderStyle.None,
            };

            Label contentLabel = new Label
            {
                Text = $"{sender}: {message}",
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Regular),
                MaximumSize = new Size(panel.Width - 50, 0),
            };

            // Kiểm tra nếu tin nhắn là hình ảnh hoặc tệp tin
            if (message.StartsWith("Đã nhận hình ảnh:"))
            {
                // Hiển thị hình ảnh
                string imageName = message.Substring("Đã nhận hình ảnh: ".Length).Trim();
                string imagePath = Path.Combine(Application.StartupPath, "ReceivedImages", imageName);

                try
                {
                    // Tạo ảnh thu nhỏ
                    Image originalImage = Image.FromFile(imagePath);
                    Image thumbnail = originalImage.GetThumbnailImage(100, 100, null, IntPtr.Zero); // Thay đổi kích thước theo ý muốn

                    PictureBox pictureBox = new PictureBox
                    {
                        Image = thumbnail,
                        SizeMode = PictureBoxSizeMode.Zoom, // Hoặc StretchImage
                        MaximumSize = new Size(panel.Width - 50, 200)
                    };

                    // Thêm ảnh thu nhỏ vào messagePanel
                    messagePanel.Controls.Add(pictureBox);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi hiển thị ảnh: {ex.Message}");
                }
            }
            else if (message.StartsWith("Đã nhận tệp:"))
            {
                // Hiển thị tệp tin
                string fileName = message.Substring("Đã nhận tệp: ".Length).Trim();
                string filePath = Path.Combine(Application.StartupPath, "ReceivedFiles", fileName);
                LinkLabel fileLinkLabel = new LinkLabel
                {
                    Text = fileName,
                    AutoSize = true,
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };
                fileLinkLabel.Click += (s, ev) =>
                {
                    System.Diagnostics.Process.Start(filePath);
                };
                messagePanel.Controls.Add(fileLinkLabel);
            }
            else
            {
                // Nếu là tin nhắn văn bản, thêm vào messagePanel
                messagePanel.Controls.Add(contentLabel);
            }

            // Thêm nội dung tin nhắn vào containerPanel
            containerPanel.Controls.Add(messagePanel);

            // Panel chứa các nút
            Panel buttonPanel = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(100),
                Padding = new Padding(100),
            };

            // Nút Xóa tin nhắn
            Button deleteButton = new Button
            {
                Text = "Xóa",
                Size = new Size(50, 30),
                Font = new Font("Arial", 8),
                BackColor = Color.Blue,
                Margin = new Padding(100),
            };

            deleteButton.Click += (s, ev) =>
            {
                panel.Controls.Remove(containerPanel);
            };

            // Nút Chuyển tiếp tin nhắn
            Button forwardButton = new Button
            {
                Text = "Chuyển tiếp",
                Size = new Size(80, 30),
                Font = new Font("Arial", 8),
                Margin = new Padding(100),
                BackColor = Color.Red, // Đặt màu nền của nút thành đỏ
                ForeColor = Color.White // Thay đổi màu chữ cho rõ ràng
            };
            forwardButton.Click += (s, ev) =>
            {
                ForwardMessage(message); // Gọi hàm chuyển tiếp
            };

            // Thêm các nút vào buttonPanel
            buttonPanel.Controls.Add(deleteButton);
            buttonPanel.Controls.Add(forwardButton);

            // Nhãn thời gian
            Label timeLabel = new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss"),
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Bottom,
            };

            // Thêm các thành phần vào containerPanel
            containerPanel.Controls.Add(buttonPanel);  // Thêm các nút
            containerPanel.Controls.Add(timeLabel);    // Thêm thời gian

            // Căn chỉnh containerPanel trong FlowLayoutPanel
            containerPanel.Dock = isSender ? DockStyle.Right : DockStyle.Left;
            panel.Controls.Add(containerPanel);

            // Tự động cuộn tới tin nhắn mới
            panel.ScrollControlIntoView(containerPanel);
        }



        private void ForwardMessage(string message)
        {
            string targetClient = comboBoxClients.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(targetClient))
            {
                MessageBox.Show("Vui lòng chọn một client để chuyển tiếp tin nhắn.");
                return;
            }

            string fullMessage = $"{targetClient}: {message}";

            // Gửi tin nhắn tới Server
            byte[] data = Encoding.UTF8.GetBytes(fullMessage);
            stream.Write(data, 0, data.Length);

            MessageBox.Show($"Tin nhắn đã được chuyển tiếp đến {targetClient}.", "Thông báo");
        }



        private void UpdateClientList(string clientList)
        {
            var clients = clientList.Split(';');
            string currentClientId = client.Client.LocalEndPoint.ToString(); // Lấy ID của chính client

            // Loại bỏ ID của chính client khỏi danh sách
            var filteredClients = clients.Where(c => c != currentClientId).ToArray();

            Invoke((MethodInvoker)(() =>
            {
                comboBoxClients.Items.Clear();
                comboBoxClients.Items.AddRange(filteredClients);

                // Nếu không có client nào khác, hiển thị thông báo
                if (filteredClients.Length == 0)
                {
                    comboBoxClients.Items.Add("Không có client nào khác.");
                }
            }));
        }

        private void btnSendClient_Click(object sender, EventArgs e)
        {
            string message = txtSendClient.Text.Trim();
            string targetClient = comboBoxClients.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Vui lòng nhập tin nhắn.");
                return;
            }

            if (string.IsNullOrWhiteSpace(targetClient))
            {
                MessageBox.Show("Không có client nào để gửi tin nhắn.");
                return;
            }

            string fullMessage = $"{targetClient}: {message}";

            // Gửi tin nhắn tới Server
            byte[] data = Encoding.UTF8.GetBytes(fullMessage);
            stream.Write(data, 0, data.Length);

            AddMessageToPanel(flowLayoutTinNhanChat, "Me", message, true);
            txtSendClient.Clear();
        }
        List<string> emojis = new List<string> {  "😀", "😂", "😍", "😎", "😢", "😡", "👍", "👎", "❤️", "🔥",
    "😱", "😅", "🤔", "😜", "😏", "💪", "🤗", "😇", "😴", "💖",
    "🥺", "😜", "🤩", "🥳", "😬", "🤭", "🙌", "💃", "🕺", "🌟",
    "🎉", "🍀", "🌈", "🎂", "🎁", "🎈", "🥳", "💥", "💫", "💎",
    "🌻", "🌼", "🌸", "🌺", "🍎", "🍓", "🍉", "🍍", "🍒", "🍓" };

        private void btnShowEmojiClient_Click(object sender, EventArgs e)
        {
            FlowLayoutPanel emojiPanel = new FlowLayoutPanel
            {
                AutoSize = false, // Không tự động thay đổi kích thước
                Size = new Size(200, 100), // Kích thước cố định của panel
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.LightGray,
                Padding = new Padding(2),
                AutoScroll = true, // Bật cuộn nếu danh sách emoji quá dài
            };
            foreach (var emoji in emojis)
            {
                Button emojiButton = new Button
                {
                    Text = emoji,
                    Size = new Size(40, 40), // Kích thước nút emoji
                    Font = new Font("Arial", 14),
                    Padding = new Padding(2),
                    Margin = new Padding(2),
                };

                emojiButton.Click += (s, ev) =>
                {
                    txtSendClient.Text += emojiButton.Text; // Thêm emoji vào TextBox của Client
                    emojiPanel.Hide(); // Ẩn bảng emoji sau khi chọn
                };

                emojiPanel.Controls.Add(emojiButton);
            }

            this.Controls.Add(emojiPanel);
            emojiPanel.BringToFront();
            emojiPanel.Location = new Point(btnShowEmojiClient.Left, btnShowEmojiClient.Bottom + 5); // Hiển thị dưới nút

        }

        private void btnDelClient_Click(object sender, EventArgs e)
        {
            // Xóa tất cả tin nhắn trong flowLayoutTinNhanChat
            var confirmResult = MessageBox.Show("Bạn có chắc muốn xóa tất cả tin nhắn không?",
                                                "Xác nhận xóa",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                flowLayoutTinNhanChat.Controls.Clear(); // Xóa toàn bộ các tin nhắn
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string imagePath = openFileDialog.FileName;
                string imageName = Path.GetFileName(imagePath);  // Extract image name

                try
                {
                    // Read the image data into a byte array
                    byte[] imageData = File.ReadAllBytes(imagePath);

                    // Prepare header with image name and size
                    string header = $"IMAGE:{imageName}:{imageData.Length}";
                    byte[] headerBytes = Encoding.UTF8.GetBytes(header);

                    // Send header to server
                    stream.Write(headerBytes, 0, headerBytes.Length);

                    // Send image data in chunks
                    SendImageInChunks(imageData);

                    MessageBox.Show($"Đang gửi hình ảnh: {imageName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi gửi hình ảnh: {ex.Message}");
                }
            }
        }
        private void SendImageInChunks(byte[] imageData)
        {
            int chunkSize = 1024; // Define chunk size
            int totalBytesSent = 0;

            while (totalBytesSent < imageData.Length)
            {
                int bytesToSend = Math.Min(chunkSize, imageData.Length - totalBytesSent);
                stream.Write(imageData, totalBytesSent, bytesToSend);
                totalBytesSent += bytesToSend;
            }
        }

        private void btnSendFindClient_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog()
         == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                byte[] fileData = File.ReadAllBytes(filePath);

                string header = $"FILE:{fileName}:{fileData.Length}";
                byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                stream.Write(headerBytes, 0, headerBytes.Length);
                stream.Write(fileData, 0, fileData.Length);
            }
        }

       
    }
}
