using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ArtLib
{
    public class Art
    {
        public ImageMagick.MagickImage Image;
        public Bitmap GifImage;
        public ImageMagick.MagickImage Preview;
        public string Title;
        public string Author;
        public string Char;
        public string Keywords;
        public string Quality;
        public string Type;
        public bool Adult;
        public bool UploadedToFA;
        public bool ReadOnly;
        public bool Paid;
        public string Extension { private set; get; } // записывать?
        public string Filename { private set; get; }
        public bool Locked { private set; get; }
        private DateTime CreationTime;

        public event EventHandler ArtChanged;

        private void LoadImage(string fname, bool changeFileName = false)
        {
            if (changeFileName)
                Filename = fname;
            Extension = Path.GetExtension(fname);
            CreationTime = (new FileInfo(fname) as FileSystemInfo).CreationTimeUtc;

            Image?.Dispose();
            Image = null;
            GifImage?.Dispose();
            GifImage = null;

            if (Extension == ".gif")
                GifImage = new Bitmap(fname);
            else
                Image = new ImageMagick.MagickImage(fname);
        }

        private void LoadImageOrPreviewFromTemp(string tmpdir)
        {
            Image?.Dispose();
            Image = null;
            GifImage?.Dispose();
            GifImage = null;

            if (!Locked)
            {
                if (File.Exists(Path.Combine(tmpdir, "gif")))
                    GifImage = new Bitmap(Path.Combine(tmpdir, "gif"));
                else
                {
                    if (File.Exists(Path.Combine(tmpdir, "img")))
                        Image = new ImageMagick.MagickImage(Path.Combine(tmpdir, "img"));
                    else
                        throw new IOException("No file");
                }
            }
            else
            {
                if (File.Exists(Path.Combine(tmpdir, "prv")))
                    Image = new ImageMagick.MagickImage(Path.Combine(tmpdir, "prv"));
                else
                    throw new IOException("No preview file");
            }
        }

        private void SaveImage(string fname)
        {
            if (GifImage != null)
                GifImage.Save(fname);
            else
            {
                if (Image != null)
                    Image.ToBitmap().Save(fname);
                else
                    throw new InvalidOperationException("No image to save");
            }
        }

        private void SaveImageToTemp(string tmpdir)
        {
            if (GifImage != null)
                GifImage.Save(Path.Combine(tmpdir, "gif"));
            else
            {
                if (Image != null)
                    Image.ToBitmap().Save(Path.Combine(tmpdir, "img"));
                else
                    throw new InvalidOperationException("No image to save");
            }
        }

        private void ClearInfo()
        {
            Title = string.Empty;
            Author = string.Empty;
            Char = string.Empty;
            Keywords = string.Empty;
            Quality = "";
            Type = "";
            Adult = false;
            UploadedToFA = false;
            ReadOnly = false;
            Paid = false;
            Locked = false;
        }

        private string GenerateDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Название: ");
            sb.AppendLine(Title);
            sb.Append("Автор: ");
            sb.AppendLine(Author);
            sb.Append("Персонаж(и): ");
            sb.AppendLine(Char);
            sb.Append("Прорисованность: ");
            sb.AppendLine(Quality);
            sb.Append("Тип арта: ");
            sb.AppendLine(Type);
            if (UploadedToFA)
                sb.AppendLine("(Арт загружен на ФА)");
            if (Paid)
                sb.AppendLine("(Арт оплачен)");
            if (!string.IsNullOrWhiteSpace(Keywords))
            {
                sb.AppendLine("Ключевые слова:");
                sb.AppendLine(Keywords);
            }
            return sb.ToString();
        }

        public static string GetToolTipDescription(string fname, bool SingleLine)
        {
            var zip = new Ionic.Zip.ZipFile(fname);
            zip.Password = "ArtViewеr";
            string info, kwds;
            using (MemoryStream ms = new MemoryStream())
            {
                zip.Entries.FirstOrDefault(e => e.FileName == "info")?.Extract(ms);
                ms.Flush();
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    info = sr.ReadToEnd();
                    sr.Close();
                }
            }
            using (MemoryStream ms = new MemoryStream())
            {
                zip.Entries.FirstOrDefault(e => e.FileName == "kwds")?.Extract(ms);
                ms.Flush();
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    kwds = sr.ReadToEnd();
                    sr.Close();
                }
            }
            var art = new Art();
            art.ParseInfo(info);

            var sb = new StringBuilder();
            if (!SingleLine)
            {
                sb.AppendFormat("Арт \"{0}\"", !string.IsNullOrWhiteSpace(art.Title) ?
                    art.Title : Path.GetFileNameWithoutExtension(art.Filename));
                sb.AppendLine(art.Adult ? " [18+]" : string.Empty);
                if (art.Locked)
                    sb.AppendLine("<заблокирован>");
                if (!string.IsNullOrWhiteSpace(art.Author))
                    sb.AppendLine("От: " + art.Author);
                if (!string.IsNullOrWhiteSpace(art.Char))
                    sb.AppendLine("Для: " + art.Char);
                if (art.Image != null)
                    sb.AppendFormat("Размер: {0}x{1}\n", art.Image.Width, art.Image.Height);
                else if (art.GifImage != null)
                    sb.AppendFormat("Размер: {0}x{1}\n", art.GifImage.Width, art.GifImage.Height);
                if (art.Paid || art.UploadedToFA)
                {
                    sb.Append("---");
                    if (art.Paid && art.UploadedToFA)
                        sb.Append("Оплачен и залит на ФА");
                    else
                    {
                        if (art.Paid)
                            sb.Append("Оплачен");
                        else if (art.UploadedToFA)
                            sb.Append("Залит на ФА");
                    }
                    sb.Append("---");
                }
            }
            else // Single line
            {
                sb.Append("Арт ");
                if (!string.IsNullOrWhiteSpace(art.Title))
                    sb.Append(art.Title + ' ');
                if (!string.IsNullOrWhiteSpace(art.Author))
                    sb.AppendFormat("от {0} ", art.Author);
                if (!string.IsNullOrWhiteSpace(art.Char))
                    sb.AppendFormat("для {0}", art.Char);
            }

            return sb.ToString();
        }

        public static Bitmap GetThumbnail(string fname, int width)
        {
            var zip = new Ionic.Zip.ZipFile(fname);
            zip.Password = "ArtViewеr";
            ImageMagick.MagickImage Thumbnail;
            using (MemoryStream ms = new MemoryStream())
            {
                zip.Entries.FirstOrDefault(e => e.FileName == "thmb")?.Extract(ms);
                Thumbnail = new ImageMagick.MagickImage(ms);
            }
            Thumbnail.Thumbnail(width, width);
            return Thumbnail.ToBitmap();
        }

        private Bitmap CreatePreview()
        {
            if (GifImage != null)
            {
                var bytes = (byte[])(new ImageConverter()).ConvertTo(GifImage, typeof(byte[]));
                Preview = new ImageMagick.MagickImage(bytes);
            }
            else
            {
                if (Image == null)
                    throw new InvalidOperationException("No image to create preview");
                Preview = new ImageMagick.MagickImage(Image);
            }
            Preview.ColorSpace = ImageMagick.ColorSpace.Gray;
            Preview.Scale(new ImageMagick.Percentage(15));
            return Preview.ToBitmap();
        }

        private static string GetTempDir()
        {
            var tmpdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpdir);
            return tmpdir;
        }

        public void New(string fname)
        {
            ClearInfo();
            LoadImage(fname, changeFileName: true);
        }

        public void Export(string fname)
        {
            if (Locked)
                return;

            SaveImage(fname);
            File.WriteAllText(Path.ChangeExtension(fname, ".txt"), GenerateDescription());
            File.SetCreationTimeUtc(fname, CreationTime);
            File.SetCreationTimeUtc(Path.ChangeExtension(fname, ".txt"), CreationTime);
        }

        private Bitmap CreateThumbnail()
        {
            var Thumbnail = new ImageMagick.MagickImage(Image);
            int w, h;
            if (Image.Width > Image.Height)
            {
                w = 192;
                h = 192 * Image.Height / Image.Width;
            }
            else
            {
                h = 192;
                w = 192 * Image.Width / Image.Height;
            }
            Thumbnail.Resize(w, h);
            Thumbnail.Quality = 50;
            return Thumbnail.ToBitmap();
        }

        private void SaveInfo(string fname)
        {
            File.WriteAllText(fname, Title + '\n'
                + Author + '\n' + Char + '\n' + (Adult ? "1" : "0")
                + (UploadedToFA ? "1" : "0") + (ReadOnly ? "1" : "0") + (Paid ? "1" : "0") +
                '\n' + Extension + '\n' + Quality + '\n' + Type);
        }

        private void LoadInfo(string tmpdir)
        {
            var info = File.ReadAllText(Path.Combine(tmpdir, "info"));
            ParseInfo(info);
        }

        private void ParseInfo(string Content)
        {
            var info = Content.Split('\n');
            Title = info[0];
            Author = info[1];
            Char = info[2];
            Adult = info[3][0] == '1';
            UploadedToFA = info[3][1] == '1';
            ReadOnly = info[3][2] == '1';
            Paid = info[3][3] == '1';
            Extension = info[4];
            Quality = info[5];
            Type = info[6];
        }

        private void SaveKeywords(string fname)
        {
            File.WriteAllText(fname, Keywords);
        }

        private void LoadKeywords(string tmpdir)
        {
            Keywords = File.ReadAllText(Path.Combine(tmpdir, "kwds"));
        }

        private void SaveZip(string dir, string fname, string Password = null)
        {
            var img = Extension == ".gif" ? "gif" : "img";

            if (File.Exists(fname))
                File.Delete(fname);
            var zip = new Ionic.Zip.ZipFile(fname);
            zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
            zip.CompressionMethod = Ionic.Zip.CompressionMethod.None;
            zip.Encryption = Ionic.Zip.EncryptionAlgorithm.WinZipAes256;
            zip.Password = "ArtViewеr";
            if (!string.IsNullOrWhiteSpace(Password))
                zip.AddFile(Path.Combine(dir, "prv"), "");
            zip.AddFile(Path.Combine(dir, "info"), "");
            zip.AddFile(Path.Combine(dir, "kwds"), "");
            zip.AddFile(Path.Combine(dir, "thmb"), "");
            zip.Password = "ArtViewеr" + Password;
            zip.AddFile(Path.Combine(dir, img), "");
            zip.Save();
            if (CreationTime != default(DateTime))
                File.SetCreationTimeUtc(fname, CreationTime);
            zip.Dispose();
        }

        public void Save(string fname, string Password)
        {
            if (Locked)
                return;

            var tmpdir = GetTempDir();

            CreatePreview().Save(Path.Combine(tmpdir, "prv"));
            CreateThumbnail().Save(Path.Combine(tmpdir, "thmb"));
            SaveInfo(Path.Combine(tmpdir, "info"));
            SaveKeywords(Path.Combine(tmpdir, "kwds"));
            SaveImageToTemp(tmpdir);

            SaveZip(tmpdir, fname, Password);

            Directory.Delete(tmpdir, true);
        }

        private void ExtractZipAndCheckLocked(string fname, string tmpdir)
        {
            CreationTime = (new FileInfo(fname) as FileSystemInfo).CreationTimeUtc;
            var zip = new Ionic.Zip.ZipFile(fname);
            zip.Password = "ArtViewеr";
            if (!zip.Entries.Any(e => e.FileName == "prv"))
            {
                zip.ExtractAll(tmpdir);
                Locked = false;
            }
            else
            {
                Locked = true;
                foreach (var e in zip.Entries)
                {
                    if (e.FileName != "img" && e.FileName != "gif")
                        e.Extract(tmpdir);
                }
            }
        }

        public void Open(string fname)
        {
            Filename = fname;
            var tmpdir = GetTempDir();

            ExtractZipAndCheckLocked(fname, tmpdir);

            LoadImageOrPreviewFromTemp(tmpdir);
            LoadInfo(tmpdir);
            LoadKeywords(tmpdir);

            Directory.Delete(tmpdir, true);

            //ArtChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool TryLoadImageFromZipWithPassword(string password, string tmpdir)
        {
            var zip = new Ionic.Zip.ZipFile(Filename);
            zip.Password = "ArtViewеr" + password;
            foreach (var e in zip.Entries)
                if (e.FileName == "img" || e.FileName == "gif")
                    try
                    {
                        e.Extract(tmpdir);
                        Locked = false;
                        LoadImageOrPreviewFromTemp(tmpdir);
                        return true;
                    }
                    catch (Ionic.Zip.BadPasswordException)
                    {
                        return false;
                    }
            throw new IOException("No image in ART-file");
        }

        public bool OpenWithPassword(string password)
        {
            var tmpdir = GetTempDir();
            bool CorrectPassword = TryLoadImageFromZipWithPassword(password, tmpdir);
            Directory.Delete(tmpdir, true);
            if (CorrectPassword)
                ArtChanged?.Invoke(this, EventArgs.Empty);
            return CorrectPassword;
        }

        public Bitmap GetBitmap()
        {
            if (GifImage != null)
                return GifImage;
            else
            {
                if (Image != null)
                    return Image.ToBitmap();
                else
                    return null;
            }
        }
    }
}
