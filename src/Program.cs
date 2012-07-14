/*
 * Copyright (c) 2007, 2008 dmz
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
 * MODIFIED 04/14/2008 by spacecat56: 
 *    Support command-line execution.
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TsRemux
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TsRemux());
            }
            else
            {
                TsRemux tsr = new TsRemux(args);
                Form f = null; 
                if ((f = tsr.exec()) != null)
                    Application.Run(f);
            }

        }
    }
}