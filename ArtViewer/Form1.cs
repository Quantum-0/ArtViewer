using ArtLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArtViewer
{
    public partial class Form1 : Form
    {
        Stopwatch sw = new Stopwatch();
        private string[] CreationArgs;
        private readonly string[] SupportedFormats = new[] { ".art", ".jpg", ".jpeg", ".jpe", ".png", ".gif", ".svg", ".psd", ".bmp", ".dib" };
        Art Art = new Art();
        List<FileSystemInfo> OpenedFiles;
        private int _CurrentFileInOpened;
        public int CurrentFileInOpened
        {
            set
            {
                _CurrentFileInOpened = value;
                listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;
                listBox1.SelectedIndex = _CurrentFileInOpened;
                listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            }
            get
            {
                return _CurrentFileInOpened;
            }
        }

        public Form1(string[] args)
        {
            InitializeComponent();
            CreationArgs = args;
        }

        private void Art_ArtChanged(object sender, EventArgs e)
        {
            this.Invoke((Action)(() => {
            textBox1.Text = Art.Title;
            textBox3.Text = Art.Char;
            textBox2.Text = Art.Keywords;
            comboBox2.Text = Art.Quality;
            comboBox3.Text = Art.Type;
            comboBox1.Text = Art.Author;
            checkBox1.Checked = Art.Adult;
            checkBox2.Checked = Art.UploadedToFA;
            checkBox3.Checked = Art.ReadOnly;
            checkBox4.Checked = Art.Paid;
            //dataGridView1.Hide();
            if (Art.Image != null)
                pictureBox1.Image = Art.Image.ToBitmap();
            else if (Art.GifImage != null)
            {
                /*var framesCount = Art.GifImage.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
                var width = 50 * Art.GifImage.Width / Art.GifImage.Height;
                dataGridView1.ColumnCount = 0;
                var bytes = (byte[])(new ImageConverter()).ConvertTo(Art.GifImage, typeof(byte[]));
                var magickGif = new ImageMagick.MagickImageCollection(bytes);
                magickGif.Coalesce();
                for (int i = 0; i < framesCount; i++)
                {
                    magickGif[i].Resize(width, 50);
                    var col = new DataGridViewImageColumn();
                    col.Width = width;
                    col.Image = magickGif[i].ToBitmap();
                    dataGridView1.Columns.Add(col);
                }
                dataGridView1.RowCount = 1;
                dataGridView1.Rows[0].Height = 50;
                dataGridView1.Show();*/
                pictureBox1.Image = Art.GifImage;
            }

            textBox1.Enabled = !Art.ReadOnly && !Art.Locked;
            textBox2.Enabled = !Art.ReadOnly && !Art.Locked;
            textBox3.Enabled = !Art.ReadOnly && !Art.Locked;
            comboBox1.Enabled = !Art.ReadOnly && !Art.Locked;
            comboBox2.Enabled = !Art.ReadOnly && !Art.Locked;
            comboBox3.Enabled = !Art.ReadOnly && !Art.Locked;
            checkBox1.Enabled = !Art.ReadOnly && !Art.Locked;
            checkBox2.Enabled = !Art.ReadOnly && !Art.Locked;
            checkBox3.Enabled = !Art.ReadOnly && !Art.Locked;
            checkBox4.Enabled = !Art.ReadOnly && !Art.Locked;
            button2.Enabled = !Art.ReadOnly && !Art.Locked;
            button3.Enabled = !Art.ReadOnly && !Art.Locked;
            }));
        }

        public void OpenFile(string fname)
        {
            Task openingTask = Task.Delay(0);
            sw.Start();
            this.ActiveControl = label1;
            if (File.Exists(fname))
            {
                ClearFields();
                this.Text = "Loading..";
                if (Path.GetExtension(fname).ToLower() == ".art")
                {
                    try
                    {
                        openingTask = Task.Run(() => { Art.Open(fname); });
                    }
                    catch
                    {
                        pictureBox1.Image = new Bitmap(16, 16);
                    }
                }
                else
                {
                    openingTask = Task.Run(() => { Art.New(fname); });
                }
                if (Path.GetExtension(fname).ToLower() == ".png" || Path.GetExtension(fname).ToLower() == ".jpg" || Path.GetExtension(fname).ToLower() == ".jpeg" || Path.GetExtension(fname).ToLower() == ".gif" || Path.GetExtension(fname).ToLower() == ".bmp")
                {
                    pictureBox1.Image = new Bitmap(fname);
                }
                else
                {
                    openingTask.Wait();
                    Art_ArtChanged(this, EventArgs.Empty);
                    pictureBox1.Image = Art.Image.ToBitmap();
                }
                this.Text = "ArtViewer";
                var openedWith = sw.ElapsedMilliseconds;

                var dir = Path.GetDirectoryName(fname);
                var sorttype = comboBox4.SelectedIndex + 1;
                Task.Run(() =>
                {
                    OpenedFiles = new DirectoryInfo(dir).GetFileSystemInfos("*.*", SearchOption.TopDirectoryOnly)
                        .Where(fn => SupportedFormats.Contains(Path.GetExtension(fn.Name)
                        .ToLower()))
                        .ToList();
                    Invoke((Action)(() => {
                        Resort(sorttype, false);
                    }));
                    var CurrentFI = OpenedFiles.Find(fi => fi.FullName == fname);
                    openingTask.Wait();
                    sw.Stop();
                    Invoke((Action)(() => {
                        try
                        {
                            if (Art.Extension == ".gif")
                                label8.Text = "Размер: " + (CurrentFI as FileInfo).Length / 1024 + "кб" + Environment.NewLine
                                        + "Дата создания: " + CurrentFI.CreationTime.ToShortTimeString() + ' ' + CurrentFI.CreationTime.ToShortDateString() + Environment.NewLine
                                        + "Дата изменения: " + CurrentFI.LastWriteTime.ToShortTimeString() + ' ' + CurrentFI.LastWriteTime.ToShortDateString() + Environment.NewLine
                                        + "Количество кадров: " + Art.GifImage.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time).ToString() + Environment.NewLine
                                        + "Размер изображения: " + Art.GifImage.Width + 'x' + Art.GifImage.Height + Environment.NewLine
                                        + "Формат пикселей: " + Art.GifImage.PixelFormat + Environment.NewLine
                                        + "Открыто за " + openedWith + "мс, информация обработана за " + sw.ElapsedMilliseconds + "мс";
                            else
                                label8.Text = "Размер: " + (CurrentFI as FileInfo).Length / 1024 + "кб" + Environment.NewLine
                                        + "Дата создания: " + CurrentFI.CreationTime.ToShortTimeString() + ' ' + CurrentFI.CreationTime.ToShortDateString() + Environment.NewLine
                                        + "Дата изменения: " + CurrentFI.LastWriteTime.ToShortTimeString() + ' ' + CurrentFI.LastWriteTime.ToShortDateString() + Environment.NewLine
                                        /*+ (Art.Extension == ".gif" ?
                                                "Количество кадров: " + Art.Image.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time).ToString() + Environment.NewLine
                                                : "")*/
                                        + "Размер изображения: " + Art.Image.Width + 'x' + Art.Image.Height + Environment.NewLine
                                        + "Цветовое пространство: " + Art.Image.ColorSpace + Environment.NewLine
                                        + "Тип цвета: " + Art.Image.ColorType + Environment.NewLine
                                        + "Сжатие: " + Art.Image.CompressionMethod + Environment.NewLine
                                        + "Глубина цвета: " + Art.Image.Depth + " бит/канал" + Environment.NewLine
                                        + "Формат изображения: " + Art.Image.Format + Environment.NewLine
                                        + "Чересстрочность: " + Art.Image.Interlace + Environment.NewLine
                                        + "Качество: " + Art.Image.Quality + Environment.NewLine
                                        + "Количество цветов: " + Art.Image.TotalColors + Environment.NewLine
                                        + "Открыто за " + openedWith + "мс, информация обработана за " + sw.ElapsedMilliseconds + "мс";
                        }
                        catch { };
                    }));
                    sw.Reset();
                    Invoke((Action)(() => {
                        label8.Top = splitContainer2.Panel2.Height - label8.Height;
                        listBox1.Height = label8.Top - 22;
                        textBox4.Clear();
                    }));
                });
            }
        }

        private void ClearFields()
        {
            this.Invoke((Action)(() => {
                textBox1.Text = "";
                textBox3.Text = "";
                textBox2.Text = "";
                comboBox2.Text = "";
                comboBox3.Text = "";
                comboBox1.Text = "";
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                checkBox3.Enabled = true;
                checkBox4.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            OpenFile(openFileDialog.FileName);
        }

        private void Resort(int By, bool Descending)
        {
            switch (By)
            {
                case 1: // по названию
                    OpenedFiles.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));
                    break;
                case 2: // по расширению
                    OpenedFiles.Sort((f1, f2) => f1.Extension.CompareTo(f2.Extension));
                    break;
                case 3: // по дате создания
                    OpenedFiles.Sort((f1, f2) => f1.CreationTimeUtc.CompareTo(f2.CreationTimeUtc));
                    break;
                case 4: // по дате изменения
                    OpenedFiles.Sort((f1, f2) => f1.LastWriteTimeUtc.CompareTo(f2.LastWriteTimeUtc));
                    break;
                case 5: // по дате открытия
                    OpenedFiles.Sort((f1, f2) => f1.LastWriteTimeUtc.CompareTo(f2.LastWriteTimeUtc));
                    break;
                default:
                    break;
            }

            if (!Descending)
                OpenedFiles.Reverse();
            
            listBox1.DataSource = OpenedFiles.Select(fn => fn.Name).ToList();
            CurrentFileInOpened = OpenedFiles.FindIndex(fi => fi.Name == Path.GetFileName(Art.Filename));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Art.Title = textBox1.Text;
            Art.Author = comboBox1.Text;
            Art.Char = textBox3.Text;
            Art.Keywords = textBox2.Text;
            Art.Adult = checkBox1.Checked;
            Art.UploadedToFA = checkBox2.Checked;
            Art.ReadOnly = checkBox3.Checked;
            Art.Paid = checkBox4.Checked;
            Art.Quality = comboBox2.Text;
            Art.Type = comboBox3.Text;
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            Art.Save(saveFileDialog.FileName, textBox4.Text);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.ActiveControl is TextBox)
                return;

            if (e.KeyData == Keys.Right || e.KeyData == Keys.Left)
            {
                if (e.KeyData == Keys.Right || e.KeyData == Keys.Down)
                    CurrentFileInOpened = Math.Max(0, Math.Min(OpenedFiles.Count - 1, CurrentFileInOpened + 1));
                else
                    CurrentFileInOpened = Math.Max(0, Math.Min(OpenedFiles.Count - 1, CurrentFileInOpened - 1));
                OpenFile(OpenedFiles[CurrentFileInOpened].FullName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var sd = new SaveFileDialog();
            sd.Filter = "Изображения " + Art.Extension.Substring(1).ToString().ToUpper() + "|*" + Art.Extension.ToUpper();
            if (sd.ShowDialog() == DialogResult.OK)
                Art.Export(sd.FileName);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.Size.Height < 450 || this.Size.Width < 500)
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer2.Panel1Collapsed = true;
            }
            else if (this.Size.Width > 768)
            {
                splitContainer1.Panel2Collapsed = false;
                splitContainer2.Panel1Collapsed = false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.ActiveControl = label1;
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
            splitContainer2.Panel1Collapsed = !splitContainer2.Panel1Collapsed;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            Resort(comboBox4.SelectedIndex + 1, false);
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                if (Art.Locked)
                    if (Art.OpenWithPassword(textBox4.Text))
                        textBox4.Text = "";
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            checkBox3.ForeColor = checkBox3.Checked ? Color.Red : Color.Black;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            label6.ForeColor = !string.IsNullOrEmpty(textBox4.Text) ? Color.Red : Color.Black;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!listBox1.Focused)
                return;

            OpenFile(OpenedFiles[listBox1.SelectedIndex].FullName);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Art.ArtChanged += Art_ArtChanged;

            if (CreationArgs.Length == 1)
            {
                OpenFile(CreationArgs[0]);
            }
            else if (CreationArgs.Length > 1)
            {
                OpenFile(CreationArgs[0]);
                CurrentFileInOpened = 0;
                OpenedFiles = CreationArgs.Select(f => new FileInfo(f) as FileSystemInfo).ToList();
            }
        }
    }
}
