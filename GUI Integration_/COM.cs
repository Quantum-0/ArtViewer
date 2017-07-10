using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using SharpShell.SharpThumbnailHandler;
using SharpShell.SharpInfoTipHandler;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using ArtLib;

namespace ArtThumbnailHandler
{
    // THUMBNAIL
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".art")]
    public class ArtThumbHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                var temp = Path.GetTempFileName();
                using (var fs = new FileStream(temp, FileMode.Create))
                {
                    while (true)
                    {
                        var Byte = SelectedItemStream.ReadByte();
                        if (Byte == -1)
                            break;
                        fs.WriteByte((byte)Byte);
                    }
                    fs.Close();
                }
                return Art.GetThumbnail(temp, (int)width);
                /*var zip = new ZipArchive(SelectedItemStream, ZipArchiveMode.Read);
                var fname = Path.GetTempFileName() + ".png";
                zip.Entries[0].ExtractToFile(fname);
                Image img = (Image)Image.FromFile(fname);

                int w, h;
                if (img.Size.Height > img.Size.Width)
                {
                    h = (int)width;
                    w = h * img.Size.Width / img.Size.Height;
                }
                else
                {
                    w = (int)width;
                    h = w * img.Size.Height / img.Size.Width;
                }

                var bmp = new Bitmap(w, h);

                using (var graphics = Graphics.FromImage(bmp))
                    graphics.DrawImage(img, 0, 0, w, h);
                
                return bmp;*/

            }
            catch
            {
                return null;
            }
        }
    }

    // INFOTIP
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".art")]
    public class ArtInfoTipHandler : SharpInfoTipHandler
    {
        protected override string GetInfo(RequestedInfoType infoType, bool singleLine)
        {
            switch (infoType)
            {
                case RequestedInfoType.InfoTip:
                    return Art.GetToolTipDescription(SelectedItemPath, singleLine);
                case RequestedInfoType.Name:
                    return Path.GetFileName(SelectedItemPath);
                case RequestedInfoType.InfoOfShortcut:
                    return "";
                case RequestedInfoType.InfoOfShortcutTarget:
                    return "";
                default:
                    break;
            }
            return string.Empty;
        }
    }

    // CONTEXT MENUS
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".psd")]
    public class PsdContextMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            
            var convertToPng = new ToolStripMenuItem
            {
                Text = "Преобразовать в PNG",
                ToolTipText = "Конвертирует данный PSD файл в PNG файл, объединяя слои"
            //Image = Properties.Resources.CountLines
            };

            var convertToGif = new ToolStripMenuItem
            {
                Text = "Создать Gif",
                ToolTipText = "Создаёт gif анимацию, преобразуя слои в кадры"
                //Image = Properties.Resources.CountLines
            };

            var copyContent = new ToolStripMenuItem
            {
                Text = "Скопировать содержимое",
                ToolTipText = "Копирует содержимое как png изображение в буфер обмена"
                //Image = Properties.Resources.CountLines
            };

            var makeArt = new ToolStripMenuItem
            {
                Text = "Преобразовать в ART-файл",
                ToolTipText = "Преобразовывает данный PSD файл в ART файл"
                //Image = Properties.Resources.CountLines
            };

            var mymenu = new ToolStripMenuItem()
            {
                Text = "ArtViewer",
                BackColor = Color.Aqua
            };

            mymenu.DropDownItems.Add(convertToPng);
            mymenu.DropDownItems.Add(convertToGif);
            mymenu.DropDownItems.Add(copyContent);
            mymenu.DropDownItems.Add(makeArt);

            //  When we click, we'll count the lines.
            convertToPng.Click += (sender, args) => ConvertToPng(SelectedItemPaths);// CountLines();

            //  Add the item to the context menu.
            menu.Items.Add(mymenu);

            //  Return the menu.
            return menu;
        }

        private void ConvertToPng(IEnumerable<string> fnames)
        {
            var art = new Art();
            foreach (var fname in fnames)
            {
                art.New(fname);
                art.Image.ToBitmap(System.Drawing.Imaging.ImageFormat.Png).Save(Path.ChangeExtension(fname, "png"));
            }
        }
    }

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".png")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".jpg")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".jpeg")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".bmp")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".dib")]
    public class OtherExtensionsContextMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var convertToArt = new ToolStripMenuItem
            {
                Text = "Преобразовать в ART-файл",
                ToolTipText = "Преобразовывает изображение в ART-файл"
                //Image = Properties.Resources.CountLines
            };
            var mymenu = new ToolStripMenuItem("ArtViewer");
            mymenu.DropDownItems.Add(convertToArt);
            convertToArt.Click += (sender, args) => ConvertToArt(SelectedItemPaths);
            menu.Items.Add(mymenu);
            return menu;
        }

        private void ConvertToArt(IEnumerable<string> fnames)
        {
            var art = new Art();
            foreach (var fname in fnames)
            {
                try
                {
                    art.New(fname);
                    art.Save(Path.ChangeExtension(fname, "art"), "");
                    File.Delete(fname);
                }
                catch
                {
                    MessageBox.Show("Не удалось преобразовать файл в ART");
                }
            }
        }
    }

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".gif")]
    public class GifContextMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var convertToArt = new ToolStripMenuItem
            {
                Text = "Преобразовать в ART-файл",
                ToolTipText = "Преобразовывает изображение в ART-файл"
                //Image = Properties.Resources.CountLines
            };

            var exportFrames = new ToolStripMenuItem
            {
                Text = "Экспорт кадров",
                ToolTipText = "Экспортирует кадры анимации в папку"
                //Image = Properties.Resources.CountLines
            };

            var convertToLayers = new ToolStripMenuItem
            {
                Text = "Преобразовать в слои PSD",
                ToolTipText = "Преобразовывает кадры изображения в слои PSD файла"
                //Image = Properties.Resources.CountLines
            };

            var mymenu = new ToolStripMenuItem()
            {
                Text = "ArtViewer",
            };

            mymenu.DropDownItems.Add(convertToArt);
            mymenu.DropDownItems.Add(exportFrames);
            mymenu.DropDownItems.Add(convertToLayers);

            //  When we click, we'll count the lines.
            convertToArt.Click += (sender, args) => ConvertToArt(SelectedItemPaths);

            //  Add the item to the context menu.
            menu.Items.Add(mymenu);
            return menu;
        }

        private void ConvertToArt(IEnumerable<string> fnames)
        {
            var art = new Art();
            foreach (var fname in fnames)
            {
                try
                {
                    art.New(fname);
                    art.Save(Path.ChangeExtension(fname, "art"), "");
                    File.Delete(fname);
                }
                catch
                {
                    MessageBox.Show("Не удалось преобразовать файл в ART");
                }
            }
        }
    }

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".art")]
    public class ArtContextMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var art = new Art();
            art.Open(SelectedItemPaths.First());
            var locked = art.Locked;

            var passwordChange = new ToolStripMenuItem
            {
                Text = locked ? "Удалить пароль" : "Добавить пароль",
                ToolTipText = "Добавляет или снимает пароль с данного арта",
                Image = locked ? Properties.Resources.Locked : Properties.Resources.Unlocked
            };

            var export = new ToolStripMenuItem
            {
                Text = "Экспортировать",
                ToolTipText = "Преобразовывает файл в исходный формат",
                Image = Properties.Resources.Export
            };

            var copyContent = new ToolStripMenuItem
            {
                Text = "Скопировать содержимое",
                ToolTipText = "Копирует изображение в буфер обмена",
                Image = Properties.Resources.Copy
            };

            var openWithSAI = new ToolStripMenuItem
            {
                Text = "Открыть в SAI",
                ToolTipText = "Открывает изображение в SAI Paint Tool"
            };

            var openInfo = new ToolStripMenuItem
            {
                Text = "Показать информацию",
                ToolTipText = "Показывает информацию, добавленную к этому арту",
                Enabled = false
            };

            var lockFile = new ToolStripMenuItem
            {
                Text = "Заблокировать",
                ToolTipText = "Блокирует файл без возможности дальнейшего изменения (Осторожно!)",
                Enabled = false
            };

            var uploadedToFA = new ToolStripMenuItem
            {
                Text = "Арт загружен на ФА",
                CheckOnClick = true,
                Enabled = false
            };

            var paid = new ToolStripMenuItem
            {
                Text = "Арт оплачен",
                CheckOnClick = true,
                Enabled = false
            };

            var marks = new ToolStripMenuItem
            {
                Text = "Пометка"
            };
            marks.DropDownItems.Add(uploadedToFA);
            marks.DropDownItems.Add(paid);

            var mymenu = new ToolStripMenuItem("ArtViewer");

            mymenu.DropDownItems.Add(passwordChange);
            mymenu.DropDownItems.Add(export);
            mymenu.DropDownItems.Add(copyContent);
            mymenu.DropDownItems.Add(openWithSAI);
            mymenu.DropDownItems.Add(openInfo);
            mymenu.DropDownItems.Add(lockFile);
            mymenu.DropDownItems.Add(marks);
            
            export.Click += (sender, args) => ConvertToImage(SelectedItemPaths);
            copyContent.Click += (sender, args) => CopyContent(SelectedItemPaths);
            passwordChange.Click += (sender, args) => ChangePassword(SelectedItemPaths);
            openWithSAI.Click += (sender, args) => OpenWithSAI(SelectedItemPaths);
            menu.Items.Add(mymenu);
            return menu;
        }

        private void OpenWithSAI(IEnumerable<string> fnames)
        {
            var art = new Art();
            
            var saipath = GetSaiPath();
            if (saipath == null)
                return;

            foreach (var fname in fnames)
            {
                art.Open(fname);
                if (art.ReadOnly)
                {
                    MessageBox.Show("Файл заблокирован.", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                if (art.Locked)
                {
                    MessageBox.Show("На файле установлен пароль.", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                var imagefname = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), art.Extension);
                art.Export(imagefname);
                System.Diagnostics.Process.Start(saipath, imagefname);
            }
        }

        private string GetSaiPath()
        {
            var saipath = (string)Properties.Settings.Default["SaiPath"];
            if (string.IsNullOrWhiteSpace(saipath))
            {
                var opendialog = new OpenFileDialog() { Filter = "Приложения|*.exe" };
                if (opendialog.ShowDialog() == DialogResult.OK)
                {
                    saipath = opendialog.FileName;
                    Properties.Settings.Default["SaiPath"] = saipath;
                    Properties.Settings.Default.Save();
                }
                else
                    return null;
            }

            if (File.Exists(saipath))
                return saipath;
            else
                return null;
        }

        private void ChangePassword(IEnumerable<string> fnames)
        {
            var art = new Art();

            if (fnames.Count() != 1)
            {
                MessageBox.Show("Для копирования содержимого должен быть выбран только один файл");
                return;
            }

            foreach (var fname in fnames)
            {
                art.Open(fname);
                if (art.ReadOnly)
                {
                    MessageBox.Show("Файл заблокирован.", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                var form = new FormPassword();
                if (art.Locked)
                {
                    form.Text = "Удаление пароля";
                    form.textBoxPassword.Text = "Введите установленный пароль";
                    form.textBoxPassword.SelectAll();
                    form.textBoxPassword.Focus();
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (art.OpenWithPassword(form.textBoxPassword.Text))
                        {
                            File.Delete(fname);
                            art.Save(fname, null);
                            MessageBox.Show("Пароль удалён");
                        }
                        else
                            MessageBox.Show("Неверный пароль", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    form.Text = "Добавление пароля";
                    form.textBoxPassword.Text = "Введите новый пароль";
                    form.textBoxPassword.SelectAll();
                    form.textBoxPassword.Focus();
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrWhiteSpace(form.textBoxPassword.Text))
                        {
                            File.Delete(fname);
                            art.Save(fname, form.textBoxPassword.Text);
                            MessageBox.Show("Пароль добавлен");
                        }
                        else
                            MessageBox.Show("Невозможно установить пустой пароль", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void ConvertToImage(IEnumerable<string> fnames)
        {
            var art = new Art();
            foreach (var fname in fnames)
            {
                art.Open(fname);
                if (art.ReadOnly)
                {
                    MessageBox.Show("Файл заблокирован.", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                if (art.Locked)
                {
                    MessageBox.Show("На файле установлен пароль.", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                art.Export(Path.ChangeExtension(fname, art.Extension));
            }
        }

        private void CopyContent(IEnumerable<string> fnames)
        {
            var art = new Art();

            if (fnames.Count() != 1)
            {
                MessageBox.Show("Для копирования содержимого должен быть выбран только один файл");
                return;
            }

            foreach (var fname in fnames)
            {
                art.Open(fname);
                if (art.ReadOnly)
                {
                    MessageBox.Show("Файл заблокирован.", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                if (art.Locked)
                {
                    MessageBox.Show("На файле установлен пароль.", "ArtViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                Clipboard.SetImage(art.GetBitmap());
            }
        }
    }
}
