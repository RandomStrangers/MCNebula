﻿// Part of fCraft | Copyright 2009-2015 Matvei Stefarov <me@matvei.org> | BSD-3 | See LICENSE.txt
using System.Windows.Forms;
using System.Drawing;
using System;
using MCNebula;

namespace MCNebula.Gui.Popups 
{
    internal sealed partial class ColorSelector : Form 
    {
        public char ColorCode;

        internal static Color LookupColor(char colorCode, out Color textColor) {
            ColorDesc color = Colors.Get(colorCode);
            
            if (color.Undefined)
                color = new ColorDesc(255, 255, 255);
            
            textColor = ColorUtils.CalcTextColor(color);
            return Color.FromArgb(color.R, color.G, color.B);
        }
        

        public ColorSelector(string title, char oldColorCode) {
            ColorCode = oldColorCode;
            InitializeComponent();
            Text = title;
            
            SuspendLayout();
            for (int i = 0; i < Colors.List.Length; i++) {
                if (Colors.List[i].Undefined) continue;
                MakeButton(Colors.List[i].Code);
            }
            
            UpdateBaseLayout();
            ResumeLayout(false);
        }
        
        void ColorSelector_Load(object sender, EventArgs e) {
            GuiUtils.SetIcon(this);
        }
        
        
        const int btnWidth = 130, btnHeight = 40, btnsPerCol = 8;
        int index = 0;
        void MakeButton(char colCode) {
            int row = index / btnsPerCol, col = index % btnsPerCol;
            index++;
            
            Button btn = new Button();
            Color textCol;
            btn.BackColor = LookupColor(colCode, out textCol);
            btn.ForeColor = textCol;
            btn.Location = new Point(9 + row * btnWidth, 7 + col * btnHeight);
            btn.Size = new Size(btnWidth, btnHeight);
            btn.Name = "b" + index;
            btn.TabIndex = index;
            
            btn.Text = Colors.Name(colCode) + " - " + colCode;
            btn.Click += delegate { ColorCode = colCode; DialogResult = DialogResult.OK; Close(); };
            btn.Margin = new Padding(0);
            btn.UseVisualStyleBackColor = false;
            btn.Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Controls.Add(btn);
        }
        
        
        void UpdateBaseLayout() {
            int rows = index / btnsPerCol;
            if ((index % btnsPerCol) != 0) rows++; // round up
            
            int x = 0;
            // Centre if even count, align under row if odd count
            if ((rows & 1) == 0) {
                x = (rows * btnWidth) / 2 - (100 / 2);
            } else {
                x = ((rows / 2) * btnWidth) + (btnWidth - 100) / 2;
            }

            btnCancel.Location = new Point(8 + x, 12 + btnHeight * btnsPerCol);
            ClientSize = new Size(18 + btnWidth * rows, 47 + btnHeight * btnsPerCol);
        }
    }
}