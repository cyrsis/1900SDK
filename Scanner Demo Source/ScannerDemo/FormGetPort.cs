/****************************************************************************************************

THIS SOFTWARE IS PROVIDED "AS IS" AND, UNLESS OTHERWISE EXPRESSLY PROVIDED HEREIN OR EXCEPT AS
REQUIRED BY LAW, HONEYWELL AND ITS LICENSORS AND SUPPLIERS EACH DISCLAIM ALL WARRANTIES, EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF TITLE, MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE, TRADE USAGE, COURSE OF DEALING,AND NON-INFRINGEMENT WITH RESPECT TO SUCH
SOFTWARE.

IN NO EVENT SHALL HONEYWELL BE RESPONSIBLE OR LIABLE FOR ANY INDIRECT, EXEMPLARY, INCIDENTAL,
SPECIAL, PUNITIVE, RELIANCE OR CONSEQUENTIAL DAMAGES, OR FOR LOSS OF PROFITS, EVEN IF HONEYWELL
HAS BEEN PREVIOUSLY ADVISED OF THE POSSIBILITY OF SUCH DAMAGES, WHETHER IN AN ACTION UNDER
CONTRACT, TORT, STRICT LIABILITY, OR OTHER THEORY, ARISING OUT OF OR IN CONNECTION WITH THE USE
OR PERFORMANCE OF THIS SOFTWARE.

Copyright (c) 2011 HONEYWELL INTERNATIONAL INC.

****************************************************************************************************/

/***************************************************************************************************************
* Dialog box to select the scanner port.
* 
* This code is UNSUPPORTED and is provided for demonstration purposes only. The techniques used to locate the
* scanner on a USB interface and to enumerate the COM ports are NOT recommended for production code as they make
* assumptions about driver names and the registry layout in general. A better method is to enumerate the ports
* using Windows Setup and Device Installer services, but this is beyond the scope of this simple example.
***************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ScannerDemo
{
	public partial class FormGetPort : Form
	{
		private static string port = null;
		private static bool cancel = false;
		private string USBportname = null;

		/// <summary>
		/// Selected port name
		/// </summary>
		public static string Port
		{
			get { return port; }
		}

		/// <summary>
		/// Dialog cancel flag
		/// </summary>
		public static bool Cancel
		{
			get { return cancel; }
		}

		public FormGetPort()
		{
			InitializeComponent();
		}

		// Initialize the dropdown with the list of COM ports
		private void FormGetPort_Load(object sender, EventArgs e)
		{
			List<string> ports = new List<string>();
			// Try to find the list of serial port devices
			RegistryKey key = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM");
			if (key != null)
			{
				// Found it - enumerate the serial ports
				foreach (string name in key.GetValueNames())
				{
				    // Make sure this is not a default key before adding it
				    if (name != "")
				    {
						// It is possible that a faulty driver has appended invalid characters to the port name. This is typically not
						// noticed by Windows, but it will cause an exception when we try to sort the list of port names. So, we must
						// check that each character in the port number is valid
						string si = (string)key.GetValue(name);
						string so = si.Substring(0, 3);
						for (int i = 3; i < si.Length; i++)
						{
							if (char.IsDigit(si[i]))
							{
								so += si[i];
							}
						}
				        ports.Add(so);
				    }
				}
				// Look for the scanner on USB
				USBportname = (string)key.GetValue("\\Device\\honeywell_cdc_AcmSerial0");
				if (USBportname != null)
				{
					// Found it - add a special "autodetect" entry to the dropdown
					cmPort.Items.Add("USB Autodetect");
				}
				// Sort and add the list of ports to the dropdown
				ports.Sort(delegate(string t1, string t2) { return int.Parse(t1.Substring(3)).CompareTo(int.Parse(t2.Substring(3))); });
				cmPort.Items.AddRange(ports.ToArray());
				// If the dropdown is not empty, select the first item, otherwise exit
				if (cmPort.Items.Count > 0)
				{
					cmPort.SelectedIndex = 0;
				}
				else
				{
					Close();
				}
			}
		}

		// Set the static "return value" of the form when the dropdown index changes
		private void cmPort_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmPort.SelectedIndex == 0 && USBportname != null)
			{
				port = USBportname;
			}
			else
			{
				port = (string)cmPort.Items[cmPort.SelectedIndex];
			}
		}

		// Exit when the OK button is clicked
		private void btOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		// Exit and cancel selection when the Cancel button is clicked
		private void btCancel_Click(object sender, EventArgs e)
		{
			cancel = true;
			Close();
		}
	}
}
