/*
 * Copyright (c) 2008 spacecat56
 * Based in part on concepts in VB.Net example code published by Paul Kimmel
 * 
 * This file is part of TsRemux.
 * 
 * TsRemux is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * TsRemux is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with TsRemux.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TsRemux
{
    public partial class frmConsole : Form
    {
        public class TBWriter : System.IO.TextWriter
        {
            private TextBoxBase concon; 

            public TBWriter(TextBox tb)
            {
                this.concon = tb;
            }

            public override  void Write( Char c  )
            {
                Write(c.ToString());
            }

            public override void Write( String txt)
            {
               if (concon.IsHandleCreated)
                   concon.AppendText(txt);
            }
            
            public override void WriteLine( String txt)
            {
                Write(txt + Environment.NewLine);
            }            

            public override Encoding Encoding
            {
                get {return new System.Text.UTF8Encoding();} // Encoding.GetEncoding(850);}
            }
        }

        private System.IO.TextWriter tw = null;
        public System.IO.TextWriter ConWriter
        {
            get 
            { 
                if (tw==null)
                    tw = new TBWriter(this.tbConsole);
                return tw; 
            }
        }

        public frmConsole()
        {
            InitializeComponent();
        }
    }
}