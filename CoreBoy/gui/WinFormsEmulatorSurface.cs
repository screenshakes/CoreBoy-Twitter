﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CoreBoy.gui
{
    public partial class WinFormsEmulatorSurface : Form
    {
        private readonly Action _onFormClosed;
        private readonly PictureBox _pictureBox;

        public WinFormsEmulatorSurface(Action onFormClosed)
        {
            _onFormClosed = onFormClosed ?? (() => { });
            InitializeComponent();

            _pictureBox = new PictureBox
            {
                Width = BitmapDisplay.DisplayWidth * 5, 
                Height = BitmapDisplay.DisplayHeight * 5, 
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Controls.Add(_pictureBox);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_pictureBox == null) return;

            _pictureBox.Width = Width;
            _pictureBox.Height = Height;
        }

        public void UpdateDisplay(object sender, byte[] frame)
        {
            using var memoryStream = new MemoryStream(frame);
            _pictureBox.Image = Image.FromStream(memoryStream);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _onFormClosed();
            base.OnFormClosed(e);
            _pictureBox.Dispose();
        }
    }
}
